using System;
using System.Collections.Generic;
using System.Threading;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;

namespace iLynx.Chatter.AuthenticationModule
{
    public class AuthenticationService : IAuthenticationService<ChatMessage, int>
    {
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly List<Guid> authenticatedClients = new List<Guid>();
        private readonly ReaderWriterLockSlim clientLock = new ReaderWriterLockSlim();

        public AuthenticationService(IBus<IApplicationEvent> applicationEventBus,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus)
        {
            this.messageBus = Guard.IsNull(() => messageBus);
            applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
            applicationEventBus.Subscribe<ClientAuthenticatedEvent>(OnClientAuthenticated);
            applicationEventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
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

        private void OnClientAuthenticated(ClientAuthenticatedEvent message)
        {
            clientLock.EnterWriteLock();
            try
            {
                if (authenticatedClients.Contains(message.ClientId)) return;
                authenticatedClients.Add(message.ClientId);
                RuntimeCommon.DefaultLogger.Log(LogLevel.Information, this, string.Format("Client {0} authenticated as {1}", message.ClientId, message.AuthenticationMessage));
            }
            finally { clientLock.ExitWriteLock(); }
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
            clientLock.EnterReadLock();
            try
            {
                return authenticatedClients.Contains(client.ClientId);
            }
            finally { clientLock.ExitReadLock(); }
        }
    }
}