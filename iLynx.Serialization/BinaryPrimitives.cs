using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Media;
using iLynx.Common;

namespace iLynx.Serialization
{
    public class BinaryPrimitives
    {
        public static Encoding TextEncoding = Encoding.Unicode;

        public class StringSerializer : ISerializer<string>
        {
            public void Serialize(object item, Stream target)
            {
                var str = item as string;
                if (null == str) return;
                var bytes = TextEncoding.GetBytes(str);
                var len = bytes.Length;
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(len), 0, sizeof(int));
                target.Write(bytes, 0, bytes.Length);
            }

            public string Deserialize(Stream source)
            {
                var buffer = new byte[4];
                source.Read(buffer, 0, buffer.Length);
                var len = BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
                buffer = new byte[len];
                source.Read(buffer, 0, buffer.Length);
                return TextEncoding.GetString(buffer);
            }

            void ISerializer<string>.Serialize(string item, Stream target)
            {
                Serialize(item, target);
            }

            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(string item)
            {
                return TextEncoding.GetByteCount(item);
            }

            public int GetOutputSize(object item)
            {
                var str = item as string;
                if (null == str) throw new InvalidCastException();
                return TextEncoding.GetByteCount(str);
            }
        }

        /// <summary>
        /// GuidSerializer
        /// </summary>
        public class GuidSerializer : ISerializer<Guid>
        {
            /// <summary>
            /// Serializes the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object item, Stream target)
            {
                Serialize((Guid)item, target);
            }

            public Guid Deserialize(Stream source)
            {
                var buf = new byte[16];
                source.Read(buf, 0, 16);
                return new Guid(buf);
            }

            public void Serialize(Guid item, Stream target)
            {
                var buf = item.ToByteArray();
                target.Write(buf, 0, buf.Length);
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(Guid item)
            {
                return 16;
            }

            public int GetOutputSize(object item)
            {
                if (!(item is Guid)) throw new InvalidCastException();
                return 16;
            }
        }

        /// <summary>
        /// CallbackSerializer
        /// </summary>
        public class CallbackSerializer : ISerializer<object>
        {
            private readonly Action<object, Stream> writeCallback;
            private readonly Func<Stream, object> readCallback;
            private readonly Func<object, int> getSizeCallback;

            /// <summary>
            /// Initializes a new instance of the <see cref="BinaryPrimitives.CallbackSerializer" /> class.
            /// </summary>
            /// <param name="writeCallback">The write callback.</param>
            /// <param name="readCallback">The read callback.</param>
            /// <param name="getSizeCallback"></param>
            public CallbackSerializer(Action<object, Stream> writeCallback, Func<Stream, object> readCallback, Func<object, int> getSizeCallback = null)
            {
                this.writeCallback = Guard.IsNull(() => writeCallback);
                this.readCallback = Guard.IsNull(() => readCallback);
                this.getSizeCallback = getSizeCallback;
            }

            /// <summary>
            /// Serializes the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object item, Stream target)
            {
                writeCallback(item, target);
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            public object Deserialize(Stream source)
            {
                return readCallback(source);
            }

            public int GetOutputSize(object item)
            {
                if (null == getSizeCallback)
                    throw new NotSupportedException();
                return getSizeCallback(item);
            }
        }

        public class UnTypedArraySerializer : SerializerBase<Array>
        {
            private readonly Type arrayType;

            public UnTypedArraySerializer(Type arrayType)
            {
                this.arrayType = Guard.IsNull(() => arrayType);
            }

            public override int GetOutputSize(Array item)
            {
                return -1;
            }

