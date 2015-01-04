using System;
using System.Net.Sockets;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Threading;

namespace iLynx.Networking.ClientServer
{
    public abstract class MessageReaderBase<TMessage, TKey> : IDisposable where TMessage : IKeyedMessage<TKey>
    {
        private readonly IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager;
        private readonly IThreadManager threadManager;
        private IWorker messageWorker;

        protected MessageReaderBase(IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager, IThreadManager threadManager)
        {
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
            this.threadManager = Guard.IsNull(() => threadManager);
        }

        ~MessageReaderBase()
        {
            Dispose(false);
        }

        protected void StartReader()
        {
            messageWorker = threadManager.StartNew(ReadSubscriptionMessages);
        }

        protected void WaitForReaderExit()
        {
            if (null == messageWorker) return;
            messageWorker.Wait();
        }

        private void ReadSubscriptionMessages()
        {
            var stub = Stub;
            while (stub.IsOpen)
            {
                try
                {
                    int rxSize;
                    var message = stub.ReadNext(out rxSize);
                    if (0 > rxSize) break;
                    OnMessageRead(rxSize);
                    PublishMessage(message, rxSize);
                }
                catch (SocketException se)
                {
                    if (SocketError.TimedOut == se.SocketErrorCode && stub.IsOpen)
                        continue;
                    break;
                }
            }
            OnStubClosed();
        }

        protected virtual void PublishMessage(TMessage message, int rxSize)
        {
            var subscribers = subscriptionManager.GetSubscribers(message.Key);
            if (null == subscribers) return;
            foreach (var subscriber in subscribers)
                subscriber(message, rxSize);
        }

        protected abstract void OnStubClosed();
        protected abstract void OnMessageRead(int totalSize);
        protected abstract IConnectionStub<TMessage, TKey> Stub { get; }
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}