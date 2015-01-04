using System;
using System.IO;
using System.Windows;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Crypto;
using iLynx.Common;
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

namespace iLynx.Chatter.Server
{
    public class ServerBootstrapper : UnityBootstrapper
    {
        private readonly ILoggerFacade loggerFacade = new LoggerFacade();
        protected override DependencyObject CreateShell()
        {
            return null;
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        public override void Run(bool runWithDefaultConfiguration)
        {
            base.Run(runWithDefaultConfiguration);
            var serverManager = Container.Resolve<ServerManager>();
            var pluginDir = Path.Combine(Environment.CurrentDirectory, "Plugins");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            var directoryCatalog = new DirectoryModuleCatalog
            {
                ModulePath = pluginDir
            };
            var manager = new ModuleManager(new ModuleInitializer(new UnityServiceLocator(Container), loggerFacade),
                directoryCatalog, loggerFacade);
            manager.Run();
            serverManager.Run();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            RegisterTypes();
        }

        protected override ILoggerFacade CreateLogger()
        {
            return loggerFacade;
        }

        private void RegisterTypes()
        {
            Container.RegisterType<ISerializerService, BinarySerializerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICommandHandlerRegistry, CommandHandlerRegistry>(new PerResolveLifetimeManager());
            Container.RegisterInstance<IConsoleHandler>(Container.Resolve<ConsoleInputHandler>(), new ContainerControlledLifetimeManager());
            Container.RegisterType<IConfigurationManager, SingletonConfigurationManager>(new ContainerControlledLifetimeManager());
            RuntimeCommon.DefaultLogger = Container.Resolve<ConsoleHandlerLogger>();
            Container.RegisterType(typeof(IAlgorithmContainer<>), typeof(AlgorithmContainer<>), new ContainerControlledLifetimeManager());
            Container.RegisterType<ILinkNegotiator, KeyExchangeLinkNegotiator>(new ContainerControlledLifetimeManager());
            SetupEncryptionContainers(Container);
            Container.RegisterType(typeof(IBus<>), typeof(QueuedBus<>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IKeyedSubscriptionManager<,>), typeof(KeyedSubscriptionManager<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IMessageServer<,>), typeof(MessageServer<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IConnectionStubListener<,>), typeof(CryptoConnectionStubListener<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IClientBuilder<,>), typeof(ClientBuilder<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType<ITimerService, SingleThreadedTimerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISerializer<ChatMessage>, ChatMessageSerializer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IThreadManager, ThreadManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IBitConverter, BigEndianBitConverter>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(RuntimeCommon.DefaultLogger);
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
    }
}
