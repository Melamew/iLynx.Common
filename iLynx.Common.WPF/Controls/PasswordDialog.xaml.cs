using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using iLynx.Common.WPF.Annotations;

namespace iLynx.Common.WPF.Controls
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : INotifyPropertyChanged
    {
        private string passwordText;

        public PasswordDialog()
        {
            InitializeComponent();
        }

        public string PasswordText
        {
            get { return passwordText; }
            set
            {
                if (value == passwordText) return;
                passwordText = value;
                OnPropertyChanged();
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
