using System;
using System.IO;
using iLynx.Chatter.AuthenticationModule.ViewModels;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.AuthenticationModule.Server
{
    public class CredentialAuthenticationService : IDisposable
    {
        private readonly IUserRegistrationService userRegistrationService;
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;

        public CredentialAuthenticationService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IBus<IApplicationEvent> applicationEventBus,
            IUserRegistrationService userRegistrationService,
            IPasswordHashingService passwordHashingService)
        {
            this.userRegistrationService = Guard.IsNull(() => userRegistrationService);
            this.passwordHashingService = Guard.IsNull(() => passwordHashingService);
            this.messageBus = Guard.IsNull(() => messageBus);
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            this.messageSubscriptionManager.Subscribe(MessageKeys.CredentialAuthenticationResponse, OnCredentialsReceived);
        }

        private void OnCredentialsReceived(ChatMessage keyedMessage, int totalSize)
        {
            CredentialsPackage package;
            var clientId = keyedMessage.ClientId;
            using (var inputStream = new MemoryStream(keyedMessage.Data))
            {
                package = Serializer.Deserialize<CredentialsPackage>(inputStream);
            }
            User user;
            if (!userRegistrationService.IsRegistered(package.Username, out user))
            {
                RejectClient(clientId);
                return;
            }
            if (passwordHashingService.PasswordMatches(package.Password, user)) AcceptClient(clientId, user.Username);
            else RejectClient(clientId);
        }

        private void AcceptClient(Guid clientId, string username)
        {
            var message = new ChatMessage
            {
                ClientId = Guid.Empty,
                Key = MessageKeys.CredentialAuthenticationAccepted
            };
            applicationEventBus.Publish(new ClientAuthenticatedEvent(clientId, username));
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(message, clientId));
        }

        private void RejectClient(Guid clientId)
        {
            var message = new ChatMessage
            {
                ClientId = Guid.Empty,
                Key = MessageKeys.CredentialAuthenticationRejected,
                Data = new byte[0]
            };
            var envelope = new MessageEnvelope<ChatMessage, int>(message, clientId);
            messageBus.Publish(envelope);
        }

        public void Dispose()
        {
            messageSubscriptionManager.Unsubscribe(MessageKeys.CredentialAuthenticationResponse, OnCredentialsReceived);
        }
    }

    public class ClientAuthenticationService : IDisposable
    {
        private readonly IWindowingService windowingService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;

        public ClientAuthenticationService(
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IWindowingService windowingService)
        {
            this.windowingService = windowingService;
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            this.messageBus = Guard.IsNull(() => messageBus);
            this.messageSubscriptionManager.Subscribe(MessageKeys.CredentialAuthenticationRequest, OnCredentialAuthenticationRequest);
        }

        private void OnCredentialAuthenticationRequest(ChatMessage keyedMessage, int totalSize)
        {
            var dialog = new UsernamePasswordDialogViewModel(windowingService);
            if (!windowingService.ShowDialog(dialog))
                return;
            var message = new ChatMessage
            {
                Key = MessageKeys.CredentialAuthenticationResponse,
                ClientId = keyedMessage.ClientId,
            };
            using (var output = new MemoryStream())
            {
                Serializer.Serialize(new CredentialsPackage { Username = dialog.Username, Password = dialog.Password }, output);
                message.Data = output.ToArray();
            }
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(message));
        }

        public void Dispose()
        {
            messageSubscriptionManager.Unsubscribe(MessageKeys.CredentialAuthenticationRequest, OnCredentialAuthenticationRequest);
        }
    }
}
