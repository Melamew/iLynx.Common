﻿using System;
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
            RegisterResource("Resources/DataTemplates.xaml");
            Container.RegisterType<IMainMenuService, MainMenuService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IConnectionDialogViewModel, ConnectionDialogViewModel>(
                new PerResolveLifetimeManager());
            Container.RegisterInstance(Container.Resolve<ClientConnectionService>());
            var menuService = Container.Resolve<IMainMenuService>();
            menuService.RegisterMenuItem(Container.Resolve<ConnectMenuItemViewModel>()); // new MenuItemViewModel("Chatter", null, 
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.HeaderRegion, () => Container.Resolve<MainMenuViewModel>());
        }
    }
}
