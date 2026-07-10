using System.IO;
using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public static class ULoggerInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoConfigureLogger()
        {
            if (!Application.isEditor)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
                Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            }

            ULogger.Configure(c => c
                .WriteTo.Unity(t => t
                    .Format(Application.isEditor
                        ? FormatOptions.RichText | FormatOptions.Tags
                        : FormatOptions.Tags | FormatOptions.LogType)
                    .Filter(Filterer.AllowAllTags()))
                .WriteTo.File(t => t
                    .Path(LogFilePath())
                    .Format(FormatOptions.Time | FormatOptions.Tags | FormatOptions.LogType)
                    .RollEvery(1024 * 1024)
                    .Keep(5)));
        }

        private static string LogFilePath()
        {
            return Application.isEditor
                ? Path.Combine(Application.dataPath, "..", "Logs", "game.log")
                : Path.Combine(Application.persistentDataPath, "Logs", "game.log");
        }
    }
}
