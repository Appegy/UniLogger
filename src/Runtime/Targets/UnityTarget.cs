using System;
using JetBrains.Annotations;
using UnityEngine;

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

            ForwardNatively(entry, entry.String);
        }

        [HideInCallstack]
        protected internal override void LogException(Exception exception, in LogEntry entry)
        {
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

            var handler = ULogger.OriginalHandler;
            if (handler == null) return;
            ULogger.BeginSuppressNativeCapture();
            try
            {
                handler.LogException(exception, entry.Context);
            }
            finally
            {
                ULogger.EndSuppressNativeCapture();
            }
        }

        [HideInCallstack]
        private static void ForwardNatively(in LogEntry entry, string message)
        {
            var handler = ULogger.OriginalHandler;
            if (handler == null) return;
            ULogger.BeginSuppressNativeCapture();
            try
            {
                handler.LogFormat(entry.LogLevel.ConvertToLogType(), entry.Context, "{0}", message);
            }
            finally
            {
                ULogger.EndSuppressNativeCapture();
            }
        }
    }
}
