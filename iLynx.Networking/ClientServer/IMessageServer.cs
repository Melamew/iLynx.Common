using System;
using System.Collections.Generic;
using System.Net;

namespace iLynx.Networking.ClientServer
{
    public interface IMessageServer<in TMessage, TKey> where TMessage : IClientMessage<TKey>
    {
        void Start(EndPoint localEndpoint);
        void Stop();
        void SendMessage(TMessage message);
        void SendMessage(TMessage message, IEnumerable<Guid> clients);
        IEnumerable<Guid> ConnectedClients { get; }
        bool IsRunning { get; }
        void Disconnect(Guid clientId);
        event EventHandler<ClientConnectedEventArgs> ClientConnected;
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
    }
}
