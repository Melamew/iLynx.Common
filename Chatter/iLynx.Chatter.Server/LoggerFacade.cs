using iLynx.Common;
using Microsoft.Practices.Prism.Logging;

namespace iLynx.Chatter.Server
{
    public class LoggerFacade : ILoggerFacade
    {
        public void Log(string message, Category category, Priority priority)
        {
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, this, message);
        }
    }
}