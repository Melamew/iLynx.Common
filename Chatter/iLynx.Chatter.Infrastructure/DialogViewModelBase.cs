using System;
using System.Windows.Input;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using Microsoft.Practices.Prism.Commands;

namespace iLynx.Chatter.Infrastructure
{
    public abstract class DialogViewModelBase : NotificationBase, IDialog
    {
        private string title;
        private bool? result;
        private ICommand okCommand;
        private ICommand cancelCommand;

        protected DialogViewModelBase(string title)
        {
            this.title = title;
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (value == title) return;
                title = value;
                OnPropertyChanged();
            }
        }

        public bool Result
        {
            get
            {
                return result ?? false;
            }
            private set
            {
                result = value;
                OnPropertyChanged();
                OnResultReceived();
            }
        }

        public double Width { get; set; }
        public double Height { get; set; }

        protected virtual void OnResultReceived()
        {
            var handler = ResultReceived;
            if (null == handler) return;
            handler.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ResultReceived;

        public ICommand OkCommand
        {
            get { return okCommand ?? (okCommand = new DelegateCommand(OnOk)); }
        }

        public ICommand CancelCommand
        {
            get { return cancelCommand ?? (cancelCommand = new DelegateCommand(OnCancel)); }
        }

        private void OnCancel()
        {
            Result = false;
        }

        private void OnOk()
        {
            Result = true;
        }
    }
}