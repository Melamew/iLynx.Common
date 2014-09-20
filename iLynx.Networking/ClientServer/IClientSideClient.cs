using System.Net;

namespace iLynx.Networking.ClientServer
{
    public interface IClientSideClient<in TMessage, TKey> : IClient<TMessage, TKey> where TMessage : IClientMessage<TKey>
    {
        void Connect(EndPoint remoteEndPoint);
    }
}