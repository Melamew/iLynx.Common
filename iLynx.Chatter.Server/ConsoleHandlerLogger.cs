using iLynx.Chatter.Infrastructure;
using iLynx.Common;

namespace iLynx.Chatter.Server
{
    public class ConsoleHandlerLogger : ILogger
    {
        private readonly IConsoleHandler consoleHandler;

        public ConsoleHandlerLogger(IConsoleHandler consoleHandler)
        {
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
        }

        public void Log(LoggingType type, object sender, string message)
        {
            consoleHandler.Log("[{0}:{1}]: {2}", type.ToString()[0], null == sender ? "NOWHERE" : sender.GetType().FullName, message);
        }
    }
}