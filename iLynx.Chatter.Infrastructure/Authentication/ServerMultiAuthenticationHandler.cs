using System;
using System.Linq;
using System.Text;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public class ServerMultiAuthenticationHandler : MultiAuthenticationHandler
    {
        public override bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            SendAuthenticatorList(connection, GetAllHandlers());
            var remoteAuthMethods = ReceiveAuthenticatorList(connection);
            var strongest = FindCommonHandlers(remoteAuthMethods).OrderByDescending(x => x.Value.Strength).FirstOrDefault();
            if (null == strongest.Value)
                return false;
            var message = new ChatMessage
            {
                Key = MessageKeys.Authentication,
                ClientId = Guid.Empty,
                Data = Encoding.Unicode.GetBytes(strongest.Key)
            };
            connection.Write(message);
            return strongest.Value.Authenticate(connection);
        }
    }
}