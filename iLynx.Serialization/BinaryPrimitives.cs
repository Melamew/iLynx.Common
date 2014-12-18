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

        public class StringSerializer : SerializerBase<string>
        {
            public override void Serialize(string item, Stream target)
            {
                if (null == item) return;
                var bytes = TextEncoding.GetBytes(item);
                var len = bytes.Length;
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(len), 0, sizeof(int));
                target.Write(bytes, 0, bytes.Length);
            }

            public override string Deserialize(Stream source)
            {
                var buffer = new byte[4];
                source.Read(buffer, 0, buffer.Length);
                var len = BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
                buffer = new byte[len];
                source.Read(buffer, 0, buffer.Length);
                return TextEncoding.GetString(buffer);
            }

            public override int GetOutputSize(string item)
            {
                return TextEncoding.GetByteCount(item);
            }
        }

        /// <summary>
        /// GuidSerializer
        /// </summary>
        public class GuidSerializer : SerializerBase<Guid>
        {
            public override Guid Deserialize(Stream source)
            {
                var buf = new byte[16];
                source.Read(buf, 0, 16);
                return new Guid(buf);
            }

            public override void Serialize(Guid item, Stream target)
            {
                var buf = item.ToByteArray();
                target.Write(buf, 0, buf.Length);
            }

            public override int GetOutputSize(Guid item)
            {
                return 16;
            }
        }

        /// <summary>
        /// CallbackSerializer
        /// </summary>
        public class CallbackSerializer<T> : SerializerBase<T>
        {
            private readonly Action<T, Stream> writeCallback;
            private readonly Func<Stream, T> readCallback;
            private readonly Func<T, int> getSizeCallback;

            /// <summary>
            /// Initializes a new instance of the <see cref="BinaryPrimitives.CallbackSerializer{T}" /> class.
            /// </summary>
            /// <param name="writeCallback">The write callback.</param>
            /// <param name="readCallback">The read callback.</param>
            /// <param name="getSizeCallback"></param>
            public CallbackSerializer(Action<T, Stream> writeCallback, Func<Stream, T> readCallback, Func<T, int> getSizeCallback)
            {
                this.writeCallback = Guard.IsNull(() => writeCallback);
                this.readCallback = Guard.IsNull(() => readCallback);
                this.getSizeCallback = Guard.IsNull(() => getSizeCallback);
            }

            public override int GetOutputSize(T item)
            {
                return getSizeCallback(item);
            }

            public override T Deserialize(Stream source)
            {
                return readCallback(source);
            }

            public override void Serialize(T item, Stream target)
            {
                writeCallback(item, target);
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
                    var type = element.GetType();
                    var name = type.AssemblyQualifiedName;
                    if (null == name)
                    {
                        target.WriteByte(0x00);
                        continue;
                    }
                    target.WriteByte(0x01);
                    Trace.WriteLine(string.Format("Writing element at {0}", target.Position));
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
        public class ArraySerializer : SerializerBase<Array>
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

            public override void Serialize(Array value, Stream target)
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

            public override Array Deserialize(Stream source)
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

            public override int GetOutputSize(Array item)
            {
                var itemElementType = item.GetType().GetElementType();
                if (itemElementType != elementType) throw new InvalidCastException();
                return item.Cast<object>().Sum(element => itemSerializer.GetOutputSize(element));
            }
        }

        /// <summary>
        /// BooleanSerializer
        /// </summary>
        public class BooleanSerializer : SerializerBase<bool>
        {
            public override void Serialize(bool item, Stream target)
            {
                target.Write(new[] { (byte)(item ? 0x01 : 0x00) }, 0, 1);
            }

            public override bool Deserialize(Stream source)
            {
                var buf = new byte[1];
                source.Read(buf, 0, 1);
                return buf[0] == 1;
            }

            public override int GetOutputSize(bool item)
            {
                return 1;
            }
        }

        public class Int16Serializer : SerializerBase<short>
        {
            public override short Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(short)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToInt16(buffer);
            }

            public override void Serialize(short item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(short));
            }

            public override int GetOutputSize(short item)
            {
                return sizeof(short);
            }
        }

        public class UInt16Serializer : SerializerBase<ushort>
        {
            public override ushort Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(short)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToUInt16(buffer);
            }

            public override void Serialize(ushort item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(ushort));
            }

            public override int GetOutputSize(ushort item)
            {
                return sizeof(ushort);
            }
        }

        public class Int32Serializer : SerializerBase<int>
        {
            public override int Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(int)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToInt32(buffer);
            }

            public override void Serialize(int item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(int));
            }

            public override int GetOutputSize(int item)
            {
                return sizeof(int);
            }
        }

        public class UInt32Serializer : SerializerBase<uint>
        {
            public override uint Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(uint)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToUInt32(buffer);
            }

            public override void Serialize(uint item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(uint));
            }

            public override int GetOutputSize(uint item)
            {
                return sizeof(uint);
            }
        }

        public class Int64Serializer : SerializerBase<long>
        {
            public override long Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(long)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToInt64(buffer);
            }

            public override void Serialize(long item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(long));
            }

            public override int GetOutputSize(long item)
            {
                return sizeof(long);
            }
        }

        public class UInt64Serializer : SerializerBase<ulong>
        {
            public override ulong Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(ulong)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToUInt64(buffer);
            }

            public override void Serialize(ulong item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(ulong));
            }

            public override int GetOutputSize(ulong item)
            {
                return sizeof(ulong);
            }
        }

        public class DoubleSerializer : SerializerBase<double>
        {
            public override double Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(double)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToDouble(buffer);
            }

            public override void Serialize(double item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(double));
            }

            public override int GetOutputSize(double item)
            {
                return sizeof(double);
            }
        }

        public class SingleSerializer : SerializerBase<float>
        {
            public override float Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(float)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToSingle(buffer);
            }

            public override void Serialize(float item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(float));
            }
        
            public override int GetOutputSize(float item)
            {
                return sizeof(float);
            }
        }

        public class DecimalSerializer : SerializerBase<decimal>
        {
            public override decimal Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(decimal)];
                source.Read(buffer, 0, buffer.Length);
                return BinarySerializerService.SingletonBitConverter.ToDecimal(buffer);
            }

            public override void Serialize(decimal item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(decimal));
            }
            
            public override int GetOutputSize(decimal item)
            {
                return sizeof(decimal);
            }
        }

        public class ByteSerializer : SerializerBase<byte>
        {
            public override byte Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(byte)];
                source.Read(buffer, 0, buffer.Length);
                return buffer[0];
            }

            public override void Serialize(byte item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item), 0, sizeof(byte));
            }

            public override int GetOutputSize(byte item)
            {
                return 1;
            }
        }

        /// <summary>
        /// CharSerializer
        /// </summary>
        public class CharSerializer : SerializerBase<char>
        {
            public override void Serialize(char item, Stream target)
            {
                var buffer = new byte[sizeof(char)];
                Buffer.BlockCopy(new[] { item }, 0, buffer, 0, sizeof(char));
                target.Write(buffer, 0, sizeof(char));
            }

            public override char Deserialize(Stream source)
            {
                var buffer = new byte[sizeof(char)];
                source.Read(buffer, 0, sizeof(char));
                var result = new char[1];
                Buffer.BlockCopy(buffer, 0, result, 0, sizeof(char));
                return result[0];
            }

            public override int GetOutputSize(char item)
            {
                return 1;
            }
        }

        public class TimeSpanSerializer : SerializerBase<TimeSpan>
        {
            public override void Serialize(TimeSpan item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item.Ticks), 0, sizeof(long));
            }

            public override TimeSpan Deserialize(Stream source)
            {
                var result = new byte[sizeof(long)];
                source.Read(result, 0, result.Length);
                return TimeSpan.FromTicks(BinarySerializerService.SingletonBitConverter.ToInt64(result));
            }

            public override int GetOutputSize(TimeSpan item)
            {
                return sizeof(long);
            }
        }

        public class DateTimeSerializer : SerializerBase<DateTime>
        {
            public override DateTime Deserialize(Stream source)
            {
                var result = new byte[sizeof(long)];
                source.Read(result, 0, result.Length);
                return new DateTime(BinarySerializerService.SingletonBitConverter.ToInt64(result));
            }

            public override void Serialize(DateTime item, Stream target)
            {
                target.Write(BinarySerializerService.SingletonBitConverter.GetBytes(item.Ticks), 0, sizeof(long));
            }

            public override int GetOutputSize(DateTime item)
            {
                return sizeof(long);
            }
        }

        public class ColorSerializer : SerializerBase<Color>
        {
            public override Color Deserialize(Stream source)
            {
                var bytes = new byte[4];
                source.Read(bytes, 0, bytes.Length);
                return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
            }

            public override void Serialize(Color item, Stream target)
            {
                target.Write(new[] { item.A, item.R, item.G, item.B }, 0, 4);
            }

            public override int GetOutputSize(Color color)
            {
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