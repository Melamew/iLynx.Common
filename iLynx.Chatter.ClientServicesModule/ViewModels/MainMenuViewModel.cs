using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using Microsoft.Practices.Prism.Commands;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public class MainMenuViewModel : NotificationBase
    {
        private readonly IMainMenuService mainMenuService;
        private IEnumerable<MenuItem> children;

        public MainMenuViewModel(IMainMenuService mainMenuService)
        {
            this.mainMenuService = Guard.IsNull(() => mainMenuService);
            this.mainMenuService.MenuItems.CollectionChanged += MenuItemsOnCollectionChanged;
            BuildMenu();
        }

        private void MenuItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            BuildMenu();
        }

        private void BuildMenu()
        {
            MenuItems = mainMenuService.MenuItems.Select(MakeMenuItem).ToList();
        }

        private MenuItem MakeMenuItem(IMenuItem item)
        {
            var result = new MenuItem
            {
                Header = item.Title,
                Command = new DelegateCommand(item.Callback),
            };
            foreach (var child in item.Children)
                result.Items.Add(MakeMenuItem(child));
            return result;
        }

        public IEnumerable<MenuItem> MenuItems
        {
            get { return children; }
            set
            {
                if (ReferenceEquals(value, children)) return;
                children = value;
                OnPropertyChanged();
            }
        }
    }
}
