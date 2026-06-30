using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class FileTargetTests
    {
        private static LogEntry E(string message)
        {
            return new LogEntry(null, LogLevel.Log, message, default, null);
        }

        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "ulogger-filetarget-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
            }
            catch
            {
            }
        }

        [Test]
        public void WhenMessageLogged_ThanItIsWrittenToCurrentFile()
        {
            using var target = new FileTarget(Path.Combine(_directory, "game.log"));

            target.Log(E("hello"), null);
            target.Flush();

            File.ReadAllText(target.CurrentFilePath).Should().Be("hello\n");
        }

        [Test]
        public void WhenStackTraceProvided_ThanItIsWrittenAfterMessage()
        {
            using var target = new FileTarget(Path.Combine(_directory, "game.log"));

            target.Log(E("msg"), "trace");
            target.Flush();

            File.ReadAllText(target.CurrentFilePath).Should().Be("msg\ntrace\n");
        }

        [Test]
        public void WhenExceptionLogged_ThanItIsWrittenToFile()
        {
            using var target = new FileTarget(Path.Combine(_directory, "game.log"));

            target.LogException(new InvalidOperationException("ignored"), E("formatted exception text"));
            target.Flush();

            File.ReadAllText(target.CurrentFilePath).Should().Contain("formatted exception text");
        }

        [Test]
        public void WhenSizeLimitExceeded_ThanLogRollsToNextFile()
        {
            using var target = new FileTarget(Path.Combine(_directory, "game.log"), fileSizeLimitBytes: 16);

            target.Log(E("0123456789"), null);
            target.Log(E("0123456789"), null);
            target.Flush();

            target.GetLogFiles().Length.Should().Be(2);
        }

        [Test]
        public void WhenRetentionLimitSet_ThanOnlyNewestFilesAreKept()
        {
            using var target = new FileTarget(Path.Combine(_directory, "game.log"), fileSizeLimitBytes: 8, retainedFileCountLimit: 2);

            for (var i = 0; i < 5; i++)
            {
                target.Log(E("12345"), null);
            }
            target.Flush();

            Directory.GetFiles(_directory).Length.Should().Be(2);
        }
    }
}
