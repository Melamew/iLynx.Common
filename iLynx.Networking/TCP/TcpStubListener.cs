using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.TCP
{
    public class TcpStubListener<TMessage, TMessageKey> : SocketListenerBase, IConnectionStubListener<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly ITimerService timerService;
        protected readonly ISerializer<TMessage> Serializer;

        public TcpStubListener(ISerializer<TMessage> serializer, ITimerService timerService)
        {
            this.timerService = Guard.IsNull(() => timerService);
            Serializer = Guard.IsNull(() => serializer);
        }

        public virtual IConnectionStub<TMessage, TMessageKey> AcceptNext()
        {
            var socket = AcceptSocket();
            if (null == socket) return null;
            return new StreamedSocketConnectionStub<TMessage, TMessageKey>(Serializer, socket, timerService);
        }
    }
}