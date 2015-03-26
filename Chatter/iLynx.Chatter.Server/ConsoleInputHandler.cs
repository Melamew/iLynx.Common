using System;
using System.Collections.Generic;
using System.Linq;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;

namespace iLynx.Chatter.Server
{
    public class ConsoleInputHandler : IConsoleHandler
    {
        private readonly ICommandHandlerRegistry registry;
        private string currentLine = string.Empty;
        private bool exit;
        private readonly Dictionary<ConsoleKey, Action> keyHandlers = new Dictionary<ConsoleKey, Action>();
        private readonly List<string> commandHistory = new List<string>();
        private int currentHistoryIndex = -1;
        private bool isExecuting;

        public ConsoleInputHandler(ICommandHandlerRegistry registry)
        {
            this.registry = Guard.IsNull(() => registry);
            keyHandlers.Add(ConsoleKey.Tab, HandleTab);
            keyHandlers.Add(ConsoleKey.Enter, HandleEnter);
            keyHandlers.Add(ConsoleKey.Backspace, HandleBackspace);
            keyHandlers.Add(ConsoleKey.Delete, HandleDelete);
            keyHandlers.Add(ConsoleKey.UpArrow, HandleUp);
            keyHandlers.Add(ConsoleKey.LeftArrow, HandleLeft);
            keyHandlers.Add(ConsoleKey.RightArrow, HandleRight);
            keyHandlers.Add(ConsoleKey.DownArrow, HandleDown);
            registry.RegisterCommand("help", OnHelp, "This help text");
        }

        private void OnHelp(string[] strings)
        {
            PrintCommands(registry.GetAllCommands().ToArray());
        }

        private void HandleDown()
        {
            if (-1 == currentHistoryIndex)
                return;
            currentHistoryIndex--;
            if (-1 == currentHistoryIndex)
            {
                currentLine = string.Empty;
                ReWriteLine();
                return;
            }
            currentLine = commandHistory[currentHistoryIndex];
            ReWriteLine();
        }

        private int PreCommandLength
        {
            get { return (Terminator + Path).Length; }
        }

        private int CurrentLineLength
        {
            get { return PreCommandLength + currentLine.Length; }
        }

        private void HandleRight()
        {
            var currentLeft = Console.CursorLeft;
            var nextLeft = currentLeft + 1;
            if (nextLeft > CurrentLineLength) return;
            var top = Console.CursorTop;
            Console.SetCursorPosition(nextLeft, top);
        }

        private void HandleLeft()
        {
            var currentLeft = Console.CursorLeft;
            var nextLeft = currentLeft - 1;
            if (PreCommandLength > nextLeft) return;
            var top = Console.CursorTop;
            Console.SetCursorPosition(nextLeft, top);
        }

        private void HandleUp()
        {
            if (currentHistoryIndex > commandHistory.Count - 2) return;
            currentHistoryIndex++;
            currentLine = commandHistory[currentHistoryIndex];
            ReWriteLine();
        }

        public void RegisterCommand(string commandString, Action<string[]> callback, string helpText = "", Func<string, string[], CommandDefinition[]> queryCallback = null)
        {
            registry.RegisterCommand(commandString, callback, helpText, queryCallback);
        }

        public void Run()
        {
            Console.WriteLine();
            Console.WriteLine();
            ReWriteLine();
            while (!exit)
            {
                var key = Console.ReadKey(true);
                HandleKeyPress(key);
            }
        }

        private void HandleTab()
        {
            SuggestAutoComplete(currentLine);
            ReWriteLine();
        }

        private void HandleEnter()
        {
            isExecuting = true;
            ExecuteCommand();
            //Console.WriteLine();
            isExecuting = false;
            ReWriteLine();
        }

        private int LineIndex
        {
            get
            {
                if (CurrentLineLength >= Console.BufferWidth)
                    return CurrentLineLength - Console.BufferWidth;
                return Console.CursorLeft - PreCommandLength;
            }
        }

