using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.Configuration;

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
            logLevelValue = configurationManager.GetValue("LogLevel", LogLevel.Information, "Logging");
            logLevel = (int)logLevelValue.Value;
            logLevelValue.ValueChanged += LogLevelValueOnValueChanged;
        }

        private void LogLevelValueOnValueChanged(object sender, ValueChangedEventArgs<object> valueChangedEventArgs)
        {
            logLevel = (int)logLevelValue.Value;
        }

        public void Log(LogLevel level, object sender, string message)
        {
            var value = (int) level;
            if (value >= logLevel)
                consoleHandler.Log("[{0}:{1}]: {2}", level.ToString()[0], null == sender ? "NOWHERE" : sender.GetType().Name, message);
        }
    }
}