using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using iLynx.Common;
using iLynx.Common.WPF;
using iLynx.TestBench.TabPages;
using Microsoft.Practices.Unity;

namespace iLynx.TestBench
{
    public class TestApplication : Application
    {
        private static ICommand shutdownCommand;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainContainer = new UnityContainer();
            SetupContainer(mainContainer);
            (new MergeDictionaryService(RuntimeCommon.DefaultLogger))
                .AddResource(RuntimeHelper.MakePackUri(Assembly.GetExecutingAssembly(), "DataTemplates.xaml"));
            mainContainer.RegisterInstance<IUnityContainer>(mainContainer);
            var vm = new ShellViewModel();
            vm.AddPage(new ContainerModel
            {
                Content = new TreeViewPageViewModel(),
                Header = "Tree View"
            });
            vm.AddPage(new ContainerModel
            {
                Content = new ExpanderPageViewModel(),
                Header = "Expanders"
            });
            vm.AddPage(new ContainerModel
            {
                Content = new ListViewPageViewModel(),
                Header = "List View",
            });
            MainWindow = new MainWindow(vm);
            MainWindow.Show();
        }

        public static void SetupContainer(IUnityContainer container)
        {
            container.RegisterType<IDispatcher, WPFApplicationDispatcher>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(RuntimeCommon.DefaultLogger);
        }

        public static ICommand ShutdownCommand
        {
            get { return shutdownCommand ?? (shutdownCommand = new DelegateCommand(OnShutdown)); }
        }

        private static void OnShutdown()
        {
            if (null == Current) return;
            Current.Shutdown();
        }
    }
}
