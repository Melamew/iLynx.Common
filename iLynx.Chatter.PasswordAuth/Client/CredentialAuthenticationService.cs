using System;
using System.IO;
using iLynx.Chatter.AuthenticationModule.ViewModels;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Serialization;

namespace iLynx.Chatter.AuthenticationModule.Client
{
    public class CredentialAuthenticationService : IDisposable
    {
        private readonly IWindowingService windowingService;
        private readonly ISerializerService serializerService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;

        public CredentialAuthenticationService(
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IWindowingService windowingService,
            ISerializerService serializerService)
        {
            this.windowingService = Guard.IsNull(() => windowingService);
            this.serializerService = Guard.IsNull(() => serializerService);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            this.messageBus = Guard.IsNull(() => messageBus);
            this.messageSubscriptionManager.Subscribe(MessageKeys.CredentialAuthenticationRequest, OnCredentialAuthenticationRequest);
        }

        private void OnCredentialAuthenticationRequest(ChatMessage keyedMessage, int totalSize)
        {
            var dialog = new UsernamePasswordDialogViewModel();
            if (!windowingService.ShowDialog(dialog))
                return;
            var message = new ChatMessage
            {
                Key = MessageKeys.CredentialAuthenticationResponse,
                ClientId = keyedMessage.ClientId,
            };
            using (var output = new MemoryStream())
            {
                serializerService.Serialize(new CredentialsPackage { Username = dialog.Username, Password = dialog.Password }, output);
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
