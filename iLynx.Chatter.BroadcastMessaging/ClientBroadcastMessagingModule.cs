using iLynx.Chatter.Infrastructure;
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
            
        }
    }
}
