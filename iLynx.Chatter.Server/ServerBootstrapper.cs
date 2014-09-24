﻿using System;
using System.IO;
using System.Windows;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.Configuration;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;
using iLynx.Networking.TCP;
using iLynx.PubSub;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.Server
{
    public class LoggerFacade : ILoggerFacade
    {
        public void Log(string message, Category category, Priority priority)
        {
            RuntimeCommon.DefaultLogger.Log(LoggingType.Information, this, message);
        }
    }

    public class ConsoleHandlerLogger : ILogger
    {
        private readonly IConsoleHandler consoleHandler;

        public ConsoleHandlerLogger(IConsoleHandler consoleHandler)
        {
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
        }

        public void Log(LoggingType type, object sender, string message)
        {
            consoleHandler.Log("[{0}:{1}]: {2}", type.ToString()[0], null == sender ? "NOWHERE" : sender.GetType().FullName, message);
        }
    }

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
            Container.RegisterType<ICommandHandlerRegistry, CommandHandlerRegistry>(new PerResolveLifetimeManager());
            Container.RegisterInstance<IConsoleHandler>(Container.Resolve<ConsoleInputHandler>(), new ContainerControlledLifetimeManager());
            RuntimeCommon.DefaultLogger = Container.Resolve<ConsoleHandlerLogger>();

            Container.RegisterType(typeof(IBus<>), typeof(QueuedBus<>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IKeyedSubscriptionManager<,>), typeof(KeyedSubscriptionManager<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IMessageServer<,>), typeof(MessageServer<,>), new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IConnectionStubListener<,>), typeof(TcpStubListener<,>));
            Container.RegisterType(typeof(IClientBuilder<,>), typeof(ClientBuilder<,>));
            Container.RegisterType<ITimerService, SingleThreadedTimerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISerializer<ChatMessage>, ChatMessageSerializer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IThreadManager, ThreadManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IBitConverter, BigEndianBitConverter>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IConfigurationManager, SingletonConfigurationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(RuntimeCommon.DefaultLogger);
        }
    }
}
