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

                var raw = exception.StackTrace;
                var stack = string.IsNullOrEmpty(raw)
                    ? StackTraceUtility.ExtractStackTrace()
                    : ManagedStackTraceConverter.Convert(raw);

                builder.Append(StackTraceCleaner.RemoveNoiseFrames(stack));
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
