using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class InMemoryTargetTests
    {
        [Test]
        public void WhenContentFitsCapacity_ThanGetContentReturnsAllText()
        {
            var target = new InMemoryTarget(32);

            target.Log("hello", null);

            target.GetContent().Should().Be("hello\n");
        }

        [Test]
        public void WhenStackTraceProvided_ThanItIsAppendedAfterMessage()
        {
            var target = new InMemoryTarget(64);

            target.Log("msg", "trace");

            target.GetContent().Should().Be("msg\ntrace\n");
        }

        [Test]
        public void WhenContentExceedsCapacity_ThanOnlyTailIsKept()
        {
            var target = new InMemoryTarget(8);

            target.Log("hello", null);
            target.Log("world", null);

            target.GetContent().Should().Be("o\nworld\n");
        }

        [Test]
        public void WhenSingleEntryLongerThanCapacity_ThanOnlyTailIsKept()
        {
            var target = new InMemoryTarget(4);

            target.Log("abcdefg", null);

            target.GetContent().Should().Be("efg\n");
        }

        [Test]
        public void WhenCleared_ThanContentIsEmpty()
        {
            var target = new InMemoryTarget(16);

            target.Log("data", null);
            target.Clear();

            target.GetContent().Should().Be(string.Empty);
        }
    }
}
