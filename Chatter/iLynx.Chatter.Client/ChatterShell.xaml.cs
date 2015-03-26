using System.ComponentModel;
using iLynx.Chatter.Infrastructure.Events;
using iLynx.PubSub;

namespace iLynx.Chatter.Client
{
    /// <summary>
    /// Interaction logic for ChatterShell.xaml
    /// </summary>
    public partial class ChatterShell
    {
        private IBus<IApplicationEvent> applicationEventBus;

        public ChatterShell()
        {
            InitializeComponent();
        }

        public IBus<IApplicationEvent> ApplicationEventBus
        {
            set { applicationEventBus = value; }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (null == applicationEventBus) return;
            applicationEventBus.Publish(new ShutdownEvent());
        }
    }
}
