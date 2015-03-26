using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
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
        private readonly IBus<IApplicationEvent> applicationEventBus;
        private readonly IDispatcher dispatcher;
        private readonly IBus<MessageEnvelope<ChatMessage, int>> messageBus;
        private readonly FlowDocument messageLog;

        private readonly IKeyedSubscriptionManager<int, MessageReceivedHandler<ChatMessage, int>> messageSubscriptionManager;

        private readonly Dictionary<Guid, List<Run>> missingNicks = new Dictionary<Guid, List<Run>>();
        private readonly INickManagerService nickManagerService;
        private string commandLine;
        private Guid clientId;
        private ICommand sendCommand;

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
            messageLog = new FlowDocument();
            Subscribe();
        }

        public ICommand SubmitCommand
        {
            get { return sendCommand ?? (sendCommand = new DelegateCommand(OnSend)); }
        }

        public FlowDocument MessageLog
        {
            get { return messageLog; }
        }

        public string CommandLine
        {
            get { return commandLine; }
            set
            {
                if (value == commandLine) return;
                commandLine = value;
                OnPropertyChanged();
            }
        }

        private Paragraph MakeBlock(LogEntry entry)
        {
            return new Paragraph(new Span
            {
                Inlines =
                {
                    MakeNickInline(entry),
                    new Run(entry.Content)
                },
            })
            {
                TextAlignment = entry.IsSelf ? TextAlignment.Left : TextAlignment.Right,
                LineHeight = 1.25
            };
        }

        private Inline MakeNickInline(LogEntry entry)
        {
            var haveNick = entry.HaveNick;
            var client = entry.SourceClient;
            var run = new Run(MakeNickString(haveNick ? entry.Nick : client.ToString()));
            if (haveNick) return new Bold(run);
            List<Run> lst;
            if (!missingNicks.TryGetValue(client, out lst))
                missingNicks.Add(client, lst = new List<Run>());
            lst.Add(run);
            return new Bold(run);
        }

        private static string MakeNickString(string nick)
        {
            return string.Format("[{0}]: ", nick);
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
            dispatcher.Invoke(msg =>
            {
                List<Run> lst;
                if (!missingNicks.TryGetValue(msg.ClientId, out lst)) return;
                var copy = lst.ToArray();
                foreach (var r in copy)
                    r.Text = MakeNickString(message.Nick);
                lst.RemoveAll(copy.Contains);
            }, message);
        }

        private void OnTextMessageReceived(ChatMessage message, int totalSize)
        {
            dispatcher.InvokeIfRequired(
                msg =>
                    messageLog.Blocks.Add(
                        MakeBlock(new LogEntry(msg.ClientId, nickManagerService.GetNickName(msg.ClientId), DateTime.Now,
                            Encoding.Unicode.GetString(msg.Data), message.ClientId == clientId))),
                message);
        }

        private async void OnSend()
        {
            string str = commandLine;
            if (string.IsNullOrEmpty(str)) return;
            await messageBus.PublishAsync(new MessageEnvelope<ChatMessage, int>(new TextMessage(str)));
            CommandLine = string.Empty;
        }
    }
}