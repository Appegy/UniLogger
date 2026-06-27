using System.Collections.Generic;
using static Appegy.UniLogger.LogLevelExtensions;

namespace Appegy.UniLogger
{
    public class Filterer
    {
        private readonly bool[] _logTypeEnabled;
        private readonly bool _allTagsEnabledByDefault;
        private readonly HashSet<string> _tags = new HashSet<string>();

        public Filterer(bool allTagsEnabledByDefault)
        {
            _allTagsEnabledByDefault = allTagsEnabledByDefault;
            _logTypeEnabled = new bool[LogTypes.Count];
            foreach (var logType in LogTypes)
            {
                _logTypeEnabled[(int)logType] = true;
            }
        }

        /// <summary>
        /// Returns true when <see cref="logLevel"/> and <see cref="tag"/> are not filtered and allowed to show
        /// </summary>
        public bool IsAllowed(LogLevel logLevel, string tag)
        {
            if (!_logTypeEnabled[(int)logLevel]) return false;
            return _allTagsEnabledByDefault switch
            {
                true => !_tags.Contains(tag),
                false => _tags.Contains(tag)
            };
        }

        public Filterer SetAllowed(LogLevel logLevel, bool value)
        {
            _logTypeEnabled[(int)logLevel] = value;
            return this;
        }

        public Filterer SetAllowed(string tag, bool value)
        {
            switch (_allTagsEnabledByDefault)
            {
                case true when value:
                case false when !value:
                    _tags.Remove(tag);
                    break;
                default:
                    _tags.Add(tag);
                    break;
            }

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