using iLynx.Networking.ClientServer;

namespace iLynx.Chatter.Infrastructure
{
    public interface IClientManager
    {
        void Manage(IMessageServer<ChatMessage, int> server);
    }
}