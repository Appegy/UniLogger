using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
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

        private static readonly Func<StackTrace, string> ExtractFormatted = CreateExtractDelegate();

        private static Func<StackTrace, string> CreateExtractDelegate()
        {
            try
            {
                var method = typeof(StackTraceUtility).GetMethod(
                    "ExtractFormattedStackTrace",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null, new[] { typeof(StackTrace) }, null);
                return method == null
                    ? null
                    : (Func<StackTrace, string>)method.CreateDelegate(typeof(Func<StackTrace, string>));
            }
            catch
            {
                return null;
            }
        }

        public static string Format(Exception exception)
        {
            if (exception == null) return string.Empty;

            var builder = StringBuilderPool.GetBuilder();
            try
            {
                var stack = FormatTrace(new StackTrace(exception, true));

                builder.Append(exception.GetType().Name).Append(": ").Append(exception.Message).Append('\n');

                if (string.IsNullOrEmpty(stack))
                {
                    AppendStrippedInternalFrames(builder, StackTraceUtility.ExtractStackTrace());
                }
                else
                {
                    builder.Append(stack);
                }

                return builder.ToString();
            }
            catch
            {
                return exception.ToString();
            }
            finally
            {
                StringBuilderPool.ReturnBuilder(builder);
            }
        }

        private static string FormatTrace(StackTrace trace)
        {
            if (ExtractFormatted == null || trace.FrameCount == 0)
            {
                return null;
            }
            return ExtractFormatted(trace);
        }

        private static void AppendStrippedInternalFrames(StringBuilder builder, string stack)
        {
            if (string.IsNullOrEmpty(stack))
            {
                return;
            }

            var start = 0;
            var length = stack.Length;
            while (start < length)
            {
                var lineEnd = stack.IndexOf('\n', start);
                var lineLength = (lineEnd < 0 ? length : lineEnd) - start;
                if (!IsInternalFrame(stack, start, lineLength))
                {
                    break;
                }
                if (lineEnd < 0)
                {
                    start = length;
                    break;
                }
                start = lineEnd + 1;
            }

            if (start >= length)
            {
                builder.Append(stack);
            }
            else
            {
                builder.Append(stack, start, length - start);
            }
        }

        private static bool IsInternalFrame(string stack, int lineStart, int lineLength)
        {
            foreach (var prefix in InternalFramePrefixes)
            {
                if (prefix.Length <= lineLength &&
                    string.CompareOrdinal(stack, lineStart, prefix, 0, prefix.Length) == 0 &&
                    (prefix.Length == lineLength || IsFrameBoundary(stack[lineStart + prefix.Length])))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsFrameBoundary(char c)
        {
            return c == '.' || c == ':' || c == '(' || c == ' ';
        }
    }
}
