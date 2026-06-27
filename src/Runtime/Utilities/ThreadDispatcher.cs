using System;
using System.Threading;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class ThreadDispatcher
    {
        public static Thread MainThread { get; private set; }

        public static bool IsMainThread => MainThread != null && MainThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void CaptureMainThread()
        {
            MainThread = Thread.CurrentThread;
        }

        public static void EnsureMainThread(string operation)
        {
            if (MainThread != null && MainThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException($"{nameof(ULogger)}.{operation} must be called from the main thread.");
            }
        }
    }
}
