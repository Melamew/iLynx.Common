using System;
using System.Collections.ObjectModel;
using System.Text;
using iLynx.Chatter.BroadcastMessaging;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.TestBench.ClientServerDemo
{
    public class ChatLogViewModel : NotificationBase, IChatLogViewModel
    {
        private readonly IDispatcher dispatcher;
        private readonly INickManagerService nickManagerService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager;
        private readonly ObservableCollection<LogEntry> logEntries;

        public ChatLogViewModel(
            IDispatcher dispatcher,
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager,
            INickManagerService nickManagerService)
        {
            this.dispatcher = dispatcher;
            this.nickManagerService = Guard.IsNull(() => nickManagerService);
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
            this.subscriptionManager.Subscribe(MessageKeys.TextMessage, OnTextMessageReceived);
            logEntries = new ObservableCollection<LogEntry>();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            subscriptionManager.Unsubscribe(MessageKeys.TextMessage, OnTextMessageReceived);
        }

        private void OnTextMessageReceived(ChatMessage keyedMessage, int totalSize)
        {
            if (!dispatcher.CheckAccess())
                dispatcher.Invoke(() => OnTextMessageReceived(keyedMessage, totalSize));
            else
            {
                var text = Encoding.Unicode.GetString(keyedMessage.Data);
                var clientId = keyedMessage.ClientId;
                logEntries.Add(new LogEntry(clientId, nickManagerService.GetNickName(clientId), DateTime.Now, text));
            }
        }

        public ObservableCollection<LogEntry> LogEntries
        {
            get { return logEntries; }
        }
    }
}
