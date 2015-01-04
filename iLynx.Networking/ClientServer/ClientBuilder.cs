using System;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Threading;

namespace iLynx.Networking.ClientServer
{
    public class ClientBuilder<TMessage, TKey> : IClientBuilder<TMessage, TKey> where TMessage : IClientMessage<TKey>, new()
    {
        private readonly IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager;
        private readonly IThreadManager threadManager;

        public ClientBuilder(IThreadManager threadManager,
            IKeyedSubscriptionManager<TKey, MessageReceivedHandler<TMessage, TKey>> subscriptionManager)
        {
            this.threadManager = Guard.IsNull(() => threadManager);
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
        }

        public IClient<TMessage, TKey> Build(IConnectionStub<TMessage, TKey> connection)
        {
            return new ServersideClient<TMessage, TKey>(connection, threadManager, subscriptionManager, Guid.NewGuid());
        }
    }
}
