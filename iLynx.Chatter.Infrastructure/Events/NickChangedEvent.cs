using System;

namespace iLynx.Chatter.Infrastructure.Events
{
    public class NickChangedEvent : IApplicationEvent
    {
        public Guid ClientId { get; private set; }

        public NickChangedEvent(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}