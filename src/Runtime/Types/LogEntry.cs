using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Appegy.UniLogger
{
    public readonly struct LogEntry
    {
        public readonly IReadOnlyList<string> Tags;
        public readonly LogLevel LogLevel;
        public readonly string String;
        public readonly Color Color;
        public readonly Object Context;

        public readonly DateTime LogTime;
        public readonly int ThreadId;
        public readonly bool IsColored;

        public LogEntry(IReadOnlyList<string> tags, LogLevel logLevel, string message, Color color, Object context)
            : this(tags, logLevel, message, color, context, DateTime.Now, Thread.CurrentThread.ManagedThreadId)
        {
        }

        internal LogEntry(IReadOnlyList<string> tags, LogLevel logLevel, string message, Color color, Object context, DateTime logTime, int threadId)
        {
            Tags = tags;
            LogLevel = logLevel;
            String = message;
            Color = color;
            Context = context;

            LogTime = logTime;
            ThreadId = threadId;
            IsColored = color != default;
        }
    }
}