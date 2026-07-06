#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Appegy.UniLogger
{
    internal static class EditorReflection
    {
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static bool _resolved;

        public static Action<string, string, LogType, bool> CallLogCallback { get; private set; }

        public static Type LogEntryType { get; private set; }
        public static MethodInfo AddMessage { get; private set; }
        public static MethodInfo OpenFile { get; private set; }
        public static FieldInfo EntryMessage { get; private set; }
        public static FieldInfo EntryFile { get; private set; }
        public static FieldInfo EntryLine { get; private set; }
        public static FieldInfo EntryColumn { get; private set; }
        public static FieldInfo EntryMode { get; private set; }
        public static FieldInfo EntryIdentifier { get; private set; }

        public static Type DoubleClickDelegateType { get; private set; }
        public static MethodInfo AddDoubleClickHandler { get; private set; }

        public static bool CanRaiseNativeLog => CallLogCallback != null;
        public static bool CanInjectConsoleEntries { get; private set; }
        public static bool CanRegisterDoubleClick { get; private set; }

        public static void Ensure()
        {
            if (_resolved) return;
            _resolved = true;

            ResolveNativeLogCallback();
            ResolveConsoleEntry();
            ResolveDoubleClick();
        }

        private static void ResolveNativeLogCallback()
        {
            try
            {
                var method = typeof(Application).GetMethod(
                    "CallLogCallback",
                    StaticFlags,
                    null,
                    new[] { typeof(string), typeof(string), typeof(LogType), typeof(bool) },
                    null);
                if (method == null) return;
                CallLogCallback = (Action<string, string, LogType, bool>)Delegate.CreateDelegate(
                    typeof(Action<string, string, LogType, bool>), method);
            }
            catch
            {
                CallLogCallback = null;
            }
        }

        private static void ResolveConsoleEntry()
        {
            try
            {
                var logEntriesType = FindType("UnityEditor.LogEntries");
                LogEntryType = FindType("UnityEditor.LogEntry");
                if (logEntriesType == null || LogEntryType == null) return;

                AddMessage = logEntriesType.GetMethod("AddMessageWithDoubleClickCallback", StaticFlags, null, new[] { LogEntryType }, null);
                OpenFile = logEntriesType.GetMethod("OpenFileOnSpecificLineAndColumn", StaticFlags);
                EntryMessage = LogEntryType.GetField("message", InstanceFlags);
                EntryFile = LogEntryType.GetField("file", InstanceFlags);
                EntryLine = LogEntryType.GetField("line", InstanceFlags);
                EntryColumn = LogEntryType.GetField("column", InstanceFlags);
                EntryMode = LogEntryType.GetField("mode", InstanceFlags);
                EntryIdentifier = LogEntryType.GetField("identifier", InstanceFlags);

                CanInjectConsoleEntries = AddMessage != null && EntryMessage != null && EntryFile != null && EntryMode != null && EntryIdentifier != null;
            }
            catch
            {
                CanInjectConsoleEntries = false;
            }
        }

        private static void ResolveDoubleClick()
        {
            try
            {
                var consoleType = FindType("UnityEditor.ConsoleWindow");
                DoubleClickDelegateType = consoleType?.GetNestedType("EntryDoubleClickedDelegate", BindingFlags.Public | BindingFlags.NonPublic);
                AddDoubleClickHandler = consoleType?.GetMethod("add_entryWithManagedCallbackDoubleClicked", StaticFlags);
                CanRegisterDoubleClick = DoubleClickDelegateType != null && AddDoubleClickHandler != null;
            }
            catch
            {
                CanRegisterDoubleClick = false;
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
#endif
