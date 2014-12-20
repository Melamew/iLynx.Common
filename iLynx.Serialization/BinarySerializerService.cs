using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using iLynx.Common;

namespace iLynx.Serialization
{
    /// <summary>
    /// BinarySerializerService
    /// </summary>
    public class BinarySerializerService : ISerializerService
    {
        private static readonly Dictionary<Type, ISerializer> Overrides = new Dictionary<Type, ISerializer>
                                                                                    {
                                                                                             { typeof(int), new BinaryPrimitives.Int32Serializer() },
                                                                                             { typeof(uint), new BinaryPrimitives.UInt32Serializer() },
                                                                                             { typeof(short), new BinaryPrimitives.Int16Serializer() },
                                                                                             { typeof(ushort), new BinaryPrimitives.UInt16Serializer() },
                                                                                             { typeof(long), new BinaryPrimitives.Int64Serializer() },
                                                                                             { typeof(ulong), new BinaryPrimitives.UInt64Serializer() },
                                                                                             { typeof(double), new BinaryPrimitives.DoubleSerializer() },
                                                                                             { typeof(float), new BinaryPrimitives.SingleSerializer() },
                                                                                             { typeof(decimal), new BinaryPrimitives.DecimalSerializer() },
                                                                                             { typeof(byte), new BinaryPrimitives.ByteSerializer() },
                                                                                             { typeof(sbyte), new BinaryPrimitives.ByteSerializer() },
                                                                                             { typeof(char), new BinaryPrimitives.CharSerializer() },
                                                                                             { typeof(string), new BinaryPrimitives.StringSerializer() },
                                                                                             { typeof(Guid), new BinaryPrimitives.GuidSerializer() },
                                                                                             { typeof(bool), new BinaryPrimitives.BooleanSerializer() },
                                                                                             { typeof(TimeSpan), new BinaryPrimitives.TimeSpanSerializer() },
                                                                                             { typeof(DateTime), new BinaryPrimitives.DateTimeSerializer() },
                                                                                             { typeof(System.Windows.Media.Color), new BinaryPrimitives.ColorSerializer() },
                                                                                             { typeof(IPAddress), new BinaryPrimitives.IPAddressSerializer() }
                                                                                         };

        /// <summary>
        /// The singleton bit converter
        /// </summary>
        public static readonly IBitConverter SingletonBitConverter = new BigEndianBitConverter();

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public virtual void Serialize<T>(T item, Stream target)
        {
            var serializer = FindSerializer<T>();
            if (null == serializer)
            {
                OnGetSerializerError<T>();
                return;
            }
            serializer.Serialize(item, target);
        }

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public virtual T Deserialize<T>(Stream source)
        {
            var serializer = FindSerializer<T>();
            if (null != serializer) return serializer.Deserialize(source);
            OnGetSerializerError<T>();
            return default(T);
        }

        protected virtual void OnGetSerializerError<T>()
        {
            this.LogError("Something went wrong... Tried to get Serializer for Type: {0}, but was unsuccesful", typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <typeparam name="T"></typeparam>
        public void AddOverride<T>(ISerializer<T> serializer)
        {
            lock (Overrides)
            {
                var type = typeof(T);
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                    type = type.GetGenericTypeDefinition();

                if (!Overrides.ContainsKey(type))
                    Overrides.Add(type, serializer);
            }
        }

        private static ISerializer CreateArraySerializer(Type arrayType)
        {
            if (arrayType.IsUnTypedArray())
                return new BinaryPrimitives.UnTypedArraySerializer(arrayType);
            return new BinaryPrimitives.ArraySerializer(arrayType);
        }

        private static ISerializer CreateSerializer(Type oType)
        {
            if (null == oType.GetConstructor(Type.EmptyTypes))
                return null;
            var instantiationMethod = typeof (BinarySerializerService).GetMethod("TryInstantiate",
                BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(oType);
            return (ISerializer)instantiationMethod.Invoke(null, null);
        }

        private static bool TryGetTypeSerializer(Type type, out ISerializer serializer)
        {
            var result = false;
            serializer = null;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                result = Overrides.TryGetValue(type.GetGenericTypeDefinition(), out serializer);

            return result || Overrides.TryGetValue(type, out serializer);
        }

        public static ISerializer GetSerializer(Type type)
        {
            ISerializer ser;
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            lock (Overrides)
            {
                if (TryGetTypeSerializer(type, out ser)) return ser;
                ser = type.IsArray ? CreateArraySerializer(type) : CreateSerializer(type);
                Overrides.Add(type, ser);
            }
            return ser;
        }

        public ISerializer<T> FindSerializer<T>()
        {
            return GetSerializer<T>();
        }

        public ISerializer FindSerializer(Type type)
        {
            return GetSerializer(type);
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISerializer<T> GetSerializer<T>()
        {
            return (ISerializer<T>)GetSerializer(typeof(T));
        }

        public static ISerializer TryInstantiate<T>() where T : new()
        {
            try
            {
                return new BinaryObjectSerializer<T>();
            }
            catch (Exception)
            {
                //LogException(e, MethodBase.GetCurrentMethod());
                return null;
            }
        }


    }
}
