using System;

namespace iLynx.Chatter.Infrastructure.Events
{
    public class NickChangedEvent : IApplicationEvent
    {
        public Guid ClientId { get; private set; }
        public string Nick { get; private set; }

        public NickChangedEvent(Guid clientId, string nick)
        {
            ClientId = clientId;
            Nick = nick;
        }
    }
}