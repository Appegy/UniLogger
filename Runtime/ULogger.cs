using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public partial class ULogger
    {
        private readonly IReadOnlyList<string> _tags;

        internal ULogger(IEnumerable<string> tags)
        {
            _tags = tags.Distinct().OrderBy(c => c).ToList();
        }

        internal void SendLogToUnity(LogLevel logLevel, string message, Color color, Object context)
        {
            if (Data == null)
            {
                Debug.unityLogger.Log(logLevel.ConvertToLogType(), (object)message, context);
                return;
            }

            // means it is possible to send log to Unity, catch using Application.logMessageReceivedThreaded and broadcast to other targets
            if (WillBeAllowedByFilterer(Data.UnityTarget.Filterer, logLevel, _tags))
            {
                var line = new LogEntry(_tags.Where(c => Data.UnityTarget.Filterer.IsAllowed(logLevel, c)), logLevel, message, color, context);
                var formattedMessage = Data.UnityTarget.Formatter.Format(line);

                // manually extract stack trace if stack it disabled by unity (or we are in separate thread) but some target needs it 
                var manualStacktrace = string.Empty;
                if (!ThreadDispatcher.IsMainThread ||
                    Application.GetStackTraceLogType(logLevel.ConvertToLogType()) == StackTraceLogType.None &&
                    Data.Targets.Any(c => WillBeAllowedByFilterer(c.Filterer, logLevel, _tags) && c.GetStackTraceEnabled(logLevel)))
                {
                    manualStacktrace = StackTraceUtility.ExtractStackTrace();
                }

                _logsHandler = onLogReceived;
                Data.LogHandler.Default.Log(logLevel.ConvertToLogType(), (object)formattedMessage, context);
                _logsHandler = null;

                void onLogReceived(string condition, string stacktrace, LogType type)
                {
                    if (string.IsNullOrEmpty(stacktrace))
                    {
                        stacktrace = manualStacktrace;
                    }
                    BroadcastLog(Data, _tags, logLevel, message, stacktrace, color, context);
                }
            }
            // means that this log should not be sent to Unity, but can be sent to other targets
            else
            {
                var manualStacktrace = string.Empty;
                var allowedByAnyFilterer = false;
                foreach (var target in Data.Targets.Where(c => WillBeAllowedByFilterer(c.Filterer, logLevel, _tags)))
                {
                    allowedByAnyFilterer = true;
                    if (string.IsNullOrEmpty(manualStacktrace) && target.GetStackTraceEnabled(logLevel))
                    {
                        manualStacktrace = StackTraceUtility.ExtractStackTrace();
                    }
                }
                if (allowedByAnyFilterer)
                {
                    BroadcastLog(Data, _tags, logLevel, message, manualStacktrace, color, context);
                }
            }
        }

        internal void BroadcastUnobservedLog(string message, string stacktrace, LogType type)
        {
            if (Data == null) return;
            var logLevel = type.ConvertToLogLevel();
            BroadcastLog(Data, _tags, logLevel, message, stacktrace, default, default);
        }

        private static void BroadcastLog(ULoggerData data, IReadOnlyList<string> tags, LogLevel logLevel, string message, string stackTrace, Color color, Object context)
        {
            foreach (var target in data.Targets.Where(c => WillBeAllowedByFilterer(c.Filterer, logLevel, tags)))
            {
                try
                {
                    var line = new LogEntry(tags.Where(c => target.Filterer.IsAllowed(logLevel, c)), logLevel, message, color, context);
                    var formattedMessage = target.Formatter.Format(line);
                    var targetStackTrace = target.GetStackTraceEnabled(line.LogLevel) ? stackTrace : null;
                    target.Log(formattedMessage, targetStackTrace);
                }
                catch
                {
                    // just don't fail on log
                }
            }
        }

        private static bool WillBeAllowedByFilterer(Filterer filterer, LogLevel logLevel, IEnumerable<string> tags)
        {
            return tags.Any(c => filterer.IsAllowed(logLevel, c));
        }
    }
}