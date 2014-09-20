using System;
using System.Collections.ObjectModel;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;

namespace iLynx.TestBench.ClientServerDemo
{
    public interface IChatLogViewModel
    {
        ObservableCollection<LogEntryModel> LogEntries { get; }
    }

    public class ChatLogViewModel : NotificationBase, IChatLogViewModel
    {
        private readonly IDispatcher dispatcher;
        private readonly INickManagerService nickManagerService;
        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager;
        private readonly ObservableCollection<LogEntryModel> logEntries;

        public ChatLogViewModel(
            IDispatcher dispatcher,
            IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> subscriptionManager,
            INickManagerService nickManagerService)
        {
            this.dispatcher = dispatcher;
            this.nickManagerService = Guard.IsNull(() => nickManagerService);
            this.subscriptionManager = Guard.IsNull(() => subscriptionManager);
            this.subscriptionManager.Subscribe(MessageKeys.TextMessage, OnTextMessageReceived);
            logEntries = new ObservableCollection<LogEntryModel>();
        }

        public override void Dispose()
        {
            base.Dispose();
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
                logEntries.Add(new LogEntryModel(clientId, nickManagerService.GetNickName(clientId), DateTime.Now, text));
            }
        }

        public ObservableCollection<LogEntryModel> LogEntries
        {
            get { return logEntries; }
        }
    }
}
