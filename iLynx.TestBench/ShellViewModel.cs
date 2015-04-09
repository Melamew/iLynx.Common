using System.Collections.ObjectModel;
using iLynx.Common;

namespace iLynx.TestBench
{
    public class ShellViewModel : NotificationBase
    {
        private readonly ObservableCollection<ContainerModel> 
            tabPages = new ObservableCollection<ContainerModel>();

        private ContainerModel selectedItem;

        public void AddPage(ContainerModel model)
        {
            tabPages.Add(model);
            if (tabPages.Count == 1)
                SelectedItem = model;
        }

        public ContainerModel SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value == selectedItem) return;
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        public void RemovePage(ContainerModel model)
        {
            tabPages.Remove(model);
        }

        public ObservableCollection<ContainerModel> TabPages
        {
            get { return tabPages; }
        }
    }
}
