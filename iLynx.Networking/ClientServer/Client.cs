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
        private readonly IAuthenticationHandler<TMessage, TKey> authenticationHandler;
        private readonly IConnectionStubBuilder<TMessage, TKey> connectionBuilder;
        private IConnectionStub<TMessage, TKey> stub;
        private Guid clientId;

        public Client(IConnectionStubBuilder<TMessage, TKey> connectionBuilder,
            IAuthenticationHandler<TMessage, TKey> authenticationHandler,
            IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager,
            IThreadManager threadManager)
            : base(subscriptionManager, threadManager)
        {
            this.authenticationHandler = authenticationHandler;
            this.connectionBuilder = Guard.IsNull(() => connectionBuilder);
        }

        public override Guid ClientId { get { return clientId; } }

        public void Connect(EndPoint remoteEndPoint)
        {
            if (IsConnected) Close();
            stub = connectionBuilder.Build(remoteEndPoint);
            if (!authenticationHandler.Authenticate(stub)) throw new InvalidOperationException("Not authorized");
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
