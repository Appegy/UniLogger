using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UnityEngine
{
    public class LoggerConfig
    {
        private const string KeyPrefix = nameof(UnityEngine) + "." + nameof(ULogger) + ".Level.";
        private readonly ConcurrentDictionary<Tag, int> _loggingLevelCache = new ConcurrentDictionary<Tag, int>();

        internal List<Target> Targets { get; } = new List<Target>();

        public Formatter UnityFormatter { get; internal set; }

        public LoggerConfig AddTarget(Target target)
        {
            Targets.Add(target);
            return this;
        }

        public int GetLevel<TLoggerType>(TLoggerType type)
            where TLoggerType : struct, Enum
        {
            return GetLevel(typeof(TLoggerType).Name, type.ToString());
        }

        public int GetLevel(string tag, string type)
        {
            var category = new Tag(tag, type);
            return GetLevel(category);
        }

        public void SetLoggingLevel<TLoggerType>(TLoggerType type, int level)
            where TLoggerType : struct, Enum
        {
            SetLoggingLevel(new Tag(typeof(TLoggerType).Name, type.ToString()), level);
        }

        public void SetLoggingLevel(string tag, string type, int level)
        {
            SetLoggingLevel(new Tag(tag, type), level);
        }

        internal bool IsLevelSupported(Tag tag, int level)
        {
            return level <= GetLevel(tag);
        }

        internal int GetLevel(Tag tag)
        {
            try
            {
                if (!_loggingLevelCache.TryGetValue(tag, out var level))
                {
                    level = ReadLevelFromPrefs(tag);
                    _loggingLevelCache[tag] = level;
                }
                return level;
            }
            catch
            {
                return int.MaxValue;
            }
        }

        internal void SetLoggingLevel(Tag tag, int level)
        {
            if (_loggingLevelCache.TryGetValue(tag, out var cachedLevel) && cachedLevel == level)
            {
                return;
            }
            _loggingLevelCache[tag] = level;
            WriteLevelToPrefs(tag, level);
        }

        private static int ReadLevelFromPrefs(Tag tag)
        {
            return PlayerPrefs.GetInt(KeyPrefix + tag.ToLongString(), 1);
        }

        private static void WriteLevelToPrefs(Tag tag, int level)
        {
            PlayerPrefs.SetInt(KeyPrefix + tag.ToLongString(), level);
        }
    }
}