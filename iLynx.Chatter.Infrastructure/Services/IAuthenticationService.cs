using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IAuthenticationService<out TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        bool IsClientAuthenticated(IClient<TMessage, TKey> client);
    }
}
