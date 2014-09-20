using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace iLynx.Common.Serialization
{
    /// <summary>
    /// Serializer
    /// </summary>
    public class Serializer : ISerializerService
    {
        /// <summary>
        /// The namespace GUID used by this serializer
        /// </summary>
        public static readonly Guid SerializerNamespace = new Guid("6CB4F946-4563-4458-938A-56E5DAB8640F");
        private static readonly Dictionary<Type, ISerializer> LookupTable = new Dictionary<Type, ISerializer>
                                                                                    {
                                                                                             { typeof(int), new Primitives.Int32Serializer() },
                                                                                             { typeof(uint), new Primitives.UInt32Serializer() },
                                                                                             { typeof(short), new Primitives.Int16Serializer() },
                                                                                             { typeof(ushort), new Primitives.UInt16Serializer() },
                                                                                             { typeof(long), new Primitives.Int64Serializer() },
                                                                                             { typeof(ulong), new Primitives.UInt64Serializer() },
                                                                                             { typeof(double), new Primitives.DoubleSerializer() },
                                                                                             { typeof(float), new Primitives.SingleSerializer() },
                                                                                             { typeof(decimal), new Primitives.DecimalSerializer() },
                                                                                             { typeof(byte), new Primitives.ByteSerializer() },
                                                                                             { typeof(sbyte), new Primitives.ByteSerializer() },
                                                                                             { typeof(char), new Primitives.CharSerializer() },
                                                                                             { typeof(string), new Primitives.StringSerializer() },
                                                                                             { typeof(Guid), new Primitives.GuidSerializer() },
                                                                                             { typeof(bool), new Primitives.BooleanSerializer() },
                                                                                             { typeof(TimeSpan), new Primitives.TimeSpanSerializer() },
                                                                                             { typeof(DateTime), new Primitives.DateTimeSerializer() },
                                                                                             { typeof(System.Windows.Media.Color), new Primitives.ColorSerializer() },
                                                                                         };

        private static readonly Dictionary<Type, ISerializer> ObjectSerializers = new Dictionary<Type, ISerializer>();

        /// <summary>
        /// Gets an instance of the serializerservice.
        /// </summary>
        /// <returns></returns>
        public static ISerializerService GetInstance()
        {
            return new Serializer();
        }

        /// <summary>
        /// The singleton bit converter
        /// </summary>
        public static readonly IBitConverter SingletonBitConverter = new BigEndianBitConverter();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddSerializer<T>(ISerializer<T> serializer)
        {
            lock (LookupTable)
            {
                var type = typeof(T);
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                    type = type.GetGenericTypeDefinition();

                if (!LookupTable.ContainsKey(type))
                    LookupTable.Add(type, serializer);
            }
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

        ISerializer ISerializerService.GetSerializer(Type type)
        {
            return GetSerializer(type);
        }

        private static bool TryGetTypeSerializer(Type type, out ISerializer serializer)
        {
            var result = false;
            serializer = null;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                result = LookupTable.TryGetValue(type.GetGenericTypeDefinition(), out serializer);

            return result || LookupTable.TryGetValue(type, out serializer);
        }

        ISerializer<T> ISerializerService.GetSerializer<T>()
        {
            return GetSerializer<T>();
        }

        public static ISerializer GetSerializer(Type type)
        {
            ISerializer ser;
            lock (LookupTable)
            {
                if (!TryGetTypeSerializer(type, out ser))
                {
                    ser = type.IsArray ? MakeArraySerializer(type) : MakeSerializer(type);
                    LookupTable.Add(type, ser);
                }
            }
            return ser;
        }

        private static ISerializer MakeArraySerializer(Type arrayType)
        {
            return new Primitives.ArraySerializer(arrayType);
        }

        private static ISerializer MakeSerializer(Type oType)
        {
            var serializeInfo = typeof(Serializer).GetMethod("Serialize", BindingFlags.Static | BindingFlags.Public); // TODO: Make this a not-so-magic-string
            serializeInfo = serializeInfo.MakeGenericMethod(oType);
            var deserializeInfo = typeof(Serializer).GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public); // TODO: Make this a not-so-magic-string
            deserializeInfo = deserializeInfo.MakeGenericMethod(oType);
            return new Primitives.CallbackSerializer((o, stream) => serializeInfo.Invoke(null, new[] { o, stream }),
                                            stream => deserializeInfo.Invoke(null, new object[] { stream }));
        }

        private static ISerializer GetNaiveSerializer(Type type)
        {
            ISerializer serializer;
            if (!ObjectSerializers.TryGetValue(type, out serializer))
            {
                if (!type.IsPrimitive)
                    serializer = TryInstantiate(type);

                if (null == serializer)
                    return TryGetTypeSerializer(type, out serializer) ? serializer : null;

                ObjectSerializers.Add(type, serializer);
            }
            return serializer;

        }

        private static ISerializer GetNaiveSerializer<T>() where T : new()
        {
            var serializer = GetNaiveSerializer(typeof(T));
            return serializer;
        }

        private static readonly MethodInfo InstantiationMethod = typeof(Serializer).GetMethod("TryInstantiate", BindingFlags.Public | BindingFlags.Static);

        private static ISerializer TryInstantiate(Type type)
        {
            var method = InstantiationMethod.MakeGenericMethod(type);
            return (ISerializer)method.Invoke(null, null);
        }

        public static ISerializer TryInstantiate<T>() where T : new()
        {
            try
            {
                return new NaiveSerializer<T>(RuntimeCommon.DefaultLogger);
            }
            catch (Exception)
            {
                //LogException(e, MethodBase.GetCurrentMethod());
                return null;
            }
        }

        T ISerializerService.Deserialize<T>(Stream source)
        {
            return Deserialize<T>(source);
        }

        void ISerializerService.Serialize<T>(T item, Stream target)
        {
            Serialize(item, target);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="target">The target.</param>
        public static void Serialize<T>(T item, Stream target) where T : new()
        {
            var serializer = GetNaiveSerializer<T>();
            if (null == serializer)
            {
                OnGetSerializerError<T>();
                return;
            }
            serializer.Serialize(item, target);
        }

        private static void OnGetSerializerError<T>()
        {
            RuntimeCommon.DefaultLogger.Log(LoggingType.Error, null, string.Format("Something went wrong here... Tried to get Serializer for Type: {0}, but was unsuccesful", typeof(T)));
        }

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream source) where T : new()
        {
            var serializer = GetNaiveSerializer<T>();
            if (null == serializer)
            {
                OnGetSerializerError<T>();
                return default(T);
            }
            return (T)serializer.Deserialize(source);
        }
    }
}
