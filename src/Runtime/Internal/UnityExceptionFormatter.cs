using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class UnityExceptionFormatter
    {
        private static readonly string[] InternalFramePrefixes =
        {
            "UnityEngine.StackTraceUtility",
            "UnityEngine.Debug",
            "UnityEngine.Logger",
            "UnityEngine.UnityLogger",
            "Appegy.UniLogger.ULogger",
            "Appegy.UniLogger.ExtendedULogger",
            "Appegy.UniLogger.UnityExceptionFormatter",
            "Appegy.UniLogger.LogDispatcher",
        };

        private static readonly MethodInfo ExtractFormatted = typeof(StackTraceUtility).GetMethod(
            "ExtractFormattedStackTrace",
            BindingFlags.Static | BindingFlags.NonPublic,
            null, new[] { typeof(StackTrace) }, null);

        public static string Format(Exception exception)
        {
            try
            {
                var stack = FormatTrace(new StackTrace(exception, true));
                if (string.IsNullOrEmpty(stack))
                {
                    stack = StripInternalFrames(StackTraceUtility.ExtractStackTrace());
                }
                return exception.GetType().Name + ": " + exception.Message + "\n" + stack;
            }
            catch
            {
                return exception.ToString();
            }
        }

        private static string FormatTrace(StackTrace trace)
        {
            if (ExtractFormatted == null || trace.FrameCount == 0) return null;
            return ExtractFormatted.Invoke(null, new object[] { trace }) as string;
        }

        private static string StripInternalFrames(string stack)
        {
            if (string.IsNullOrEmpty(stack)) return stack;
            var lines = stack.Split('\n');
            var start = 0;
            while (start < lines.Length && IsInternalFrame(lines[start])) start++;
            return start == 0 ? stack : string.Join("\n", lines, start, lines.Length - start);
        }

        private static bool IsInternalFrame(string line)
        {
            foreach (var prefix in InternalFramePrefixes)
            {
                if (line.StartsWith(prefix, StringComparison.Ordinal)) return true;
            }
            return false;
        }
    }
}
