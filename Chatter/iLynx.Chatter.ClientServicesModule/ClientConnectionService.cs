using System;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;

namespace iLynx.Chatter.ClientServicesModule
{
    public class ClientConnectionService
    {
        private readonly IBus<IApplicationCommand> commandBus;
        private readonly IBus<IApplicationEvent> eventBus;
        private readonly IClientSideClient<ChatMessage, int> client;

        public ClientConnectionService(IClientSideClient<ChatMessage, int> client,
            IBus<IApplicationCommand> commandBus,
            IBus<IApplicationEvent> eventBus)
        {
            this.commandBus = Guard.IsNull(() => commandBus);
            this.eventBus = Guard.IsNull(() => eventBus);
            this.client = Guard.IsNull(() => client);
            this.client.Disconnected += ClientOnDisconnected;
            Subscribe();
        }

        private void ClientOnDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            eventBus.Publish(new ClientDisconnectedEvent(clientDisconnectedEventArgs.ClientId));
        }

        private void Subscribe()
        {
            commandBus.Subscribe<ConnectCommand>(OnConnect);
            commandBus.Subscribe<DisconnectCommand>(OnDisconnected);
        }

        private void OnDisconnected(DisconnectCommand message)
        {
            var clientId = Guid.Empty;
            try
            {
                if (!client.IsConnected) return;
                clientId = client.ClientId;
                client.Disconnect(new ChatMessage
                {
                    ClientId = clientId,
                    Key = MessageKeys.ExitMessage,
                    Data = Encoding.Unicode.GetBytes("Disconnecting")
                });
            }
            finally { eventBus.Publish(new ClientDisconnectedEvent(clientId));}
        }

        private void OnConnect(ConnectCommand message)
        {
            try
            {
                if (client.IsConnected)
                    client.Disconnect(new ChatMessage { Key = MessageKeys.ExitMessage, ClientId = client.ClientId, Data = Encoding.Unicode.GetBytes("Exiting") });
                client.Connect(message.RemoteEndpoint);
            }
            finally
            {
                if (client.IsConnected)
                    eventBus.Publish(new ClientConnectedEvent(client.ClientId));
            }
        }
    }
}
