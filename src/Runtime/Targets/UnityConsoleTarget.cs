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
            // Editor: route through the managed-callback console entry so the displayed stack is ours
            // (clean, with clickable frames) and row double-click lands on the real call site.
            if (ThreadDispatcher.IsMainThread && UnityConsoleBridge.TryLog(entry.LogLevel, entry.String, stackTrace, entry.Context))
            {
                return;
            }

            // Player build, a background thread, or the bridge is unavailable: forward to the engine.
            var handler = ULogger.OriginalHandler;
            if (handler == null) return;

            ULogger.BeginSuppressNativeCapture();
            try
            {
                handler.LogFormat(entry.LogLevel.ConvertToLogType(), entry.Context, "{0}", entry.String);
            }
            finally
            {
                ULogger.EndSuppressNativeCapture();
            }
        }

        [HideInCallstack]
        protected internal override void LogException(Exception exception, in LogEntry entry)
        {
            // Editor: route through the bridge so double-click lands on the first project frame
            // (the formatted message already carries the cleaned exception stack).
            if (ThreadDispatcher.IsMainThread && UnityConsoleBridge.TryLog(LogLevel.Error, entry.String, null, entry.Context))
            {
                return;
            }

            // Player build, a background thread, or the bridge is unavailable: forward to the engine.
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
    }
}
