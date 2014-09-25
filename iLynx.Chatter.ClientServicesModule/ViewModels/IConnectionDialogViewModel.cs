using iLynx.Chatter.Infrastructure.Services;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public interface IConnectionDialogViewModel : IDialog
    {
        string RemoteHost { get; }
        ushort RemotePort { get; }
    }
}