using JetBrains.Annotations;
using static Appegy.UniLogger.LogLevelExtensions;

namespace Appegy.UniLogger
{
    public abstract class TargetBase
    {
        [NotNull]
        public Formatter Formatter { get; internal set; }

        [NotNull]
        public Filterer Filterer { get; internal set; }

        protected TargetBase()
        {
            Formatter = new Formatter();
            Filterer = new Filterer(true);
        }
    }

    public abstract class Target : TargetBase
    {
        private readonly bool[] _stackTraceLogType;

        protected Target()
        {
            _stackTraceLogType = new bool[LogTypes.Count];
            foreach (var logType in LogTypes)
            {
                SetStackTraceEnabled(logType, true);
            }
        }

        public void SetStackTraceEnabled(LogLevel logLevel, bool enabled)
        {
            _stackTraceLogType[(int)logLevel] = enabled;
        }

        public bool GetStackTraceEnabled(LogLevel logLevel)
        {
            return _stackTraceLogType[(int)logLevel];
        }

        protected internal abstract void Log(string message, [CanBeNull] string stackTrace);

        protected internal virtual void Flush()
        {
        }
    }
}