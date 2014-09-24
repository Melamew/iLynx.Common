using System.Net;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Common.Configuration;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;

namespace iLynx.Chatter.Server
{
    public class ServerManager
    {
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IConsoleHandler consoleHandler;
        private readonly IMessageServer<ChatMessage, int> chatMessageServer;
        private readonly IConfigurableValue<string> bindAddress;
        private readonly IConfigurableValue<ushort> bindPort;

        public ServerManager(IConfigurationManager configurationManager,
            IMessageServer<ChatMessage, int> chatMessageServer,
            IBus<IApplicationEvent> applicationEventBus,
            IConsoleHandler consoleHandler)
        {
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
            this.chatMessageServer = Guard.IsNull(() => chatMessageServer);
            this.consoleHandler.RegisterCommand("exit", OnExit, "Shutdown the server and exit");
            bindAddress = configurationManager.GetValue("BindAddress", "0.0.0.0");
            bindPort = configurationManager.GetValue<ushort>("BindPort", 5000);
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
