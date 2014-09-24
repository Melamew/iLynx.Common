using System;
using System.Collections.Generic;

namespace iLynx.Chatter.Infrastructure
{
    public interface ICommandHandlerRegistry
    {
        CommandDefinition[] SuggestAutoComplete(string text, params string[] parameters);
        bool Execute(string command, string[] parameters);
        void RegisterCommand(string commandString, Action<string[]> handler, string helpText, Func<string, string[], CommandDefinition[]> querySubCommandCallback = null);
        IEnumerable<CommandDefinition> GetAllCommands();
    }
}