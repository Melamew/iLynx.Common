using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public interface IConnectionDialogViewModel : IDialog
    {
        string RemoteHost { get; }
        ushort RemotePort { get; }
    }

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
