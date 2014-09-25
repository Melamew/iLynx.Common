using System.Collections.ObjectModel;

namespace iLynx.TestBench.ClientServerDemo
{
    public interface IChatLogViewModel
    {
        ObservableCollection<LogEntryModel> LogEntries { get; }
    }
}