using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using Assembly = UnityEditor.Compilation.Assembly;

namespace Appegy.UniLogger
{
    public class CompileWithDefinesTests
    {
        private const string TargetAssembly = "Appegy.UniLogger";
        private const string LangVersion = "9.0";

        private static IEnumerable<TestCaseData> DefineCombinations()
        {
            yield return Combination("all logs enabled");
            yield return Combination("trace off", "ULOGGER_TRACE_OFF");
            yield return Combination("logs off", "ULOGGER_LOGS_OFF");
            yield return Combination("warnings off", "ULOGGER_WARNINGS_OFF");
            yield return Combination("errors off", "ULOGGER_ERRORS_OFF");
            yield return Combination("disable all logs", "ULOGGER_DISABLE_ALL_LOGS");
            yield return Combination("every level off", "ULOGGER_TRACE_OFF", "ULOGGER_LOGS_OFF", "ULOGGER_WARNINGS_OFF", "ULOGGER_ERRORS_OFF");
        }

        private static TestCaseData Combination(string name, params string[] defines)
        {
            return new TestCaseData((object)defines).SetName(name);
        }

        [TestCaseSource(nameof(DefineCombinations))]
        public void CompilesWithoutErrorsOrWarnings(string[] extraDefines)
        {
            var assembly = CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                .FirstOrDefault(c => c.name == TargetAssembly);
            Assert.IsNotNull(assembly, $"Assembly '{TargetAssembly}' was not found.");

            var dotnet = ResolveDotnet();
            var compiler = ResolveCompiler();
            if (dotnet == null || compiler == null)
            {
                Assert.Ignore("Bundled Roslyn compiler was not found under the editor installation.");
            }

            var responseFile = Path.Combine("Temp", TargetAssembly + ".compiletest.rsp");
            File.WriteAllText(responseFile, BuildResponseFile(assembly, extraDefines));

            using var process = new Process
            {
                StartInfo =
                {
                    FileName = dotnet,
                    Arguments = $"\"{compiler}\" -noconfig @\"{responseFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();

            var diagnostics = output
                .Split('\n')
                .Select(c => c.Trim())
                .Where(c => c.Contains(": error CS") || c.Contains(": warning CS"))
                .ToArray();

            var combo = extraDefines.Length == 0 ? "all logs enabled" : string.Join(", ", extraDefines);
            Assert.IsEmpty(diagnostics, $"[{combo}] produced compiler diagnostics:\n{string.Join("\n", diagnostics)}");
        }

        private static string BuildResponseFile(Assembly assembly, string[] extraDefines)
        {
            var defines = assembly.defines.Concat(extraDefines).Distinct();
            var builder = new StringBuilder();
            builder.AppendLine("-target:library");
            builder.AppendLine($"-out:\"{Path.Combine("Temp", TargetAssembly + ".compiletest.dll")}\"");
            builder.AppendLine("-nostdlib+");
            builder.AppendLine($"-langversion:{LangVersion}");
            builder.AppendLine($"-define:{string.Join(";", defines)}");
            foreach (var reference in assembly.allReferences)
            {
                builder.AppendLine($"-reference:\"{reference}\"");
            }
            foreach (var source in assembly.sourceFiles)
            {
                builder.AppendLine($"\"{Path.GetFullPath(source)}\"");
            }
            return builder.ToString();
        }

        private static string ResolveDotnet()
        {
            var root = EditorApplication.applicationContentsPath;
            return new[]
                {
                    Path.Combine(root, "NetCoreRuntime", "dotnet.exe"),
                    Path.Combine(root, "NetCoreRuntime", "dotnet")
                }
                .FirstOrDefault(File.Exists);
        }

        private static string ResolveCompiler()
        {
            var root = EditorApplication.applicationContentsPath;
            return new[]
                {
                    Path.Combine(root, "DotNetSdkRoslyn", "csc.dll"),
                    Path.Combine(root, "Tools", "Roslyn", "csc.dll")
                }
                .FirstOrDefault(File.Exists);
        }
    }
}
