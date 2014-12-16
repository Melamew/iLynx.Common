using System;
using System.IO;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;

namespace iLynx.Networking
{
    public class StreamConnectionStub<TMessage, TMessageKey> : IConnectionStub<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly ISerializer<TMessage> serializer;
        private readonly Stream stream;

        public StreamConnectionStub(ISerializer<TMessage> serializer, Stream stream)
        {
            this.serializer = Guard.IsNull(() => serializer);
            this.stream = Guard.IsNull(() => stream);
        }

        public virtual int Write(TMessage message)
        {
            Guard.IsNull(() => message);
            try
            {
                var size = serializer.GetOutputSize(message);
                serializer.Serialize(message, stream);
                stream.Flush();
                return size;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public virtual TMessage ReadNext(out int size)
        {
            try
            {
                var result = serializer.Deserialize(stream);
                size = serializer.GetOutputSize(result);
                return result;
            }
            catch (Exception)
            {
                size = -1;
                return default(TMessage);
            }
        }

        public virtual bool IsOpen { get { return CanRead || CanWrite; } }
        public virtual bool CanRead { get { return stream.CanRead; } }
        public virtual bool CanWrite { get { return stream.CanWrite; } }

        ~StreamConnectionStub()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            try
            {
                stream.Close();
                stream.Dispose();
            }
            catch (ObjectDisposedException) { }
        }
    }
}
