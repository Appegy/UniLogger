using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Appegy.UniLogger
{
    public partial class ULogger
    {
        private readonly IReadOnlyList<string> _tags;

        internal ULogger(string tag)
        {
            _tags = new[] { tag };
        }

        internal ULogger(IEnumerable<string> tags)
        {
            _tags = tags.Distinct().OrderBy(c => c).ToList();
        }

        [HideInCallstack]
        internal void SendLog(LogLevel logLevel, string message, Color color, Object context)
        {
            var data = Data;
            if (data == null)
            {
                Debug.unityLogger.Log(logLevel.ConvertToLogType(), (object)message, context);
                return;
            }

            var entry = new LogEntry(_tags, logLevel, message, color, context, DateTime.Now, Thread.CurrentThread.ManagedThreadId);
            var stackTrace = NeedsStackTrace(data, logLevel, _tags) ? CaptureStackTrace() : null;

            DeliverToSyncTargets(data, in entry, stackTrace);

            if (data.AsyncTargets.Length > 0)
            {
                data.Dispatcher.Enqueue(new LogRecord(in entry, stackTrace));
            }
        }

        internal void BroadcastUnobservedLog(string message, string stacktrace, LogType type)
        {
            var data = Data;
            if (data == null || data.AsyncTargets.Length == 0) return;
            var logLevel = type.ConvertToLogLevel();
            var entry = new LogEntry(_tags, logLevel, message, default, default, DateTime.Now, Thread.CurrentThread.ManagedThreadId);
            data.Dispatcher.Enqueue(new LogRecord(in entry, stacktrace));
        }

        internal static void DispatchException(Exception exception, Object context)
        {
            if (exception == null) return;
            var data = Data;
            if (data == null)
            {
                Debug.unityLogger.logHandler.LogException(exception, context);
                return;
            }

            exception = UnwrapException(exception);
            var message = UnityExceptionFormatter.Format(exception);
            var entry = new LogEntry(null, LogLevel.Error, message, default, context, DateTime.Now, Thread.CurrentThread.ManagedThreadId);

            var syncTargets = data.SyncTargets;
            for (var i = 0; i < syncTargets.Length; i++)
            {
                try
                {
                    syncTargets[i].LogException(exception, entry);
                }
                catch
                {
                    // just don't fail on log
                }
            }

            if (data.AsyncTargets.Length > 0)
            {
                data.Dispatcher.Enqueue(new LogRecord(exception, in entry));
            }
        }

        private static bool NeedsStackTrace(ULoggerData data, LogLevel logLevel, IReadOnlyList<string> tags)
        {
            var targets = data.Targets;
            for (var i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                if (target.GetStackTraceEnabled(logLevel) && WillBeAllowedByFilterer(target.Filterer, logLevel, tags))
                {
                    return true;
                }
            }
            return false;
        }

        private static string CaptureStackTrace()
        {
            return StackTraceCleaner.StripLeadingInternalFrames(StackTraceUtility.ExtractStackTrace());
        }

        [HideInCallstack]
        private static void DeliverToSyncTargets(ULoggerData data, in LogEntry entry, string stackTrace)
        {
            var targets = data.SyncTargets;
            for (var i = 0; i < targets.Length; i++)
            {
                DeliverLog(targets[i], entry, stackTrace);
            }
        }

        internal static void Deliver(ULoggerData data, in LogRecord record)
        {
            var targets = data.AsyncTargets;
            if (record.Exception != null)
            {
                for (var i = 0; i < targets.Length; i++)
                {
                    try
                    {
                        targets[i].LogException(record.Exception, record.Entry);
                    }
                    catch
                    {
                        // just don't fail on log
                    }
                }
                return;
            }

            for (var i = 0; i < targets.Length; i++)
            {
                DeliverLog(targets[i], record.Entry, record.StackTrace);
            }
        }

        [HideInCallstack]
        internal static void DeliverLog(Target target, in LogEntry entry, string stackTrace)
        {
            if (!WillBeAllowedByFilterer(target.Filterer, entry.LogLevel, entry.Tags))
            {
                return;
            }
            try
            {
                var tags = FilterTags(entry.Tags, target.Filterer, entry.LogLevel);
                var raw = ReferenceEquals(tags, entry.Tags) ? entry : entry.WithTags(tags);
                var formatted = target.Formatter.Format(raw);
                var outEntry = raw.WithMessage(formatted);
                var stackTraceForTarget = target.GetStackTraceEnabled(entry.LogLevel) ? stackTrace : null;
                target.Log(in outEntry, stackTraceForTarget);
            }
            catch
            {
                // just don't fail on log
            }
        }

        private static bool WillBeAllowedByFilterer(Filterer filterer, LogLevel logLevel, IReadOnlyList<string> tags)
        {
            for (var i = 0; i < tags.Count; i++)
            {
                if (filterer.IsAllowed(logLevel, tags[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static IReadOnlyList<string> FilterTags(IReadOnlyList<string> tags, Filterer filterer, LogLevel logLevel)
        {
            List<string> filtered = null;
            for (var i = 0; i < tags.Count; i++)
            {
                if (filterer.IsAllowed(logLevel, tags[i]))
                {
                    filtered?.Add(tags[i]);
                }
                else if (filtered == null)
                {
                    filtered = new List<string>(tags.Count);
                    for (var j = 0; j < i; j++)
                    {
                        filtered.Add(tags[j]);
                    }
                }
            }
            return filtered ?? tags;
        }
    }
}
