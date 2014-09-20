﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace iLynx.Common.Serialization
{
    /// <summary>
    /// NaiveSerializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NaiveSerializer<T> : ComponentBase, ISerializer
    {
        private readonly IEnumerable<SerializationInfo<T>> sortedGraph;
        private const BindingFlags FieldFlags = BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags PropertyFlags = BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="NaiveSerializer{T}" /> class.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Missing Types are currently not supported</exception>
        public NaiveSerializer(ILogger logger)
            : base(logger)
        {
            if (typeof(T) == Type.Missing.GetType()) throw new NotSupportedException("Missing Types are currently not supported");
            sortedGraph = BuildObjectGraph(typeof(T)).Values;
        }

        /// <summary>
        /// Builds the object graph.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static SortedList<Guid, SerializationInfo<T>> BuildObjectGraph(Type targetType)
        {
            var graph = new SortedList<Guid, SerializationInfo<T>>();
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
                              .Select(c => new SerializationInfo<T>(c, c.FieldType))
                              .Concat(targetType.GetProperties(PropertyFlags)    // Apparently BindingFLAGS don't work like flags...
                              .Where(p => null != p.SetMethod && p.SetMethod.IsPublic && null != p.GetMethod && p.GetMethod.IsPublic && p.GetMethod.GetParameters().Length == 0 && p.SetMethod.GetParameters().Length == 1)
                              .Where(p => !p.IsDefined(typeof(NotSerializedAttribute)))
                              .Select(p => new SerializationInfo<T>(p, p.PropertyType)))
                )
            {
                var id = hasAttribute
                             ? fieldInfo.Member.Name.CreateGuidV5(name)
                             : fieldInfo.Member.Name.CreateGuidV5(Serializer.SerializerNamespace);
                graph.Add(id, fieldInfo);
            }
            return graph;
        }

        public void Serialize(object item, Stream target)
        {
            if (!(item is T)) throw new InvalidOperationException("The specified item is not of the correct type");
            Serialize((T)item, target);
        }

        object ISerializer.Deserialize(Stream source)
        {
            return Deserialize(source);
        }

        public int GetOutputSize(object item)
        {
            var accum = 0;
            foreach (var member in sortedGraph)
            {
                ISerializer serializer;
                var value = member.IsDelegate ? null : member.GetValue(item);
                if (null == value || member.IsUntyped)
                {
                    value = value ?? new NullType();
                    var type = value.GetType();
                    serializer = Serializer.GetSerializer(type);
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
        public T Deserialize(Stream source)
        {
            var target = Activator.CreateInstance(typeof(T));
            foreach (var member in sortedGraph)
            {
                ISerializer serializer;
                var readType = source.ReadByte() == 0x01;
                if (readType)
                {
                    var memberType = ReadType(source);
                    if (null == memberType)
                        continue;
                    serializer = Serializer.GetSerializer(memberType);
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

        /// <summary>
        /// Posts the quit.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="m">The m.</param>
        private void PostQuit(Exception e, MethodBase m)
        {
            LogException(e, m);
            LogCritical("Last Error was unrecoverable. Giving up");
        }

        // ReSharper disable StaticFieldInGenericType
        private static readonly Encoding Unicode = Encoding.Unicode;
        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public void Serialize(T item, Stream target)
        {
            foreach (var member in sortedGraph)
            {
                ISerializer serializer;
                var value = member.IsDelegate ? null : member.GetValue(item);
                
                if (null == value || member.IsUntyped)
                {
                    value = value ?? new NullType();
                    var type = value.GetType();
                    serializer = Serializer.GetSerializer(type);
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
            var len = Serializer.SingletonBitConverter.ToInt32(length);
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
            var length = Serializer.SingletonBitConverter.GetBytes(typeBytes.Length);
            target.Write(length, 0, length.Length);
            target.Write(typeBytes, 0, typeBytes.Length);
        }
    }
}