        private void HandleBackspace()
        {
            var lineIndex = LineIndex;
            var removeIndex = lineIndex - 1;
            if (removeIndex < 0 || removeIndex >= currentLine.Length) return;
            currentLine = currentLine.Remove(removeIndex, 1);
            ReWriteLine();
            var top = Console.CursorTop;
            Console.SetCursorPosition(removeIndex + PreCommandLength, top);
        }

        private void ReWriteLine()
        {
            ClearLine();
            WritePrompt();
        }

        private void WritePrompt()
        {
            var line = path + terminator + currentLine;
            Console.Write(line);
            SetCursorToEnd(line);
        }

        private static void ClearLine()
        {
            var top = Console.CursorTop;
            Console.SetCursorPosition(0, top);
            Console.Write(" ".Repeat(Console.BufferWidth - 1).CombineToString());
            Console.SetCursorPosition(0, top);
        }

        private void HandleDelete()
        {
            var lineIndex = LineIndex;
            if (lineIndex >= currentLine.Length) return;
            currentLine = currentLine.Remove(lineIndex, 1);
            ReWriteLine();
            var top = Console.CursorTop;
            Console.SetCursorPosition(lineIndex + PreCommandLength, top);
        }

        private void HandleKeyPress(ConsoleKeyInfo keyInfo)
        {
            Action keyHandler;
            if (keyHandlers.TryGetValue(keyInfo.Key, out keyHandler))
            {
                keyHandler.Invoke();
                return;
            }
            currentLine += keyInfo.KeyChar;
            Console.Write(keyInfo.KeyChar);
        }

        private void ExecuteCommand()
        {
            var line = currentLine.Trim();
            currentLine = string.Empty;
            Console.WriteLine();
            // TODO: Move this down to bottom; only here for testing purposes
            var commandLine = GetCommandLine(line);
            if (registry.Execute(commandLine.Item1, commandLine.Item2))
                commandHistory.Insert(0, line);
            else
            {
                if (string.IsNullOrEmpty(commandLine.Item1)) return;
                Console.WriteLine();
                Console.WriteLine(@"Unknown Command ""{0}""", commandLine.Item1);
            }
        }

        private static Tuple<string, string[]> GetCommandLine(string line)
        {
            var commandLine = line.Split(' ');
            return new Tuple<string, string[]>(commandLine[0], commandLine.Skip(1).ToArray());
        }

        private void SuggestAutoComplete(string line)
        {
            var commandLine = GetCommandLine(line);
            var matches = registry.SuggestAutoComplete(commandLine.Item1, commandLine.Item2);
            if (0 >= matches.Length) return;
            if (matches.Length == 1)
                currentLine = matches[0].Command + " ";
            Console.WriteLine();
            PrintCommands(matches);
        }

        public void PrintCommands(CommandDefinition[] commands)
        {
            var maxLength = commands.Max(x => x.Command.Length) + 4;
            foreach (var command in commands)
            {
                Console.Write(command.Command);
                var currentLeft = Console.CursorLeft;
                var currentTop = Console.CursorTop;
                Console.SetCursorPosition(currentLeft + (maxLength - currentLeft), currentTop);
                Console.WriteLine(command.HelpText);
            }
        }

        private static void SetCursorToEnd(string line)
        {
            var currentTop = Console.CursorTop;
            var targetIndex = line.Length;
            if (targetIndex >= Console.BufferWidth)
                targetIndex = targetIndex - Console.BufferWidth;
            Console.SetCursorPosition(targetIndex, currentTop);
        }

        public void Break()
        {
            exit = true;
        }

        private string terminator = ">";
        private string path = Environment.CurrentDirectory;

        public string Terminator
        {
            get
            {
                return terminator ?? string.Empty;
            }
            set
            {
                terminator = value;
                ReWriteLine();
            }
        }

        public string Path
        {
            get
            {
                return path ?? string.Empty;
            }
            set
            {
                path = value;
                ReWriteLine();
            }
        }

        public void Log(string format, params object[] args)
        {
            WriteLine(format, args);
        }

        public void WriteLine(string format, params object[] args)
        {
            ClearLine();
            Console.WriteLine(format, args);
            if (isExecuting) return;
            WritePrompt();
        }
    }
}
