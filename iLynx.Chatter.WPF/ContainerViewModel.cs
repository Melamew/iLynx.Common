using System;
using iLynx.Common;

namespace iLynx.Chatter.WPF
{
    public class ContainerViewModel : NotificationBase
    {
        private string header;
        private object content;

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
    }
}
