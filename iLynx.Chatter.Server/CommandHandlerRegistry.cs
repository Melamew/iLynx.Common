using System;
using System.Collections.Generic;
using System.Linq;
using iLynx.Chatter.Infrastructure;

namespace iLynx.Chatter.Server
{
    public class CommandHandlerRegistry : ICommandHandlerRegistry
    {
        private readonly List<CommandDefinition> commandHandlers = new List<CommandDefinition>();
        public CommandDefinition[] SuggestAutoComplete(string text, params string[] parameters)
        {
            var handlers = commandHandlers.Where(x => x.Command.StartsWith(text)).ToArray();
            if (1 != handlers.Length) return handlers;
            var handler = handlers[0];
            if (null == handler.QuerySubCommand)
                return handlers;
            if (handler.Command != text)
                return handlers;
            parameters = null == parameters || parameters.Length < 1 ? new[] { "" } : parameters;
            return handler.QuerySubCommand.Invoke(parameters[0], parameters.Skip(1).ToArray())
                .Select(definition => new CommandDefinition
                {
                    Command = handler.Command + " " + definition.Command,
                    Callback = definition.Callback,
                    HelpText = definition.HelpText,
                    QuerySubCommand = definition.QuerySubCommand
                })
                .ToArray();
        }

        public bool Execute(string command, string[] parameters)
        {
            var definition = commandHandlers.FirstOrDefault(x => x.Command == command);
            if (null == definition) return false;
            definition.Callback.Invoke(parameters);
            return true;
        }

        public void RegisterCommand(string commandString, Action<string[]> callback, string helpText, Func<string, string[], CommandDefinition[]> querySubCommandCallback = null)
        {
            var existing = commandHandlers.FirstOrDefault(x => x.Command == commandString);
            if (null != existing)
                commandHandlers.Remove(existing);
            commandHandlers.Add(new CommandDefinition
            {
                Callback = callback,
                Command = commandString,
                HelpText = helpText,
                QuerySubCommand = querySubCommandCallback,
            });
        }

        public IEnumerable<CommandDefinition> GetAllCommands()
        {
            return commandHandlers.AsReadOnly();
        }
    }
}