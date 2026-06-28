using System.IO;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public static class ULoggerInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoConfigureLogger()
        {
            // Disable stacktrace for logs and warnings in build
            if (!Application.isEditor)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
                Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            }

            // Initialize ULogger and the unity console target
            InitializeUnityTarget();

            // Mirror logs into a rolling file and an on-screen overlay
            InitializeFileTarget();
            InitializeInMemoryTarget();
        }

        private static void InitializeUnityTarget()
        {
            // Customize formatter for logs unity target
            var formatter = Application.isEditor
                ? new Formatter(FormatOptions.RichText | FormatOptions.Tags)
                : new Formatter(FormatOptions.Tags | FormatOptions.LogType);

            // Prepare filterer for unity target (by default all logs are allowed)
            var filterer = new Filterer(true);

            // Now you can disable logs in filterer by log's level or tag
            // For example
            // Disable all Trace logs:
            // filterer.Disable(LogLevel.Trace);
            // Disable all logs with tag Unsorted
            // filterer.Mute("Unsorted");

            // When formatter and filterer are ready - initialize logger
            ULogger.Initialize(formatter, filterer);
        }

        private static void InitializeFileTarget()
        {
            // Rolling log files in Appegy.UniLogger.Lab/Logs (next to the Assets folder)
            var path = Path.Combine(Application.dataPath, "..", "Logs", "game.log");
            var formatter = new Formatter(FormatOptions.Time | FormatOptions.Tags | FormatOptions.LogType);
            ULogger.AddTarget(new FileTarget(path, fileSizeLimitBytes: 1024 * 1024, retainedFileCountLimit: 5, formatter: formatter));
        }

        private static void InitializeInMemoryTarget()
        {
            // Keep the most recent formatted logs in memory and show them in the on-screen overlay
            var formatter = new Formatter(FormatOptions.RichText | FormatOptions.Time | FormatOptions.Tags | FormatOptions.LogType);
            ULogger.AddTarget(new InMemoryTarget(32 * 1024, formatter));
            InMemoryLogOverlay.Spawn();
        }
    }
}
