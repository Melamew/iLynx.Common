using System;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public class ClientAuthenticatedEvent : IApplicationEvent
    {
        public Guid ClientId { get; private set; }

        public ClientAuthenticatedEvent(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}