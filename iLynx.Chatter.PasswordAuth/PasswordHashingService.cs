using System;
using System.Linq;
using System.Security.Cryptography;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;
using iLynx.Configuration;
using iLynx.Serialization;

namespace iLynx.Chatter.AuthenticationModule
{
    public class PasswordHashingService : IPasswordHashingService
    {
        private readonly IBitConverter bitConverter = BinarySerializerService.SingletonBitConverter;
        private readonly IConfigurableValue<int> keySizeValue;
        private readonly RandomNumberGenerator cryptoRng = new RNGCryptoServiceProvider();

        public PasswordHashingService(IConfigurationManager configurationManager)
        {
            Guard.IsNull(() => configurationManager);
            keySizeValue = configurationManager.GetValue("PasswordHashSize", 1024, "Server");
        }

        public byte[] GetPasswordHash(string password, out long salt)
        {
            var saltBytes = new byte[sizeof (long)];
            cryptoRng.GetBytes(saltBytes);
            var derriveBytes = new Rfc2898DeriveBytes(password, saltBytes);
            var bytes = derriveBytes.GetBytes(keySizeValue.Value);
            salt = bitConverter.ToInt64(saltBytes);
            return bytes;
        }

        public bool PasswordMatches(string password, User user)
        {
            var saltBytes = bitConverter.GetBytes(user.PasswordSalt);
            var derriveBytes = new Rfc2898DeriveBytes(password, saltBytes);
            var bytes = derriveBytes.GetBytes(keySizeValue.Value);
            var storedHash = user.PasswordHash;
            RuntimeCommon.DefaultLogger.Log(LogLevel.Debug, this, string.Format("Comparing Hashes:\r\n{0}\r\n{1}", storedHash.ToString(":"), bytes.ToString(":")));
            return bytes.SequenceEqual(storedHash);
        }
    }
}