using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.BroadcastMessaging.Server
{
    public class BroadcastMessagingService : IDisposable
    {
        private readonly IAuthenticationService<ChatMessage, int> authenticationService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;

        public BroadcastMessagingService(
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IAuthenticationService<ChatMessage, int> authenticationService)
        {
            this.authenticationService = Guard.IsNull(() => authenticationService);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            this.messageBus = Guard.IsNull(() => messageBus);
            Subscribe();
        }

        ~BroadcastMessagingService()
        {
            Dispose(false);
        }

        private void Subscribe()
        {
            messageSubscriptionManager.Subscribe(MessageKeys.TextMessage, OnMessageReceived);
        }

        private void OnMessageReceived(ChatMessage keyedMessage, int totalSize)
        {
            if (!authenticationService.IsClientAuthenticated(keyedMessage.ClientId)) return;

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}
