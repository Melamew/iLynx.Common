using System;
using System.Collections.Generic;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;

namespace iLynx.Chatter.Infrastructure
{
    public class ClientManager : IClientManager
    {
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private IMessageServer<ChatMessage, int> server;
        private readonly List<Guid> connectedClients = new List<Guid>(); 

        public ClientManager(IBus<IApplicationEvent> applicationEventBus)
        {
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
        }

        private void AddClient(Guid clientId)
        {
            if (connectedClients.Contains(clientId)) return;
            connectedClients.Add(clientId);
            OnClientConnected(clientId);
        }

        protected virtual void OnClientConnected(Guid clientId)
        {
            applicationEventBus.Publish(new ClientConnectedEvent(clientId));
        }

        private void RemoveClientId(Guid clientId)
        {
            if (connectedClients.Remove(clientId))
                OnClientDisconnected(clientId);
        }

        private void ClearClients()
        {
            var old = connectedClients.ToArray();
            connectedClients.Clear();
            foreach (var id in old)
                OnClientDisconnected(id);
        }

        protected virtual void OnClientDisconnected(Guid clientId)
        {
            applicationEventBus.Publish(new ClientDisconnectedEvent(clientId));
        }

        public void Manage(IMessageServer<ChatMessage, int> messageServer)
        {
            ClearClients();
            server = messageServer;
            server.ClientConnected += ServerOnClientConnected;
            server.ClientDisconnected += ServerOnClientDisconnected;
        }

        private void ServerOnClientDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            RemoveClientId(clientDisconnectedEventArgs.ClientId);
        }

        private void ServerOnClientConnected(object sender, ClientConnectedEventArgs clientConnectedEventArgs)
        {
            AddClient(clientConnectedEventArgs.ClientId);
        }
    }
}
