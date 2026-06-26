using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal class ULoggerData
    {
        private readonly object _targetsLock = new object();
        private volatile Target[] _targets = Array.Empty<Target>();

        public UnityTarget UnityTarget { get; init; }
        public UnityLogger LogHandler { get; init; }

        public Target[] Targets => _targets;

        public bool AddTarget(Target target)
        {
            lock (_targetsLock)
            {
                var type = target.GetType();
                foreach (var existing in _targets)
                {
                    if (existing.GetType() == type) return false;
                }
                var next = new Target[_targets.Length + 1];
                Array.Copy(_targets, next, _targets.Length);
                next[_targets.Length] = target;
                _targets = next;
                return true;
            }
        }

        public Target RemoveTarget(Type type)
        {
            lock (_targetsLock)
            {
                var index = -1;
                for (var i = 0; i < _targets.Length; i++)
                {
                    if (type.IsInstanceOfType(_targets[i]))
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0) return null;
                var removed = _targets[index];
                if (_targets.Length == 1)
                {
                    _targets = Array.Empty<Target>();
                    return removed;
                }
                var next = new Target[_targets.Length - 1];
                Array.Copy(_targets, 0, next, 0, index);
                Array.Copy(_targets, index + 1, next, index, _targets.Length - index - 1);
                _targets = next;
                return removed;
            }
        }
    }
}
