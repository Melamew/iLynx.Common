using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.ServerServicesModule
{
    public class ServerServicesModule : ModuleBase
    {
        public ServerServicesModule(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            Container.RegisterInstance<INickManagerService>(Container.Resolve<ServerNickManagerService>(),
                new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IUserPermissionService>(Container.Resolve<UserPermissionService>(), new ContainerControlledLifetimeManager());
        }
    }
}
