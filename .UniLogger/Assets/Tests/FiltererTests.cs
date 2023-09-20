using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.UniLogger.Tests
{
    public class FiltererTests
    {
        [Test]
        public void WhenAllTagsEnabledByDefault_AndNothingChanged_ThanAllLogsAreAllowed()
        {
            // Arrange
            var filterer = new Filterer(true);

            // Act

            // Assert
            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenAllTagsEnabledByDefault_AndTagDisabled_ThanDisabledTagIsNotAllowed()
        {
            // Arrange
            var filterer = new Filterer(true);

            // Act
            filterer.SetAllowed("Tag", false);

            // Assert
            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }

        [Test]
        public void WhenAllTagsEnabledByDefault_AndTagDisabledAndEnabled_ThanTagIsAllowed()
        {
            // Arrange
            var filterer = new Filterer(true);

            // Act
            filterer.SetAllowed("Tag", false);
            filterer.SetAllowed("Tag", true);

            // Assert
            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenAllTagsDisabledByDefault_AndNothingChanged_ThanAllLogsAreNotAllowed()
        {
            // Arrange
            var filterer = new Filterer(false);

            // Act

            // Assert
            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }

        [Test]
        public void WhenAllTagsDisabledByDefault_AndTagEnabled_ThanEnabledTagIsAllowed()
        {
            // Arrange
            var filterer = new Filterer(false);

            // Act
            filterer.SetAllowed("Tag", true);

            // Assert
            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(true);
        }

        [Test]
        public void WhenAllTagsDisabledByDefault_AndTagEnabledAndDisabled_ThanTagIsNotAllowed()
        {
            // Arrange
            var filterer = new Filterer(false);

            // Act
            filterer.SetAllowed("Tag", true);
            filterer.SetAllowed("Tag", false);

            // Assert
            filterer.IsAllowed(LogLevel.Trace, "Tag").Should().Be(false);
        }
    }
}