namespace Appegy.UniLogger
{
    // Drops noise frames from a Unity-formatted stack trace. Every group below is an independent filter:
    // to add one (e.g. UniTask frames) declare a new string[] and list it in NoiseGroups; to stop
    // filtering a group, remove it from NoiseGroups. Matching is by namespace/type prefix at a frame
    // boundary, so a group entry like "UnityEngine.UnitySynchronizationContext" drops every frame inside
    // that type (and its nested types), wherever it appears in the trace.
    internal static class StackTraceCleaner
    {
        // Our own logging plumbing.
        private static readonly string[] UniLoggerFrames =
        {
            "Appegy.UniLogger.ULogger",
            "Appegy.UniLogger.ExtendedULogger",
            "Appegy.UniLogger.UnityConsoleTarget",
            "Appegy.UniLogger.UnityConsoleBridge",
            "Appegy.UniLogger.UnityExceptionFormatter",
            "Appegy.UniLogger.LogDispatcher",
        };

        // Unity's logging entry points.
        private static readonly string[] UnityLogFrames =
        {
            "UnityEngine.StackTraceUtility",
            "UnityEngine.Debug",
            "UnityEngine.Logger",
            "UnityEngine.UnityLogger",
        };

        // UnityEngine.Assertions internals (the assert that raised, not the call site).
        private static readonly string[] AssertionFrames =
        {
            "UnityEngine.Assertions",
        };

        // async/await state-machine, task, thread-pool and timer plumbing. "System.Threading" covers
        // Tasks.*, ExecutionContext, ThreadPool*, Timer and the await continuations in one prefix.
        private static readonly string[] AsyncMachineryFrames =
        {
            "System.Runtime.CompilerServices.AsyncMethodBuilderCore",
            "System.Threading",
            "UnityEngine.UnitySynchronizationContext",
        };

        private static readonly string[][] NoiseGroups =
        {
            UniLoggerFrames,
            UnityLogFrames,
            AssertionFrames,
            AsyncMachineryFrames,
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
            foreach (var group in NoiseGroups)
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
            // '.' ':' '(' ' ' end a type/namespace; '/' and '+' separate Unity/Mono nested types.
            return c == '.' || c == ':' || c == '(' || c == ' ' || c == '/' || c == '+';
        }
    }
}
