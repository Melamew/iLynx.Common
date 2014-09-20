using System;

namespace iLynx.Chatter.Infrastructure
{
    public class ClientDisconnectedEvent : IApplicationEvent
    {
        public Guid ClientId { get; private set; }

        public ClientDisconnectedEvent(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}