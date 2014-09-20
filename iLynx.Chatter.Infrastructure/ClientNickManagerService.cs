using System;
using iLynx.Common;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.Infrastructure
{
    public class ClientNickManagerService : NickManagerBase
    {
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager;

        public ClientNickManagerService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager)
            : base(subscriptionManager)
        {
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
            this.subscriptionManager.Subscribe(MessageKeys.ChangeNickMessage, OnClientNickChanged);
        }

        private void OnClientNickChanged(ChatMessage keyedMessage, int totalSize)
        {
            SetNick(keyedMessage.ClientId, GetNickName(keyedMessage));
        }
    }
}