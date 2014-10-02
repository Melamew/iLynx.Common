using System;
using iLynx.Common;

namespace iLynx.Chatter.BroadcastMessaging
{
    public class LogEntry : NotificationBase
    {
        public Guid SourceClient { get; private set; }
        public string Nick { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Content { get; private set; }
        public bool HaveNick { get; private set; }
        public bool IsSelf { get; private set; }

        public LogEntry(Guid sourceClient, string nick, DateTime timeStamp, string content, bool isSelf)
        {
            SourceClient = sourceClient;
            HaveNick = !string.IsNullOrEmpty(nick);
            Nick = HaveNick ? nick : sourceClient.ToString();
            TimeStamp = timeStamp;
            Content = content;
            IsSelf = isSelf;
        }

        public void SetNickname(string nickName)
        {
            Nick = nickName;
            RaisePropertyChanged(() => Nick);
        }
    }
}