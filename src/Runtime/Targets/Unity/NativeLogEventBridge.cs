#if UNITY_EDITOR
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class NativeLogEventBridge
    {
        public static void Raise(LogType type, string message, string stackTrace, bool onMainThread)
        {
            if (ReentrancyCounter.IsEntered) return;
            EditorReflection.Ensure();
            var callback = EditorReflection.CallLogCallback;
            if (callback == null) return;

            using var _ = ReentrancyCounter.Enter();
            try
            {
                callback(message, stackTrace ?? string.Empty, type, onMainThread);
            }
            catch
            {
                // swallow
            }
        }
    }
}
#endif
