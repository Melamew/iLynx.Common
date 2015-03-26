using System.Windows;

namespace iLynx.Chatter.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new ClientBootstrapper().Run(true);
        }
    }
}
