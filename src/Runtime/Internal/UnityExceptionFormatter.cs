using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class UnityExceptionFormatter
    {
        public static string Format(Exception exception)
        {
            if (exception == null) return string.Empty;

            var builder = StringBuilderPool.GetBuilder();
            try
            {
                builder.Append(exception.GetType().Name).Append(": ").Append(exception.Message).Append('\n');

                // The exception's own string preserves the original throw site across async/await rethrows,
                // unlike new StackTrace(exception); convert it from Mono format to clickable Unity format.
                var raw = exception.StackTrace;
                var stack = string.IsNullOrEmpty(raw)
                    ? StackTraceCleaner.StripLeadingInternalFrames(StackTraceUtility.ExtractStackTrace())
                    : ManagedStackTraceConverter.Convert(raw);

                builder.Append(StackTraceCleaner.StripLeadingFramesWithoutLocation(stack));
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
    }
}
