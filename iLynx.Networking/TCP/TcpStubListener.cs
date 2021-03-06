using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;

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
            return null == socket ? null : new StreamedSocketConnectionStub<TMessage, TMessageKey>(Serializer, socket, TimerService);
        }
    }
}