using System.Net;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.Cryptography
{
    public class CryptoConnectionStubBuilder<TMessage, TKey> : IConnectionStubBuilder<TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        private readonly ITimerService timerService;
        private readonly ILinkNegotiator negotiator;
        private readonly ISerializer<TMessage> serializer;

        public CryptoConnectionStubBuilder(ISerializer<TMessage> serializer, ILinkNegotiator negotiator, ITimerService timerService)
        {
            this.timerService = Guard.IsNull(() => timerService);
            this.negotiator = Guard.IsNull(() => negotiator);
            this.serializer = Guard.IsNull(() => serializer);
        }

        public IConnectionStub<TMessage, TKey> Build(EndPoint endpoint)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            //var stub = new CryptoConnectionStub<TMessage, TKey>(serializer, socket, negotiator, timerService);
            var stub = new ManualCryptoConnectionStub<TMessage, TKey>(serializer, socket, negotiator, timerService);
            this.LogDebug("==> BUILDER: Begin Negotiate");
            stub.NegotiateTransportKeys();
            this.LogDebug("<== BUILDER: End Negotiate");
            return stub;
        }
    }
}
