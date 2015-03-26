using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Serialization;

namespace iLynx.Chatter.AuthenticationModule.Server
{
    public class CredentialAuthenticationService : IDisposable, IAuthenticationService<ChatMessage, int>
    {
        private readonly IBus<IServerCommand> serverCommandBus;
        private readonly ISerializerService serializerService;
        private readonly IUserRegistrationService userRegistrationService;
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly Dictionary<Guid, User> authenticatedClients = new Dictionary<Guid, User>();
        private readonly ReaderWriterLockSlim clientLock = new ReaderWriterLockSlim();

        public CredentialAuthenticationService(IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IBus<IApplicationEvent> applicationEventBus,
            IBus<IServerCommand> serverCommandBus,
            IUserRegistrationService userRegistrationService,
            IPasswordHashingService passwordHashingService,
            ISerializerService serializerService)
        {
            this.serverCommandBus = Guard.IsNull(() => serverCommandBus);
            this.serializerService = Guard.IsNull(() => serializerService);
            this.userRegistrationService = Guard.IsNull(() => userRegistrationService);
            this.passwordHashingService = Guard.IsNull(() => passwordHashingService);
            this.messageBus = Guard.IsNull(() => messageBus);
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.applicationEventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
            this.applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            this.messageSubscriptionManager.Subscribe(MessageKeys.CredentialAuthenticationResponse, OnCredentialsReceived);
        }

        private void OnClientConnected(ClientConnectedEvent message)
        {
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(new ChatMessage
            {
                Key = MessageKeys.CredentialAuthenticationRequest,
                ClientId = Guid.Empty,
                Data = new byte[0]
            }, message.ClientId));
        }

        private void OnCredentialsReceived(ChatMessage keyedMessage, int totalSize)
        {
            CredentialsPackage package;
            var clientId = keyedMessage.ClientId;
            using (var inputStream = new MemoryStream(keyedMessage.Data))
            {
                package = serializerService.Deserialize<CredentialsPackage>(inputStream);
            }
            User user;
            if (!userRegistrationService.IsRegistered(package.Username, out user))
            {
                RejectClient(clientId);
                return;
            }
            if (string.IsNullOrEmpty(package.Password))
            {
                RejectClient(clientId);
                return;
            }
            if (passwordHashingService.PasswordMatches(package.Password, user)) AcceptClient(clientId, user);
            else RejectClient(clientId);
        }

        private void AcceptClient(Guid clientId, User user)
        {
            var message = new ChatMessage
            {
                ClientId = Guid.Empty,
                Key = MessageKeys.CredentialAuthenticationAccepted
            };
            AddOrUpdateClientAuthentication(clientId, user);
            messageBus.Publish(new MessageEnvelope<ChatMessage, int>(message, clientId));
        }

        private void AddOrUpdateClientAuthentication(Guid client, User user)
        {
            clientLock.EnterReadLock();
            try
            {
                if (authenticatedClients.ContainsKey(client))
                    authenticatedClients[client] = user;
                else
                    authenticatedClients.Add(client, user);
            }
            finally { clientLock.ExitReadLock(); }
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, this, string.Format("Client {0} authenticated as {1}", client, user.Username));
            applicationEventBus.Publish(new ClientAuthenticatedEvent(client, user.Username));
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
            envelope.Wait();
            serverCommandBus.Publish(new DisconnectCommand(clientId));
        }

        public void Dispose()
        {
            messageSubscriptionManager.Unsubscribe(MessageKeys.CredentialAuthenticationResponse, OnCredentialsReceived);
        }

        private void OnClientDisconnected(ClientDisconnectedEvent message)
        {
            clientLock.EnterWriteLock();
            try
            {
                authenticatedClients.Remove(message.ClientId);
            }
            finally { clientLock.ExitWriteLock(); }
        }

        public bool IsClientAuthenticated(IClient<ChatMessage, int> client)
        {
            return IsClientAuthenticated(client.ClientId);
        }

        public bool IsClientAuthenticated(Guid clientId)
        {
            clientLock.EnterReadLock();
            try
            {
                return authenticatedClients.ContainsKey(clientId);
            }
            finally { clientLock.ExitReadLock(); }
        }

        public User GetAuthenticatedUser(Guid clientId)
        {
            clientLock.EnterReadLock();
            try
            {
                User result;
                return authenticatedClients.TryGetValue(clientId, out result) ? result : null;
            }
            finally { clientLock.ExitReadLock(); }
        }
    }
}
