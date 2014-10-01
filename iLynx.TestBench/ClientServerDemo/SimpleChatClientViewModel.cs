using System.Net;
using System.Windows.Input;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Cryptography;
using iLynx.Networking.Interfaces;
using iLynx.Networking.TCP;
using Microsoft.Practices.Unity;

namespace iLynx.TestBench.ClientServerDemo
{
    public class SimpleChatClientViewModel : NotificationBase
    {
        private readonly IChatLogViewModel chatLog;
        private readonly IUnityContainer container;
        private IPAddress remoteHost = IPAddress.Loopback;
        private ushort remotePort = 5832;
        private IClient<ChatMessage, int> client;
        private ICommand connectCommand;
        private ICommand disconnectCommand;
        private bool encryptTraffic;

        public SimpleChatClientViewModel(IClientContainer container)
        {
            chatLog = container.Resolve<IChatLogViewModel>();
            this.container = Guard.IsNull(() => container);
        }

        public IChatLogViewModel ChatLog
        {
            get { return chatLog; }
        }

        public IPAddress RemoteHost
        {
            get { return remoteHost; }
            set
            {
                if (Equals(value, remoteHost)) return;
                remoteHost = value;
                OnPropertyChanged();
            }
        }

        public ushort RemotePort
        {
            get { return remotePort; }
            set
            {
                if (value == remotePort) return;
                remotePort = value;
                OnPropertyChanged();
            }
        }

        public bool EncryptTraffic
        {
            get { return encryptTraffic; }
            set
            {
                if (value == encryptTraffic) return;
                encryptTraffic = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand
        {
            get { return connectCommand ?? (connectCommand = new DelegateCommand(OnConnect)); }
        }

        public ICommand DisconnectCommand
        {
            get { return disconnectCommand ?? (disconnectCommand = new DelegateCommand(OnDisconnect)); }
        }

        public bool IsConnected
        {
            get { return null != client && client.IsConnected; }
        }

        private void OnDisconnect()
        {
            if (null == client) return;
            client.Disconnected -= ClientOnDisconnected;
            client.Disconnect(new ChatMessage { Key = MessageKeys.ExitMessage });
            RaisePropertyChanged(() => IsConnected);
        }

        private void OnConnect()
        {
            if (IsConnected) return;
            var clientSideClient = container.Resolve<IClientSideClient<ChatMessage, int>>(new DependencyOverride<IConnectionStubBuilder<ChatMessage, int>>(encryptTraffic
                    ? container.Resolve<CryptoConnectionStubBuilder<ChatMessage, int>>()
                    : (IConnectionStubBuilder<ChatMessage, int>)container.Resolve<TcpStubBuilder<ChatMessage, int>>()));
            clientSideClient.Connect(new IPEndPoint(remoteHost, remotePort));
            client = clientSideClient;
            client.Disconnected += ClientOnDisconnected;
            RaisePropertyChanged(() => IsConnected);
        }

        private void ClientOnDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            RaisePropertyChanged(() => IsConnected);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            OnDisconnect();
        }

        private string chatLine;
        public string ChatLine
        {
            get { return chatLine; }
            set
            {
                if (value == chatLine) return;
                chatLine = value;
                OnPropertyChanged();
            }
        }

        private ICommand sendLineCommand;

        public ICommand SendLineCommand { get { return sendLineCommand ?? (sendLineCommand = new DelegateCommand(OnSendLine)); } }

        private void OnSendLine()
        {
            if (!IsConnected) return;
            if (string.IsNullOrEmpty(chatLine)) return;
            client.Send(new TextMessage(chatLine));
            ChatLine = string.Empty;
        }
    }
}
