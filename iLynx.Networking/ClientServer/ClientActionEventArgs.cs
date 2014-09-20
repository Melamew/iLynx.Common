using System;

namespace iLynx.Networking.ClientServer
{
    public class ClientActionEventArgs : EventArgs
    {
        public Guid ClientId { get; private set; }

        public ClientActionEventArgs(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}