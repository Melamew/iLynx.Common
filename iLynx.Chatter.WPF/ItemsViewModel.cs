using System.Collections.ObjectModel;
using System.Windows.Input;
using iLynx.Common;
using iLynx.Common.WPF;

namespace iLynx.Chatter.WPF
{
    public class ItemsViewModel<TItem> : NotificationBase, IItemsContainer<TItem>
    {
        private readonly IDispatcher dispatcher;
        private readonly ObservableCollection<TItem> items = new ObservableCollection<TItem>();
        private ICommand removeItemCommand;
        private TItem selectedItem;

        public ItemsViewModel(IDispatcher dispatcher)
        {
            this.dispatcher = Guard.IsNull(() => dispatcher);
        }

        public void AddItem(TItem item)
        {
            dispatcher.InvokeIfRequired(x => items.Add(x), item);
        }

        public void RemoveItem(TItem item)
        {
            dispatcher.InvokeIfRequired(x => items.Remove(x), item);
        }

        public TItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (Equals(value, selectedItem)) return;
                selectedItem = value;
                OnPropertyChanged();
            }
        }
        
        public void ClearItems()
        {
            items.Clear();
        }

        public ObservableCollection<TItem> Items
        {
            get { return items; }
        }

        public ICommand RemoveItemCommand
        {
            get { return removeItemCommand ?? (removeItemCommand = new DelegateCommand<TItem>(OnRemoveItem)); }
        }

        private void OnRemoveItem(TItem item)
        {
            RemoveItem(item);
        }
    }
}
