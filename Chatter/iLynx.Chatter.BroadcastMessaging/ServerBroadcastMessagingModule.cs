using iLynx.Chatter.BroadcastMessaging.Server;
using iLynx.Chatter.Infrastructure;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.BroadcastMessaging
{
    public class ServerBroadcastMessagingModule : ModuleBase
    {
        public ServerBroadcastMessagingModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            Container.RegisterInstance(Container.Resolve<BroadcastMessagingService>());
        }
    }
}
