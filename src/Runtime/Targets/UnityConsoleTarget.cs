using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Appegy.UniLogger
{
    public class UnityConsoleTarget : Target
    {
        public override bool RunSynchronously => true;

        public UnityConsoleTarget([CanBeNull] Formatter formatter = null, [CanBeNull] Filterer filterer = null)
            : base(formatter, filterer)
        {
        }

        [HideInCallstack]
        protected internal override void Log(in LogEntry entry, string stackTrace)
        {
            // Editor main thread: managed-callback entry (clean stack + row double-click).
            // Editor background thread: forward with the engine stack suppressed + our hyperlinked stack.
            if (ThreadDispatcher.IsMainThread)
            {
                if (UnityConsoleBridge.TryLog(entry.LogLevel, entry.String, stackTrace, entry.Context)) return;
            }
            else if (UnityConsoleBridge.TryForwardCleanStack(entry.LogLevel, entry.String, stackTrace, entry.Context))
            {
                return;
            }

            ForwardNatively(entry, entry.String);
        }

        [HideInCallstack]
        protected internal override void LogException(Exception exception, in LogEntry entry)
        {
            // The formatted message already carries the cleaned exception stack.
            if (ThreadDispatcher.IsMainThread)
            {
                if (UnityConsoleBridge.TryLog(LogLevel.Error, entry.String, null, entry.Context)) return;
            }
            else if (UnityConsoleBridge.TryForwardCleanStack(LogLevel.Error, entry.String, null, entry.Context))
            {
                return;
            }

            // Player build / fallback: native exception so crash reporters and the player log see it.
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
