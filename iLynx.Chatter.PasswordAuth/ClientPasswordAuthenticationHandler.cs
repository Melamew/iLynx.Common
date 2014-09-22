using System;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.AuthenticationModule
{
    public static class Identifiers
    {
        public static readonly Guid PreSharedSecretAuth = RuntimeHelper.Lynxify("Simple Password Auth");

        public static readonly Guid UsernamePasswordAuth = RuntimeHelper.Lynxify("Username / Password Auth");
    }

    public class ClientPasswordAuthenticationHandler : IAuthenticationHandler<ChatMessage, int>
    {
        public bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            var dialog = new Common.WPF.Controls.PasswordDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == false)
            {
                connection.Write(new ChatMessage
                {
                    ClientId = Guid.Empty,
                    Key = -1,
                });
                return false;
            }
            connection.Write(new ChatMessage
            {
                ClientId = Guid.Empty,
                Key = 0,
                Data = Encoding.Unicode.GetBytes(dialog.PasswordText),
            });
            int size;
            var result = connection.ReadNext(out size);
            return result.Key == 1;
        }

        public int Strength { get { return (int) AuthenticationStrength.Weak; } }

        public Guid AuthenticatorId { get { return Identifiers.PreSharedSecretAuth; } }
    }
}
