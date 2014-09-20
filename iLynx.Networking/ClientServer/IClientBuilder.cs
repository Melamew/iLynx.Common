using iLynx.Networking.Interfaces;

namespace iLynx.Networking.ClientServer
{
    public interface IClientBuilder<TMessage, TKey> where TMessage : IClientMessage<TKey>
    {
        IClient<TMessage, TKey> Build(IConnectionStub<TMessage, TKey> connection);
    }
}