using System;
using System.IO;
using iLynx.Common;
using iLynx.Serialization;

namespace iLynx.Networking.Serialization
{
    public class SimpleMessageSerializer<TMessageKey> : ISerializer<SimpleMessage<TMessageKey>>
    {
        private readonly ISerializer<TMessageKey> keySerializer;

        public SimpleMessageSerializer(ISerializer<TMessageKey> keySerializer)
        {
            this.keySerializer = Guard.IsNull(() => keySerializer);
        }

        public SimpleMessageSerializer()
        {
            keySerializer = Serializer.GetSerializer<TMessageKey>();
        } 

        public void Serialize(object item, Stream target)
        {
            var msg = item as SimpleMessage<TMessageKey>;
            if (null == msg) throw new InvalidCastException();
            Serialize(msg, target);
        }

        public SimpleMessage<TMessageKey> Deserialize(Stream source)
        {
            var key = keySerializer.Deserialize(source);
            var readBuffer = new byte[sizeof (int)];
            source.Read(readBuffer, 0, readBuffer.Length);
            var dataLength = Serializer.SingletonBitConverter.ToInt32(readBuffer);
            if (0 > dataLength) return new SimpleMessage<TMessageKey>(key);
            if (0 >= dataLength) return new SimpleMessage<TMessageKey>(key, readBuffer);
            readBuffer = new byte[dataLength];
            source.Read(readBuffer, 0, readBuffer.Length);
            return new SimpleMessage<TMessageKey>(key, readBuffer);
        }

        public void Serialize(SimpleMessage<TMessageKey> item, Stream target)
        {
            Guard.IsNull(() => item);
            keySerializer.Serialize(item.Key, target);
            var data = item.Data ?? new byte[0];
            var writeBuffer = Serializer.SingletonBitConverter.GetBytes(data.Length);
            target.Write(writeBuffer, 0, writeBuffer.Length);
            target.Write(data, 0, data.Length);
        }

        public int GetOutputSize(SimpleMessage<TMessageKey> item)
        {
            return keySerializer.GetOutputSize(item.Key) + sizeof (int) + (null == item.Data ? 0 : item.Data.Length);
        }

        object ISerializer.Deserialize(Stream source)
        {
            return Deserialize(source);
        }

        public int GetOutputSize(object item)
        {
            var msg = item as SimpleMessage<TMessageKey>;
            if (null == msg) throw new InvalidCastException();
            return GetOutputSize(msg);
        }
    }
}