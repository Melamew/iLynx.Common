using System;
using System.IO;
using iLynx.Common;
using iLynx.Serialization;

namespace iLynx.Chatter.Infrastructure
{
    public class ChatMessageSerializer : ISerializer<ChatMessage>
    {
        private readonly ISerializer<int> keySerializer = new BinaryPrimitives.Int32Serializer();
        private readonly ISerializer<Guid> idSerializer = new BinaryPrimitives.GuidSerializer();

        public void Serialize(object item, Stream target)
        {
            var message = item as ChatMessage;
            if (null == message) throw new InvalidCastException();
            Serialize(message, target);
        }

        public ChatMessage Deserialize(Stream source)
        {
            var result = new ChatMessage
            {
                Key = keySerializer.Deserialize(source),
                ClientId = idSerializer.Deserialize(source)
            };

            var dataLength = keySerializer.Deserialize(source);
            if (0 == dataLength) return result;
            if (0 > dataLength) return null;
            var data = new byte[dataLength];
            source.Read(data, 0, data.Length);
            result.Data = data;
            return result;
        }

        public void Serialize(ChatMessage item, Stream target)
        {
            Guard.IsNull(() => item);
            keySerializer.Serialize(item.Key, target);
            idSerializer.Serialize(item.ClientId, target);
            if (null == item.Data)
            {
                keySerializer.Serialize(0, target);
                return;
            }
            var data = item.Data;
            keySerializer.Serialize(data.Length, target);
            target.Write(data, 0, data.Length);
        }

        public int GetOutputSize(ChatMessage item)
        {
            Guard.IsNull(() => item);
            var data = item.Data;
            return (keySerializer.GetOutputSize(item.Key) * 2) + idSerializer.GetOutputSize(item.ClientId)
                   + (null == data ? 0 : data.Length);
        }

        object ISerializer.Deserialize(Stream source)
        {
            return Deserialize(source);
        }

        public int GetOutputSize(object item)
        {
            var message = item as ChatMessage;
            if (null == message) throw new InvalidCastException();
            return GetOutputSize(message);
        }
    }
}