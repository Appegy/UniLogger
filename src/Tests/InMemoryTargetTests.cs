using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class InMemoryTargetTests
    {
        private static LogEntry E(string message)
        {
            return new LogEntry(null, LogLevel.Log, message, default, null);
        }

        [Test]
        public void WhenContentFitsCapacity_ThanGetContentReturnsAllText()
        {
            var target = new InMemoryTarget(32);

            target.Log(E("hello"), null);

            target.GetContent().Should().Be("hello\n");
        }

        [Test]
        public void WhenStackTraceProvided_ThanItIsAppendedAfterMessage()
        {
            var target = new InMemoryTarget(64);

            target.Log(E("msg"), "trace");

            target.GetContent().Should().Be("msg\ntrace\n");
        }

        [Test]
        public void WhenContentExceedsCapacity_ThanOnlyTailIsKept()
        {
            var target = new InMemoryTarget(8);

            target.Log(E("hello"), null);
            target.Log(E("world"), null);

            target.GetContent().Should().Be("o\nworld\n");
        }

        [Test]
        public void WhenSingleEntryLongerThanCapacity_ThanOnlyTailIsKept()
        {
            var target = new InMemoryTarget(4);

            target.Log(E("abcdefg"), null);

            target.GetContent().Should().Be("efg\n");
        }

        [Test]
        public void WhenExceptionLogged_ThanItIsWrittenAsText()
        {
            var target = new InMemoryTarget(256);

            target.LogException(new System.InvalidOperationException("ignored"), E("formatted exception text"));

            target.GetContent().Should().Contain("formatted exception text");
        }

        [Test]
        public void WhenCleared_ThanContentIsEmpty()
        {
            var target = new InMemoryTarget(16);

            target.Log(E("data"), null);
            target.Clear();

            target.GetContent().Should().Be(string.Empty);
        }
    }
}
