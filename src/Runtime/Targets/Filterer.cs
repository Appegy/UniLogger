using System.Collections.Generic;
using static Appegy.UniLogger.LogLevelExtensions;

namespace Appegy.UniLogger
{
    public class Filterer
    {
        private sealed class Snapshot
        {
            public readonly bool[] LogTypeEnabled;
            public readonly HashSet<string> Tags;

            public Snapshot(bool[] logTypeEnabled, HashSet<string> tags)
            {
                LogTypeEnabled = logTypeEnabled;
                Tags = tags;
            }
        }

        private readonly bool _allTagsEnabledByDefault;
        private volatile Snapshot _snapshot;

        public Filterer(bool allTagsEnabledByDefault)
        {
            _allTagsEnabledByDefault = allTagsEnabledByDefault;
            var logTypeEnabled = new bool[LogTypes.Count];
            foreach (var logType in LogTypes)
            {
                logTypeEnabled[(int)logType] = true;
            }
            _snapshot = new Snapshot(logTypeEnabled, new HashSet<string>());
        }

        /// <summary>
        /// Returns true when <see cref="logLevel"/> and <see cref="tag"/> are not filtered and allowed to show
        /// </summary>
        public bool IsAllowed(LogLevel logLevel, string tag)
        {
            var snapshot = _snapshot;
            if (!snapshot.LogTypeEnabled[(int)logLevel]) return false;
            return _allTagsEnabledByDefault switch
            {
                true => !snapshot.Tags.Contains(tag),
                false => snapshot.Tags.Contains(tag)
            };
        }

        public Filterer SetAllowed(LogLevel logLevel, bool value)
        {
            ThreadDispatcher.EnsureMainThread(nameof(SetAllowed));
            var snapshot = _snapshot;
            var logTypeEnabled = (bool[])snapshot.LogTypeEnabled.Clone();
            logTypeEnabled[(int)logLevel] = value;
            _snapshot = new Snapshot(logTypeEnabled, snapshot.Tags);
            return this;
        }

        public Filterer SetAllowed(string tag, bool value)
        {
            ThreadDispatcher.EnsureMainThread(nameof(SetAllowed));
            var snapshot = _snapshot;
            var removeFromSet = _allTagsEnabledByDefault == value;
            if (removeFromSet ? !snapshot.Tags.Contains(tag) : snapshot.Tags.Contains(tag))
            {
                return this;
            }
            var tags = new HashSet<string>(snapshot.Tags);
            if (removeFromSet)
            {
                tags.Remove(tag);
            }
            else
            {
                tags.Add(tag);
            }
            _snapshot = new Snapshot(snapshot.LogTypeEnabled, tags);
            return this;
        }

        public Filterer Enable(LogLevel logLevel)
        {
            return SetAllowed(logLevel, true);
        }

        public Filterer Disable(LogLevel logLevel)
        {
            return SetAllowed(logLevel, false);
        }

        public Filterer Allow(string tag)
        {
            return SetAllowed(tag, true);
        }

        public Filterer Mute(string tag)
        {
            return SetAllowed(tag, false);
        }
    }
}
