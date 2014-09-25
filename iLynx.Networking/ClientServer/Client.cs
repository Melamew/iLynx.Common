using System;
using System.Net;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

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
            this.messageBus.Subscribe<MessageEnvelope<TMessage, TKey>>(OnSendMessage);
        }

        private void OnSendMessage(MessageEnvelope<TMessage, TKey> envelope)
        {
            Send(envelope.Message);
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
