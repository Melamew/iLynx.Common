using System.Net;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.Cryptography
{
    public class CryptoConnectionStubBuilder<TMessage, TKey> : IConnectionStubBuilder<TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        private readonly ILinkNegotiator negotiator;
        private readonly ISerializer<TMessage> serializer;

        public CryptoConnectionStubBuilder(ISerializer<TMessage> serializer, ILinkNegotiator negotiator)
        {
            this.negotiator = Guard.IsNull(() => negotiator);
            this.serializer = Guard.IsNull(() => serializer);
        }

        public IConnectionStub<TMessage, TKey> Build(EndPoint endpoint)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            var stub = new CryptoConnectionStub<TMessage, TKey>(serializer, socket, negotiator);
            stub.NegotiateTransportKeys();
            return stub;
        }
    }
}
