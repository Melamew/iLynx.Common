using System;
using iLynx.Common;

namespace iLynx.TestBench
{
    public class ContainerViewModel : NotificationBase
    {
        private string header;
        private object content;
        private double contentWidth;
        private double headerSize = 16;

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

        public double HeaderSize
        {
            get { return headerSize; }
            set
            {
                if (Math.Abs(value - headerSize) < double.Epsilon) return;
                headerSize = value;
                OnPropertyChanged();
            }
        }

        public double ContentWidth
        {
            get { return contentWidth; }
            set
            {
                if (Math.Abs(value - contentWidth) < double.Epsilon) return;
                contentWidth = value;
                OnPropertyChanged();
            }
        }
    }
}
