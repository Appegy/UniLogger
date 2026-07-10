using System;
using JetBrains.Annotations;

namespace Appegy.UniLogger
{
    public sealed class MemoryTargetBuilder
    {
        private int _capacity;
        private FormatOptions _format = FormatOptions.None;
        private Filterer _filterer;

        public MemoryTargetBuilder Capacity(int capacity)
        {
            _capacity = capacity;
            return this;
        }

        public MemoryTargetBuilder Format(FormatOptions options)
        {
            _format = options;
            return this;
        }

        public MemoryTargetBuilder Filter([CanBeNull] Filterer filterer)
        {
            _filterer = filterer;
            return this;
        }

        internal InMemoryTarget Build()
        {
            if (_capacity <= 0)
            {
                throw new InvalidOperationException("Memory target requires a positive capacity. Use .Capacity(...).");
            }
            var formatter = new Formatter(_format);
            var filterer = _filterer ?? Filterer.AllowAllTags();
            return new InMemoryTarget(_capacity, formatter, filterer);
        }
    }
}
