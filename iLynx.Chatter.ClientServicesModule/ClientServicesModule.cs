using System;
using System.Windows.Controls;
using iLynx.Chatter.ClientServicesModule.ViewModels;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.ClientServicesModule
{
    public class ClientServicesModule : ModuleBase
    {
        public ClientServicesModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            Container.RegisterType<IMainMenuService, MainMenuService>(new ContainerControlledLifetimeManager());
            var menuService = Container.Resolve<IMainMenuService>();
            menuService.RegisterMenuItem(new MenuItemViewModel("Chatter", null, new MenuItemViewModel("Connect", OnConnect)));
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.HeaderRegion, () => Container.Resolve<MainMenuViewModel>());
        }

        private void OnConnect()
        {
            
        }
    }
}
