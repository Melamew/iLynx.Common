using System.Reflection;
using System.Windows;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Crypto;
using iLynx.Chatter.ServerServicesModule;
using iLynx.Chatter.WPF;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Configuration;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Cryptography;
using iLynx.PubSub;
using iLynx.Serialization;
using iLynx.TestBench.ClientServerDemo;
using iLynx.Threading;
using Microsoft.Practices.Unity;

namespace iLynx.TestBench
{
    public class TestApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainContainer = new UnityContainer();
            SetupContainer(mainContainer);
            (new MergeDictionaryService(RuntimeCommon.DefaultLogger))
                .AddResource(RuntimeHelper.MakePackUri(Assembly.GetExecutingAssembly(), "DataTemplates.xaml"));
            mainContainer.RegisterInstance<IUnityContainer>(mainContainer);
            mainContainer.RegisterType<IClientContainer, ClientContainer>(new PerResolveLifetimeManager());
            mainContainer.RegisterType<IServerContainer, ServerContainer>(new PerResolveLifetimeManager());
            var serverContainerViewModel = new ContainerViewModel
            {
                Header = "Simple Chat Server",
                Content = mainContainer.Resolve<SimpleChatServerViewModel>()
            };
            var clientContainerViewModel = new ContainerViewModel
            {
                Header = "Simple Chat Client",
                Content = mainContainer.Resolve<ChatClientListViewModel>()
            };
            var vm = new HellViewModel();
            vm.AddContainer(serverContainerViewModel);
            vm.AddContainer(clientContainerViewModel);
            MainWindow = new MainWindow(vm);
            MainWindow.Show();
        }

        public static void SetupContainer(IUnityContainer container)
        {
            container.RegisterType<IDispatcher, WPFApplicationDispatcher>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISerializer<int>, BinaryPrimitives.Int32Serializer>(
                new ContainerControlledLifetimeManager());
            container.RegisterType<ISerializer<ChatMessage>, ChatMessageSerializer>(
                new ContainerControlledLifetimeManager());
            
            container.RegisterType(typeof(IAlgorithmContainer<>), typeof(AlgorithmContainer<>), new ContainerControlledLifetimeManager());
            container.RegisterType<ILinkNegotiator, KeyExchangeLinkNegotiator>(new ContainerControlledLifetimeManager());
            SetupEncryptionContainers(container);

            container.RegisterType<ITimerService, SingleThreadedTimerService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IChatLogViewModel, ChatLogViewModel>(new PerResolveLifetimeManager());
            container.RegisterType<IClientManager, ClientManager>(new ContainerControlledLifetimeManager());
            container.RegisterType(typeof (IClientSideClient<,>), typeof (Client<,>));
            container.RegisterType(typeof(IBus<>), typeof(Bus<>), new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IKeyedSubscriptionManager<,>), typeof(KeyedSubscriptionManager<,>), new ContainerControlledLifetimeManager());
            container.RegisterType<INickManagerService, ServerNickManagerService>(new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IClientBuilder<,>), typeof(ClientBuilder<,>));
            container.RegisterType<IThreadManager, ThreadManager>(new ContainerControlledLifetimeManager());
            container.RegisterType<IConfigurationManager, SingletonConfigurationManager>(new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IMessageServer<,>), typeof(MessageServer<,>), new PerResolveLifetimeManager());
            container.RegisterInstance(RuntimeCommon.DefaultLogger);
        }

        private static void SetupEncryptionContainers(IUnityContainer container)
        {
            var symmetricContainer = container.Resolve<IAlgorithmContainer<ISymmetricAlgorithmDescriptor>>();
            symmetricContainer.AddAlgorithm(new AesDescriptor(256));
            var asymmetricContainer = container.Resolve<IAlgorithmContainer<IKeyExchangeAlgorithmDescriptor>>();
            asymmetricContainer.AddAlgorithm(new RsaDescriptor(1024));
        }
    }
}
