using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Appegy.UniLogger
{
    // Routes a log into the Unity editor console as a managed-callback entry so the displayed
    // stack is fully ours (cleaned, with clickable <a href> frames) and double-clicking the row
    // navigates to a source location we choose. Pure reflection against internal editor API, so it
    // compiles in the runtime assembly and simply stays disabled in player builds.
    internal static class UnityConsoleBridge
    {
        private const int Marker = unchecked((int)0x754C4F47); // "uLOG"

        private const int ModeError = 0x100;
        private const int ModeWarning = 0x200;
        private const int ModeLog = 0x400;

        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Regex FrameWithLocation = new Regex(@"\(at (Assets[^:)\n]+):(\d+)\)", RegexOptions.Compiled);

        private static bool _reflectionReady;
        private static bool _reflectionAvailable;
        private static bool _handlerRegistered;

        private static Type _logEntryType;
        private static MethodInfo _addMessage;
        private static MethodInfo _openFile;
        private static FieldInfo _fMessage;
        private static FieldInfo _fFile;
        private static FieldInfo _fLine;
        private static FieldInfo _fColumn;
        private static FieldInfo _fMode;
        private static FieldInfo _fIdentifier;

        private static Delegate _doubleClickDelegate;

        public static bool TryLog(LogLevel logLevel, string message, string stackTrace, Object context)
        {
            if (!EnsureReflection()) return false;
            EnsureDoubleClickHandlerRegistered();
            try
            {
                var entry = Activator.CreateInstance(_logEntryType);

                // For exceptions the stack is already part of the message; for normal logs it is separate.
                var combined = string.IsNullOrEmpty(stackTrace) ? message : message + "\n" + stackTrace;
                // Double-click target is the first frame that points into the project (Assets/...),
                // so engine-internal frames (e.g. UnityEngine.Assertions at /home/bokken/...) are skipped.
                TryGetTopFrame(combined, out var file, out var line);
                var finalMessage = Hyperlinkify(combined);

                _fMessage.SetValue(entry, finalMessage);
                if (file != null)
                {
                    _fFile.SetValue(entry, file);
                    _fLine?.SetValue(entry, line);
                    _fColumn?.SetValue(entry, 0);
                }
                _fMode.SetValue(entry, ModeFor(logLevel));
                _fIdentifier.SetValue(entry, Marker);

                _addMessage.Invoke(null, new[] { entry });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool EnsureReflection()
        {
            if (_reflectionReady) return _reflectionAvailable;
            _reflectionReady = true;
            try
            {
                var logEntriesType = FindType("UnityEditor.LogEntries");
                _logEntryType = FindType("UnityEditor.LogEntry");
                if (logEntriesType == null || _logEntryType == null) return false; // player build

                _addMessage = logEntriesType.GetMethod("AddMessageWithDoubleClickCallback", StaticFlags, null, new[] { _logEntryType }, null);
                _openFile = logEntriesType.GetMethod("OpenFileOnSpecificLineAndColumn", StaticFlags);
                _fMessage = _logEntryType.GetField("message", InstanceFlags);
                _fFile = _logEntryType.GetField("file", InstanceFlags);
                _fLine = _logEntryType.GetField("line", InstanceFlags);
                _fColumn = _logEntryType.GetField("column", InstanceFlags);
                _fMode = _logEntryType.GetField("mode", InstanceFlags);
                _fIdentifier = _logEntryType.GetField("identifier", InstanceFlags);

                _reflectionAvailable = _addMessage != null && _fMessage != null && _fFile != null && _fMode != null && _fIdentifier != null;
                return _reflectionAvailable;
            }
            catch
            {
                _reflectionAvailable = false;
                return false;
            }
        }

        // Registers the console row double-click handler. Driven once per editor domain from the editor
        // assembly (so it survives play-mode exit and domain reloads) and defensively from TryLog.
        internal static void EnsureDoubleClickHandlerRegistered()
        {
            if (_handlerRegistered) return;
            if (!EnsureReflection()) return;
            try
            {
                var consoleType = FindType("UnityEditor.ConsoleWindow");
                var delegateType = consoleType?.GetNestedType("EntryDoubleClickedDelegate", BindingFlags.Public | BindingFlags.NonPublic);
                var add = consoleType?.GetMethod("add_entryWithManagedCallbackDoubleClicked", StaticFlags);
                if (delegateType == null || add == null) return;

                var handler = typeof(UnityConsoleBridge).GetMethod(nameof(OnEntryDoubleClicked), BindingFlags.Static | BindingFlags.NonPublic);
                _doubleClickDelegate = Delegate.CreateDelegate(delegateType, handler);
                add.Invoke(null, new object[] { _doubleClickDelegate });
                _handlerRegistered = true;
            }
            catch
            {
                // double-click routing is best-effort: <a href> frames stay clickable even without it
                _doubleClickDelegate = null;
            }
        }

        private static void OnEntryDoubleClicked(object entry)
        {
            try
            {
                if (!(_fIdentifier.GetValue(entry) is int id) || id != Marker) return; // not ours
                if (!(_fFile.GetValue(entry) is string file) || string.IsNullOrEmpty(file)) return;
                var line = _fLine?.GetValue(entry) is int l ? l : 0;
                var column = _fColumn?.GetValue(entry) is int c ? c : 0;
                _openFile?.Invoke(null, new object[] { file, line, column });
            }
            catch
            {
                // never throw out of an editor callback
            }
        }

        private static int ModeFor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Warning: return ModeWarning;
                case LogLevel.Error: return ModeError;
                default: return ModeLog;
            }
        }

        private static string Hyperlinkify(string stackTrace)
        {
            return FrameWithLocation.Replace(stackTrace, "(at <a href=\"$1\" line=\"$2\">$1:$2</a>)");
        }

        private static void TryGetTopFrame(string stackTrace, out string file, out int line)
        {
            file = null;
            line = 0;
            var match = FrameWithLocation.Match(stackTrace);
            if (match.Success && int.TryParse(match.Groups[2].Value, out var parsed))
            {
                file = match.Groups[1].Value;
                line = parsed;
            }
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName))
                .FirstOrDefault(type => type != null);
        }
    }
}
