using System;
using System.Threading;

namespace UnityEngine
{
    public readonly struct LogEntry
    {
        public readonly Tag Tag;
        public readonly LogType LogType;
        public readonly string String;
        public readonly Color Color;
        public readonly Object Context;
        public readonly int Level;

        public readonly DateTime LogTime;
        public readonly int ThreadId;
        public readonly bool IsColored;

        public LogEntry(Tag tag, LogType logType, string message, Color color, Object context, int level)
        {
            Tag = tag;
            LogType = logType;
            String = message;
            Color = color;
            Context = context;
            Level = level;

            LogTime = DateTime.Now;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            IsColored = color != default;
        }
    }
}