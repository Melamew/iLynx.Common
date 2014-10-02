using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.BroadcastMessaging.Client
{
    public class MessageLogViewModel : NotificationBase
    {
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IDispatcher dispatcher;
        private readonly INickManagerService nickManagerService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly ObservableCollection<LogEntry> entries = new ObservableCollection<LogEntry>();
        private ICommand sendCommand;
        private string chatLine;
        private Guid clientId;

        public MessageLogViewModel(INickManagerService nickManagerService,
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IBus<IApplicationEvent> applicationEventBus,
            IDispatcher dispatcher)
        {
            this.messageBus = Guard.IsNull(() => messageBus);
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.dispatcher = Guard.IsNull(() => dispatcher);
            this.nickManagerService = Guard.IsNull(() => nickManagerService);
            this.messageSubscriptionManager = Guard.IsNull(() => messageSubscriptionManager);
            Subscribe();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            messageSubscriptionManager.Unsubscribe(MessageKeys.TextMessage, OnTextMessageReceived);
            applicationEventBus.Unsubscribe<NickChangedEvent>(OnNickChanged);
            applicationEventBus.Unsubscribe<ClientConnectedEvent>(OnClientConnected);
        }

        private void OnClientConnected(ClientConnectedEvent message)
        {
            clientId = message.ClientId;
        }

        private void Subscribe()
        {
            messageSubscriptionManager.Subscribe(MessageKeys.TextMessage, OnTextMessageReceived);
            applicationEventBus.Subscribe<NickChangedEvent>(OnNickChanged);
            applicationEventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
        }

        private void OnNickChanged(NickChangedEvent message)
        {
            foreach (var entry in entries.Where(x => x.SourceClient == message.ClientId && !x.HaveNick))
                entry.SetNickname(message.Nick);
        }

        private void OnTextMessageReceived(ChatMessage message, int totalSize)
        {
            dispatcher.InvokeIfRequired(
                msg => entries.Add(new LogEntry(msg.ClientId, nickManagerService.GetNickName(msg.ClientId), DateTime.Now, Encoding.Unicode.GetString(msg.Data), message.ClientId == clientId)),
                message);
        }

        public ObservableCollection<LogEntry> Entries
        {
            get { return entries; }
        }

        public ICommand SendCommand
        {
            get { return sendCommand ?? (sendCommand = new DelegateCommand(OnSend)); }
        }

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

        private async void OnSend()
        {
            await messageBus.PublishAsync(new MessageEnvelope<ChatMessage, int>(new TextMessage(chatLine)));
            ChatLine = string.Empty;
        }
    }
}
