using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public class ClientPasswordAuthModule : AuthModuleBase
    {
        public ClientPasswordAuthModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterAuthHandlers(ICompositeAuthenticationHandler<ChatMessage, int> compositeHandler)
        {
            compositeHandler.AddHandler(Container.Resolve<ServerPasswordAuthenticationHandler>());
        }
    }
}