using System;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.TestBench.ClientServerDemo
{
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
    }
}
