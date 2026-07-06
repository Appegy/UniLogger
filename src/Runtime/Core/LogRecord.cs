using System;

namespace Appegy.UniLogger
{
    internal readonly struct LogRecord
    {
        public readonly LogEntry Entry;
        public readonly string StackTrace;
        public readonly Exception Exception;
        public readonly bool NativeBroadcast;

        public LogRecord(in LogEntry entry, string stackTrace, bool nativeBroadcast = false)
        {
            Entry = entry;
            StackTrace = stackTrace;
            Exception = null;
            NativeBroadcast = nativeBroadcast;
        }

        public LogRecord(Exception exception, in LogEntry entry)
        {
            Exception = exception;
            Entry = entry;
            StackTrace = null;
            NativeBroadcast = false;
        }
    }
}
