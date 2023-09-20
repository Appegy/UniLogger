using System;

namespace UnityEngine
{
    // TODO work still in progress
    internal class InMemoryTarget : Target
    {
        private readonly char[] _buffer;
        private readonly int _sizeInBytes;
        private int _curPos;
        private int _bufferLength;

        public InMemoryTarget(int sizeInBytes, Formatter formatter = null, Filterer filterer = null)
            : base(formatter, filterer)
        {
            _buffer = new char[sizeInBytes];
            _sizeInBytes = sizeInBytes;
        }

        public override void Log(string message, string stackTrace)
        {
            Write(message);
            if (!string.IsNullOrEmpty(stackTrace))
            {
                Write("\r\n");
                Write(stackTrace);
            }
            Write("\r\n");
        }

        private void Write(string content)
        {
            var len = content.Length;

            lock (_buffer)
            {
                var outOfScope = Math.Max(0, _curPos + len - _sizeInBytes);
                if (outOfScope > 0)
                {
                    if (outOfScope <= _sizeInBytes)
                    {
                        var inScope = len - outOfScope;

                        content.CopyTo(0, _buffer, _curPos, inScope);
                        content.CopyTo(inScope, _buffer, 0, outOfScope);
                        _curPos = outOfScope;
                        _bufferLength = _sizeInBytes;
                    }
                    else
                    {
                        content.CopyTo(len - _sizeInBytes, _buffer, 0, _sizeInBytes);
                        _curPos = _sizeInBytes;
                    }
                    _bufferLength = _sizeInBytes;
                }
                else
                {
                    content.CopyTo(0, _buffer, _curPos, len);
                    _curPos += len;
                    _bufferLength = Math.Max(_bufferLength, _curPos);
                }
            }
        }

        public string GetContent()
        {
            lock (_buffer)
            {
                var firstPartLength = _bufferLength - _curPos;
                if (firstPartLength > 0)
                {
                    return new string(_buffer, _curPos, _bufferLength - _curPos) + new string(_buffer, 0, _curPos);
                }
                else
                {
                    return new string(_buffer, 0, _curPos);
                }
            }
        }
    }
}