using iLynx.Chatter.AuthenticationModule.Server;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public class ServerAuthenticationModule : ModuleBase
    {
        public ServerAuthenticationModule(IUnityContainer container)
            : base(container)
        {
            
        }

        protected override void RegisterTypes()
        {
            Container.RegisterType<IPasswordHashingService, PasswordHashingService>(
                new ContainerControlledLifetimeManager());
            Container.RegisterType<IUserRegistrationService, UserRegistrationService>(
                new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IAuthenticationService<ChatMessage, int>>(
                Container.Resolve<CredentialAuthenticationService>());
            Container.RegisterInstance(Container.Resolve<UserListService>());
        }
    }
}
