using System.Diagnostics;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Configuration;

namespace iLynx.Chatter.Server
{
    public class ConsoleHandlerLogger : ILogger
    {
        private readonly IConsoleHandler consoleHandler;
        private readonly IConfigurableValue<LogLevel> logLevelValue;
        private int logLevel;

        public ConsoleHandlerLogger(IConsoleHandler consoleHandler,
            IConfigurationManager configurationManager)
        {
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
            logLevelValue = configurationManager.GetValue("LogLevel", LogLevel.Debug, "Logging");
            logLevel = (int)logLevelValue.Value;
            logLevelValue.ValueChanged += LogLevelValueOnValueChanged;
        }

        private void LogLevelValueOnValueChanged(object sender, ValueChangedEventArgs<LogLevel> changedEventArgs)
        {
            logLevel = (int)logLevelValue.Value;
        }

        public void Log(LogLevel level, object sender, string message)
        {
            var value = (int) level;
            
            var line = string.Format("[{0}:{1}]: {2}", level.ToString()[0], null == sender ? "NOWHERE" : sender.GetType().Name, message);
            if (value >= logLevel)
                consoleHandler.Log(line);
#if DEBUG
            Trace.WriteLine(line);
#endif
        }
    }
}