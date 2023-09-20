﻿using UnityEngine;

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

            // Initialize ULogger and unity target
            InitializeUnityTarget();

            // Add file target (currently FileTarget still in development and not fully tested yet)
            // InitializeFileTarget();
        }

        private static void InitializeUnityTarget()
        {
            // Customize formatter for logs unity target 
            var formatter = Application.isEditor
                ? new Formatter(FormatOptions.RichText | FormatOptions.TagCategory | FormatOptions.TagName)
                : new Formatter(FormatOptions.TagCategory | FormatOptions.TagName | FormatOptions.LogType);

            // When formatter and filterer are ready - initialize logger 
            ULogger.Initialize(formatter);
        }
    }
}