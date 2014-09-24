using System;
using iLynx.Chatter.Infrastructure.Events;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public class ClientAuthenticatedEvent : IApplicationEvent
    {
        public Guid ClientId { get; private set; }
        public string AuthenticationMessage { get; private set; }

        public ClientAuthenticatedEvent(Guid clientId, string authenticationMessage)
        {
            ClientId = clientId;
            AuthenticationMessage = authenticationMessage;
        }
    }
}