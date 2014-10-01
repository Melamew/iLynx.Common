using System;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.Infrastructure
{
    public class ClientNickManagerService : NickManagerBase
    {
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager;

        public ClientNickManagerService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IBus<IApplicationEvent> applicationEventBus)
            : base(subscriptionManager)
        {
            this.applicationEventBus = applicationEventBus;
            this.messageBus = Guard.IsNull(() => messageBus);
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
            Subscribe();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            Unsubscribe();
        }

        private void Subscribe()
        {
            subscriptionManager.Subscribe(MessageKeys.ChangeNickMessage, OnClientNickChanged);
        }

        private void Unsubscribe()
        {
            subscriptionManager.Unsubscribe(MessageKeys.ChangeNickMessage, OnClientNickChanged);
        }

        public override string GetNickName(Guid clientId)
        {
            var nick = base.GetNickName(clientId);
            if (string.IsNullOrEmpty(nick))
                RequestNickname(clientId);
            return nick;
        }

        private void RequestNickname(Guid clientId)
        {
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(new ChatMessage
            {
                Key = MessageKeys.RequestNick,
                ClientId = Guid.Empty,
                Data = clientId.ToByteArray()
            }));
        }

        private void OnClientNickChanged(ChatMessage keyedMessage, int totalSize)
        {
            var nickName = GetNickName(keyedMessage);
            SetNick(keyedMessage.ClientId, nickName);
            applicationEventBus.Publish(new NickChangedEvent(keyedMessage.ClientId, nickName));
        }
    }
}