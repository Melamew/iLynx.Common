using System;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Networking.ClientServer
{
    public class ServersideClient<TMessage, TKey> : ClientBase<TMessage, TKey> where TMessage : IClientMessage<TKey>, new()
    {
        private readonly IConnectionStub<TMessage, TKey> stub;
        private readonly Guid clientId;

        public ServersideClient(IConnectionStub<TMessage, TKey> stub, IThreadManager threadManager, IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> messageHandler, Guid clientId) :
            base(messageHandler, threadManager)
        {
            this.stub = Guard.IsNull(() => stub);
            stub.Write(new TMessage { ClientId = clientId });
            this.clientId = clientId;
            StartReader();
        }

        protected override void PublishMessage(TMessage message, int rxSize)
        {
            message.ClientId = clientId;
            base.PublishMessage(message, rxSize);
        }

        public override Guid ClientId { get { return clientId; } }

        protected override IConnectionStub<TMessage, TKey> Stub
        {
            get { return stub; }
        }
    }
}