using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Networking.ClientServer;

namespace iLynx.Chatter.Client.ViewModels
{
    public class ConnectionStatusViewModel : NotificationBase
    {
        private readonly IClient<ChatMessage, int> client;

        public ConnectionStatusViewModel(IClient<ChatMessage, int> client)
        {
            this.client = client;
        }
    }
}
