using System;

namespace iLynx.Chatter.Infrastructure.Events
{
    public class ClientConnectedEvent : IApplicationEvent
    {
        public Guid ClientId { get; private set; }

        public ClientConnectedEvent(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}