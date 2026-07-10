using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class FiltererFactoryTests
    {
        [Test]
        public void WhenAllowAllTags_ThanTagIsAllowedByDefault()
        {
            var filterer = Filterer.AllowAllTags();

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenBlockAllTags_ThanTagIsNotAllowedByDefault()
        {
            var filterer = Filterer.BlockAllTags();

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }

        [Test]
        public void WhenAllowAllTags_AndTagMuted_ThanTagIsNotAllowed()
        {
            var filterer = Filterer.AllowAllTags().Mute("Tag");

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }

        [Test]
        public void WhenAllowAllTags_AndLevelDisabled_ThanLevelIsNotAllowed()
        {
            var filterer = Filterer.AllowAllTags().Disable(LogLevel.Trace);

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
            filterer.IsAllowed(LogLevel.Log, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenBlockAllTags_AndTagAllowed_ThanTagIsAllowed()
        {
            var filterer = Filterer.BlockAllTags().Allow("Tag");

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }
    }
}
