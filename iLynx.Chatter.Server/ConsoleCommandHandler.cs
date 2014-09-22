using System;
using System.Collections.Generic;

namespace iLynx.Chatter.Server
{
    public interface IConsoleCommandHandler
    {
        void RegisterCommand(string commandString, Action<string[]> callback);
        void Run();
        void Break();
    }

    public class ConsoleCommandHandler : IConsoleCommandHandler
    {
        private readonly Dictionary<string, Action<string[]>> commandHandlers = new Dictionary<string, Action<string[]>>();
        private bool exit;

        public ConsoleCommandHandler()
        {
        }

        public void RegisterCommand(string commandString, Action<string[]> callback)
        {
            if (commandHandlers.ContainsKey(commandString))
                commandHandlers[commandString] = callback;
            else
                commandHandlers.Add(commandString, callback);
        }

        public void Run()
        {
            while (!exit)
            {
                var line = Console.ReadLine();
                if (null == line) continue;
                var commandLine = line.Split(' ');
                if (0 >= commandLine.Length) continue;
                var command = commandLine[0];
                Action<string[]> handler;
                if (!commandHandlers.TryGetValue(command, out handler)) continue;
                handler.Invoke(commandLine.Slice(1, commandLine.Length - 1));
            }
        }

        public void Break()
        {
            exit = true;
        }
    }
}
