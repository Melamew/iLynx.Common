using System;
using System.Security.Cryptography;
using System.Text;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.Configuration;
using iLynx.Networking.ClientServer;
using iLynx.Networking.Interfaces;

namespace iLynx.Chatter.PasswordAuth
{
    public class ServerPasswordAuthenticationHandler : IAuthenticationHandler<ChatMessage, int>
    {
        private readonly HashAlgorithm hashAlgorithm;
        private readonly IConfigurableValue<string> passwordHashValue;
 
        public ServerPasswordAuthenticationHandler(HashAlgorithm hashAlgorithm, IConfigurationManager configurationManager)
        {
            this.hashAlgorithm = Guard.IsNull(() => hashAlgorithm);
            Guard.IsNull(() => configurationManager);
            passwordHashValue = configurationManager.GetValue("PasswordHash", ComputeHash("password", Environment.UserName));
        }

        private string ComputeHash(string password, string salt)
        {
            return Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(password + salt)));
        }

        public string Password
        {
            set { passwordHashValue.Value = ComputeHash(value, Environment.UserName); }
        }

        public bool Authenticate(IConnectionStub<ChatMessage, int> connection)
        {
            int size;
            var message = connection.ReadNext(out size);
            if (message.Key == -1) return false; // Canceled
            var passwordString = Encoding.Unicode.GetString(message.Data);
            var hash = ComputeHash(passwordString, Environment.UserName);
            var accept = hash == passwordHashValue.Value;
            connection.Write(new ChatMessage
            {
                ClientId = Guid.Empty,
                Key = accept ? 1 : 0,
            });
            return accept;
        }
    }
}
