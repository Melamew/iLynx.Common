using System;
using System.Threading;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Threading;

namespace iLynx.Networking.ClientServer
{
    public abstract class ClientBase<TMessage, TKey> : MessageReaderBase<TMessage, TKey>, IClient<TMessage, TKey>
        where TMessage : IKeyedMessage<TKey>
    {
        private int txBytes;
        private int rxBytes;

        protected ClientBase(IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager, IThreadManager threadManager)
            : base(subscriptionManager, threadManager) { }

        public void Send(TMessage message)
        {
            var stub = Stub;
            if (null == stub || !stub.IsOpen) throw new InvalidOperationException("Not connected");
            var tx = stub.Write(message);
            Interlocked.Add(ref txBytes, tx);
        }

        public void Close()
        {
            var stub = Stub;
            if (null == stub) return;
            stub.Dispose();
            try
            {
                WaitForReaderExit();
            }
            catch (Exception e)
            {
                this.LogDebug("Reader exited with exception: {0}", e);
            }
        }

        public void Disconnect(TMessage exitMessage)
        {
            Send(exitMessage);
            Close();
        }

        public event EventHandler<ClientDisconnectedEventArgs> Disconnected;

        public long SentBytes { get { return txBytes; } }
        public long ReceivedBytes { get { return rxBytes; } }

        protected override void OnStubClosed()
        {
            OnDisconnected();
        }

        protected virtual void OnDisconnected()
        {
            var handler = Disconnected;
            if (null == handler) return;
            handler(this, new ClientDisconnectedEventArgs(ClientId));
        }

        protected override void OnMessageRead(int totalSize)
        {
            Interlocked.Add(ref rxBytes, totalSize);
        }

        public abstract Guid ClientId { get; }

        public virtual bool IsConnected
        {
            get
            {
                var stub = Stub;
                return null != stub && stub.IsOpen;
            }
        }
    }
}