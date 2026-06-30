using System;

namespace Appegy.UniLogger
{
    internal readonly struct LogRecord
    {
        public readonly LogEntry Entry;
        public readonly string StackTrace;
        public readonly Exception Exception;

        public LogRecord(in LogEntry entry, string stackTrace)
        {
            Entry = entry;
            StackTrace = stackTrace;
            Exception = null;
        }

        public LogRecord(Exception exception, in LogEntry entry)
        {
            Exception = exception;
            Entry = entry;
            StackTrace = null;
        }
    }
}
