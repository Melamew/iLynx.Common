﻿using System.Net;
using iLynx.Chatter.Infrastructure;

namespace iLynx.Chatter.ClientServicesModule
{
    public class ConnectCommand : IApplicationCommand
    {
        public EndPoint RemoteEndpoint { get; private set; }

        public ConnectCommand(EndPoint remoteEndpoint)
        {
            RemoteEndpoint = remoteEndpoint;
        }
    }
}
