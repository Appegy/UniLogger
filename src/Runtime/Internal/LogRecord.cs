using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Appegy.UniLogger
{
    internal readonly struct LogRecord
    {
        public readonly IReadOnlyList<string> Tags;
        public readonly LogLevel LogLevel;
        public readonly string Message;
        public readonly string StackTrace;
        public readonly Color Color;
        public readonly Object Context;
        public readonly DateTime LogTime;
        public readonly int ThreadId;

        public LogRecord(IReadOnlyList<string> tags, LogLevel logLevel, string message, string stackTrace, Color color, Object context, DateTime logTime, int threadId)
        {
            Tags = tags;
            LogLevel = logLevel;
            Message = message;
            StackTrace = stackTrace;
            Color = color;
            Context = context;
            LogTime = logTime;
            ThreadId = threadId;
        }
    }
}
