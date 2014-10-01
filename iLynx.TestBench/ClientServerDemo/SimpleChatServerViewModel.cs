using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows.Automation;
using System.Windows.Input;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Cryptography;
using iLynx.Networking.Interfaces;
using iLynx.Networking.TCP;
using iLynx.PubSub;
using Microsoft.Practices.Unity;

namespace iLynx.TestBench.ClientServerDemo
{
    public class SimpleChatServerViewModel : NotificationBase
    {
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly IClientManager clientManager;
        private readonly IDispatcher dispatcher;
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IUnityContainer container;
        private readonly ObservableCollection<ServerClientViewModel> connectedClients = new ObservableCollection<ServerClientViewModel>();
        private readonly IChatLogViewModel chatLog;
        private IMessageServer<ChatMessage, int> server;
        private ICommand startServerCommand;
        private ICommand stopServerCommand;
        private bool encryptCommunications;
        private IPAddress localAddress = IPAddress.Any;
        private ushort localPort = 5832;

        public SimpleChatServerViewModel(IServerContainer container)
        {
            this.container = Guard.IsNull(() => container);
            messageSubscriptionManager = container.Resolve<IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>>>();
            clientManager = container.Resolve<IClientManager>();
            dispatcher = container.Resolve<IDispatcher>();
            applicationEventBus = container.Resolve<IBus<IApplicationEvent>>();
            chatLog = container.Resolve<IChatLogViewModel>();
            messageSubscriptionManager.Subscribe(MessageKeys.TextMessage, OnTextMessageReceived);
            applicationEventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
            applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        private void OnTextMessageReceived(ChatMessage keyedMessage, int totalSize)
        {
            server.SendMessage(keyedMessage);
        }

        private void OnClientConnected(ClientConnectedEvent message)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(OnClientConnected, message);
                return;
            }
            var vm = container.Resolve<ServerClientViewModel>();
            vm.Id = message.ClientId;
            connectedClients.Add(vm);
        }

        private void OnClientDisconnected(ClientDisconnectedEvent message)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(OnClientDisconnected, message);
                return;
            }
            var vm = connectedClients.FirstOrDefault(x => x.Id == message.ClientId);
            if (null == vm) return;
            connectedClients.Remove(vm);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            messageSubscriptionManager.Unsubscribe(MessageKeys.TextMessage, OnTextMessageReceived);
            applicationEventBus.Unsubscribe<ClientConnectedEvent>(OnClientConnected);
            applicationEventBus.Unsubscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        public ICommand StartServerCommand
        {
            get { return startServerCommand ?? (startServerCommand = new DelegateCommand(OnStartServer)); }
        }

        public ICommand StopServerCommand
        {
            get { return stopServerCommand ?? (stopServerCommand = new DelegateCommand(OnStopServer)); }
        }

        public bool Encrypt
        {
            get { return encryptCommunications; }
            set
            {
                if (value == encryptCommunications) return;
                encryptCommunications = value;
                OnPropertyChanged();
            }
        }

        public IPAddress LocalAddress
        {
            get { return localAddress; }
            set
            {
                if (Equals(value, localAddress)) return;
                localAddress = value;
                OnPropertyChanged();
            }
        }

        public ushort LocalPort
        {
            get { return localPort; }
            set
            {
                if (value == localPort) return;
                localPort = value;
                OnPropertyChanged();
            }
        }

        public IChatLogViewModel ChatLog
        {
            get { return chatLog; }
        }

        protected virtual void OnStartServer()
        {
            if (null != server && server.IsRunning) return;
            ResolverOverride listenerOverride = encryptCommunications
                ? new DependencyOverride<IConnectionStubListener<ChatMessage, int>>(
                    container.Resolve<CryptoConnectionStubListener<ChatMessage, int>>())
                : new DependencyOverride<IConnectionStubListener<ChatMessage, int>>(
                    container.Resolve<TcpStubListener<ChatMessage, int>>());

            server = container.Resolve<IMessageServer<ChatMessage, int>>(listenerOverride);
            clientManager.Manage(server);
            server.Start(new IPEndPoint(localAddress, localPort));
            RaisePropertyChanged(() => IsRunning);
        }

        protected virtual void OnStopServer()
        {
            if (null == server) return;
            server.Stop();
            RaisePropertyChanged(() => IsRunning);
        }

        public bool IsRunning
        {
            get { return null != server && server.IsRunning; }
        }

        public ObservableCollection<ServerClientViewModel> ConnectedClients
        {
            get { return connectedClients; }
        }
    }
}
