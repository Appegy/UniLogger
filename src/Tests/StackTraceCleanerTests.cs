using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class StackTraceCleanerTests
    {
        [Test]
        public void WhenLeadingFramesAreInternal_ThanTheyAreStripped()
        {
            var stack =
                "UnityEngine.StackTraceUtility:ExtractStackTrace ()\n" +
                "Appegy.UniLogger.ULogger:SendLog (Appegy.UniLogger.LogLevel,string)\n" +
                "Appegy.UniLogger.ExtendedULogger:Log (Appegy.UniLogger.ULogger,string)\n" +
                "MyGame.Player:Start () (at Assets/Player.cs:10)\n" +
                "UnityEngine.MonoBehaviour:Invoke ()";

            var cleaned = StackTraceCleaner.StripLeadingInternalFrames(stack);

            cleaned.Should().StartWith("MyGame.Player:Start () (at Assets/Player.cs:10)");
            cleaned.Should().NotContain("Appegy.UniLogger.ULogger:");
            cleaned.Should().NotContain("UnityEngine.StackTraceUtility");
        }

        [Test]
        public void WhenNoLeadingInternalFrames_ThanStackIsUnchanged()
        {
            var stack = "MyGame.Player:Start () (at Assets/Player.cs:10)";

            StackTraceCleaner.StripLeadingInternalFrames(stack).Should().Be(stack);
        }

        [Test]
        public void WhenAllFramesAreInternal_ThanOriginalIsKept()
        {
            var stack = "Appegy.UniLogger.ULogger:SendLog ()\nUnityEngine.Debug:Log ()";

            StackTraceCleaner.StripLeadingInternalFrames(stack).Should().Be(stack);
        }

        [Test]
        public void WhenPrefixIsSharedButTypeDiffers_ThanFrameIsNotStripped()
        {
            // ULoggerInitializer shares the "Appegy.UniLogger.ULogger" prefix but is a different type
            var stack = "Appegy.UniLogger.ULoggerInitializer:Configure () (at Assets/Init.cs:5)";

            StackTraceCleaner.StripLeadingInternalFrames(stack).Should().Be(stack);
        }

        [Test]
        public void WhenStackIsNullOrEmpty_ThanReturnedAsIs()
        {
            StackTraceCleaner.StripLeadingInternalFrames(null).Should().BeNull();
            StackTraceCleaner.StripLeadingInternalFrames(string.Empty).Should().BeEmpty();
        }
    }
}
