using System;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.PubSub;

namespace iLynx.TestBench.ClientServerDemo
{
    public class ServerClientViewModel : NotificationBase
    {
        private readonly INickManagerService nickManager;
        private readonly IBus<IApplicationEvent> applicationEventBus;

        public ServerClientViewModel(IBus<IApplicationEvent> applicationEventBus,
            INickManagerService nickManager)
        {
            this.nickManager = nickManager;
            this.applicationEventBus = Guard.IsNull(() => applicationEventBus);
            this.applicationEventBus.Subscribe<NickChangedEvent>(OnNickChanged);
        }

        private void OnNickChanged(NickChangedEvent message)
        {
            if (message.ClientId != id) return;
            Nick = nickManager.GetNickName(id);
        }

        public override void Dispose()
        {
            base.Dispose();
            applicationEventBus.Unsubscribe<NickChangedEvent>(OnNickChanged);
        }

        private Guid id;
        private string nick;
        public Guid Id
        {
            get { return id; }
            set
            {
                if (value == id) return;
                id = value;
                Nick = nickManager.GetNickName(id);
                OnPropertyChanged();
            }
        }

        public string Nick
        {
            get
            {
                return nick;
            }
            set
            {
                if (value == nick) return;
                nick = value;
                OnPropertyChanged();
            }
        }
    }
}