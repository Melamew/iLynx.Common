using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;
using iLynx.Threading;

namespace iLynx.Networking.TCP
{
    public class TcpConnectionListener<TMessage, TMessageKey> : SocketListenerBase,
        IConnectionListener<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly ITimerService timerService;
        private readonly ISerializer<TMessage> serializer;
        private readonly IThreadManager threadManager;

        public TcpConnectionListener(ISerializer<TMessage> serializer, IThreadManager threadManager, ITimerService timerService)
        {
            this.timerService = timerService;
            this.serializer = Guard.IsNull(() => serializer);
            this.threadManager = Guard.IsNull(() => threadManager);
        }

        public IConnection<TMessage, TMessageKey> AcceptNext()
        {
            var socket = AcceptSocket();
            if (null == socket) return null;
            var stub = new StreamedSocketConnectionStub<TMessage, TMessageKey>(serializer, socket, timerService);
            return new StubConnection<TMessage, TMessageKey>(threadManager, stub);
        }
    }
}