using System;
using System.Collections.ObjectModel;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.Chatter.BroadcastMessaging.Client
{
    public class MessageLogViewModel : NotificationBase
    {
        private readonly IDispatcher dispatcher;
        private readonly INickManagerService nickManagerService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;
        private readonly ObservableCollection<LogEntryModel> entries = new ObservableCollection<LogEntryModel>();

        public MessageLogViewModel(INickManagerService nickManagerService,
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager,
            IDispatcher dispatcher)
        {
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
    }
}
