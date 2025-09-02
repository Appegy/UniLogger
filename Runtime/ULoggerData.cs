using System.Collections.Generic;

namespace UnityEngine
{
    internal class ULoggerData
    {
        public List<Target> Targets { get; } = new();
        public UnityTarget UnityTarget { get; init; }
        public UnityLogger LogHandler { get; init; }
    }
}