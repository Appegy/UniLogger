#define ULOGGER_TRACE_ON
#define ULOGGER_LOGS_ON
#define ULOGGER_WARNINGS_ON
#define ULOGGER_ERRORS_ON

#if ULOGGER_TRACE_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_TRACE_ON
#endif
#if ULOGGER_LOGS_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_LOGS_ON
#endif
#if ULOGGER_WARNINGS_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_WARNINGS_ON
#endif
#if ULOGGER_ERRORS_OFF || ULOGGER_DISABLE_ALL_LOGS
#undef ULOGGER_ERRORS_ON
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Appegy.UniLogger
{
    public partial class ULogger
    {
#if ULOGGER_DISABLE_ALL_LOGS
        private static readonly ULogger _disabledLogger = new ULogger(Tags.Disabled);
#else
        private static readonly ConcurrentDictionary<string, ULogger> _loggersByTag = new();
        private static readonly Func<string, ULogger> _loggerFactory = tag => new ULogger(tag);
#endif

        private static readonly ULogger _unsortedLogger = ULogger.GetLogger(Tags.Unsorted);

        [ThreadStatic] private static bool _suppressNativeCapture;

        [CanBeNull]
        private static ULoggerData Data { get; set; }

        internal static ILogHandler OriginalHandler => Data?.OriginalHandler;

        internal static void BeginSuppressNativeCapture()
        {
            _suppressNativeCapture = true;
        }

        internal static void EndSuppressNativeCapture()
        {
            _suppressNativeCapture = false;
        }

        public static IEnumerable<Target> GetTargets()
        {
            if (Data == null) yield break;
            foreach (var target in Data.Targets)
            {
                yield return target;
            }
        }

        public static T GetTarget<T>() where T : Target
        {
            if (Data == null) return null;
            foreach (var target in Data.Targets)
            {
                if (target is T match)
                {
                    return match;
                }
            }
            return null;
        }

        public static void AddTarget<T>(T target) where T : Target
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            ThreadDispatcher.EnsureMainThread(nameof(AddTarget));
            if (Data == null) throw new InvalidOperationException($"{nameof(ULogger)}.{nameof(Initialize)} must be called before adding targets.");
            if (!Data.AddTarget(target))
            {
                throw new InvalidOperationException($"A target of type '{target.GetType().Name}' is already registered.");
            }
        }

        public static void Initialize()
        {
            ThreadDispatcher.EnsureMainThread(nameof(Initialize));
            if (Data != null)
            {
                Terminate();
            }
            var originalHandler = Debug.unityLogger.logHandler;
            ManagedStackTraceConverter.SetProjectRoot(Application.dataPath);
            Data = new ULoggerData
            {
                OriginalHandler = originalHandler,
                LogHandler = new UnityLogger(),
            };
            Data.Dispatcher = new LogDispatcher(Data);
            Debug.unityLogger.logHandler = Data.LogHandler;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            Application.quitting += Terminate;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        public static void Terminate()
        {
            ThreadDispatcher.EnsureMainThread(nameof(Terminate));
            if (Data == null) return;
            Debug.unityLogger.logHandler = Data.OriginalHandler;
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            Application.quitting -= Terminate;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

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

        private static void OnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
        {
            if (type == LogType.Exception) return;
            if (_suppressNativeCapture) return;
            _unsortedLogger.BroadcastUnobservedLog(condition, stacktrace, type);
        }

        [HideInCallstack]
        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DispatchException(e.Exception, null);
        }

        internal static Exception UnwrapException(Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                var inner = aggregate.Flatten().InnerExceptions;
                if (inner.Count == 1) return inner[0];
            }
            return exception;
        }

        #region GetLogger

        public static ULogger GetLogger<TLoggerTag>(TLoggerTag tag)
            where TLoggerTag : struct, Enum
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return _loggersByTag.GetOrAdd(EnumTagCache<TLoggerTag>.Get(tag), _loggerFactory);
#endif
        }

        public static ULogger GetLogger(Type tag)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return _loggersByTag.GetOrAdd(tag.GetTag(), _loggerFactory);
#endif
        }

        public static ULogger GetLogger(string tag)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return _loggersByTag.GetOrAdd(tag, _loggerFactory);
#endif
        }

        public static ULogger GetLogger(params object[] tags)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(tags.Select(c => c.GetTag()));
#endif
        }

        #endregion
    }
}