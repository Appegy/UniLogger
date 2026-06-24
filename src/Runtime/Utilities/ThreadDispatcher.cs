using System.Threading;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class ThreadDispatcher
    {
        public static Thread MainThread { get; private set; }

        public static bool IsMainThread => MainThread != null && MainThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoConfigureLogger()
        {
            MainThread = Thread.CurrentThread;
        }
    }
}