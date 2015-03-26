using System;
using System.Linq;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;
using iLynx.Common.DataAccess;

namespace iLynx.Chatter.AuthenticationModule
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IDataAdapter<User> userAdapter;

        public UserRegistrationService(IDataAdapter<User> userAdapter,
            IPasswordHashingService passwordHashingService)
        {
            this.passwordHashingService = Guard.IsNull(() => passwordHashingService);
            this.userAdapter = Guard.IsNull(() => userAdapter);
        }

        public bool RegisterUser(string username, string password)
        {
            User user;
            if (IsRegistered(username, out user)) return false;
            long salt;
            var hash = passwordHashingService.GetPasswordHash(password, out salt);
            user = new User
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
            };
            userAdapter.Save(user);
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, this, string.Format("Inserted user {0} with password {1}", username, user.PasswordHash.ToString(":")));
            return true;
        }

        public bool IsRegistered(string username, out User user)
        {
            user = userAdapter.Query().FirstOrDefault(x => x.Username == username);
            return null != user;
        }

        public void SetPassword(User user, string password)
        {
            long salt;
            user.PasswordHash = passwordHashingService.GetPasswordHash(password, out salt);
            user.PasswordSalt = salt;
            userAdapter.SaveOrUpdate(user);
        }
    }
}
