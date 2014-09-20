using System.Net;

namespace iLynx.Networking.Interfaces
{
    public interface IListener
    {
        void BindTo(EndPoint localEndpoint);
        bool IsBound { get; }
        void Close();
    }
}