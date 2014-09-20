using System;
using iLynx.Common;

namespace iLynx.Networking.ClientServer
{
    public class MessageEnvelope<TMessage, TKey> where TMessage : IClientMessage<TKey>
    {
        public TMessage Message { get; private set; }
        public Guid[] TargetClients { get; private set; }

        public MessageEnvelope(TMessage message, params Guid[] targetClients)
        {
            Message = Guard.IsNull(() => message);
            TargetClients = targetClients;
        }
    }
}