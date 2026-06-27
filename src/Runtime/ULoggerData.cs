using System;
using System.Collections.Generic;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal class ULoggerData
    {
        public List<Target> Targets { get; } = new List<Target>();
        public UnityTarget UnityTarget { get; init; }
        public UnityLogger LogHandler { get; init; }

        public bool AddTarget(Target target)
        {
            var type = target.GetType();
            foreach (var existing in Targets)
            {
                if (existing.GetType() == type) return false;
            }
            Targets.Add(target);
            return true;
        }

        public Target RemoveTarget(Type type)
        {
            for (var i = 0; i < Targets.Count; i++)
            {
                if (type.IsInstanceOfType(Targets[i]))
                {
                    var removed = Targets[i];
                    Targets.RemoveAt(i);
                    return removed;
                }
            }
            return null;
        }
    }
}
