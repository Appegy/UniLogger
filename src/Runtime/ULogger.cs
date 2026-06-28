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
        internal void SendLogToUnity(LogLevel logLevel, string message, Color color, Object context)
        {
            if (Data == null)
            {
                Debug.unityLogger.Log(logLevel.ConvertToLogType(), (object)message, context);
                return;
            }

            var logTime = DateTime.Now;
            var threadId = Thread.CurrentThread.ManagedThreadId;

            // means it is possible to send log to Unity, catch using Application.logMessageReceivedThreaded and broadcast to other targets
            if (WillBeAllowedByFilterer(Data.UnityTarget.Filterer, logLevel, _tags))
            {
                var line = new LogEntry(FilterTags(_tags, Data.UnityTarget.Filterer, logLevel), logLevel, message, color, context, logTime, threadId);
                var formattedMessage = Data.UnityTarget.Formatter.Format(line);

                // manually extract stack trace if stack it disabled by unity (or we are in separate thread) but some target needs it
                var manualStacktrace = string.Empty;
                if (!ThreadDispatcher.IsMainThread ||
                    Application.GetStackTraceLogType(logLevel.ConvertToLogType()) == StackTraceLogType.None &&
                    AnyTargetNeedsStacktrace(logLevel))
                {
                    manualStacktrace = StackTraceUtility.ExtractStackTrace();
                }

                _pending = new PendingBroadcast
                {
                    Data = Data,
                    Tags = _tags,
                    LogLevel = logLevel,
                    Message = message,
                    ManualStacktrace = manualStacktrace,
                    Color = color,
                    Context = context,
                    LogTime = logTime,
                    ThreadId = threadId,
                };
                Data.LogHandler.Default.Log(logLevel.ConvertToLogType(), (object)formattedMessage, context);
                _pending = null;
            }
            // means that this log should not be sent to Unity, but can be sent to other targets
            else
            {
                var manualStacktrace = string.Empty;
                var allowedByAnyFilterer = false;
                foreach (var target in Data.Targets)
                {
                    if (!WillBeAllowedByFilterer(target.Filterer, logLevel, _tags))
                    {
                        continue;
                    }
                    allowedByAnyFilterer = true;
                    if (string.IsNullOrEmpty(manualStacktrace) && target.GetStackTraceEnabled(logLevel))
                    {
                        manualStacktrace = StackTraceUtility.ExtractStackTrace();
                    }
                }
                if (allowedByAnyFilterer)
                {
                    Data.Dispatcher.Enqueue(new LogRecord(_tags, logLevel, message, manualStacktrace, color, context, logTime, threadId));
                }
            }
        }

        private bool AnyTargetNeedsStacktrace(LogLevel logLevel)
        {
            foreach (var target in Data.Targets)
            {
                if (WillBeAllowedByFilterer(target.Filterer, logLevel, _tags) && target.GetStackTraceEnabled(logLevel))
                {
                    return true;
                }
            }
            return false;
        }

        internal void BroadcastUnobservedLog(string message, string stacktrace, LogType type)
        {
            if (Data == null) return;
            var logLevel = type.ConvertToLogLevel();
            Data.Dispatcher.Enqueue(new LogRecord(_tags, logLevel, message, stacktrace, default, default, DateTime.Now, Thread.CurrentThread.ManagedThreadId));
        }

        internal static void Deliver(ULoggerData data, in LogRecord record)
        {
            if (record.Exception != null)
            {
                foreach (var target in data.Targets)
                {
                    try
                    {
                        target.LogException(record.Exception, record.Message);
                    }
                    catch
                    {
                        // just don't fail on log
                    }
                }
                return;
            }

            foreach (var target in data.Targets)
            {
                if (!WillBeAllowedByFilterer(target.Filterer, record.LogLevel, record.Tags))
                {
                    continue;
                }
                try
                {
                    var line = new LogEntry(FilterTags(record.Tags, target.Filterer, record.LogLevel), record.LogLevel, record.Message, record.Color, record.Context, record.LogTime, record.ThreadId);
                    var formattedMessage = target.Formatter.Format(line);
                    var targetStackTrace = target.GetStackTraceEnabled(record.LogLevel) ? record.StackTrace : null;
                    target.Log(formattedMessage, targetStackTrace);
                }
                catch
                {
                    // just don't fail on log
                }
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
