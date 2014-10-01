using System.Collections.ObjectModel;
using iLynx.Chatter.BroadcastMessaging;
using iLynx.Common;

namespace iLynx.TestBench
{
    public class HellViewModel : NotificationBase
    {
        private readonly ObservableCollection<ContainerViewModel> items = new ObservableCollection<ContainerViewModel>();

        public void AddContainer(ContainerViewModel container)
        {
            items.Add(container);
        }

        public void RemoveContainer(ContainerViewModel container)
        {
            items.Remove(container);
        }

        public ObservableCollection<ContainerViewModel> Items { get { return items; } }
    }
}
