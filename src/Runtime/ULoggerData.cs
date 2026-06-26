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
                if (Array.IndexOf(_targets, target) >= 0) return false;
                var next = new Target[_targets.Length + 1];
                Array.Copy(_targets, next, _targets.Length);
                next[_targets.Length] = target;
                _targets = next;
                return true;
            }
        }

        public bool RemoveTarget(Target target)
        {
            lock (_targetsLock)
            {
                var index = Array.IndexOf(_targets, target);
                if (index < 0) return false;
                if (_targets.Length == 1)
                {
                    _targets = Array.Empty<Target>();
                    return true;
                }
                var next = new Target[_targets.Length - 1];
                Array.Copy(_targets, 0, next, 0, index);
                Array.Copy(_targets, index + 1, next, index, _targets.Length - index - 1);
                _targets = next;
                return true;
            }
        }
    }
}