            public override Array Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(int)];
                source.Read(buffer, 0, buffer.Length);
                var elementCount = BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
                if (0 > elementCount) throw new InvalidDataException();
                var target = Array.CreateInstance(arrayType.GetElementType(), elementCount);
                for (var i = 0; i < elementCount; ++i)
                {
                    var isNotNull = source.ReadByte();
                    if (isNotNull == 0) continue;
                    Trace.WriteLine(string.Format("Reading Element at {0}", source.Position));
                    buffer = new byte[sizeof(int)];
                    source.Read(buffer, 0, buffer.Length); // Length of the type string
                    var textLength = BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
                    if (0 > textLength) throw new InvalidDataException();
                    buffer = new byte[textLength];
                    source.Read(buffer, 0, buffer.Length);
                    var typeName = TextEncoding.GetString(buffer);
                    var elementType = Type.GetType(typeName);
                    if (null == elementType)
                    {
                        continue;
                    }
                    var serializer = BinarySerializerService.GetSerializer(elementType);
                    var item = serializer.Deserialize(source);
                    target.SetValue(item, i);
                }
                return target;
            }

            public override void Serialize(Array item, Stream target)
            {
                var buffer = BinarySerializerService.SingletonBitConverter.GetBytes(item.Length);
                target.Write(buffer, 0, buffer.Length);
                foreach (var element in item)
                {
                    if (null == element)
                    {
                        target.WriteByte(0x00);
                        continue;
                    }
                    
                    target.WriteByte(0x01);
                    Trace.WriteLine(string.Format("Writing element at {0}", target.Position));
                    var type = element.GetType();
                    var name = type.AssemblyQualifiedName;
                    var textBuffer = TextEncoding.GetBytes(name);
                    buffer = BinarySerializerService.SingletonBitConverter.GetBytes(textBuffer.Length);
                    target.Write(buffer, 0, buffer.Length);
                    target.Write(textBuffer, 0, textBuffer.Length);
                    var serializer = BinarySerializerService.GetSerializer(type);
                    serializer.Serialize(element, target);
                }
            }
        }

        /// <summary>
        /// ArraySerializer
        /// // TODO: Maybe this could handle polymorphic arrays too?
        /// </summary>
        public class ArraySerializer : ISerializer<Array>
        {
            private readonly Type elementType;
            private readonly ISerializer itemSerializer;

            /// <summary>
            /// Initializes a new instance of the <see cref="BinaryPrimitives.ArraySerializer" /> class.
            /// </summary>
            /// <param name="arrayType">Type of the array.</param>
            /// <exception cref="WhatTheFuckException"></exception>
            public ArraySerializer(Type arrayType)
            {
                if (!arrayType.IsArray) throw new WhatTheFuckException();
                elementType = arrayType.GetElementType();
                itemSerializer = BinarySerializerService.GetSerializer(elementType);
                if (null == itemSerializer) throw new WhatTheFuckException();
            }

            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            /// <exception cref="WhatTheFuckException"></exception>
            public void Serialize(object value, Stream target)
            {
                Serialize((Array)value, target);
            }

            public void Serialize(Array value, Stream target)
            {
                using (var memStream = new MemoryStream())
                {
                    var cnt = value.Length;
                    for (var i = 0; i < cnt; ++i)
                        itemSerializer.Serialize(value.GetValue(i), memStream);
                    var buffer = BinarySerializerService.SingletonBitConverter.GetBytes(cnt);
                    target.Write(buffer, 0, buffer.Length);
                    memStream.WriteTo(target);
                }
            }

            public Array Deserialize(Stream source)
            {
                var buffer = new byte[4];
                var count = source.Read(buffer, 0, buffer.Length);
                if (4 != count) throw new WhatTheFuckException();
                var elements = BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
                var array = Array.CreateInstance(elementType, elements);
                for (var i = 0; i < elements; ++i)
                    array.SetValue(itemSerializer.Deserialize(source), i);
                return array;
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            /// <exception cref="WhatTheFuckException"></exception>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(Array item)
            {
                var itemElementType = item.GetType().GetElementType();
                if (itemElementType != elementType) throw new InvalidCastException();
                return item.Cast<object>().Sum(element => itemSerializer.GetOutputSize(element));
            }

            public int GetOutputSize(object item)
            {
                var arr = item as Array;
                if (null == arr) throw new InvalidCastException();
                return GetOutputSize(arr);
            }
        }

        /// <summary>
        /// BooleanSerializer
        /// </summary>
        public class BooleanSerializer : ISerializer<bool>
        {
            /// <summary>
            /// Serializes the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object item, Stream target)
            {
                Serialize((bool)item, target);
            }

