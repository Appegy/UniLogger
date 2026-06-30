using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal class ULoggerData
    {
        private volatile Target[] _targets = Array.Empty<Target>();
        private volatile Target[] _syncTargets = Array.Empty<Target>();
        private volatile Target[] _asyncTargets = Array.Empty<Target>();

        public Target[] Targets => _targets;
        public Target[] SyncTargets => _syncTargets;
        public Target[] AsyncTargets => _asyncTargets;

        public ILogHandler OriginalHandler { get; init; }
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
            RebuildPartitions(updated);
            return true;
        }

        private void RebuildPartitions(Target[] targets)
        {
            var syncCount = 0;
            foreach (var target in targets)
            {
                if (target.RunSynchronously) syncCount++;
            }

            var sync = new Target[syncCount];
            var async = new Target[targets.Length - syncCount];
            var si = 0;
            var ai = 0;
            foreach (var target in targets)
            {
                if (target.RunSynchronously) sync[si++] = target;
                else async[ai++] = target;
            }
            _syncTargets = sync;
            _asyncTargets = async;
        }
    }
}
