using System.Collections.ObjectModel;
using System.Windows.Input;
using iLynx.Common;
using iLynx.Common.WPF;
using Microsoft.Practices.Unity;

namespace iLynx.TestBench.ClientServerDemo
{
    public class ChatClientListViewModel : NotificationBase
    {
        private readonly IUnityContainer container;
        private readonly ObservableCollection<SimpleChatClientViewModel> clients = new ObservableCollection<SimpleChatClientViewModel>();
        private ICommand addClientCommand;
        private ICommand removeClientCommand;
        private SimpleChatClientViewModel selectedClient;

        public ChatClientListViewModel(IUnityContainer container)
        {
            this.container = Guard.IsNull(() => container);
            clients.Add(container.Resolve<SimpleChatClientViewModel>());
        }

        public ObservableCollection<SimpleChatClientViewModel> Clients
        {
            get { return clients; }
        }

        public SimpleChatClientViewModel SelectedClient
        {
            get { return selectedClient; }
            set
            {
                if (value == selectedClient) return;
                selectedClient = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveClientCommand
        {
            get { return removeClientCommand ?? (removeClientCommand = new DelegateCommand(OnRemoveClient)); }
        }

        private void OnRemoveClient()
        {
            var client = SelectedClient;
            if (null == client) return;
            clients.Remove(client);
            client.Dispose();
        }

        public ICommand AddClientCommand
        {
            get { return addClientCommand ?? (addClientCommand = new DelegateCommand(OnAddClient)); }
        }

        private void OnAddClient()
        {
            var client = container.Resolve<SimpleChatClientViewModel>();
            Clients.Add(client);
            SelectedClient = client;
        }
    }
}
