using System.Collections.Generic;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Chatter.WPF;
using iLynx.Common;

namespace iLynx.Chatter.ClientServicesModule
{
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