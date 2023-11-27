using JetBrains.Annotations;
using static UnityEngine.LogLevelExtensions;

namespace UnityEngine
{
    public abstract class Target
    {
        private readonly bool[] _stackTraceLogType;

        [NotNull]
        public Formatter Formatter { get; }

        [NotNull]
        public Filterer Filterer { get; }

        protected Target() : this(null, null)
        {
        }

        protected Target([CanBeNull] Formatter formatter = null, [CanBeNull] Filterer filterer = null)
        {
            Formatter = formatter ?? new Formatter();
            Filterer = filterer ?? new Filterer(true);

            _stackTraceLogType = new bool[LogTypes.Count];
            foreach (var logType in LogTypes)
            {
                SetStackTraceEnabled(logType, true);
            }
        }

        public Target SetStackTraceEnabled(LogLevel logLevel, bool enabled)
        {
            _stackTraceLogType[(int)logLevel] = enabled;
            return this;
        }

        public bool GetStackTraceEnabled(LogLevel logLevel)
        {
            return _stackTraceLogType[(int)logLevel];
        }

        public abstract void Log(string message, [CanBeNull] string stackTrace);

        public virtual void Flush()
        {
        }
    }
}