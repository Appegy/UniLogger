using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Appegy.UniLogger
{
    public class UnityTarget : Target
    {
        public override bool RunSynchronously => true;

        public override bool CapturesNativeLogs => false;

        public UnityTarget([CanBeNull] Formatter formatter = null, [CanBeNull] Filterer filterer = null)
            : base(formatter, filterer)
        {
        }

        [HideInCallstack]
        protected internal override void Log(in LogEntry entry, string stackTrace)
        {
#if UNITY_EDITOR
            if (ThreadDispatcher.IsMainThread)
            {
                if (UnityConsoleBridge.TryLog(entry.LogLevel, entry.String, stackTrace, entry.Context))
                {
                    NativeLogEventBridge.Raise(entry.LogLevel.ConvertToLogType(), entry.String, stackTrace, true);
                    return;
                }
            }
            else if (UnityConsoleBridge.TryEnqueueForMainThread(entry.LogLevel, entry.String, stackTrace, entry.Context))
            {
                NativeLogEventBridge.Raise(entry.LogLevel.ConvertToLogType(), entry.String, stackTrace, false);
                return;
            }
#endif
            ForwardNatively(entry.LogLevel.ConvertToLogType(), entry.String, entry.Context);
        }

        [HideInCallstack]
        protected internal override void LogException(Exception exception, in LogEntry entry)
        {
#if UNITY_EDITOR
            if (ThreadDispatcher.IsMainThread)
            {
                if (UnityConsoleBridge.TryLog(LogLevel.Error, entry.String, null, entry.Context))
                {
                    NativeLogEventBridge.Raise(LogType.Exception, entry.String, null, true);
                    return;
                }
            }
            else if (UnityConsoleBridge.TryEnqueueForMainThread(LogLevel.Error, entry.String, null, entry.Context))
            {
                NativeLogEventBridge.Raise(LogType.Exception, entry.String, null, false);
                return;
            }
#endif
            ForwardExceptionNatively(exception, entry.Context);
        }

        [HideInCallstack]
        private static void ForwardNatively(LogType logType, string message, Object context)
        {
            var handler = ULogger.OriginalHandler;
            if (handler == null) return;
            using var _ = ReentrancyCounter.Enter();
            handler.LogFormat(logType, context, "{0}", message);
        }

        [HideInCallstack]
        private static void ForwardExceptionNatively(Exception exception, Object context)
        {
            var handler = ULogger.OriginalHandler;
            if (handler == null) return;
            using var _ = ReentrancyCounter.Enter();
            handler.LogException(exception, context);
        }
    }
}
