using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal class ULoggerData
    {
        private volatile Target[] _targets = Array.Empty<Target>();

        public Target[] Targets => _targets;
        public UnityTarget UnityTarget { get; init; }
        public UnityLogger LogHandler { get; init; }
        public LogDispatcher Dispatcher { get; set; }

        public bool AddTarget(Target target)
        {
            var type = target.GetType();
            var current = _targets;
            foreach (var existing in current)
            {
                if (existing.GetType() == type) return false;
            }
            var updated = new Target[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[current.Length] = target;
            _targets = updated;
            return true;
        }

        public Target RemoveTarget(Type type)
        {
            var current = _targets;
            for (var i = 0; i < current.Length; i++)
            {
                if (type.IsInstanceOfType(current[i]))
                {
                    var removed = current[i];
                    var updated = new Target[current.Length - 1];
                    Array.Copy(current, 0, updated, 0, i);
                    Array.Copy(current, i + 1, updated, i, current.Length - i - 1);
                    _targets = updated;
                    return removed;
                }
            }
            return null;
        }
    }
}
