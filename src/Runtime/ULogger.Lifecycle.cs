using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Appegy.UniLogger
{
    public partial class ULogger
    {
        public static void Initialize()
        {
            Configure(_ => { });
        }

        public static void Configure(Action<LoggerConfiguration> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            ThreadDispatcher.EnsureMainThread(nameof(Configure));

            var data = BuildData(configure);

            if (Data != null)
            {
                Terminate();
            }

            OriginalHandler = Debug.unityLogger.logHandler;
            ManagedStackTraceConverter.SetProjectRoot(Application.dataPath);
            Data = data;
            Debug.unityLogger.logHandler = new UnityLogger();

            Application.logMessageReceivedThreaded += OnNativeLogReceived;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            Application.quitting += Terminate;
        }

        private static ULoggerData BuildData(Action<LoggerConfiguration> configure)
        {
            var configuration = new LoggerConfiguration();
            var data = new ULoggerData();
            try
            {
                configure(configuration);
                foreach (var target in configuration.BuildTargets())
                {
                    if (!data.AddTarget(target))
                    {
                        throw new InvalidOperationException($"A target of type '{target.GetType().Name}' is already registered.");
                    }
                }
            }
            catch
            {
                DisposeTargets(configuration.BuildTargets());
                throw;
            }
            data.Dispatcher = new LogDispatcher(data);
            return data;
        }

        private static void DisposeTargets(IReadOnlyList<Target> targets)
        {
            foreach (var target in targets)
            {
                if (target is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
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
            DisposeTargets(Data.Targets);
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
