using System.Collections.Generic;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Chatter.WPF;
using iLynx.Common;
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
            Container.RegisterType<IItemsContainer<ContainerViewModel>, ContainerItemsViewModel>(new PerResolveLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => Container.Resolve<ContainerItemsViewModel>());
        }
    }

    public class TabRegionService : ITabRegionService
    {
        private readonly Dictionary<int, ContainerViewModel> registrations = new Dictionary<int, ContainerViewModel>();
        private readonly IItemsContainer<ContainerViewModel> container;
        private int nextId;

        public TabRegionService(IItemsContainer<ContainerViewModel> container)
        {
            this.container = Guard.IsNull(() => container);
        }

        protected int NextId
        {
            get { return nextId++; }
        }

        public int RegisterView(object view, string header)
        {
            var viewModel = new ContainerViewModel
            {
                Content = view,
                Header = header,
            };
            var id = NextId;
            registrations.Add(id, viewModel);
            container.AddItem(viewModel);
            return id;
        }

        public void RemoveView(int view)
        {
            ContainerViewModel vm;
            if (!registrations.TryGetValue(view, out vm)) return;
            container.RemoveItem(vm);
        }

        public void ActivateView(int view)
        {
            ContainerViewModel vm;
            if (!registrations.TryGetValue(view, out vm)) return;
            container.SelectedItem = vm;
        }
    }
}
