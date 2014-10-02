using iLynx.Chatter.Client.ViewModels;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.WPF;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.Client
{
    public class ClientShellModule : ModuleBase
    {
        public ClientShellModule(IUnityContainer container)
            : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            RegisterResource("DataTemplates.xaml");
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => Container.Resolve<ContainerItemsViewModel>());
            regionManager.RegisterViewWithRegion(RegionNames.StatusRegion, () => Container.Resolve<ConnectionStatusViewModel>());
        }
    }
}
