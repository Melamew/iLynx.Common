using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public interface ICompositeAuthenticationHandler<TMessage, TKey> : IAuthenticationHandler<TMessage, TKey>
        where TMessage : IKeyedMessage<TKey>
    {
        void AddHandler(IAuthenticationHandler<TMessage, TKey> handler);
        void RemoveHandler(IAuthenticationHandler<TMessage, TKey> handler);
    }
}