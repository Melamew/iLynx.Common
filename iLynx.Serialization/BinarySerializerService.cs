using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace iLynx.Serialization
{
    /// <summary>
    /// BinarySerializerService
    /// </summary>
    public class BinarySerializerService : SerializerServiceBase
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
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <typeparam name="T"></typeparam>
        public override void AddOverride<T>(ISerializer<T> serializer)
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

        public override ISerializer<T> FindSerializer<T>()
        {
            return GetSerializer<T>();
        }

        public override ISerializer FindSerializer(Type type)
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
