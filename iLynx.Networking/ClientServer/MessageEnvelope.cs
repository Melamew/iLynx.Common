using System;
using System.Threading;
using iLynx.Common;

namespace iLynx.Networking.ClientServer
{
    public class MessageEnvelope<TMessage, TKey> where TMessage : IClientMessage<TKey>
    {
        public TMessage Message { get; private set; }
        public Guid[] TargetClients { get; private set; }
        public bool Handled { get; set; }
        public Exception Error { get; set; }

        public void Wait()
        {
            while (!Handled)
                Thread.CurrentThread.Join(1);
        }

        public MessageEnvelope(TMessage message, params Guid[] targetClients)
        {
            Message = Guard.IsNull(() => message);
            TargetClients = targetClients;
        }
    }
}