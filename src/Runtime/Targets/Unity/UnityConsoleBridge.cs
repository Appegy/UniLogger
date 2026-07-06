#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace Appegy.UniLogger
{
    internal static class UnityConsoleBridge
    {
        private const int Marker = 0x754C4F47;

        private const int ModeError = 0x100;
        private const int ModeWarning = 0x200;
        private const int ModeLog = 0x400;

        private static readonly Regex _frameWithLocation = new(@"\(at ([^\n]+):(\d+)\)", RegexOptions.Compiled);

        private static readonly ConcurrentQueue<PendingWrite> _pending = new();

        private static bool _handlerRegistered;
        private static Delegate _doubleClickDelegate;

        private readonly struct PendingWrite
        {
            public readonly LogLevel Level;
            public readonly string Message;
            public readonly string StackTrace;
            public readonly Object Context;

            public PendingWrite(LogLevel level, string message, string stackTrace, Object context)
            {
                Level = level;
                Message = message;
                StackTrace = stackTrace;
                Context = context;
            }
        }

        public static bool TryLog(LogLevel logLevel, string message, string stackTrace, Object context)
        {
            EditorReflection.Ensure();
            if (!EditorReflection.CanInjectConsoleEntries) return false;
            EnsureDoubleClickHandlerRegistered();
            try
            {
                var entry = Activator.CreateInstance(EditorReflection.LogEntryType);

                var combined = string.IsNullOrEmpty(stackTrace) ? message : message + "\n" + stackTrace;
                TryGetTopFrame(combined, out var file, out var line);
                var finalMessage = Hyperlinkify(combined);

                EditorReflection.EntryMessage.SetValue(entry, finalMessage);
                if (file != null)
                {
                    EditorReflection.EntryFile.SetValue(entry, file);
                    EditorReflection.EntryLine?.SetValue(entry, line);
                    EditorReflection.EntryColumn?.SetValue(entry, 0);
                }
                EditorReflection.EntryMode.SetValue(entry, ModeFor(logLevel));
                EditorReflection.EntryIdentifier.SetValue(entry, Marker);

                EditorReflection.AddMessage.Invoke(null, new[] { entry });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryEnqueueForMainThread(LogLevel logLevel, string message, string stackTrace, Object context)
        {
            EditorReflection.Ensure();
            if (!EditorReflection.CanInjectConsoleEntries) return false;
            _pending.Enqueue(new PendingWrite(logLevel, message, stackTrace, context));
            return true;
        }

        internal static void DrainPending()
        {
            while (_pending.TryDequeue(out var write))
            {
                TryLog(write.Level, write.Message, write.StackTrace, write.Context);
            }
        }

        internal static void EnsureDoubleClickHandlerRegistered()
        {
            if (_handlerRegistered) return;
            EditorReflection.Ensure();
            if (!EditorReflection.CanRegisterDoubleClick) return;
            try
            {
                var handler = typeof(UnityConsoleBridge).GetMethod(nameof(OnEntryDoubleClicked), BindingFlags.Static | BindingFlags.NonPublic);
                _doubleClickDelegate = Delegate.CreateDelegate(EditorReflection.DoubleClickDelegateType, handler);
                EditorReflection.AddDoubleClickHandler.Invoke(null, new object[] { _doubleClickDelegate });
                _handlerRegistered = true;
            }
            catch
            {
                _doubleClickDelegate = null;
            }
        }

        private static void OnEntryDoubleClicked(object entry)
        {
            try
            {
                if (EditorReflection.EntryIdentifier.GetValue(entry) is not Marker) return;
                if (EditorReflection.EntryFile.GetValue(entry) is not string file || string.IsNullOrEmpty(file)) return;
                var line = EditorReflection.EntryLine?.GetValue(entry) is int l ? l : 0;
                var column = EditorReflection.EntryColumn?.GetValue(entry) is int c ? c : 0;
                EditorReflection.OpenFile?.Invoke(null, new object[] { file, line, column });
            }
            catch
            {
                // swallow
            }
        }

        private static int ModeFor(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Warning => ModeWarning,
            LogLevel.Error => ModeError,
            _ => ModeLog
        };

        private static string Hyperlinkify(string stackTrace)
        {
            return _frameWithLocation.Replace(stackTrace, "(at <a href=\"$1\" line=\"$2\">$1:$2</a>)");
        }

        private static void TryGetTopFrame(string stackTrace, out string file, out int line)
        {
            file = null;
            line = 0;
            var match = _frameWithLocation.Match(stackTrace);
            if (match.Success && int.TryParse(match.Groups[2].Value, out var parsed))
            {
                file = match.Groups[1].Value;
                line = parsed;
            }
        }
    }
}
#endif
