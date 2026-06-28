using System;

namespace Appegy.UniLogger
{
    public class InMemoryTarget : Target
    {
        private readonly char[] _buffer;
        private readonly int _capacity;
        private readonly object _gate = new();
        private int _start;
        private int _count;

        public InMemoryTarget(int capacity, Formatter formatter = null, Filterer filterer = null)
            : base(formatter, filterer)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            _capacity = capacity;
            _buffer = new char[capacity];
        }

        protected internal override void Log(string message, string stackTrace)
        {
            lock (_gate)
            {
                Append(message);
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    Append("\n");
                    Append(stackTrace);
                }
                Append("\n");
            }
        }

        protected internal override void LogException(Exception exception, string message)
        {
            Log(message, null);
        }

        public string GetContent()
        {
            lock (_gate)
            {
                if (_count == 0) return string.Empty;
                var firstChunk = Math.Min(_count, _capacity - _start);
                if (firstChunk == _count)
                {
                    return new string(_buffer, _start, _count);
                }
                var result = new char[_count];
                Array.Copy(_buffer, _start, result, 0, firstChunk);
                Array.Copy(_buffer, 0, result, firstChunk, _count - firstChunk);
                return new string(result);
            }
        }

        public void Clear()
        {
            lock (_gate)
            {
                _start = 0;
                _count = 0;
            }
        }

        private void Append(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var length = text.Length;
            if (length >= _capacity)
            {
                text.CopyTo(length - _capacity, _buffer, 0, _capacity);
                _start = 0;
                _count = _capacity;
                return;
            }

            var writePos = (_start + _count) % _capacity;
            var firstChunk = Math.Min(length, _capacity - writePos);
            text.CopyTo(0, _buffer, writePos, firstChunk);
            if (firstChunk < length)
            {
                text.CopyTo(firstChunk, _buffer, 0, length - firstChunk);
            }

            var newCount = _count + length;
            if (newCount > _capacity)
            {
                _start = (_start + (newCount - _capacity)) % _capacity;
                _count = _capacity;
            }
            else
            {
                _count = newCount;
            }
        }
    }
}
