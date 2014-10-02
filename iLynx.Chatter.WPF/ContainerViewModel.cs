using iLynx.Common;

namespace iLynx.Chatter.WPF
{
    public class ContainerViewModel : NotificationBase
    {
        private string header;
        private object content;
        private bool canClose = true;

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

        public object Content
        {
            get { return content; }
            set
            {
                if (value == content)
                    return;
                content = value;
                OnPropertyChanged();
            }
        }

        public bool CanClose
        {
            get { return canClose; }
            set
            {
                if (value == canClose)
                    return;
                canClose = value;
                OnPropertyChanged();
            }
        }
    }
}
