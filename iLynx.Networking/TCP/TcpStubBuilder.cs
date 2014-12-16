using System.Net;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.Serialization;

namespace iLynx.Networking.TCP
{
    public class TcpStubBuilder<TMessage, TMessageKey> : IConnectionStubBuilder<TMessage, TMessageKey> where TMessage : IKeyedMessage<TMessageKey>
    {
        private readonly ITimerService timerService;
        private readonly ISerializer<TMessage> serializer;

        public TcpStubBuilder(ISerializer<TMessage> serializer, ITimerService timerService)
        {
            this.serializer = Guard.IsNull(() => serializer);
            this.timerService = Guard.IsNull(() => timerService);
        }

        public IConnectionStub<TMessage, TMessageKey> Build(EndPoint remoteEndpoint)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEndpoint);
            return new StreamedSocketConnectionStub<TMessage, TMessageKey>(serializer, socket, timerService);
        }
    }
}