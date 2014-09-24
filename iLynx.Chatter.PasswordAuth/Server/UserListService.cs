using System;
using System.Linq;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;

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
            this.userRegistrationService = userRegistrationService;
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
            this.commandRegistry = Guard.IsNull(() => commandRegistry);
            this.userAdapter = Guard.IsNull(() => userAdapter);
            consoleHandler.RegisterCommand("user", OnUserCommand, "Commands relating to users", QuerySubCommandCallback);
            commandRegistry.RegisterCommand("list", OnListUsers, "Lists all users matching the specified criteria");
            commandRegistry.RegisterCommand("register", OnRegisterUser, "Registers a new user, ie. register {username} {password}");
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
            //consoleHandler.PrintCommands(commandRegistry.GetAllCommands()
            //    .Select(x => new CommandDefinition
            //    {
            //        Command = "user " + x.Command,
            //        HelpText = x.HelpText,
            //        Callback = x.Callback,
            //        QuerySubCommand = x.QuerySubCommand
            //    }).ToArray());
        }
    }
}
