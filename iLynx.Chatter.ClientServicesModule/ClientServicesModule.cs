using System;
using System.Net;
using System.Windows.Controls;
using iLynx.Chatter.ClientServicesModule.ViewModels;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.PubSub;
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
            Container.RegisterInstance(Container.Resolve<ClientConnectionService>());
            var menuService = Container.Resolve<IMainMenuService>();
            menuService.RegisterMenuItem(new MenuItemViewModel("Chatter", null, new MenuItemViewModel("Connect", OnConnect)));
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.HeaderRegion, () => Container.Resolve<MainMenuViewModel>());
        }

        private void OnConnect()
        {
            var windowingService = Container.Resolve<IWindowingService>();
            var dialog = Container.Resolve<ConnectionDialogViewModel>();
            var result = windowingService.ShowDialog(dialog);
            if (!result) return;
            var commandBus = Container.Resolve<IBus<IApplicationCommand>>();
            commandBus.Publish(new ConnectCommand(GetEndpoint(dialog.RemoteHost, dialog.RemotePort)));
        }

        private EndPoint GetEndpoint(string host, ushort port)
        {
            IPAddress address;
            if (!IPAddress.TryParse(host, out address))
                return new DnsEndPoint(host, port);
            return new IPEndPoint(address, port);
        }

    }
}
