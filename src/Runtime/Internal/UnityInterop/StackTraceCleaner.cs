namespace Appegy.UniLogger
{
    internal static class StackTraceCleaner
    {
        private static readonly string[] _uniLoggerFrames =
        {
            "Appegy.UniLogger.ULogger",
            "Appegy.UniLogger.ExtendedULogger",
            "Appegy.UniLogger.UnityConsoleTarget",
            "Appegy.UniLogger.UnityConsoleBridge",
            "Appegy.UniLogger.UnityExceptionFormatter",
            "Appegy.UniLogger.LogDispatcher",
        };

        private static readonly string[] _unityLogFrames =
        {
            "UnityEngine.StackTraceUtility",
            "UnityEngine.Debug",
            "UnityEngine.Logger",
            "UnityEngine.UnityLogger",
        };

        private static readonly string[] _assertionFrames =
        {
            "UnityEngine.Assertions",
        };

        private static readonly string[] _asyncMachineryFrames =
        {
            "System.Runtime.CompilerServices.AsyncMethodBuilderCore",
            "System.Threading",
            "UnityEngine.UnitySynchronizationContext",
        };

        private static readonly string[][] _noiseGroups =
        {
            _uniLoggerFrames,
            _unityLogFrames,
            _assertionFrames,
            _asyncMachineryFrames,
        };

        public static string RemoveNoiseFrames(string stack)
        {
            if (string.IsNullOrEmpty(stack)) return stack;

            var builder = StringBuilderPool.GetBuilder();
            try
            {
                var first = true;
                var start = 0;
                var length = stack.Length;
                while (start < length)
                {
                    var lineEnd = stack.IndexOf('\n', start);
                    var lineStop = lineEnd < 0 ? length : lineEnd;
                    if (!IsNoiseFrame(stack, start, lineStop - start))
                    {
                        if (!first) builder.Append('\n');
                        builder.Append(stack, start, lineStop - start);
                        first = false;
                    }
                    if (lineEnd < 0) break;
                    start = lineEnd + 1;
                }
                return builder.ToString();
            }
            finally
            {
                StringBuilderPool.ReturnBuilder(builder);
            }
        }

        private static bool IsNoiseFrame(string stack, int lineStart, int lineLength)
        {
            foreach (var group in _noiseGroups)
            {
                foreach (var prefix in group)
                {
                    if (prefix.Length <= lineLength &&
                        string.CompareOrdinal(stack, lineStart, prefix, 0, prefix.Length) == 0 &&
                        (prefix.Length == lineLength || IsFrameBoundary(stack[lineStart + prefix.Length])))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsFrameBoundary(char c)
        {
            return c == '.' || c == ':' || c == '(' || c == ' ' || c == '/' || c == '+';
        }
    }
}