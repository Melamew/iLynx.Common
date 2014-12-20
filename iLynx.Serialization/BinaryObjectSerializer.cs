using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using iLynx.Common;

namespace iLynx.Serialization
{
    public static class SerializerExtensions
    {
        private const BindingFlags FieldFlags = BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags PropertyFlags = BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Builds the object graph.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="getSerializerCallback"></param>
        /// <returns></returns>
        public static SortedList<Guid, SerializationInfo<TSerializer>> BuildObjectGraph<TSerializer>(this Type targetType, Func<Type, TSerializer> getSerializerCallback)
        {
            var graph = new SortedList<Guid, SerializationInfo<TSerializer>>();
            var namespaceAttribute = targetType.GetCustomAttribute<GuidAttribute>();
            var name = Guid.Empty;
            var hasAttribute = false;
            if (null != namespaceAttribute) // Could be set to Guid.Empty for whatever reason...
            {
                name = new Guid(namespaceAttribute.Value);
                hasAttribute = true;
            }
            foreach (
                var fieldInfo in
                    targetType.GetFields(FieldFlags)
                              .Where(f => !f.IsDefined(typeof(NotSerializedAttribute)) && !f.IsNotSerialized)
                              .Select(c => new SerializationInfo<TSerializer>(c, c.FieldType, getSerializerCallback))
                              .Concat(targetType.GetProperties(PropertyFlags)    // Apparently BindingFLAGS don't work like flags...
                              .Where(p => null != p.SetMethod && p.SetMethod.IsPublic && null != p.GetMethod && p.GetMethod.IsPublic && p.GetMethod.GetParameters().Length == 0 && p.SetMethod.GetParameters().Length == 1)
                              .Where(p => !p.IsDefined(typeof(NotSerializedAttribute)))
                              .Select(p => new SerializationInfo<TSerializer>(p, p.PropertyType, getSerializerCallback)))
                )
            {
                var idBase = fieldInfo.Member.Name + fieldInfo.Type.FullName;
                var id = hasAttribute
                             ? idBase.CreateGuidV5(name)
                             : idBase.CreateGuidV5(RuntimeComm.SerializerNamespace);
                if (graph.ContainsKey(id))
                {
                    Trace.WriteLine("...");
                    continue;
                }
                graph.Add(id, fieldInfo);
            }
            return graph;
        }
    }

    public abstract class ObjectSerializerBase<T> : SerializerBase<T>
    {
        protected readonly IEnumerable<SerializationInfo<ISerializer>> Graph;

        protected ObjectSerializerBase(Func<Type, ISerializer> getSerializerCallback)
        {
            if (typeof(T) == Type.Missing.GetType()) throw new NotSupportedException("Missing Types are currently not supported");
            Graph = typeof(T).BuildObjectGraph(getSerializerCallback).Values;
        }

        /// <summary>
        /// Posts the quit.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="m">The m.</param>
        protected virtual void PostQuit(Exception e, MethodBase m)
        {
            this.LogError("{0}: {1}", e, m);
            this.LogCritical("Last Error was unrecoverable. Giving up");
        }
    }

    /// <summary>
    /// BinaryObjectSerializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryObjectSerializer<T> : ObjectSerializerBase<T>
    {
        public BinaryObjectSerializer()
            : base(BinarySerializerService.GetSerializer)
        {
        }

        public override int GetOutputSize(T item)
        {
            var accum = 0;
            foreach (var member in Graph)
            {
                ISerializer serializer;
                var value = member.IsDelegate ? null : member.GetValue(item);
                if (null == value || member.IsUntyped)
                {
                    value = value ?? new NullType();
                    var type = value.GetType();
                    serializer = BinarySerializerService.GetSerializer(type);
                }
                else
                    serializer = member.TypeSerializer;
                accum += serializer.GetOutputSize(value);
            }
            return accum;
        }

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public override T Deserialize(Stream source)
        {
            var target = Activator.CreateInstance(typeof(T));
            foreach (var member in Graph)
            {
                ISerializer serializer;
                var readType = source.ReadByte() == 0x01;
                if (readType)
                {
                    var memberType = ReadType(source);
                    if (null == memberType)
                        continue;
                    serializer = BinarySerializerService.GetSerializer(memberType);
                }
                else serializer = member.TypeSerializer;

                var value = serializer.Deserialize(source);
                try
                {
                    if (value is NullType)
                        member.SetValue(ref target, null);
                    else
                        member.SetValue(ref target, value);
                }
                catch (Exception e)
                {
                    PostQuit(e, MethodBase.GetCurrentMethod());
                    break;
                }
            }
            return (T)target;
        }

        // ReSharper disable StaticFieldInGenericType
        private static readonly Encoding Unicode = Encoding.Unicode;
        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public override void Serialize(T item, Stream target)
        {
            foreach (var member in Graph)
            {
                ISerializer serializer;
                var value = member.IsDelegate ? null : member.GetValue(item);

                if (null == value || member.IsUntyped)
                {
                    value = value ?? new NullType();
                    var type = value.GetType();
                    serializer = BinarySerializerService.GetSerializer(type);
                    target.WriteByte(0x01); // Indicate that we need to read the type when we deserialize.
                    WriteType(target, type);
                }
                else
                {
                    target.WriteByte(0x00); // Indicate that we do NOT need to read the type when we deserialize.
                    serializer = member.TypeSerializer;
                }

                try
                {
                    serializer.Serialize(value, target);
                }
                catch (Exception e)
                {
                    PostQuit(e, MethodBase.GetCurrentMethod());
                    break;
                }
            }
        }

        /// <summary>
        /// Reads the type.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        private static Type ReadType(Stream source)
        {
            var length = new byte[sizeof(int)];
            source.Read(length, 0, length.Length);
            var len = BinarySerializerService.SingletonBitConverter.ToInt32(length);
            if (len <= 0 || len >= 4096)
                return null;
            var field = new byte[len];
            source.Read(field, 0, field.Length);
            var typeString = Unicode.GetString(field);
            return Type.GetType(typeString);//, name => Assembly.Load(name.FullName), (assembly, s, arg3) => assembly == null ? Type.GetType(s) : assembly.GetType(s, false, arg3));
        }

        /// <summary>
        /// Writes the type.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="type">The type.</param>
        private static void WriteType(Stream target, Type type)
        {
            var typeBytes = Unicode.GetBytes(type.AssemblyQualifiedName ?? type.FullName);
            var length = BinarySerializerService.SingletonBitConverter.GetBytes(typeBytes.Length);
            target.Write(length, 0, length.Length);
            target.Write(typeBytes, 0, typeBytes.Length);
        }
    }
}
