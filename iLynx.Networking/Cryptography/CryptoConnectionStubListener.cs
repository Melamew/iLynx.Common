using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.Networking.TCP;
using iLynx.Serialization;

namespace iLynx.Networking.Cryptography
{
    public class CryptoConnectionStubListener<TMessage, TKey> : TcpStubListener<TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        private readonly ILinkNegotiator linkNegotiator;

        public CryptoConnectionStubListener(ISerializer<TMessage> serializer, ILinkNegotiator linkNegotiator, ITimerService timerService) : base(serializer, timerService)
        {
            this.linkNegotiator = Guard.IsNull(() => linkNegotiator);
        }

        public override IConnectionStub<TMessage, TKey> AcceptNext()
        {
            var socket = AcceptSocket();
            if (null == socket) return null;
            //var stub = new CryptoConnectionStub<TMessage, TKey>(Serializer, socket, linkNegotiator, TimerService);
            var stub = new ManualCryptoConnectionStub<TMessage, TKey>(Serializer, socket, linkNegotiator, TimerService);
            this.LogDebug("==> LISTENER: Begin Negotiate");
            stub.NegotiateTransportKeys();
            this.LogDebug("<== LISTENER: End Negotiate");
            return stub;
        }
    }
}
