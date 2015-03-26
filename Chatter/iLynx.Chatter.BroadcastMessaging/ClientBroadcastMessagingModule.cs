using iLynx.Chatter.BroadcastMessaging.Client;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.BroadcastMessaging
{
    public class ClientBroadcastMessagingModule : ModuleBase
    {
        public ClientBroadcastMessagingModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            RegisterResource("DataTemplates.xaml");
            var tabRegionService = Container.Resolve<ITabRegionService>();
            var id = tabRegionService.RegisterView(Container.Resolve<MessageLogViewModel>(), "Broadcast");
            tabRegionService.ActivateView(id);
        }
    }
}
