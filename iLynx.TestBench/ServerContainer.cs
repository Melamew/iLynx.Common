using Microsoft.Practices.Unity;

namespace iLynx.TestBench
{
    public class ServerContainer : UnityContainer, IServerContainer
    {
        public ServerContainer()
        {
            TestApplication.SetupContainer(this);
            //var authHandler =
            //    this.Resolve<ServerPasswordAuthenticationHandler>(
            //        new DependencyOverride<HashAlgorithm>(new SHA512Cng()));
            //this.RegisterInstance<IAuthenticationHandler<ChatMessage, int>>(authHandler);
        }
    }
}