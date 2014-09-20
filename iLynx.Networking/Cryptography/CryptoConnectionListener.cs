using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.Networking.TCP;

namespace iLynx.Networking.Cryptography
{
    public class CryptoConnectionListener<TMessage, TMessageKey> : SocketListenerBase,
        IConnectionListener<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly ISerializer<TMessage> serializer;
        private readonly IThreadManager threadManager;
        private readonly ILinkNegotiator linkNegotiator;

        public CryptoConnectionListener(ISerializer<TMessage> serializer, IThreadManager threadManager, ILinkNegotiator linkNegotiator)
        {
            this.serializer = Guard.IsNull(() => serializer);
            this.threadManager = Guard.IsNull(() => threadManager);
            this.linkNegotiator = Guard.IsNull(() => linkNegotiator);
        }

        public IConnection<TMessage, TMessageKey> AcceptNext()
        {
            var socket = AcceptSocket();
            if (null == socket) return null;
            var stub = new CryptoConnectionStub<TMessage, TMessageKey>(serializer, socket, linkNegotiator);
            return new StubConnection<TMessage, TMessageKey>(threadManager, stub);
        }
    }
}