using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using iLynx.Networking.ClientServer;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public abstract class AuthModuleBase : ModuleBase
    {
        protected AuthModuleBase(IUnityContainer container)
            : base(container)
        {
        }

        protected sealed override void RegisterTypes()
        {
            Container.RegisterType<IAuthenticationHandler<ChatMessage, int>, CompositeAuthenticationHandler>(new ContainerControlledLifetimeManager());
            var handler = Container.Resolve<CompositeAuthenticationHandler>();
            Container.RegisterInstance<ICompositeAuthenticationHandler<ChatMessage, int>>(handler, new ContainerControlledLifetimeManager());
            RegisterAuthHandlers(handler);
        }

        protected abstract void RegisterAuthHandlers(ICompositeAuthenticationHandler<ChatMessage, int> compositeHandler);
    }
}
