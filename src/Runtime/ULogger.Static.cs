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

        private static readonly ULogger _unsortedLogger = GetLogger(Tags.Unsorted);

        [CanBeNull]
        private static ULoggerData Data { get; set; }

        [CanBeNull]
        internal static ILogHandler OriginalHandler { get; set; }

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