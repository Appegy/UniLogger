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

namespace UnityEngine
{
    public static class ExtendedULogger
    {
        #region Trace

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(this ULogger logger, string message)
        {
            logger.Trace(message, default, null);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(this ULogger logger, string message, Color color)
        {
            logger.Trace(message, color, null);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(this ULogger logger, string message, Object context)
        {
            logger.Trace(message, default, context);
        }

#if !ULOGGER_TRACE_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Trace(this ULogger logger, string message, Color color, Object context)
        {
            logger.SendLogToUnity(LogLevel.Trace, message, color, context);
        }

        #endregion

        #region Log

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message)
        {
            logger.Log(message, default, null);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Color color)
        {
            logger.Log(message, color, null);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Object context)
        {
            logger.Log(message, default, context);
        }

#if !ULOGGER_LOGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void Log(this ULogger logger, string message, Color color, Object context)
        {
            logger.SendLogToUnity(LogLevel.Log, message, color, context);
        }

        #endregion

        #region Warning

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message)
        {
            logger.LogWarning(message, default, null);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Color color)
        {
            logger.LogWarning(message, color, null);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Object context)
        {
            logger.LogWarning(message, default, context);
        }

#if !ULOGGER_WARNINGS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogWarning(this ULogger logger, string message, Color color, Object context)
        {
            logger.SendLogToUnity(LogLevel.Warning, message, color, context);
        }

        #endregion

        #region LogError

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message)
        {
            logger.LogError(message, default, null);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Color color)
        {
            logger.LogError(message, color, null);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Object context)
        {
            logger.LogError(message, default, context);
        }

#if !ULOGGER_ERRORS_ON
        [System.Diagnostics.Conditional("ULOGGER_INTERNAL_FALSE")]
#endif
        public static void LogError(this ULogger logger, string message, Color color, Object context)
        {
            logger.SendLogToUnity(LogLevel.Error, message, color, context);
        }

        #endregion

        public static void LogException(this ULogger logger, Exception exception, Object context = null)
        {
            ULogger.LogException(exception, context);
        }
    }
}