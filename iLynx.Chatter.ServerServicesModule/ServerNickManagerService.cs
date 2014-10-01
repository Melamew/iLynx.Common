using System;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.ServerServicesModule
{
    public class ServerNickManagerService : NickManagerBase
    {
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>>
            messageSubscriptionManager;

        public ServerNickManagerService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager,
            IBus<IApplicationEvent> applicationEventBus,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus)
            : base(subscriptionManager)
        {
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.messageBus = Guard.IsNull(() => messageBus);
            messageSubscriptionManager = Guard.IsNull(() => subscriptionManager);
            messageSubscriptionManager.Subscribe(MessageKeys.ChangeNickMessage, HandleChangeNickMessage);
            messageSubscriptionManager.Subscribe(MessageKeys.RequestNick, OnRequestNick);
            applicationEventBus.Subscribe<ClientAuthenticatedEvent>(OnClientAuthenticated);
        }

        private void OnRequestNick(ChatMessage keyedMessage, int totalSize)
        {
            var requestedClient = new Guid(keyedMessage.Data);
            var nickName = GetNickName(requestedClient);
            if (string.IsNullOrEmpty(nickName)) return;
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(
                new ChatMessage
                {
                    Key = MessageKeys.ChangeNickMessage,
                    ClientId = requestedClient,
                    Data = Encoding.Unicode.GetBytes(nickName)
                }, keyedMessage.ClientId));
        }

        private void OnClientAuthenticated(ClientAuthenticatedEvent message)
        {
            SetNick(message.ClientId, message.Identity);
            NotifyNickChanged(message.ClientId, message.Identity);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            messageSubscriptionManager.Unsubscribe(MessageKeys.ChangeNickMessage, HandleChangeNickMessage);
        }

        private void HandleChangeNickMessage(ChatMessage message, int messageSize)
        {
            var nick = GetNickName(message);
            var client = message.ClientId;
            if (string.IsNullOrEmpty(nick) || NickExists(nick))
                SendRequestDenied(client);
            else
                NotifyNickChanged(client, nick);
        }

        private void NotifyNickChanged(Guid client, string nick)
        {
            var message = new ChatMessage
            {
                ClientId = client,
                Key = MessageKeys.ChangeNickMessage,
                Data = Encoding.Unicode.GetBytes(nick)
            };
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(message)); // Also send to all other clients to notify them of the change
            
            // Remember to notify localy as well
            applicationEventBus.Publish(new NickChangedEvent(client, nick));
        }

        private void SendRequestDenied(Guid client)
        {
            var message = new ChatMessage
            {
                ClientId = client,
                Key = MessageKeys.ChangeNickMessage,
                Data = new byte[] { 0x00 }
            };
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(message, client));
        }
    }
}
