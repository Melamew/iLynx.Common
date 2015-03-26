using System.Threading;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;
using iLynx.Threading;

namespace iLynx.Chatter.Client.ViewModels
{
    public class ConnectionStatusViewModel : NotificationBase
    {
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IDispatcher dispatcher;
        private readonly ITimerService timerService;
        private readonly IClient<ChatMessage, int> client;
        private readonly int timerId;
        private long rxBytes, txBytes;
        private bool isConnected;

        public ConnectionStatusViewModel(IClient<ChatMessage, int> client,
            ITimerService timerService,
            IDispatcher dispatcher,
            IBus<IApplicationEvent> applicationEventBus)
        {
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.dispatcher = Guard.IsNull(() => dispatcher);
            this.timerService = Guard.IsNull(() => timerService);
            this.client = Guard.IsNull(() => client);
            IsConnected = client.IsConnected; // Initialize the property
            Subscribe();
            timerId = timerService.StartNew(PollConnectionState, 1000, Timeout.Infinite);
        }

        private void Subscribe()
        {
            applicationEventBus.Subscribe<ClientConnectedEvent>(OnConnected);
            applicationEventBus.Subscribe<ClientDisconnectedEvent>(OnDisconnected);
        }

        private void OnDisconnected(ClientDisconnectedEvent message)
        {
            dispatcher.InvokeIfRequired(() => IsConnected = false);
        }

        private void OnConnected(ClientConnectedEvent message)
        {
            dispatcher.InvokeIfRequired(() => IsConnected = true);
            timerService.Change(timerId, 1000, Timeout.Infinite);
        }

        private void Unsubscribe()
        {
            applicationEventBus.Unsubscribe<ClientConnectedEvent>(OnConnected);
            applicationEventBus.Unsubscribe<ClientDisconnectedEvent>(OnDisconnected);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            Unsubscribe();
        }

        private void PollConnectionState()
        {
            dispatcher.InvokeIfRequired(() =>
            {
                RxBytes = client.ReceivedBytes;
                TxBytes = client.SentBytes;

            });
            if (!client.IsConnected) return;
            timerService.Change(timerId, 1000, Timeout.Infinite);
        }

        public long RxBytes
        {
            get { return rxBytes; }
            private set
            {
                if (value == rxBytes) return;
                rxBytes = value;
                OnPropertyChanged();
            }
        }

        public long TxBytes
        {
            get { return txBytes; }
            private set
            {
                if (value == txBytes) return;
                txBytes = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                if (value == isConnected) return;
                isConnected = value;
                OnPropertyChanged();
            }
        }
    }
}
