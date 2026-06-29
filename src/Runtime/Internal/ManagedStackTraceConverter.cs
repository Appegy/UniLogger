using System;
using System.Text.RegularExpressions;

namespace Appegy.UniLogger
{
    // Converts a managed (Mono) exception stack-trace string into Unity console format. A line like
    //   "  at NS.Class.Method (args) [0x00083] in /proj/Assets/Foo.cs:23"
    // becomes
    //   "NS.Class:Method (args) (at Assets/Foo.cs:23)"
    // so console frames stay clickable. Frames outside the project (engine /home/bokken paths, hashed
    // dynamic assemblies, no file info) lose their location and render as bare method names.
    //
    // Exceptions use this instead of a live StackTrace because, unlike new StackTrace(exception), the
    // exception's own string preserves the original throw site across async/await rethrows.
    internal static class ManagedStackTraceConverter
    {
        private static readonly Regex MonoFrame = new Regex(
            @"^\s*at\s+(?<method>.+?)\s*(?:\[0x[0-9a-fA-F]+\]\s*in\s+(?<file>.+):(?<line>\d+))?\s*$",
            RegexOptions.Compiled);

        private static string _projectRoot;

        // Application.dataPath, e.g. "/proj/Assets". We keep the part before "Assets" so both
        // Assets/... and Packages/... frames relativize against the project root.
        public static void SetProjectRoot(string assetsPath)
        {
            if (string.IsNullOrEmpty(assetsPath))
            {
                _projectRoot = null;
                return;
            }
            const string assets = "Assets";
            _projectRoot = assetsPath.EndsWith(assets, StringComparison.Ordinal)
                ? assetsPath.Substring(0, assetsPath.Length - assets.Length)
                : assetsPath;
        }

        public static string Convert(string monoStackTrace)
        {
            if (string.IsNullOrEmpty(monoStackTrace)) return monoStackTrace;

            var builder = StringBuilderPool.GetBuilder();
            try
            {
                var first = true;
                foreach (var rawLine in monoStackTrace.Split('\n'))
                {
                    var line = rawLine.TrimEnd('\r');
                    if (line.Length == 0) continue;
                    if (line.IndexOf("--- ", StringComparison.Ordinal) >= 0) continue; // "previous location" markers

                    if (!first) builder.Append('\n');
                    builder.Append(ConvertLine(line));
                    first = false;
                }
                return builder.ToString();
            }
            finally
            {
                StringBuilderPool.ReturnBuilder(builder);
            }
        }

        private static string ConvertLine(string line)
        {
            var match = MonoFrame.Match(line);
            if (!match.Success)
            {
                return line.Trim();
            }

            var method = PrettifyMethod(match.Groups["method"].Value);
            if (!match.Groups["file"].Success)
            {
                return method;
            }

            var relative = Relativize(match.Groups["file"].Value);
            if (relative == null)
            {
                return method;
            }
            return method + " (at " + relative + ":" + match.Groups["line"].Value + ")";
        }

        // Mono prints "NS.Class.Method (args)"; Unity prints "NS.Class:Method (args)". Swap the last
        // dot before the parameter list so frames read like the rest of the console.
        private static string PrettifyMethod(string method)
        {
            var paren = method.IndexOf(" (", StringComparison.Ordinal);
            var scan = paren > 0 ? paren : method.Length;
            var lastDot = method.LastIndexOf('.', scan - 1);
            if (lastDot > 0)
            {
                return method.Substring(0, lastDot) + ":" + method.Substring(lastDot + 1);
            }
            return method;
        }

        private static string Relativize(string file)
        {
            if (string.IsNullOrEmpty(_projectRoot)) return null;
            var normalized = file.Replace('\\', '/');
            if (normalized.StartsWith(_projectRoot, StringComparison.Ordinal))
            {
                return normalized.Substring(_projectRoot.Length);
            }
            return null;
        }
    }
}
