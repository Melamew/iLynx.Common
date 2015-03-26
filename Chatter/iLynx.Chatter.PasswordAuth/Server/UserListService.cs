using System.Collections.Generic;
using System.Linq;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;
using iLynx.Common.DataAccess;

namespace iLynx.Chatter.AuthenticationModule.Server
{
    public class UserListService
    {
        private readonly IUserRegistrationService userRegistrationService;
        private readonly IConsoleHandler consoleHandler;
        private readonly ICommandHandlerRegistry commandRegistry;
        private readonly IDataAdapter<User> userAdapter;

        public UserListService(
            IConsoleHandler consoleHandler,
            ICommandHandlerRegistry commandRegistry,
            IDataAdapter<User> userAdapter,
            IUserRegistrationService userRegistrationService)
        {
            this.userRegistrationService = Guard.IsNull(() => userRegistrationService);
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
            this.commandRegistry = Guard.IsNull(() => commandRegistry);
            this.userAdapter = Guard.IsNull(() => userAdapter);
            consoleHandler.RegisterCommand("user", OnUserCommand, "Commands relating to users", QuerySubCommandCallback);
            commandRegistry.RegisterCommand("list", OnListUsers, "Lists all users");
            commandRegistry.RegisterCommand("register", OnRegisterUser, "Registers a new user. register {username} {password}");
            commandRegistry.RegisterCommand("passwd", OnChangeUserPassword, "Change the users password. passwd {username} {newpassword}");
            commandRegistry.RegisterCommand("permissions", OnListPermissions, "Lists the permissions of the specified user");
        }

        private string GetUsername(IEnumerable<string> source)
        {
            return source.FirstOrDefault();
        }

        private void OnListPermissions(string[] obj)
        {
            var userName = GetUsername(obj);
            var user = userAdapter.GetFirst(x => x.Username == userName);
            if (null == user)
            {
                consoleHandler.WriteLine("Could not find a user with username {0}", userName);
                return;
            }
            var perms = user.Permissions;
            if (null == perms)
            {
                consoleHandler.WriteLine("User {0} does not have any permissions", userName);
                return;
            }
            consoleHandler.WriteLine("User {0} has the following permissions:", userName);
            foreach (var permission in user.Permissions)
                consoleHandler.WriteLine("  {0}", permission.PermissionIdentifier);
        }

        private void OnChangeUserPassword(string[] obj)
        {
            var userName = obj.FirstOrDefault();
            var password = obj.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return;
            User user;
            if (!userRegistrationService.IsRegistered(userName, out user))
            {
                consoleHandler.WriteLine("User {0} does not exist", userName);
                return;
            }
            userRegistrationService.SetPassword(user, password);
        }

        private void OnRegisterUser(string[] strings)
        {
            var userName = strings.FirstOrDefault();
            var password = strings.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return;
            User user;
            if (userRegistrationService.IsRegistered(userName, out user))
            {
                consoleHandler.WriteLine("User {0} is already registered", userName);
                return;
            }
            consoleHandler.WriteLine(userRegistrationService.RegisterUser(userName, password)
                ? "User {0} has been registered"
                : "User registration failed", userName);
        }

        private void OnListUsers(string[] strings)
        {
            foreach (var user in userAdapter.GetAll())
                consoleHandler.WriteLine("{0}, {1}", user.UniqueId, user.Username);
        }

        private CommandDefinition[] QuerySubCommandCallback(string s, string[] parameters)
        {
            return commandRegistry.SuggestAutoComplete(s, parameters);
        }

        private void OnUserCommand(string[] strings)
        {
            commandRegistry.Execute(strings[0], strings.Skip(1).ToArray());
        }
    }
}
