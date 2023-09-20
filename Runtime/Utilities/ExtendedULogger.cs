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

namespace UnityEngine
{
    public static class ExtendedULogger
    {
        #region Config

        public static LoggerConfig SetUnityFormatter(this LoggerConfig config, Formatter formatter)
        {
            config.UnityFormatter = formatter;
            return config;
        }

        internal static LoggerConfig AddFileTarget(this LoggerConfig config, Formatter formatter)
        {
            const long logSizeLimit = 10 * 1024 * 1024; // 10 MB
            var logPath = $"{Application.persistentDataPath}/Logs/{Application.identifier}_{DateTime.Now:yyyyMMdd-HHmmss}.log";
            var fileTarget = new FileTarget(config, formatter, logPath, logSizeLimit, TimeSpan.FromDays(14), autoFlush: false);
            return config.AddTarget(fileTarget);
        }

        #endregion

        #region Log

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message)
        {
            logger.Log(message, default, null, 1);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Color color)
        {
            logger.Log(message, color, null, 1);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Object context)
        {
            logger.Log(message, default, context, 1);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, int lvl)
        {
            logger.Log(message, default, null, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Color color, Object context)
        {
            logger.Log(message, color, context, 1);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Color color, int lvl)
        {
            logger.Log(message, color, null, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Object context, int lvl)
        {
            logger.Log(message, default, context, lvl);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Color color, Object context, int lvl)
        {
            logger.SendLogToUnity(LogType.Log, message, color, context, lvl);
        }

        #endregion

        #region Warning

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message)
        {
            logger.LogWarning(message, default, null, 1);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Color color)
        {
            logger.LogWarning(message, color, null, 1);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Object context)
        {
            logger.LogWarning(message, default, context, 1);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, int lvl)
        {
            logger.LogWarning(message, default, null, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Color color, Object context)
        {
            logger.LogWarning(message, color, context, 1);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Color color, int lvl)
        {
            logger.LogWarning(message, color, null, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Object context, int lvl)
        {
            logger.LogWarning(message, default, context, lvl);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Color color, Object context, int lvl)
        {
            logger.SendLogToUnity(LogType.Warning, message, color, context, lvl);
        }

        #endregion

        #region LogError

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message)
        {
            logger.LogError(message, default, null, 1);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Color color)
        {
            logger.LogError(message, color, null, 1);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Object context)
        {
            logger.LogError(message, default, context, 1);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, int lvl)
        {
            logger.LogError(message, default, null, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Color color, Object context)
        {
            logger.LogError(message, color, context, 1);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Color color, int lvl)
        {
            logger.LogError(message, color, null, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Object context, int lvl)
        {
            logger.LogError(message, default, context, lvl);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Color color, Object context, int lvl)
        {
            logger.SendLogToUnity(LogType.Error, message, color, context, lvl);
        }

        #endregion

        public static void LogException(this ULogger logger, Exception exception, Object context = null)
        {
            ULogger.LogException(exception, context);
        }

        internal static void Log(this ULogger logger, LogType type, string message, Object context, int lvl)
        {
            switch (type)
            {
                case LogType.Log:
                    logger.Log(message, context, lvl);
                    break;
                case LogType.Warning:
                    logger.LogWarning(message, context, lvl);
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    logger.LogError(message, context, lvl);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}