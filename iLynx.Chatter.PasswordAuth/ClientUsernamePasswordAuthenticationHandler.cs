using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLynx.Chatter.Infrastructure;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.AuthenticationModule
{
    public class ClientUsernamePasswordAuthenticationHandler : IAuthenticationHandler<ChatMessage, int>
    {
        public bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            
        }

        public int Strength { get { return (int) AuthenticationStrength.Reasonable; } }
        public Guid AuthenticatorId { get { return Identifiers.PreSharedSecretAuth} }
    }
}
