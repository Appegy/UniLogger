using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public partial class ULogger
    {
        private readonly IReadOnlyList<string> _tags;
        private readonly LoggerConfig _config;

        internal ULogger(LoggerConfig config, IEnumerable<string> tags)
        {
            _tags = tags.Distinct().OrderBy(c => c).ToList();
            _config = config;
        }

        internal void SendLogToUnity(LogLevel logLevel, string message, Color color, Object context)
        {
            if (UnityLogHandler == null)
            {
                Debug.unityLogger.Log(logLevel.ConvertToLogType(), (object)message, context);
                return;
            }

            if (_tags.All(c => !_config.IsTagEnabled(c, logLevel)))
            {
                return;
            }

            var line = new LogEntry(_tags.Where(c => _config.IsTagEnabled(c, logLevel)), logLevel, message, color, context);
            var formattedMessage = Config.UnityFormatter != null ? Config.UnityFormatter.Format(line) : message;

            _logsHandler = onLogReceived;
            DefaultLogger.Log(logLevel.ConvertToLogType(), (object)formattedMessage, context);
            _logsHandler = null;

            void onLogReceived(string condition, string stacktrace, LogType type)
            {
                BroadcastLog(line, stacktrace);
            }
        }

        internal void BroadcastUnobservedLog(string condition, string stacktrace, LogType type)
        {
            var logLevel = type.ConvertToLogLevel();
            if (_tags.All(c => !_config.IsTagEnabled(c, logLevel)))
            {
                return;
            }
            var line = new LogEntry(_tags.Where(c => _config.IsTagEnabled(c, logLevel)), logLevel, condition, default, null);
            BroadcastLog(line, stacktrace);
        }

        private void BroadcastLog(LogEntry line, string stackTrace)
        {
            foreach (var target in Config.Targets)
            {
                try
                {
                    var formattedMessage = target.Formatter != null ? target.Formatter.Format(line) : line.String;
                    var targetStackTrace = target.GetStackTraceEnabled(line.LogLevel) ? stackTrace : null;
                    target.Log(formattedMessage, targetStackTrace);
                }
                catch
                {
                    // just don't fail on log
                }
            }
        }
    }
}