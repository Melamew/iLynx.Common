using System.Net;
using iLynx.Chatter.ClientServicesModule.ViewModels;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.PubSub;

namespace iLynx.Chatter.ClientServicesModule
{
    public class ConnectMenuItemViewModel : MenuItemViewModel
    {
        private readonly IConnectionDialogViewModel dialog;
        private readonly IWindowingService windowingService;
        private readonly IBus<IApplicationCommand> commandBus;
        private readonly IDispatcher dispatcher;

        public ConnectMenuItemViewModel(IBus<IApplicationEvent> applicationEventBus,
            IBus<IApplicationCommand> commandBus,
            IDispatcher dispatcher,
            IConnectionDialogViewModel dialog,
            IWindowingService windowingService,
            params IMenuItem[] children) : base("Connect", null, children)
        {
            this.dialog = Guard.IsNull(() => dialog);
            this.windowingService = Guard.IsNull(() => windowingService);
            this.commandBus = Guard.IsNull(() => commandBus);
            this.dispatcher = Guard.IsNull(() => dispatcher);
            Guard.IsNull(() => applicationEventBus);
            Command = new DelegateCommand(OnConnect);
            applicationEventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
            applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        private void OnClientDisconnected(ClientDisconnectedEvent message)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(OnClientDisconnected, message);
                return;
            }
            Title = "Connect";
            Command = new DelegateCommand(OnConnect);
        }

        private void OnClientConnected(ClientConnectedEvent message)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(OnClientConnected, message);
                return;
            }
            Title = "Disconnect";
            Command = new DelegateCommand(OnDisconnect);
        }

        private void OnDisconnect()
        {
            commandBus.Publish(new DisconnectCommand());
        }

        private void OnConnect()
        {
            var result = windowingService.ShowDialog(dialog);
            if (!result) return;
            commandBus.Publish(new ConnectCommand(GetEndpoint(dialog.RemoteHost, dialog.RemotePort)));
        }

        private EndPoint GetEndpoint(string host, ushort port)
        {
            IPAddress address;
            if (!IPAddress.TryParse(host, out address))
                return new DnsEndPoint(host, port);
            return new IPEndPoint(address, port);
        }
    }
}