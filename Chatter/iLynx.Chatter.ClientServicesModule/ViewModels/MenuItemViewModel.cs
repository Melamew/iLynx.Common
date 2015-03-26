using System.Collections.ObjectModel;
using System.Windows.Input;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public class MenuItemViewModel : NotificationBase, IMenuItem
    {
        private readonly ObservableCollection<IMenuItem> children;
        private string title;
        private ICommand command;

        public MenuItemViewModel(string title, ICommand command, params IMenuItem[] children)
        {
            this.command = command;
            this.title = Guard.IsNull(() => title);
            this.children = null == children || children.Length < 1 ? null : new ObservableCollection<IMenuItem>(children);
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (value == title) return;
                title = value;
                OnPropertyChanged();
            }
        }

        public ICommand Command
        {
            get { return command; }
            set
            {
                if (value == command) return;
                command = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IMenuItem> Children { get { return children; } }

        public void AddChild(IMenuItem item)
        {
            children.Add(item);
        }

        public void RemoveChild(IMenuItem item)
        {
            children.Remove(item);
        }
    }
}
