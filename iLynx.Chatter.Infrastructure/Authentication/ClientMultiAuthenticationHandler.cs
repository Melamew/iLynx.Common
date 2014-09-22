using System.Text;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public class ClientMultiAuthenticationHandler : MultiAuthenticationHandler
    {
        public override bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            var list = ReceiveAuthenticatorList(connection);
            var matches = FindCommonHandlers(list);
            SendAuthenticatorList(connection, matches);
            int size;
            var message = connection.ReadNext(out size);
            if (null == message) return false;
            var handlerId = Encoding.Unicode.GetString(message.Data);
            var handler = GetHandler(handlerId);
            return null != handler && handler.Authenticate(connection);
        }
    }
}