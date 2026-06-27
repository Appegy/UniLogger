using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal class ULoggerData
    {
        public UnityTarget UnityTarget { get; init; }
        public UnityLogger LogHandler { get; init; }
        public Target[] Targets { get; init; } = Array.Empty<Target>();
    }
}
