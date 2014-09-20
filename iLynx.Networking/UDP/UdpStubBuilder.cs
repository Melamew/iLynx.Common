using System.Net;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.UDP
{
    public class UdpStubBuilder<TMessage, TMessageKey> : IConnectionStubBuilder<TMessage, TMessageKey>
        where TMessage : IKeyedDatagramMessage<TMessageKey>
    {
        private readonly IBitConverter bitConverter;
        private readonly ISerializer<TMessage> serializer;

        public UdpStubBuilder(ISerializer<TMessage> serializer, IBitConverter bitConverter)
        {
            this.serializer = Guard.IsNull(() => serializer);
            this.bitConverter = Guard.IsNull(() => bitConverter);
        }

        public IConnectionStub<TMessage, TMessageKey> Build(EndPoint localEndpoint)
        {
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEndpoint);
            return new DatagramSocketConnectionStub<TMessage, TMessageKey>(serializer, bitConverter, socket);
        }
    }
}