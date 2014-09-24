using System.Windows.Input;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.WPF;

namespace iLynx.Chatter.AuthenticationModule.ViewModels
{
    public class UsernamePasswordDialogViewModel : NotificationBase, IDialog
    {
        private readonly IWindowingService windowingService;
        private string username;
        private string password;
        private ICommand okCommand;
        private ICommand cancelCommand;
        private bool dialogResult;

        public UsernamePasswordDialogViewModel(IWindowingService windowingService)
        {
            this.windowingService = Guard.IsNull(() => windowingService);
        }

        public string Username
        {
            get { return username; }
            set
            {
                if (value == username) return;
                username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (value == password) return;
                password = value;
                OnPropertyChanged();
            }
        }

        public ICommand OkCommand
        {
            get { return okCommand ?? (okCommand = new DelegateCommand(OnOk)); }
        }

        private void OnOk()
        {
            dialogResult = true;
            var windowId = windowingService.FindIdByContent(this);
            windowingService.Close(windowId);
        }

        public ICommand CancelCommand
        {
            get { return cancelCommand ?? (cancelCommand = new DelegateCommand(OnCancel)); }
        }

        private void OnCancel()
        {
            dialogResult = false;
            var windowId = windowingService.FindIdByContent(this);
            windowingService.Close(windowId);
        }

        public string Title { get { return "Please authenticate yourself"; } }
        public bool Result { get { return dialogResult; } }
    }
}
