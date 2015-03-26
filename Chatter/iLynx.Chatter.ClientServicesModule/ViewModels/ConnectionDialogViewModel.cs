using iLynx.Chatter.Infrastructure;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public class ConnectionDialogViewModel : DialogViewModelBase, IConnectionDialogViewModel
    {
        private string remoteHost;
        private ushort remotePort;

        public ConnectionDialogViewModel()
            : base("Connect")
        {
            Height = 110;
            Width = 200;
        }

        public string RemoteHost
        {
            get { return remoteHost; }
            set
            {
                if (value == remoteHost) return;
                remoteHost = value;
                OnPropertyChanged();
            }
        }

        public ushort RemotePort
        {
            get { return remotePort; }
            set
            {
                if (value == remotePort) return;
                remotePort = value;
                OnPropertyChanged();
            }
        }
    }
}
