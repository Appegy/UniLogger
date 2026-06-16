using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityEngine
{
    public readonly struct LogEntry
    {
        public readonly IEnumerable<string> Tags;
        public readonly LogLevel LogLevel;
        public readonly string String;
        public readonly Color Color;
        public readonly Object Context;

        public readonly DateTime LogTime;
        public readonly int ThreadId;
        public readonly bool IsColored;

        public LogEntry(IEnumerable<string> tags, LogLevel logLevel, string message, Color color, Object context)
        {
            Tags = tags;
            LogLevel = logLevel;
            String = message;
            Color = color;
            Context = context;

            LogTime = DateTime.Now;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            IsColored = color != default;
        }
    }
}