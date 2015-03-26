using System;

namespace iLynx.Chatter.Infrastructure
{
    public class CommandDefinition
    {
        public string Command { get; set; }
        public string HelpText { get; set; }
        public Action<string[]> Callback { get; set; }
        public Func<string, string[], CommandDefinition[]> QuerySubCommand { get; set; }
    }
}