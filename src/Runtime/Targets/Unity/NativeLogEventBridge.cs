#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class NativeLogEventBridge
    {
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static bool _reflectionReady;
        private static Action<string, string, LogType, bool> _callLogCallback;

        public static void Warmup()
        {
            EnsureReflection();
        }

        public static void Raise(LogType type, string message, string stackTrace, bool onMainThread)
        {
            if (ReentrancyCounter.IsEntered) return;
            if (!EnsureReflection()) return;

            using var _ = ReentrancyCounter.Enter();
            try
            {
                _callLogCallback(message, stackTrace ?? string.Empty, type, onMainThread);
            }
            catch
            {
                // swallow
            }
        }

        private static bool EnsureReflection()
        {
            if (_reflectionReady) return _callLogCallback != null;
            _reflectionReady = true;
            try
            {
                var method = typeof(Application).GetMethod(
                    "CallLogCallback",
                    StaticFlags,
                    null,
                    new[] { typeof(string), typeof(string), typeof(LogType), typeof(bool) },
                    null);
                if (method == null) return false;
                _callLogCallback = (Action<string, string, LogType, bool>)Delegate.CreateDelegate(
                    typeof(Action<string, string, LogType, bool>), method);
                return true;
            }
            catch
            {
                _callLogCallback = null;
                return false;
            }
        }
    }
}
#endif
