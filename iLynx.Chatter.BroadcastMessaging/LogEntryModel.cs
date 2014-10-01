using System;

namespace iLynx.Chatter.BroadcastMessaging
{
    public class LogEntryModel
    {
        public Guid SourceClient { get; private set; }
        public string Nick { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Content { get; private set; }

        public LogEntryModel(Guid sourceClient, string nick, DateTime timeStamp, string content)
        {
            SourceClient = sourceClient;
            Nick = string.IsNullOrEmpty(nick) ? sourceClient.ToString() : nick;
            TimeStamp = timeStamp;
            Content = content;
        }
    }
}