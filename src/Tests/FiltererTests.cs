using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.UniLogger
{
    public class FiltererTests
    {
        [Test]
        public void WhenAllTagsEnabledByDefault_AndNothingChanged_ThanAllLogsAreAllowed()
        {
            var filterer = new Filterer(true);


            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenAllTagsEnabledByDefault_AndTagDisabled_ThanDisabledTagIsNotAllowed()
        {
            var filterer = new Filterer(true);

            filterer.SetAllowed("Tag", false);

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }

        [Test]
        public void WhenAllTagsEnabledByDefault_AndTagDisabledAndEnabled_ThanTagIsAllowed()
        {
            var filterer = new Filterer(true);

            filterer.SetAllowed("Tag", false);
            filterer.SetAllowed("Tag", true);

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenAllTagsDisabledByDefault_AndNothingChanged_ThanAllLogsAreNotAllowed()
        {
            var filterer = new Filterer(false);


            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }

        [Test]
        public void WhenAllTagsDisabledByDefault_AndTagEnabled_ThanEnabledTagIsAllowed()
        {
            var filterer = new Filterer(false);

            filterer.SetAllowed("Tag", true);

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenAllTagsDisabledByDefault_AndTagEnabledAndDisabled_ThanTagIsNotAllowed()
        {
            var filterer = new Filterer(false);

            filterer.SetAllowed("Tag", true);
            filterer.SetAllowed("Tag", false);

            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }
    }
}