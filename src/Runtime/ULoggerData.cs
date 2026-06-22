using System.Collections.Generic;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal class ULoggerData
    {
        public List<Target> Targets { get; } = new();
        public UnityTarget UnityTarget { get; init; }
        public UnityLogger LogHandler { get; init; }
    }
}