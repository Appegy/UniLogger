using System.Collections.Generic;

namespace UnityEngine
{
    internal class ULoggerData
    {
        public List<Target> Targets { get; } = new List<Target>();
        public Formatter UnityFormatter { get; set; }
        public Filterer UnityFilterer { get; set; }
        public UnityLogger LogHandler { get; set; }
    }
}