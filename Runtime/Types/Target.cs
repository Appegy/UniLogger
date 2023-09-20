using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace UnityEngine
{
    public abstract class Target
    {
        private static readonly Dictionary<LogLevel, bool> _stackTraceLogType = new Dictionary<LogLevel, bool>();

        [NotNull]
        public LoggerConfig Config { get; }

        [CanBeNull]
        public Formatter Formatter { get; }

        protected Target([NotNull] LoggerConfig config, [CanBeNull] Formatter formatter)
        {
            Config = config;
            Formatter = formatter;
            foreach (var logType in Enum.GetValues(typeof(LogLevel)).OfType<LogLevel>())
            {
                SetStackTraceEnabled(logType, true);
            }
        }

        public Target SetStackTraceEnabled(LogLevel logLevel, bool enabled)
        {
            _stackTraceLogType[logLevel] = enabled;
            return this;
        }

        public bool GetStackTraceEnabled(LogLevel logLevel)
        {
            return _stackTraceLogType[logLevel];
        }

        public abstract void Log(string message, [CanBeNull] string stackTrace);

        public virtual void Flush()
        {
        }
    }
}