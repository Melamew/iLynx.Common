using System;

namespace iLynx.Networking.ClientServer
{
    public class ClientConnectedEventArgs : ClientActionEventArgs
    {
        public ClientConnectedEventArgs(Guid clientId)
            : base(clientId)
        {
        }
    }
}