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
