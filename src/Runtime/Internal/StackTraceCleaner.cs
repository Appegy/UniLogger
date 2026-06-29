using System;

namespace Appegy.UniLogger
{
    internal static class StackTraceCleaner
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
            "Appegy.UniLogger.UnityConsoleTarget",
            "Appegy.UniLogger.UnityConsoleBridge",
            "Appegy.UniLogger.LogDispatcher",
        };

        public static string StripLeadingInternalFrames(string stack)
        {
            if (string.IsNullOrEmpty(stack)) return stack;

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

            // nothing stripped, or everything was internal: keep the original rather than returning empty
            if (start <= 0 || start >= length)
            {
                return stack;
            }
            return stack.Substring(start);
        }

        // Drops leading frames that have no source location (e.g. UnityEngine.Assertions.Assert:Fail),
        // stopping at the first frame that points to a file. Keeps the original if nothing has a location.
        public static string StripLeadingFramesWithoutLocation(string stack)
        {
            if (string.IsNullOrEmpty(stack)) return stack;

            var start = 0;
            var length = stack.Length;
            while (start < length)
            {
                var lineEnd = stack.IndexOf('\n', start);
                var lineStop = lineEnd < 0 ? length : lineEnd;
                if (stack.IndexOf("(at ", start, lineStop - start, StringComparison.Ordinal) >= 0)
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

            if (start <= 0 || start >= length)
            {
                return stack;
            }
            return stack.Substring(start);
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