            public void Serialize(bool item, Stream target)
            {
                target.Write(new[] { (byte)(item ? 0x01 : 0x00) }, 0, 1);
            }

            public bool Deserialize(Stream source)
            {
                var buf = new byte[1];
                source.Read(buf, 0, 1);
                return buf[0] == 1;
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(bool item)
            {
                return 1;
            }

            public int GetOutputSize(object item)
            {
                if (!(item is bool)) throw new InvalidCastException();
                return 1;
            }
        }

        public class Int16Serializer : ISerializer<short>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((short) value, target);
            }

            public short Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(short)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToInt16(buffer);
            }

            public void Serialize(short item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(short));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(short item)
            {
                return sizeof (short);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is short)) throw new InvalidCastException();
                return sizeof (short);
            }
        }

        public class UInt16Serializer : ISerializer<ushort>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((ushort)value, target);
            }

            public ushort Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(short)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToUInt16(buffer);
            }

            public void Serialize(ushort item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(ushort));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(ushort item)
            {
                return sizeof(ushort);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is ushort)) throw new InvalidCastException();
                return sizeof(ushort);
            }
        }

        public class Int32Serializer : ISerializer<int>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((int)value, target);
            }

            public int Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(int)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
            }

            public void Serialize(int item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(int));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(int item)
            {
                return sizeof(int);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is int)) throw new InvalidCastException();
                return sizeof(int);
            }
        }

        public class UInt32Serializer : ISerializer<uint>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((uint)value, target);
            }

            public uint Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(uint)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToUInt32(buffer);
            }

            public void Serialize(uint item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(uint));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(uint item)
            {
                return sizeof(uint);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is uint)) throw new InvalidCastException();
                return sizeof(uint);
            }
        }

        public class Int64Serializer : ISerializer<long>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((long)value, target);
            }

            public long Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(long)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToInt64(buffer);
            }

            public void Serialize(long item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(long));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(long item)
            {
                return sizeof(long);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is long)) throw new InvalidCastException();
                return sizeof(long);
            }
        }

        public class UInt64Serializer : ISerializer<ulong>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((ulong)value, target);
            }

            public ulong Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(ulong)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToUInt64(buffer);
            }

            public void Serialize(ulong item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(ulong));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(ulong item)
            {
                return sizeof(ulong);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is ulong)) throw new InvalidCastException();
                return sizeof(ulong);
            }
        }

        public class DoubleSerializer : ISerializer<double>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((double)value, target);
            }

            public double Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(double)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToDouble(buffer);
            }

            public void Serialize(double item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(double));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(double item)
            {
                return sizeof(double);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is double)) throw new InvalidCastException();
                return sizeof(double);
            }
        }

        public class SingleSerializer : ISerializer<float>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((float)value, target);
            }

            public float Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(float)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToSingle(buffer);
            }

            public void Serialize(float item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(float));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(float item)
            {
                return sizeof(float);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is float)) throw new InvalidCastException();
                return sizeof(float);
            }
        }

        public class DecimalSerializer : ISerializer<decimal>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((decimal)value, target);
            }

            public decimal Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(decimal)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToDecimal(buffer);
            }

            public void Serialize(decimal item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(decimal));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(decimal item)
            {
                return sizeof(decimal);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is decimal)) throw new InvalidCastException();
                return sizeof(decimal);
            }
        }

        public class ByteSerializer : ISerializer<byte>
        {
            /// <summary>
            /// Writes to.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object value, Stream target)
            {
                Serialize((byte)value, target);
            }

            public byte Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(byte)];
                source.Read(buffer, 0, buffer.Length);
                return buffer[0];
            }

            public void Serialize(byte item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(byte));
            }

            /// <summary>
            /// Reads from.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(byte item)
            {
                return 1;
            }

            public int GetOutputSize(object item)
            {
                if (!(item is byte)) throw new InvalidCastException();
                return 1;
            }
        }

        /// <summary>
        /// CharSerializer
        /// </summary>
        public class CharSerializer : ISerializer<char>
        {
            /// <summary>
            /// Serializes the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object item, Stream target)
            {
                Serialize((char)item, target);
            }

            public void Serialize(char item, Stream target)
            {
                var buffer = new byte[sizeof(char)];
                Buffer.BlockCopy(new[] { item }, 0, buffer, 0, sizeof(char));
                target.Write(buffer, 0, sizeof(char));
            }

            public char Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(char)];
                source.Read(buffer, 0, sizeof(char));
                var result = new char[1];
                Buffer.BlockCopy(buffer, 0, result, 0, sizeof(char));
                return result[0];
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(char item)
            {
                return 1;
            }

            public int GetOutputSize(object item)
            {
                if (!(item is char)) throw new InvalidCastException();
                return 1;
            }
        }

        public class TimeSpanSerializer : ISerializer<TimeSpan>
        {
            /// <summary>
            /// Serializes the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object item,
                                Stream target)
            {
                Serialize((TimeSpan)item, target);
            }

            public void Serialize(TimeSpan item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item.Ticks), 0, sizeof(long));
            }

            public TimeSpan Deserialize(Stream source)
            {
                var result = new byte[sizeof(long)];
                source.Read(result, 0, result.Length);
                return TimeSpan.FromTicks(BinarySerializerService.SingletonBitConverter.ToInt64(result));
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(TimeSpan item)
            {
                return sizeof(long);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is TimeSpan)) throw new InvalidCastException();
                return sizeof(long);
            }
        }

        public class DateTimeSerializer : ISerializer<DateTime>
        {
            /// <summary>
            /// Serializes the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="target">The target.</param>
            public void Serialize(object item,
                                Stream target)
            {
                Serialize((DateTime) item, target);
            }

            public DateTime Deserialize(Stream source)
            {
                var result = new byte[sizeof(long)];
                source.Read(result, 0, result.Length);
                return new DateTime(BinarySerializerService.SingletonBitConverter.ToInt64(result));
            }

            public void Serialize(DateTime item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item.Ticks), 0, sizeof(long));
            }

            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns></returns>
            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(DateTime item)
            {
                return sizeof (long);
            }

            public int GetOutputSize(object item)
            {
                if (!(item is DateTime)) throw new InvalidCastException();
                return sizeof (long);
            }
        }

        public class ColorSerializer : ISerializer<Color>
        {
            public void Serialize(object item, Stream target)
            {
                Serialize((Color)item, target);
            }

            public Color Deserialize(Stream source)
            {
                var bytes = new byte[4];
                source.Read(bytes, 0, bytes.Length);
                return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
            }

            public void Serialize(Color item, Stream target)
            {
                target.Write(new[] { item.A, item.R, item.G, item.B }, 0, 4);
            }

            object ISerializer.Deserialize(Stream source)
            {
                return Deserialize(source);
            }

            public int GetOutputSize(Color color)
            {
                return 4;
            }

            public int GetOutputSize(object item)
            {
                if (!(item is Color)) throw new InvalidCastException();
                return 4;
            }
        }

        public class IPAddressSerializer : SerializerBase<IPAddress>
        {
            public override int GetOutputSize(IPAddress item)
            {
                return item.GetAddressBytes().Length;
            }

            public override IPAddress Deserialize(Stream source)
            {
                var lengthBytes = new byte[sizeof(int)];
                source.Read(lengthBytes, 0, lengthBytes.Length);
                var length = BinarySerializerService.SingletonBitConverter.ToInt32(lengthBytes);
                var bytes = new byte[length];
                source.Read(bytes, 0, bytes.Length);
                return new IPAddress(bytes);
            }

            public override void Serialize(IPAddress item, Stream target)
            {
                var addressBytes = item.GetAddressBytes();
                var lengthBytes = BinarySerializerService.SingletonBitConverter.GetBytes(addressBytes.Length);
                target.Write(lengthBytes, 0, lengthBytes.Length);
                target.Write(addressBytes, 0, addressBytes.Length);
            }
        }
    }
}