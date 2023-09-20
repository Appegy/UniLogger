using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UnityEngine
{
    public class LoggerConfig
    {
        private const string KeyPrefix = nameof(UnityEngine) + "." + nameof(ULogger) + ".Level.";
        private readonly ConcurrentDictionary<(Tag, LogLevel), bool> _loggingLevelCache = new ConcurrentDictionary<(Tag, LogLevel), bool>();

        internal List<Target> Targets { get; } = new List<Target>();

        public Formatter UnityFormatter { get; internal set; }

        public LoggerConfig AddTarget(Target target)
        {
            Targets.Add(target);
            return this;
        }

        public bool IsLogEnabled<TLoggerType>(TLoggerType enumType, LogLevel type)
            where TLoggerType : struct, Enum
        {
            var tag = enumType.GetTag();
            return IsLogEnabled(tag, type);
        }

        public bool IsLogEnabled(string category, string name, LogLevel type)
        {
            var tag = new Tag(category, name);
            return IsLogEnabled(tag, type);
        }

        public bool IsLogEnabled(Tag tag, LogLevel type)
        {
            var key = (tag, level: type);
            if (!_loggingLevelCache.TryGetValue(key, out var supported))
            {
                supported = ReadFromPrefs(tag, type);
                _loggingLevelCache[key] = supported;
            }
            return supported;
        }

        public void SetLogEnabled<TLoggerType>(TLoggerType enumType, LogLevel type, bool enabled)
            where TLoggerType : struct, Enum
        {
            var tag = enumType.GetTag();
            SetLogEnabled(tag, type, enabled);
        }

        public void SetLogEnabled(string category, string name, LogLevel level, bool enabled)
        {
            var tag = new Tag(category, name);
            SetLogEnabled(tag, level, enabled);
        }

        public void SetLogEnabled(Tag tag, LogLevel type, bool enabled)
        {
            var key = (tag, level: type);
            if (_loggingLevelCache.TryGetValue(key, out var current) && current == enabled)
            {
                return;
            }
            _loggingLevelCache[key] = enabled;
            WriteToPrefs(tag, type, enabled);
        }

        private static bool ReadFromPrefs(Tag tag, LogLevel type)
        {
            return PlayerPrefs.GetInt(KeyPrefix + tag.ToLongString() + "." + type, 1) == 1;
        }

        private static void WriteToPrefs(Tag tag, LogLevel type, bool enabled)
        {
            PlayerPrefs.SetInt(KeyPrefix + tag.ToLongString() + "." + type, enabled ? 1 : 0);
        }
    }
}