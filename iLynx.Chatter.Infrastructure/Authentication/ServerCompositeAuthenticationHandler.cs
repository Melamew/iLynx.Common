using System;
using System.Linq;
using System.Text;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public class ServerCompositeAuthenticationHandler : CompositeAuthenticationHandler
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
                Data = strongest.Key.ToByteArray()
            };
            connection.Write(message);
            return strongest.Value.Authenticate(connection);
        }
    }
}