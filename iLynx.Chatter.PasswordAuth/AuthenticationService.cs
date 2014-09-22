using System;
using System.Collections.Generic;
using System.Threading;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;

namespace iLynx.Chatter.AuthenticationModule
{
    public class AuthenticationService : IAuthenticationService<ChatMessage, int>
    {
        private readonly List<Guid> authenticatedClients = new List<Guid>();
        private readonly ReaderWriterLockSlim clientLock = new ReaderWriterLockSlim();

        public AuthenticationService(IBus<IApplicationEvent> applicationEventBus)
        {
            applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
            applicationEventBus.Subscribe<ClientAuthenticatedEvent>(OnClientAuthenticated);
        }

        private void OnClientAuthenticated(ClientAuthenticatedEvent message)
        {
            clientLock.EnterWriteLock();
            try
            {
                if (authenticatedClients.Contains(message.ClientId)) return;
                authenticatedClients.Add(message.ClientId);
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