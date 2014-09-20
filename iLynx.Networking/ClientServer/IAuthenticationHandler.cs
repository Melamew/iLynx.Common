using iLynx.Networking.Interfaces;

namespace iLynx.Networking.ClientServer
{
    public interface IAuthenticationHandler<TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        bool Authenticate(IConnectionStub<TMessage, TKey> connection);
    }
}