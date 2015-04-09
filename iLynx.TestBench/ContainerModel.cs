using iLynx.Common;

namespace iLynx.TestBench
{
    public class ContainerModel : NotificationBase
    {
        private object header;
        private object content;

        public object Header
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
                if (value == content) return;
                content = value;
                OnPropertyChanged();
            }
        }
    }
}
