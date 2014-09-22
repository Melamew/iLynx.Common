using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.AuthenticationModule
{
    public interface IAuthenticationService<out TMessage, TKey> where TMessage : IKeyedMessage<TKey>
    {
        bool IsClientAuthenticated(IClient<TMessage, TKey> client);
    }
}
