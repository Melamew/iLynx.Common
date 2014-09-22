using System;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.Infrastructure.Authentication
{
    public class ClientCompositeAuthenticationHandler : CompositeAuthenticationHandler
    {
        public override bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            var list = ReceiveAuthenticatorList(connection);
            var matches = FindCommonHandlers(list);
            SendAuthenticatorList(connection, matches);
            int size;
            var message = connection.ReadNext(out size);
            if (null == message) return false;
            var handlerId = new Guid(message.Data);
            var handler = GetHandler(handlerId);
            return null != handler && handler.Authenticate(connection);
        }
    }
}