using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using iLynx.Chatter.Infrastructure;
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
        private readonly IDispatcher dispatcher;
        private readonly INickManagerService nickManagerService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly ObservableCollection<LogEntryModel> entries = new ObservableCollection<LogEntryModel>();
        private ICommand sendCommand;
        private string chatLine;

        public MessageLogViewModel(INickManagerService nickManagerService,
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IBus<MessageEnvelope<ChatMessage, int>> messageBus,
            IDispatcher dispatcher)
        {
            this.messageBus = messageBus;
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
        }

        private void Subscribe()
        {
            messageSubscriptionManager.Subscribe(MessageKeys.TextMessage, OnTextMessageReceived);
        }

        private void OnTextMessageReceived(ChatMessage message, int totalSize)
        {
            dispatcher.InvokeIfRequired(
                msg => entries.Add(new LogEntryModel(msg.ClientId, nickManagerService.GetNickName(msg.ClientId), DateTime.Now, Encoding.Unicode.GetString(msg.Data))),
                message);
        }

        public ObservableCollection<LogEntryModel> Entries
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
