using System.Collections.ObjectModel;
using System.Windows.Input;
using iLynx.Common;
using iLynx.Common.WPF;

namespace iLynx.TestBench.TabPages
{
    public class ListViewItemViewModel : NotificationBase
    {
        private string header;
        private string item1;

        public string Header
        {
            get { return header; }
            set
            {
                if (value == header) return;
                header = value;
                OnPropertyChanged();
            }
        }

        public string Item1
        {
            get { return item1; }
            set
            {
                if (value == item1) return;
                item1 = value;
                OnPropertyChanged();
            }
        }
    }

    public class ListViewPageViewModel : NotificationBase
    {
        private ICommand addCommand;

        private readonly ObservableCollection<ListViewItemViewModel> items =
            new ObservableCollection<ListViewItemViewModel>();

        public ObservableCollection<ListViewItemViewModel> Items
        {
            get { return items; }
        }

        public ICommand AddCommand
        {
            get { return addCommand ?? (addCommand = new DelegateCommand(OnAdd)); }
        }

        private void OnAdd()
        {
            items.Add(new ListViewItemViewModel
            {
                Header = "New Entry",
                Item1 = "Content..."
            });
        }
    }
}
