#define ULOGGER_TRACE_ON
#define ULOGGER_LOGS_ON
#define ULOGGER_WARNINGS_ON
#define ULOGGER_ERRORS_ON

#if ULOGGER_TRACE_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_TRACE_ON
#endif
#if ULOGGER_LOGS_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_LOGS_ON
#endif
#if ULOGGER_WARNINGS_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_WARNINGS_ON
#endif
#if ULOGGER_ERRORS_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_ERRORS_ON
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UnityEngine
{
    public partial class ULogger
    {
        public static readonly LoggerConfig Config = new LoggerConfig();

#if ULOGGER_DISABLE_ALL_LOGS
        private static readonly ULogger _disabledLogger = new ULogger(Config, Tags.Disabled);
#endif

        private static readonly ULogger _unsortedLogger = ULogger.GetLogger(Tags.Unsorted);
        private static Action<string, string, LogType> _logsHandler;

        internal static ILogger DefaultLogger { get; private set; }
        internal static ILogHandler UnityLogHandler { get; private set; }
        internal static ILogHandler CurrentLogHandler { get; private set; }

        public static LoggerConfig Initialize(Formatter unityFormatter)
        {
            // prepare default loggers and swap unity logger to custom
            Config.UnityFormatter = unityFormatter;
            DefaultLogger = new Logger(Debug.unityLogger.logHandler);
            UnityLogHandler = Debug.unityLogger.logHandler;
            CurrentLogHandler = new UnityLogger();
            Debug.unityLogger.logHandler = CurrentLogHandler;

            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            return Config;
        }

        public static void RestoreUnityLogger()
        {
            if (UnityLogHandler != null)
            {
                Debug.unityLogger.logHandler = UnityLogHandler;
            }
            UnityLogHandler = null;
            CurrentLogHandler = null;

            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        }

        private static void OnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
        {
            if (_logsHandler != null)
            {
                _logsHandler(condition, stacktrace, type);
            }
            else
            {
                _unsortedLogger.BroadcastUnobservedLog(condition, stacktrace, type);
            }
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DefaultLogger.LogException(e.Exception);
            Flush();
        }

        public static void Flush()
        {
            foreach (var target in Config.Targets)
            {
                target.Flush();
            }
        }

        #region GetLogger

        public static ULogger GetLogger<T>(T tag)
            where T : struct, Enum
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(Config, tag.GetTag().AsEnumerable());
#endif
        }

        public static ULogger GetLogger(Type tag)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(Config, tag.GetTag().AsEnumerable());
#endif
        }

        public static ULogger GetLogger(string tag)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(Config, tag.AsEnumerable());
#endif
        }

        public static ULogger GetLogger(params object[] tags)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(Config, tags.Select(c => c.GetTag()));
#endif
        }

        #endregion

        #region Trace

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace<TLoggerTag>(TLoggerTag enumValue, string message)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Trace(message);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace<TLoggerTag>(TLoggerTag enumValue, string message, Color color)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Trace(message, color);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace<TLoggerTag>(TLoggerTag enumValue, string message, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Trace(message, context);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Trace(message, color, context);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(string message)
        {
            _unsortedLogger.Trace(message);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(string message, Color color)
        {
            _unsortedLogger.Trace(message, color);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(string message, Object context)
        {
            _unsortedLogger.Trace(message, context);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(string message, Color color, Object context)
        {
            _unsortedLogger.Trace(message, color, context);
        }

        #endregion

        #region Log

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, Color color)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, color);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, context);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, color, context);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message)
        {
            _unsortedLogger.Log(message);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Color color)
        {
            _unsortedLogger.Log(message, color);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Object context)
        {
            _unsortedLogger.Log(message, context);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Color color, Object context)
        {
            _unsortedLogger.Log(message, color, context);
        }

        #endregion

        #region LogWarning

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, Color color)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, color);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, context);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, color, context);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message)
        {
            _unsortedLogger.LogWarning(message);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Color color)
        {
            _unsortedLogger.LogWarning(message, color);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Object context)
        {
            _unsortedLogger.LogWarning(message, context);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Color color, Object context)
        {
            _unsortedLogger.LogWarning(message, color, context);
        }

        #endregion

        #region LogError

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, Color color)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, color);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, context);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, color, context);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message)
        {
            _unsortedLogger.LogError(message);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Color color)
        {
            _unsortedLogger.LogError(message, color);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Object context)
        {
            _unsortedLogger.LogError(message, context);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Color color, Object context)
        {
            _unsortedLogger.LogError(message, color, context);
        }

        #endregion

        #region LogException

        public static void LogException(Exception exception, Object context = null)
        {
            if (UnityLogHandler != null)
            {
                UnityLogHandler.LogException(exception, context);
            }
            else
            {
                Debug.unityLogger.logHandler.LogException(exception, context);
            }
        }

        #endregion
    }
}