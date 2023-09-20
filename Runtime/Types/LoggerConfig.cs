using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UnityEngine
{
    public class LoggerConfig
    {
        private const string KeyPrefix = nameof(ULogger) + ".Tags.";
        private readonly ConcurrentDictionary<(string, LogLevel), bool> _loggingLevelCache = new ConcurrentDictionary<(string, LogLevel), bool>();

        internal List<Target> Targets { get; } = new List<Target>();

        public Formatter UnityFormatter { get; internal set; }

        public LoggerConfig AddTarget(Target target)
        {
            Targets.Add(target);
            return this;
        }

        public bool IsTagEnabled<TLoggerType>(TLoggerType tag, LogLevel type)
            where TLoggerType : struct, Enum
        {
            return IsTagEnabled(tag.GetTag(), type);
        }

        public bool IsTagEnabled(Type tag, LogLevel type)
        {
            return IsTagEnabled(tag.GetTag(), type);
        }

        public bool IsTagEnabled(string tag, LogLevel type)
        {
            var key = (tag, type);
            if (!_loggingLevelCache.TryGetValue(key, out var supported))
            {
                supported = ReadFromPrefs(tag, type);
                _loggingLevelCache[key] = supported;
            }
            return supported;
        }

        public void SetTagEnabled<TLoggerType>(TLoggerType tag, LogLevel type, bool enabled)
            where TLoggerType : struct, Enum
        {
            SetTagEnabled(tag.GetTag(), type, enabled);
        }

        public void SetTagEnabled(Type tag, LogLevel type, bool enabled)
        {
            SetTagEnabled(tag.GetTag(), type, enabled);
        }

        public void SetTagEnabled(string tag, LogLevel type, bool enabled)
        {
            var key = (tag, level: type);
            if (_loggingLevelCache.TryGetValue(key, out var current) && current == enabled)
            {
                return;
            }
            _loggingLevelCache[key] = enabled;
            WriteToPrefs(tag, type, enabled);
        }

        private static bool ReadFromPrefs(string tag, LogLevel type)
        {
            return PlayerPrefs.GetInt(KeyPrefix + tag + "." + type, 1) == 1;
        }

        private static void WriteToPrefs(string tag, LogLevel type, bool enabled)
        {
            PlayerPrefs.SetInt(KeyPrefix + tag + "." + type, enabled ? 1 : 0);
        }
    }
}