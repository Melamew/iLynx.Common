using iLynx.Chatter.Infrastructure;

namespace iLynx.Chatter.AuthenticationModule.ViewModels
{
    public class UsernamePasswordDialogViewModel : DialogViewModelBase
    {
        private string username;
        private string password;

        public UsernamePasswordDialogViewModel()
            : base("Please authenticate yourself")
        {
            Height = 110;
            Width = 200;
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
    }
}
