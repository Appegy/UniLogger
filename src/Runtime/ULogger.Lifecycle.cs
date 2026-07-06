using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Appegy.UniLogger
{
    public partial class ULogger
    {
        public static void Initialize()
        {
            ThreadDispatcher.EnsureMainThread(nameof(Initialize));
            if (Data != null)
            {
                Terminate();
            }

            Data = new ULoggerData();
            Data.Dispatcher = new LogDispatcher(Data);

            OriginalHandler = Debug.unityLogger.logHandler;
            ManagedStackTraceConverter.SetProjectRoot(Application.dataPath);
            Debug.unityLogger.logHandler = new UnityLogger();

            Application.logMessageReceivedThreaded += OnNativeLogReceived;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            Application.quitting += Terminate;
        }

        public static void Terminate()
        {
            ThreadDispatcher.EnsureMainThread(nameof(Terminate));
            if (Data == null) return;

            Debug.unityLogger.logHandler = OriginalHandler;
            OriginalHandler = null;

            Application.logMessageReceivedThreaded -= OnNativeLogReceived;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            Application.quitting -= Terminate;

            Data.Dispatcher.Dispose();
            foreach (var target in Data.Targets)
            {
                if (target is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            Data = null;
        }

        private static void OnNativeLogReceived(string condition, string stacktrace, LogType type)
        {
            if (type == LogType.Exception) return;
            if (ReentrancyCounter.IsEntered) return;
            _unsortedLogger.BroadcastUnobservedLog(condition, stacktrace, type);
        }

        [HideInCallstack]
        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DispatchException(e.Exception, null);
        }
    }
}
