using JetBrains.Annotations;
using static UnityEngine.LogLevelExtensions;

namespace UnityEngine
{
    public abstract class TargetBase
    {
        [NotNull]
        public Formatter Formatter { get; }

        [NotNull]
        public Filterer Filterer { get; }

        protected TargetBase() : this(null, null)
        {
        }

        protected TargetBase([CanBeNull] Formatter formatter = null, [CanBeNull] Filterer filterer = null)
        {
            Formatter = formatter ?? new Formatter();
            Filterer = filterer ?? new Filterer(true);
        }
    }

    public abstract class Target : TargetBase
    {
        private readonly bool[] _stackTraceLogType;

        protected Target() : this(null, null)
        {
        }

        protected Target([CanBeNull] Formatter formatter = null, [CanBeNull] Filterer filterer = null) : base(formatter, filterer)
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