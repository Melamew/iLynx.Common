using System.Collections.ObjectModel;

namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IMainMenuService
    {
        void RegisterMenuItem(IMenuItem item);
        void RemoveMenuItem(IMenuItem item);
        ObservableCollection<IMenuItem> MenuItems { get; } 
    }
}
