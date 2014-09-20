using System;
using System.Net;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.Cryptography;
using iLynx.Networking.Interfaces;

namespace iLynx.Networking.TCP
{
    public abstract class SocketListenerBase : IListener
    {
        private Socket localSocket;

        protected SocketListenerBase()
        {
            localSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void BindTo(EndPoint localEndpoint)
        {
            if (IsBound) throw new InvalidOperationException("This listener is already bound");
            localSocket.Bind(localEndpoint);
            localSocket.Listen(5);
        }

        protected virtual Socket AcceptSocket()
        {
            if (null == localSocket) throw new InvalidOperationException("This listener is not yet bound - call BindTo in order to bind it to a local address");
            try
            {
                var socket = localSocket.Accept();
                return socket;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsBound { get { return null != localSocket && localSocket.IsBound; } }

        public void Close()
        {
            if (null == localSocket) return;
            try
            {
                localSocket.Close();
                localSocket.Dispose();
                localSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            }
            catch (NullReferenceException) { }
        }
    }

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