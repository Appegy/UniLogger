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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace UnityEngine
{
    public partial class ULogger
    {
#if ULOGGER_DISABLE_ALL_LOGS
        private static readonly ULogger _disabledLogger = new ULogger(Config, Tags.Disabled.AsEnumerable());
#endif

        private static readonly ULogger _unsortedLogger = ULogger.GetLogger(Tags.Unsorted);
        private static Action<string, string, LogType> _logsHandler;

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
            return Data.Targets.First(c => c is T) as T;
        }

        public static void Initialize(Formatter unityFormatter = null, Filterer unityFilterer = null)
        {
            // prepare default loggers and swap unity logger to custom
            unityFormatter ??= new Formatter();
            unityFilterer ??= new Filterer(true);
            var unityTarget = new UnityTarget(unityFormatter, unityFilterer);
            Data = new ULoggerData
            {
                UnityTarget = unityTarget,
                LogHandler = new UnityLogger(Debug.unityLogger.logHandler),
            };
            Debug.unityLogger.logHandler = Data.LogHandler;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        public static void Terminate()
        {
            if (Data == null) return;
            Debug.unityLogger.logHandler = Data.LogHandler.Default;
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            Data = null;
        }

        private static void OnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
        {
            if (_logsHandler != null)
            {
                _logsHandler(condition, stacktrace, type);
            }
            else
            {
                _unsortedLogger.BroadcastUnobservedLog(condition, stacktrace, type);
            }
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (Data == null) return;
            Data.LogHandler.Default.LogException(e.Exception);
            Flush();
        }

        public static void Flush()
        {
            if (Data == null) return;
            foreach (var target in Data.Targets)
            {
                target.Flush();
            }
        }

        #region GetLogger

        public static ULogger GetLogger<TLoggerTag>(TLoggerTag tag)
            where TLoggerTag : struct, Enum
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(tag.GetTag().AsEnumerable());
#endif
        }

        public static ULogger GetLogger(Type tag)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(tag.GetTag().AsEnumerable());
#endif
        }

        public static ULogger GetLogger(string tag)
        {
#if ULOGGER_DISABLE_ALL_LOGS
            return _disabledLogger;
#else
            return new ULogger(tag.AsEnumerable());
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