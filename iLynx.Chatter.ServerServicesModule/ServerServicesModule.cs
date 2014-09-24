using iLynx.Chatter.Infrastructure;
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
        }
    }
}
