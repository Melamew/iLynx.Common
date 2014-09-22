using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public class AuthenticationModule : ModuleBase
    {
        public AuthenticationModule(IUnityContainer container)
            : base(container)
        {
            
        }

        protected override void RegisterTypes()
        {
            Container.RegisterInstance<IAuthenticationService<ChatMessage, int>>(
                Container.Resolve<AuthenticationService>());
            Container.RegisterInstance(Container.Resolve<CredentialAuthenticationService>());
        }
    }
}
