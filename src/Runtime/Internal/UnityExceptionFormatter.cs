using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class UnityExceptionFormatter
    {
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
                    builder.Append(StackTraceCleaner.StripLeadingInternalFrames(StackTraceUtility.ExtractStackTrace()));
                }
                else
                {
                    builder.Append(StackTraceCleaner.StripLeadingFramesWithoutLocation(stack));
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
    }
}
