using System.Net;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Configuration;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;

namespace iLynx.Chatter.Server
{
    public class ServerManager
    {
        private readonly INickManagerService nickManagerService;
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IConsoleHandler consoleHandler;
        private readonly IMessageServer<ChatMessage, int> chatMessageServer;
        private readonly IConfigurableValue<string> bindAddress;
        private readonly IConfigurableValue<ushort> bindPort;

        public ServerManager(IConfigurationManager configurationManager,
            IMessageServer<ChatMessage, int> chatMessageServer,
            IBus<IApplicationEvent> applicationEventBus,
            IConsoleHandler consoleHandler,
            INickManagerService nickManagerService)
        {
            this.nickManagerService = Guard.IsNull(() => nickManagerService);
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
            this.chatMessageServer = Guard.IsNull(() => chatMessageServer);
            this.consoleHandler.RegisterCommand("exit", OnExit, "Shutdown the server and exit");
            bindAddress = configurationManager.GetValue("BindAddress", "0.0.0.0");
            bindPort = configurationManager.GetValue<ushort>("BindPort", 5000);
            chatMessageServer.ClientConnected += ChatMessageServerOnClientConnected;
            chatMessageServer.ClientDisconnected += ChatMessageServerOnClientDisconnected;
            applicationEventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
            applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        private void ChatMessageServerOnClientDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            applicationEventBus.Publish(new ClientDisconnectedEvent(clientDisconnectedEventArgs.ClientId));
        }

        private void ChatMessageServerOnClientConnected(object sender, ClientConnectedEventArgs clientConnectedEventArgs)
        {
            applicationEventBus.Publish(new ClientConnectedEvent(clientConnectedEventArgs.ClientId));
        }

        private void OnClientDisconnected(ClientDisconnectedEvent message)
        {
            var nick = nickManagerService.GetNickName(message.ClientId);
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, this, string.Format("Client {0} disconnected.", string.IsNullOrEmpty(nick) ? message.ClientId.ToString() : nick));
        }

        private void OnClientConnected(ClientConnectedEvent message)
        {
            var nick = nickManagerService.GetNickName(message.ClientId);
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, this, string.Format("Client {0} connected.", string.IsNullOrEmpty(nick) ? message.ClientId.ToString() : nick));
        }

        private void OnExit(string[] strings)
        {
            consoleHandler.Break();
        }

        private EndPoint GetLocalEndPoint()
        {
            IPAddress address;
            return !IPAddress.TryParse(bindAddress.Value, out address) ? GetLocalDnsEndPoint() : new IPEndPoint(address, bindPort.Value);
        }

        private EndPoint GetLocalDnsEndPoint()
        {
            return new DnsEndPoint(bindAddress.Value, bindPort.Value);
        }

        public void Run()
        {
            chatMessageServer.Start(GetLocalEndPoint());
            consoleHandler.Run();
            applicationEventBus.Publish(new ShutdownEvent());
            chatMessageServer.Stop();
        }
    }
}
