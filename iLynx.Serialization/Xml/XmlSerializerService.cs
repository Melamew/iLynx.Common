using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using JetBrains.Annotations;

namespace iLynx.Serialization.Xml
{
    public class XmlSerializerService : ISerializerService
    {
        private static readonly Dictionary<Type, IXmlSerializer> Overrides = new Dictionary<Type, IXmlSerializer>
        {
            {typeof (string), new XmlPrimitives.StringSerializer()},
            {typeof (char), new XmlPrimitives.CharSerializer()},
            {typeof (Guid), new XmlPrimitives.GuidSerializer()},
            {typeof (byte), new XmlPrimitives.ByteSerializer()},
            {typeof (int), new XmlPrimitives.Int32Serializer()}
        };

        public void Serialize<T>(T item, Stream target)
        {
            using (var writer = XmlWriter.Create(target, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                Serialize(item, writer);
            }
        }

        public T Deserialize<T>(Stream source)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings
            {
                IgnoreComments = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            }))
            {
                return Deserialize<T>(reader);
            }
        }

        public T Deserialize<T>(XmlReader reader)
        {
            var serializer = GetSerializer<T>();
            var result = serializer.Deserialize(reader);
            return result;
        }

        public void Serialize<T>(T item, XmlWriter writer)
        {
            var serializer = GetSerializer<T>();
            serializer.Serialize(item, writer);
        }

        private static bool TryGetTypeSerializer(Type type, out IXmlSerializer serializer)
        {
            var result = false;
            serializer = null;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                result = Overrides.TryGetValue(type.GetGenericTypeDefinition(), out serializer);

            return result || Overrides.TryGetValue(type, out serializer);
        }


        private static IXmlSerializer CreateArraySerializer([NotNull] Type arrayType)
        {
            if (arrayType == null) throw new ArgumentNullException("arrayType");
            if (arrayType.IsUnTypedArray())
                return new XmlPrimitives.UnTypedArraySerializer(arrayType);
            return new XmlPrimitives.ArraySerializer(arrayType);
        }

        private static IXmlSerializer CreateSerializer(Type oType)
        {
            if (null == oType.GetConstructor(Type.EmptyTypes))
                return null;
            var instantiationMethod = typeof(XmlSerializerService).GetMethod("TryInstantiate",
                BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(oType);
            return (IXmlSerializer)instantiationMethod.Invoke(null, null);
        }

        public static IXmlSerializer GetSerializer(Type type)
        {
            IXmlSerializer ser;
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

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IXmlSerializer<T> GetSerializer<T>()
        {
            return (IXmlSerializer<T>)GetSerializer(typeof(T));
        }

        public void AddOverride<T>(IXmlSerializer<T> serializer)
        {
            var xmlSerializer = serializer;
            lock (Overrides)
            {
                var type = typeof(T);
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                    type = type.GetGenericTypeDefinition();

                if (!Overrides.ContainsKey(type))
                    Overrides.Add(type, xmlSerializer);
            }
        }

        public IXmlSerializer FindSerializer(Type type)
        {
            return GetSerializer(type);
        }

        public IXmlSerializer<T> FindSerializer<T>()
        {
            return GetSerializer<T>();
        }

        public static IXmlSerializer TryInstantiate<T>() where T : new()
        {
            try
            {
                return new XmlObjectSerializer<T>();
            }
            catch (Exception)
            {
                //LogException(e, MethodBase.GetCurrentMethod());
                return null;
            }
        }
    }

    public class XmlPrimitives
    {
        public class StringSerializer : XmlSerializerBase<string>
        {
            public override void Serialize(string item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(string).Name, item);
            }

            public override string Deserialize(XmlReader reader)
            {
                return reader.ReadElementString(typeof(string).Name);
            }
        }

        public class ByteSerializer : XmlSerializerBase<byte>
        {
            public override void Serialize(byte item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(byte).Name, item.ToString("X"));
            }

            public override byte Deserialize(XmlReader reader)
            {
                return byte.Parse(reader.ReadElementString(typeof(byte).Name));
            }
        }

        public class Int32Serializer : XmlSerializerBase<int>
        {
            public override void Serialize(int item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(int).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override int Deserialize(XmlReader reader)
            {
                return int.Parse(reader.ReadElementString(typeof(int).Name));
            }
        }

        public class CharSerializer : XmlSerializerBase<char>
        {
            public override void Serialize(char item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(char).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override char Deserialize(XmlReader reader)
            {
                return char.Parse(reader.ReadElementString(typeof(char).Name));
            }
        }

        public class GuidSerializer : XmlSerializerBase<Guid>
        {
            public override void Serialize(Guid item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(Guid).Name, item.ToString());
            }

            public override Guid Deserialize(XmlReader reader)
            {
                return new Guid(reader.ReadElementString(typeof(Guid).Name));
            }
        }

        public class UnTypedArraySerializer : XmlSerializerBase<Array>
        {
            private readonly Type arrayElementType;
            public UnTypedArraySerializer(Type arrayType)
            {
                arrayElementType = arrayType.GetElementType();
            }

            public override void Serialize(Array item, XmlWriter writer)
            {
                writer.WriteStartElement("Array");
                try
                {
                    writer.WriteAttributeString("Count", item.Length.ToString(CultureInfo.InvariantCulture));
                    foreach (var element in item)
                    {
                        writer.WriteStartElement("Entry");
                        try
                        {
                            if (null == element)
                            {
                                writer.WriteAttributeString("IsNull", true.ToString());
                                continue;
                            }
                            writer.WriteAttributeString("IsNull", false.ToString());
                            var elementType = element.GetType();
                            writer.WriteAttributeString("Type", elementType.AssemblyQualifiedName ?? "???");
                            var serializer = XmlSerializerService.GetSerializer(elementType);
                            serializer.Serialize(element, writer);
                        }
                        finally
                        {
                            writer.WriteEndElement();
                        }
                    }
                }
                finally { writer.WriteEndElement(); }
            }

            public override Array Deserialize(XmlReader reader)
            {
                reader.Read();
                try
                {
                    var countString = reader.GetAttribute("Count");
                    int count;
                    if (!int.TryParse(countString, out count)) return null;
                    var target = Array.CreateInstance(arrayElementType, count);
                    for (var i = 0; i < count; ++i)
                    {
                        reader.ReadStartElement("Entry");
                        try
                        {
                            var isNull = bool.Parse(reader.GetAttribute("IsNull") ?? "true");
                            if (isNull)
                                continue;
                            var elementTypeString = reader.GetAttribute("Type");
                            if (null == elementTypeString) continue;
                            var elementType = Type.GetType(elementTypeString);
                            if (null == elementType) continue;
                            var serializer = XmlSerializerService.GetSerializer(elementType);
                            var result = serializer.Deserialize(reader);
                            target.SetValue(result, i);
                        }
                        finally { reader.ReadEndElement(); }
                    }
                    return target;
                }
                finally { reader.ReadEndElement(); }
            }
        }

        public class ArraySerializer : XmlSerializerBase<Array>
        {
            private readonly IXmlSerializer elementSerializer;
            private readonly Type elementType;

            public ArraySerializer(Type arrayType)
            {
                elementType = arrayType.GetElementType();
                elementSerializer = XmlSerializerService.GetSerializer(elementType);
            }

            public override void Serialize(Array item, XmlWriter writer)
            {
                writer.WriteStartElement("Array");
                try
                {
                    writer.WriteAttributeString("Count", item.Length.ToString(CultureInfo.InvariantCulture));
                    foreach (var element in item)
                        elementSerializer.Serialize(element, writer);
                }
                finally { writer.WriteEndElement(); }
            }

            public override Array Deserialize(XmlReader reader)
            {
                reader.Read();
                try
                {
                    var countAttrib = reader.GetAttribute("Count");
                    int count;
                    if (!int.TryParse(countAttrib, out count)) return null;
                    var target = Array.CreateInstance(elementType, count);
                    if (!reader.Read()) return null;
                    for (var i = 0; i < count; ++i)
                    {
                        var element = elementSerializer.Deserialize(reader);
                        target.SetValue(element, i);
                    }
                    return target;
                }
                finally { reader.ReadEndElement(); }
            }
        }
    }
}