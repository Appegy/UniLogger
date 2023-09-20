#define ULOGGER_LOGS_ON
#define ULOGGER_WARNINGS_ON
#define ULOGGER_ERRORS_ON

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
using System.Reflection;
using System.Threading.Tasks;

namespace UnityEngine
{
    public partial class ULogger
    {
        public static readonly LoggerConfig Config = new LoggerConfig();

        private static readonly Dictionary<Tag, ULogger> _loggers = new Dictionary<Tag, ULogger>();
        private static readonly ULogger _unsortedLogger = ULogger.GetLogger(Tags.Unsorted);

        private static Action<string, string, LogType> _logsHandler;

        internal static ILogger DefaultLogger { get; private set; }
        internal static ILogHandler UnityLogHandler { get; private set; }
        internal static ILogHandler CurrentLogHandler { get; private set; }

        public static LoggerConfig Initialize()
        {
            // prepare default loggers and swap unity logger to custom
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

        public static ULogger GetLogger<TLoggerTag>(TLoggerTag enumValue)
            where TLoggerTag : struct, Enum
        {
            var tag = enumValue.GetTag();
            return GetLogger(tag.Category, tag.Name);
        }

        public static ULogger GetLogger(Type name)
        {
            return GetLogger(Tags.Default, name);
        }

        public static ULogger GetLogger(string category, Type type)
        {
            var attr = type.GetCustomAttribute<LoggerTagNameAttribute>(true);
            var name = attr != null ? attr.Name : type.Name;
            return GetLogger(category, name);
        }

        public static ULogger GetLogger(string name)
        {
            return GetLogger(Tags.Default, name);
        }

        public static ULogger GetLogger(string category, string name)
        {
            var tag = new Tag(category, name);
            if (!_loggers.TryGetValue(tag, out var logger))
            {
                logger = new ULogger(tag, Config);
                _loggers[tag] = logger;
            }
            return logger;
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
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, lvl);
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
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, Color color, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, color, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, Object context, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, context, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).Log(message, color, context, lvl);
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
        public static void Log(string message, int lvl)
        {
            _unsortedLogger.Log(message, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Color color, Object context)
        {
            _unsortedLogger.Log(message, color, context);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Color color, int lvl)
        {
            _unsortedLogger.Log(message, color, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Object context, int lvl)
        {
            _unsortedLogger.Log(message, context, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(string message, Color color, Object context, int lvl)
        {
            _unsortedLogger.Log(message, color, context, lvl);
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
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, lvl);
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
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, Color color, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, color, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, Object context, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, context, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogWarning(message, color, context, lvl);
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
        public static void LogWarning(string message, int lvl)
        {
            _unsortedLogger.LogWarning(message, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Color color, Object context)
        {
            _unsortedLogger.LogWarning(message, color, context);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Color color, int lvl)
        {
            _unsortedLogger.LogWarning(message, color, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Object context, int lvl)
        {
            _unsortedLogger.LogWarning(message, context, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(string message, Color color, Object context, int lvl)
        {
            _unsortedLogger.LogWarning(message, color, context, lvl);
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
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, lvl);
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
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, Color color, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, color, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, Object context, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, context, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError<TLoggerTag>(TLoggerTag enumValue, string message, Color color, Object context, int lvl)
            where TLoggerTag : struct, Enum
        {
            GetLogger(enumValue).LogError(message, color, context, lvl);
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
        public static void LogError(string message, int lvl)
        {
            _unsortedLogger.LogError(message, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Color color, Object context)
        {
            _unsortedLogger.LogError(message, color, context);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Color color, int lvl)
        {
            _unsortedLogger.LogError(message, color, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Object context, int lvl)
        {
            _unsortedLogger.LogError(message, context, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(string message, Color color, Object context, int lvl)
        {
            _unsortedLogger.LogError(message, color, context, lvl);
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