using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;
using iLynx.Common.Configuration;
using iLynx.Common.Serialization;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.AuthenticationModule
{
    public class CredentialsPackage
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    //public class UserRegistrationService : IDisposable
    //{
    //    public UserRegistrationService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> )
    //    public void Dispose()
    //    {
            
    //    }
    //}

    public class CredentialAuthenticationService : IDisposable
    {
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IDataAdapter<User> userAdapter;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly IBitConverter bitConverter = Serializer.SingletonBitConverter;
        private readonly IConfigurableValue<int> keySizeValue;

        public CredentialAuthenticationService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IBus<IApplicationEvent> applicationEventBus,
            IDataAdapter<User> userAdapter,
            IConfigurationManager configurationManager)
        {
            this.messageBus = Guard.IsNull(() => messageBus);
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.userAdapter = Guard.IsNull(() => userAdapter);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);

            keySizeValue = configurationManager.GetValue("PasswordHashSize", 1024, "Server");
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
            var user = userAdapter.Query().FirstOrDefault(x => x.Username == package.Username);
            if (null == user)
            {
                RejectClient(clientId);
                return;
            }
            var saltBytes = bitConverter.GetBytes(user.PasswordSalt);
            var derriveBytes = new Rfc2898DeriveBytes(package.Password, saltBytes);
            var bytes = derriveBytes.GetBytes(keySizeValue.Value);
            var storedHash = Convert.FromBase64String(user.PasswordHash);
            if (bytes.SequenceEqual(storedHash)) AcceptClient(clientId);
            else RejectClient(clientId);
        }

        private void AcceptClient(Guid clientId)
        {
            var message = new ChatMessage
            {
                ClientId = Guid.Empty,
                Key = MessageKeys.CredentialAuthenticationAccepted
            };
            applicationEventBus.Publish(new ClientAuthenticatedEvent(clientId));
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
}
