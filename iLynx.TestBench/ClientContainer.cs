using Microsoft.Practices.Unity;

namespace iLynx.TestBench
{
    public class ClientContainer : UnityContainer, IClientContainer
    {
        public ClientContainer()
        {
            TestApplication.SetupContainer(this);
            //this.RegisterType<IAuthenticationHandler<ChatMessage, int>, ClientPasswordAuthenticationHandler>(new ContainerControlledLifetimeManager());
        }
    }
}