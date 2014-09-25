using System.Collections.ObjectModel;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public class MainMenuViewModel : NotificationBase
    {
        private readonly IMainMenuService mainMenuService;

        public MainMenuViewModel(IMainMenuService mainMenuService)
        {
            this.mainMenuService = Guard.IsNull(() => mainMenuService);
        }

        public ObservableCollection<IMenuItem> MenuItems
        {
            get { return mainMenuService.MenuItems; }
        }
    }
}
