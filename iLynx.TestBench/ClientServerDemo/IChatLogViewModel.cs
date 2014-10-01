using System.Collections.ObjectModel;
using iLynx.Chatter.BroadcastMessaging;

namespace iLynx.TestBench.ClientServerDemo
{
    public interface IChatLogViewModel
    {
        ObservableCollection<LogEntry> LogEntries { get; }
    }
}