using System.Collections.ObjectModel;
using iLynx.Chatter.Infrastructure.Services;

namespace iLynx.Chatter.ClientServicesModule
{
    public class MainMenuService : IMainMenuService
    {
        private readonly  ObservableCollection<IMenuItem> items = new ObservableCollection<IMenuItem>(); 
        public void RegisterMenuItem(IMenuItem item)
        {
            if (items.Contains(item))
                return;
            items.Add(item);
        }

        public void RemoveMenuItem(IMenuItem item)
        {
            items.Remove(item);
        }

        public ObservableCollection<IMenuItem> MenuItems { get { return items; } }
    }
}
