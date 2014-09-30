using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.TCP
{
    public class TcpStubListener<TMessage, TMessageKey> : SocketListenerBase, IConnectionStubListener<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        protected readonly ITimerService TimerService;
        protected readonly ISerializer<TMessage> Serializer;

        public TcpStubListener(ISerializer<TMessage> serializer, ITimerService timerService)
        {
            TimerService = Guard.IsNull(() => timerService);
            Serializer = Guard.IsNull(() => serializer);
        }

        public virtual IConnectionStub<TMessage, TMessageKey> AcceptNext()
        {
            var socket = AcceptSocket();
            if (null == socket) return null;
            return new StreamedSocketConnectionStub<TMessage, TMessageKey>(Serializer, socket, TimerService);
        }
    }
}