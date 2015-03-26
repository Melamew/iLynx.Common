using System.Collections.ObjectModel;
using System.Windows.Input;

namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IMenuItem
    {
        string Title { get; }
        ICommand Command { get; }
        ObservableCollection<IMenuItem> Children { get; }
        void AddChild(IMenuItem item);
        void RemoveChild(IMenuItem item);
    }
}