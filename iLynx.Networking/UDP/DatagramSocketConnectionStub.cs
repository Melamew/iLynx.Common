using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;

namespace iLynx.Networking.UDP
{
    public class DatagramSocketConnectionStub<T, TMessageKey> : IConnectionStub<T, TMessageKey> where T : IKeyedDatagramMessage<TMessageKey>
    {
        private readonly ISerializer<T> serializer;
        private readonly IBitConverter bitConverter;
        private readonly Socket socket;
        private readonly object syncRoot = new object();

        public DatagramSocketConnectionStub(ISerializer<T> serializer, IBitConverter bitConverter, Socket socket)
        {
            this.serializer = Guard.IsNull(() => serializer);
            this.bitConverter = Guard.IsNull(() => bitConverter);
            this.socket = Guard.IsNull(() => socket);
            if (SocketType.Dgram != socket.SocketType)
                throw new NotSupportedException(
                    string.Format(
                        "The specified socket cannot be used as it is a {0} socket, whereas this type of connection only supports {1} sockets",
                        socket.SocketType, SocketType.Dgram));
        }

        public int Write(T message)
        {
            byte[] buffer;
            lock (syncRoot)
            {
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(message, stream);
                    buffer = stream.ToArray();
                    socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, message.RemoteEndPoint);
                }
            }
            return buffer.Length;
        }

        public T ReadNext(out int size)
        {
            var buffer = new byte[4];
            EndPoint remoteEndpoint = null;
            socket.FillReceiveFrom(buffer, ref remoteEndpoint);
            var messageLength = bitConverter.ToInt32(buffer);
            if (0 > messageLength)
            {
                size = -1;
                return default(T);
            }
            buffer = new byte[messageLength];
            socket.FillReceive(buffer);
            size = 4 + messageLength;
            using (var stream = new MemoryStream(buffer))
                return serializer.Deserialize(stream);
        }

        public bool IsOpen { get { return socket.Connected; } }
        public bool CanRead { get { return IsOpen; } }
        public bool CanWrite { get { return IsOpen; } }

        public void Dispose()
        {
            Dispose(true);
        }

        ~DatagramSocketConnectionStub()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket.Dispose();
        }
    }
}