using System.Collections.Generic;
using System.IO;
using System.Windows;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Crypto;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.Configuration;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Cryptography;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using iLynx.Serialization;
using iLynx.Threading;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.Client
{
    internal class LoggerFacade : ILoggerFacade
    {
        private static readonly Dictionary<Category, LogLevel> LevelMap = new Dictionary<Category, LogLevel>
        {
            {Category.Debug, LogLevel.Debug},
            {Category.Exception, LogLevel.Error},
            {Category.Info, LogLevel.Information},
            {Category.Warn, LogLevel.Warning}
        };

        public void Log(string message, Category category, Priority priority)
        {
            LogLevel level;
            if (!LevelMap.TryGetValue(category, out level))
                level = LogLevel.Critical;
            RuntimeCommon.DefaultLogger.Log(level, this, message);
        }
    }
    public class ClientBootstrapper : UnityBootstrapper
    {
        private ChatterShell shell;
        private ILoggerFacade loggerFacade;

        protected override DependencyObject CreateShell()
        {
            return (shell = new ChatterShell());
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override ILoggerFacade CreateLogger()
        {
            return (loggerFacade = new LoggerFacade());
        }

        public override void Run(bool runWithDefaultConfiguration)
        {
            base.Run(runWithDefaultConfiguration);
            if (!Directory.Exists(@".\Plugins"))
                Directory.CreateDirectory(@".\Plugins");
            var manager = new ModuleManager(new ModuleInitializer(new UnityServiceLocator(Container), loggerFacade),
                                            new DirectoryModuleCatalog { ModulePath = @".\Plugins" }, loggerFacade);
            manager.Run();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterType<ISerializerService, BinarySerializerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IBus<>), typeof(QueuedBus<>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IKeyedSubscriptionManager<,>), typeof(KeyedSubscriptionManager<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IMessageServer<,>), typeof(MessageServer<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IConnectionStubBuilder<,>), typeof(CryptoConnectionStubBuilder<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IAlgorithmContainer<>), typeof(AlgorithmContainer<>), new ContainerControlledLifetimeManager());
            Container.RegisterType<ILinkNegotiator, KeyExchangeLinkNegotiator>(new ContainerControlledLifetimeManager());
            SetupEncryptionContainers(Container);
            Container.RegisterType<IMergeDictionaryService, MergeDictionaryService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDispatcher, WPFApplicationDispatcher>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowingService, WindowingService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITimerService, SingleThreadedTimerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISerializer<ChatMessage>, ChatMessageSerializer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IThreadManager, ThreadManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IBitConverter, BigEndianBitConverter>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IConfigurationManager, SingletonConfigurationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(RuntimeCommon.DefaultLogger);
            Container.RegisterType<IClientSideClient<ChatMessage, int>, Client<ChatMessage, int>>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IClient<ChatMessage, int>>(Container.Resolve<IClientSideClient<ChatMessage, int>>(), new ContainerControlledLifetimeManager());
        }

        private static void SetupEncryptionContainers(IUnityContainer container)
        {
            var symmetricContainer = container.Resolve<IAlgorithmContainer<ISymmetricAlgorithmDescriptor>>();
            symmetricContainer.AddAlgorithm(new AesDescriptor(256));
            symmetricContainer.AddAlgorithm(new Threefish256Descriptor());
            symmetricContainer.AddAlgorithm(new Threefish512Descriptor());
            symmetricContainer.AddAlgorithm(new Threefish1024Descriptor());
            var asymmetricContainer = container.Resolve<IAlgorithmContainer<IKeyExchangeAlgorithmDescriptor>>();
            asymmetricContainer.AddAlgorithm(new RsaDescriptor(3072));
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = shell;
            Application.Current.MainWindow.Show();
        }
    }
}
