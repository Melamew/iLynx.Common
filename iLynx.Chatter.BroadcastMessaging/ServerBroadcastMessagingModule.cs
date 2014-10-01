using iLynx.Chatter.BroadcastMessaging.Client;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
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
            var tabRegionService = Container.Resolve<ITabRegionService>();
            tabRegionService.RegisterView(Container.Resolve<MessageLogViewModel>(), "Broadcast");
        }
    }
}
