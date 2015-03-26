using System;

namespace iLynx.Chatter.Infrastructure
{
    public interface IConsoleHandler
    {
        void RegisterCommand(string commandString, Action<string[]> handler, string helpText, Func<string, string[], CommandDefinition[]> querySubCommandCallback = null);
        void Run();
        void Break();
        string Terminator { get; set; }
        string Path { get; set; }
        void WriteLine(string format, params object[] args);
        void Log(string format, params object[] args);
        void PrintCommands(CommandDefinition[] commands);
    }
}