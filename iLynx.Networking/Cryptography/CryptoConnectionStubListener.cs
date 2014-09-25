using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.Networking.TCP;

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
            var stub = new CryptoConnectionStub<TMessage, TKey>(Serializer, socket, linkNegotiator);
            stub.NegotiateTransportKeys();
            return stub;
        }
    }
}
