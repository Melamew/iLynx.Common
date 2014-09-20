using System;

namespace iLynx.Chatter.Infrastructure
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