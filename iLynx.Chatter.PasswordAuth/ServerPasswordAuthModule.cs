using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public class ServerPasswordAuthModule : ModuleBase
    {
        public ServerPasswordAuthModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            var baseHandler = Container.Resolve<IMultiAuthenticationHandler<ChatMessage, int>>();
            AddAuthenticationHandler(baseHandler);
        }

        protected virtual void AddAuthenticationHandler(IMultiAuthenticationHandler<ChatMessage, int> multiHandler)
        {
            multiHandler.AddHandler(Container.Resolve<ServerPasswordAuthenticationHandler>());
        }
    }

    public class ClientPasswordAuthModule : ModuleBase
    {
        public ClientPasswordAuthModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            var baseHandler = Container.Resolve<IMultiAuthenticationHandler<ChatMessage, int>>();
            AddAuthenticationHandler(baseHandler);
        }

        protected virtual void AddAuthenticationHandler(IMultiAuthenticationHandler<ChatMessage, int> multiHandler)
        {
            multiHandler.AddHandler(Container.Resolve<ClientPasswordAuthenticationHandler>());
        }
    }
}
