﻿using System.Windows;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.Configuration;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Common.WPF;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Cryptography;
using iLynx.Networking.Interfaces;
using iLynx.PubSub;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.Client
{
    public class ClientBootstrapper : UnityBootstrapper
    {
        private ChatterShell shell;

        protected override DependencyObject CreateShell()
        {
            return (shell = new ChatterShell());
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterType(typeof(IBus<>), typeof(QueuedBus<>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IKeyedSubscriptionManager<,>), typeof(KeyedSubscriptionManager<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IMessageServer<,>), typeof(MessageServer<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IConnectionStubBuilder<,>), typeof(CryptoConnectionStubBuilder<,>));
            Container.RegisterType(typeof(IAlgorithmContainer<>), typeof(AlgorithmContainer<>), new ContainerControlledLifetimeManager());
            Container.RegisterType<ILinkNegotiator, KeyExchangeLinkNegotiator>(new ContainerControlledLifetimeManager());
            SetupEncryptionContainers(Container);
            Container.RegisterType<IClientSideClient<ChatMessage, int>, Client<ChatMessage, int>>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMergeDictionaryService, MergeDictionaryService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDispatcher, WPFApplicationDispatcher>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowingService, WindowingService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITimerService, SingleThreadedTimerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISerializer<ChatMessage>, ChatMessageSerializer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IThreadManager, ThreadManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IBitConverter, BigEndianBitConverter>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IConfigurationManager, SingletonConfigurationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(RuntimeCommon.DefaultLogger);
        }

        private static void SetupEncryptionContainers(IUnityContainer container)
        {
            var symmetricContainer = container.Resolve<IAlgorithmContainer<ISymmetricAlgorithmDescriptor>>();
            symmetricContainer.AddAlgorithm(new AesDescriptor(256));
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