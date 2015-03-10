using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using iLynx.Common;
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
            {typeof (ushort), new XmlPrimitives.UInt16Serializer()},
            {typeof (short), new XmlPrimitives.Int16Serializer()},
            {typeof (uint), new XmlPrimitives.UInt32Serializer()},
            {typeof (int), new XmlPrimitives.Int32Serializer()},
            {typeof (ulong), new XmlPrimitives.UInt64Serializer()},
            {typeof (long), new XmlPrimitives.Int64Serializer()},
            {typeof (float), new XmlPrimitives.SingleSerializer()},
            {typeof (double), new XmlPrimitives.DoubleSerializer()},
            {typeof (decimal), new XmlPrimitives.DecimalSerializer()},
            {typeof (Color), new XmlPrimitives.ColorSerializer()},
            {typeof (DateTime), new XmlPrimitives.DateTimeSerializer()},
            {typeof (TimeSpan), new XmlPrimitives.TimeSpanSerializer()},
            {typeof(bool), new XmlPrimitives.BooleanSerializer()}
        };

        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
                                                                   {
                                                                       OmitXmlDeclaration = true,
                                                                       ConformanceLevel = ConformanceLevel.Fragment
                                                                   };

        private static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings
                                                                   {
                                                                       IgnoreComments = true,
                                                                       ConformanceLevel = ConformanceLevel.Fragment,
                                                                       CloseInput = false,
                                                                   };

        public void Serialize<T>(T item, Stream target)
        {
            using (var writer = XmlWriter.Create(target, WriterSettings))
            {
                writer.WriteStartElement("Data");
                Serialize(item, writer);
                writer.WriteAttributeString("StreamOffset", target.Position.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
        }

        public T Deserialize<T>(Stream source)
        {
            using (var reader = XmlReader.Create(source, ReaderSettings))
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

        public static void AddOverride<T>(IXmlSerializer<T> serializer)
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

        public class Int16Serializer : XmlSerializerBase<short>
        {
            #region Overrides of XmlSerializerBase<ushort>

            public override void Serialize(short item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(short).ToString(), item.ToString(CultureInfo.InvariantCulture));
            }

            public override short Deserialize(XmlReader reader)
            {
                return short.Parse(reader.ReadElementString(typeof(short).Name), CultureInfo.InvariantCulture);
            }

            #endregion
        }

        public class UInt16Serializer : XmlSerializerBase<ushort>
        {
            #region Overrides of XmlSerializerBase<ushort>

            public override void Serialize(ushort item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(ushort).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override ushort Deserialize(XmlReader reader)
            {
                return ushort.Parse(reader.ReadElementString(typeof(ushort).Name), CultureInfo.InvariantCulture);
            }

            #endregion
        }

        public class Int32Serializer : XmlSerializerBase<int>
        {
            public override void Serialize(int item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(int).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override int Deserialize(XmlReader reader)
            {
                return int.Parse(reader.ReadElementString(typeof(int).Name), CultureInfo.InvariantCulture);
            }
        }

        public class UInt32Serializer : XmlSerializerBase<uint>
        {
            public override void Serialize(uint item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(uint).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override uint Deserialize(XmlReader reader)
            {
                return uint.Parse(reader.ReadElementString(typeof(uint).Name), CultureInfo.InvariantCulture);
            }
        }

        public class Int64Serializer : XmlSerializerBase<long>
        {
            public override void Serialize(long item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(long).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override long Deserialize(XmlReader reader)
            {
                return long.Parse(reader.ReadElementString(typeof(long).Name), CultureInfo.InvariantCulture);
            }
        }

        public class UInt64Serializer : XmlSerializerBase<ulong>
        {
            public override void Serialize(ulong item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(ulong).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override ulong Deserialize(XmlReader reader)
            {
                return ulong.Parse(reader.ReadElementString(typeof(ulong).Name), CultureInfo.InvariantCulture);
            }
        }

        public class DoubleSerializer : XmlSerializerBase<double>
        {
            public override void Serialize(double item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(double).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override double Deserialize(XmlReader reader)
            {
                return double.Parse(reader.ReadElementString(typeof(double).Name), CultureInfo.InvariantCulture);
            }
        }

        public class SingleSerializer : XmlSerializerBase<float>
        {
            public override void Serialize(float item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(float).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override float Deserialize(XmlReader reader)
            {
                return float.Parse(reader.ReadElementString(typeof(float).Name), CultureInfo.InvariantCulture);
            }
        }

        public class DecimalSerializer : XmlSerializerBase<decimal>
        {
            public override void Serialize(decimal item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(decimal).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override decimal Deserialize(XmlReader reader)
            {
                return decimal.Parse(reader.ReadElementString(typeof(decimal).Name), CultureInfo.InvariantCulture);
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

        public class DateTimeSerializer : XmlSerializerBase<DateTime>
        {
            public override void Serialize(DateTime item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(DateTime).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override DateTime Deserialize(XmlReader reader)
            {
                return DateTime.Parse(reader.ReadElementString(typeof(DateTime).Name), CultureInfo.InvariantCulture);
            }
        }

        public class TimeSpanSerializer : XmlSerializerBase<TimeSpan>
        {
            public override void Serialize(TimeSpan item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(TimeSpan).Name, item.ToString());
            }

            public override TimeSpan Deserialize(XmlReader reader)
            {
                return TimeSpan.Parse(reader.ReadElementString(typeof(TimeSpan).Name), CultureInfo.InvariantCulture);
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
                reader.SkipToElement("Array");
                try
                {
                    var countString = reader.GetAttribute("Count");
                    int count;
                    if (!int.TryParse(countString, out count)) return null;
                    var target = Array.CreateInstance(arrayElementType, count);
                    if (reader.IsEmptyElement) return target;
                    reader.ReadStartElement("Array");
                    for (var i = 0; i < count; ++i)
                    {
                        try
                        {
                            reader.SkipToElement("Entry");
                            var isNull = bool.Parse(reader.GetAttribute("IsNull") ?? "true");
                            if (isNull)
                                continue;
                            var elementTypeString = reader.GetAttribute("Type");
                            if (null == elementTypeString) continue;
                            var elementType = Type.GetType(elementTypeString);
                            if (null == elementType) continue;
                            reader.ReadStartElement("Entry");
                            var serializer = XmlSerializerService.GetSerializer(elementType);
                            var result = serializer.Deserialize(reader);
                            target.SetValue(result, i);
                        }
                        finally { reader.ReadEndElement(); }
                    }
                    return target;
                }
                finally
                {
                    if (reader.IsEmptyElement)
                        reader.Skip();
                    else
                        reader.ReadEndElement();
                }
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
                reader.SkipToElement("Array");
                try
                {
                    var countAttrib = reader.GetAttribute("Count");
                    int count;
                    if (!int.TryParse(countAttrib, out count)) return null;
                    var target = Array.CreateInstance(elementType, count);
                    if (reader.IsEmptyElement) return target;
                    reader.ReadStartElement("Array");
                    for (var i = 0; i < count; ++i)
                    {
                        var element = elementSerializer.Deserialize(reader);
                        target.SetValue(element, i);
                    }
                    return target;
                }
                finally
                {
                    if (reader.IsEmptyElement)
                        reader.Skip();
                    else
                        reader.ReadEndElement();
                }
            }
        }

        public class ColorSerializer : XmlSerializerBase<Color>
        {
            #region Overrides of XmlSerializerBase<Color>

            public override void Serialize(Color item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(Color).Name, string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}",
                    item.A,
                    item.R,
                    item.G,
                    item.B));
            }

            public override Color Deserialize(XmlReader reader)
            {
                var str = reader.ReadElementString(typeof(Color).Name);
                var haveAlpha = str.Length - 1 == 8;
                var offset = 1;
                byte a = 0xFF;
                if (haveAlpha)
                {
                    a = byte.Parse(str.Substring(offset, 2), NumberStyles.HexNumber);
                    offset += 2;
                }
                var r = byte.Parse(str.Substring(offset, 2), NumberStyles.HexNumber);
                offset += 2;
                var g = byte.Parse(str.Substring(offset, 2), NumberStyles.HexNumber);
                offset += 2;
                var b = byte.Parse(str.Substring(offset, 2), NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }

            #endregion
        }

        public class BooleanSerializer : XmlSerializerBase<bool>
        {
            public override void Serialize(bool item, XmlWriter writer)
            {
                writer.WriteElementString(typeof(bool).Name, item.ToString(CultureInfo.InvariantCulture));
            }

            public override bool Deserialize(XmlReader reader)
            {
                return bool.Parse(reader.ReadElementString(typeof(bool).Name));
            }
        }
    }
}