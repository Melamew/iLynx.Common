using System;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.BroadcastMessaging.Server
{
    public static class BroadcastPermissions
    {
        public const string CanBroadcast = "BroadcastPermissions.Broadcast";
    }

    public class BroadcastMessagingService : IDisposable
    {
        private readonly IUserPermissionService permissionService;
        private readonly IAuthenticationService<ChatMessage, int> authenticationService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;

        public BroadcastMessagingService(
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IAuthenticationService<ChatMessage, int> authenticationService,
            IUserPermissionService permissionService)
        {
            this.permissionService = Guard.IsNull(() => permissionService);
            this.authenticationService = Guard.IsNull(() => authenticationService);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            this.messageBus = Guard.IsNull(() => messageBus);
            RegisterPermissions();
            Subscribe();
        }

        private void RegisterPermissions()
        {
            permissionService.CreatePermission(BroadcastPermissions.CanBroadcast);
        }

        ~BroadcastMessagingService()
        {
            Dispose(false);
        }

        private void Subscribe()
        {
            messageSubscriptionManager.Subscribe(MessageKeys.TextMessage, OnMessageReceived);
        }

        private void Unsubscribe()
        {
            messageSubscriptionManager.Unsubscribe(MessageKeys.TextMessage, OnMessageReceived);
        }

        private void OnMessageReceived(ChatMessage keyedMessage, int totalSize)
        {
            var sourceClient = keyedMessage.ClientId;
            if (!authenticationService.IsClientAuthenticated(sourceClient))
            {
                SendDeniedResponse(sourceClient);
                return;
            }
            var user = authenticationService.GetAuthenticatedUser(sourceClient);
            if (!permissionService.HasPermission(user, BroadcastPermissions.CanBroadcast))
            {
                SendDeniedResponse(sourceClient);
                return;
            }
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(keyedMessage));
        }

        private void SendDeniedResponse(Guid clientId)
        {
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(new ChatMessage
            {
                ClientId = Guid.Empty,
                Data = Encoding.Unicode.GetBytes("Unauthorized"),
                Key = MessageKeys.RequestDenied,
            }, clientId));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Unsubscribe();
        }
    }
}
