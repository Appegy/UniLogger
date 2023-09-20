namespace UnityEngine
{
    public partial class ULogger
    {
        private readonly Tag _tag;
        private readonly LoggerConfig _config;

        internal ULogger(Tag tag, LoggerConfig config)
        {
            _tag = tag;
            _config = config;
        }

        internal void SendLogToUnity(LogLevel logLevel, string message, Color color, Object context)
        {
            if (UnityLogHandler == null)
            {
                Debug.unityLogger.Log(logLevel.ConvertToLogType(), (object)message, context);
                return;
            }
            
            if (!_config.IsLogEnabled(_tag, logLevel))
            {
                return;
            }
            
            var line = new LogEntry(_tag, logLevel, message, color, context);
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
            var line = new LogEntry(_tag, type.ConvertToLogLevel(), condition, default, null);
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