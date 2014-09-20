using System;

namespace iLynx.Networking.ClientServer
{
    public class ClientDisconnectedEventArgs : ClientActionEventArgs
    {
        public ClientDisconnectedEventArgs(Guid clientId)
            : base(clientId)
        {
        }
    }
}