using System.Data;
using System.Linq;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;

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
            return true;
        }

        public bool IsRegistered(string username, out User user)
        {
            user = userAdapter.Query().FirstOrDefault(x => x.Username == username);
            return null != user;
        }
    }
}
