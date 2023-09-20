using System;
using System.Threading;

namespace UnityEngine
{
    public readonly struct LogEntry
    {
        public readonly Tag Tag;
        public readonly LogLevel LogLevel;
        public readonly string String;
        public readonly Color Color;
        public readonly Object Context;

        public readonly DateTime LogTime;
        public readonly int ThreadId;
        public readonly bool IsColored;

        public LogEntry(Tag tag, LogLevel logLevel, string message, Color color, Object context)
        {
            Tag = tag;
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