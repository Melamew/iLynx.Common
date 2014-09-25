using System.Windows;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.WPF;
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
            Container.RegisterType<IMergeDictionaryService, MergeDictionaryService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDispatcher, WPFApplicationDispatcher>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowingService, WindowingService>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(RuntimeCommon.DefaultLogger);
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = shell;
            Application.Current.MainWindow.Show();
        }
    }
}
