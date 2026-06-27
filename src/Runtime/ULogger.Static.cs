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
        private static readonly ConcurrentDictionary<string, ULogger> _loggersByTag = new ConcurrentDictionary<string, ULogger>();
        private static readonly Func<string, ULogger> _loggerFactory = tag => new ULogger(tag);
#endif

        private static readonly ULogger _unsortedLogger = ULogger.GetLogger(Tags.Unsorted);

        [ThreadStatic] private static PendingBroadcast? _pending;

        private struct PendingBroadcast
        {
            public ULoggerData Data;
            public IReadOnlyList<string> Tags;
            public LogLevel LogLevel;
            public string Message;
            public string ManualStacktrace;
            public Color Color;
            public UnityEngine.Object Context;
            public DateTime LogTime;
            public int ThreadId;
        }

        [CanBeNull]
        private static ULoggerData Data { get; set; }

        public static IEnumerable<TargetBase> GetTargets()
        {
            if (Data == null) yield break;
            yield return Data.UnityTarget;
            foreach (var target in Data.Targets)
            {
                yield return target;
            }
        }

        public static T GetTarget<T>() where T : TargetBase
        {
            if (Data == null) return null;
            if (Data.UnityTarget is T unityTarget)
            {
                return unityTarget;
            }
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

        public static bool RemoveTarget<T>() where T : Target
        {
            ThreadDispatcher.EnsureMainThread(nameof(RemoveTarget));
            if (Data == null) return false;
            var removed = Data.RemoveTarget(typeof(T));
            if (removed == null) return false;

            Data.Dispatcher.Flush();

            if (removed is IDisposable disposable)
            {
                disposable.Dispose();
            }
            else
            {
                removed.Flush();
            }
            return true;
        }

        public static void Initialize(Formatter unityFormatter = null, Filterer unityFilterer = null)
        {
            ThreadDispatcher.EnsureMainThread(nameof(Initialize));
            if (Data != null)
            {
                Terminate();
            }
            unityFormatter ??= new Formatter();
            unityFilterer ??= new Filterer(true);
            var unityTarget = new UnityTarget(unityFormatter, unityFilterer);
            Data = new ULoggerData
            {
                UnityTarget = unityTarget,
                LogHandler = new UnityLogger(Debug.unityLogger.logHandler),
            };
            Data.Dispatcher = new LogDispatcher(Data);
            Debug.unityLogger.logHandler = Data.LogHandler;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            Application.quitting += Terminate;
            Application.focusChanged += OnApplicationFocusChanged;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        public static void Terminate()
        {
            ThreadDispatcher.EnsureMainThread(nameof(Terminate));
            if (Data == null) return;
            Debug.unityLogger.logHandler = Data.LogHandler.Default;
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            Application.quitting -= Terminate;
            Application.focusChanged -= OnApplicationFocusChanged;
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

        private static void OnApplicationFocusChanged(bool focused)
        {
            if (!focused)
            {
                Flush();
            }
        }

        private static void OnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
        {
            if (_pending.HasValue)
            {
                var pending = _pending.Value;
                if (string.IsNullOrEmpty(stacktrace))
                {
                    stacktrace = pending.ManualStacktrace;
                }
                pending.Data.Dispatcher.Enqueue(new LogRecord(pending.Tags, pending.LogLevel, pending.Message, stacktrace, pending.Color, pending.Context, pending.LogTime, pending.ThreadId));
            }
            else
            {
                _unsortedLogger.BroadcastUnobservedLog(condition, stacktrace, type);
            }
        }

        [HideInCallstack]
        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (Data == null) return;
            Data.LogHandler.Default.LogException(e.Exception.InnerException ?? e.Exception);
            Flush();
        }

        public static void Flush()
        {
            var data = Data;
            if (data == null) return;
            data.Dispatcher.Flush();
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