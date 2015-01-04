using System;
using System.Net;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Threading;

namespace iLynx.Networking.ClientServer
{
    public class Client<TMessage, TKey> : ClientBase<TMessage, TKey>, IClientSideClient<TMessage, TKey> where TMessage : IClientMessage<TKey>
    {
        private readonly IBus<MessageEnvelope<TMessage, TKey>> messageBus;
        private readonly IConnectionStubBuilder<TMessage, TKey> connectionBuilder;
        private IConnectionStub<TMessage, TKey> stub;
        private Guid clientId;

        public Client(IConnectionStubBuilder<TMessage, TKey> connectionBuilder,
            IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager,
            IThreadManager threadManager,
            IBus<MessageEnvelope<TMessage, TKey>> messageBus)
            : base(subscriptionManager, threadManager)
        {
            this.messageBus = Guard.IsNull(() => messageBus);
            this.connectionBuilder = Guard.IsNull(() => connectionBuilder);
            Subscribe();
        }

        private void Subscribe()
        {
            messageBus.Subscribe<MessageEnvelope<TMessage, TKey>>(OnSendMessage);
        }

        private void Unsubscribe()
        {
            messageBus.Unsubscribe<MessageEnvelope<TMessage, TKey>>(OnSendMessage);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            Unsubscribe();
        }

        private void OnSendMessage(MessageEnvelope<TMessage, TKey> envelope)
        {
            try
            {
                Send(envelope.Message);
            }
            catch (Exception e)
            {
                envelope.Error = e;
            }
            envelope.Handled = true;
        }

        public override Guid ClientId { get { return clientId; } }

        public void Connect(EndPoint remoteEndPoint)
        {
            if (IsConnected) Close();
            stub = connectionBuilder.Build(remoteEndPoint);
            int rx;
            var firstMessage = stub.ReadNext(out rx);
            OnMessageRead(rx);
            clientId = firstMessage.ClientId;
            StartReader();
        }

        protected override IConnectionStub<TMessage, TKey> Stub
        {
            get { return stub; }
        }
    }
}
