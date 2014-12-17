using System;
using System.Reflection;
using iLynx.Common.Serialization;
using JetBrains.Annotations;

namespace iLynx.Common
{
    /// <summary>
    /// Contains a set of static properties that are considered "Common" inside the T0yK4T.Tools library
    /// </summary>
    public static class RuntimeCommon
    {
        private static readonly LoggingProxy Proxy = new LoggingProxy(new ConsoleLogger("Log.log"));

        /// <summary>
        /// Gets a reference to a <see cref="ILogger"/> implementation that is "common" for this runtime
        /// <para/>
        /// This property defaults to the default <see cref="ConsoleLogger"/> implementation of <see cref="ILogger"/>
        /// <remarks>
        /// Note that setting this value will not break the logging functionality for other components,
        /// <para/>
        /// the components that already have a reference to the <see cref="ILogger"/> will merely log to the
        /// <para/>
        /// new <see cref="ILogger"/> implementation
        /// </remarks>
        /// </summary>
        public static ILogger DefaultLogger
        {
            get { return Proxy; }
            set { Proxy.Logger = value; }
        }

        [StringFormatMethod("format")]
        public static void LogWarning(this object sender, string format, params object[] args)
        {
            Proxy.Log(LogLevel.Warning, sender, string.Format(format, args));
        }

        [StringFormatMethod("format")]
        public static void LogError(this object sender, string format, params object[] args)
        {
            Proxy.Log(LogLevel.Error, sender, string.Format(format, args));
        }

        public static void LogException(this object sender, Exception e, MethodBase method)
        {
            Proxy.Log(LogLevel.Critical, sender, string.Format("{0} Caught Exception: {1}", method, e));
        }

        [StringFormatMethod("format")]
        public static void LogInformation(this object sender, string format, params object[] args)
        {
            Proxy.Log(LogLevel.Information, sender, string.Format(format, args));
        }

        [StringFormatMethod("format")]
        public static void LogDebug(this object sender, string format, params object[] args)
        {
            Proxy.Log(LogLevel.Debug, sender, string.Format(format, args));
        }

        [StringFormatMethod("format")]
        public static void LogCritical(this object sender, string format, params object[] args)
        {
            Proxy.Log(LogLevel.Critical, sender, string.Format(format, args));
        }
    }
}