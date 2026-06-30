using System;
using JetBrains.Annotations;
using static Appegy.UniLogger.LogLevelExtensions;

namespace Appegy.UniLogger
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
                _stackTraceLogType[(int)logType] = true;
            }
        }

        public virtual bool RunSynchronously => false;

        public void SetStackTraceEnabled(LogLevel logLevel, bool enabled)
        {
            ThreadDispatcher.EnsureMainThread(nameof(SetStackTraceEnabled));
            _stackTraceLogType[(int)logLevel] = enabled;
        }

        public bool GetStackTraceEnabled(LogLevel logLevel)
        {
            return _stackTraceLogType[(int)logLevel];
        }

        protected internal abstract void Log(in LogEntry entry, [CanBeNull] string stackTrace);

        protected internal abstract void LogException([NotNull] Exception exception, in LogEntry entry);

        protected internal virtual void Flush()
        {
        }
    }
}