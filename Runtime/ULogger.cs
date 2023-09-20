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

        public bool IsLevelSupported(int level)
        {
            return _config.IsLevelSupported(_tag, level);
        }

        public int GetLogLevel()
        {
            return _config.GetLevel(_tag);
        }

        internal void SendLogToUnity(LogType logType, string message, Color color, Object context, int level)
        {
            if (UnityLogHandler == null)
            {
                Debug.unityLogger.Log(logType, (object)message, context);
                return;
            }
            if (!_config.IsLevelSupported(_tag, level)) return;
            var line = new LogEntry(_tag, logType, message, color, context, level);
            var formattedMessage = Config.UnityFormatter != null ? Config.UnityFormatter.Format(line) : message;

            _logsHandler = onLogReceived;
            DefaultLogger.Log(logType, (object)formattedMessage, context);
            _logsHandler = null;

            void onLogReceived(string condition, string stacktrace, LogType type)
            {
                BroadcastLog(line, stacktrace);
            }
        }

        internal void BroadcastUnobservedLog(string condition, string stacktrace, LogType type)
        {
            var line = new LogEntry(_tag, type, condition, default, null, 0);
            BroadcastLog(line, stacktrace);
        }

        private void BroadcastLog(LogEntry line, string stackTrace)
        {
            foreach (var target in Config.Targets)
            {
                try
                {
                    var targetStackTrace = target.NeedExtractStackTraceFor(line.LogType) ? stackTrace : null;
                    var (msg, st) = target.FormatLog(line, targetStackTrace);
                    target.Log(msg, st);
                }
                catch
                {
                    // just don't fail on log
                }
            }
        }
    }
}