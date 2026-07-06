using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class ManagedStackTraceConverterTests
    {
        [SetUp]
        public void SetUp()
        {
            ManagedStackTraceConverter.SetProjectRoot("/proj/Assets");
        }

        [Test]
        public void WhenFrameIsInProject_ThanItBecomesClickableRelativePath()
        {
            var input = "  at MyGame.Player.Update () [0x00010] in /proj/Assets/Scripts/Player.cs:42";

            ManagedStackTraceConverter.Convert(input)
                .Should().Be("MyGame.Player:Update () (at Assets/Scripts/Player.cs:42)");
        }

        [Test]
        public void WhenFrameIsOutsideProject_ThanLocationIsDropped()
        {
            var input = "  at UnityEngine.Foo.Bar () [0x00000] in /home/bokken/build/Engine.cs:10";

            ManagedStackTraceConverter.Convert(input).Should().Be("UnityEngine.Foo:Bar ()");
        }

        [Test]
        public void WhenFrameHasHashedAssembly_ThanLocationIsDropped()
        {
            var input = "  at System.Foo.Bar (System.Object s) [0x00000] in <abc123>:0";

            ManagedStackTraceConverter.Convert(input).Should().Be("System.Foo:Bar (System.Object s)");
        }

        [Test]
        public void WhenFrameHasNoFileInfo_ThanMethodIsKept()
        {
            var input = "  at MyGame.Player.Update ()";

            ManagedStackTraceConverter.Convert(input).Should().Be("MyGame.Player:Update ()");
        }

        [Test]
        public void WhenMultipleFrames_ThanEachIsConvertedAndJoined()
        {
            var input =
                "  at A.B.C () [0x0] in /proj/Assets/A.cs:1\n" +
                "  at D.E.F () [0x0] in /home/bokken/D.cs:2";

            ManagedStackTraceConverter.Convert(input)
                .Should().Be("A.B:C () (at Assets/A.cs:1)\nD.E:F ()");
        }

        [Test]
        public void WhenPreviousLocationMarkerPresent_ThanItIsSkipped()
        {
            var input =
                "  at A.B.C () [0x0] in /proj/Assets/A.cs:1\n" +
                "--- End of stack trace from previous location where exception was thrown ---\n" +
                "  at D.E.F () [0x0] in /proj/Assets/D.cs:2";

            ManagedStackTraceConverter.Convert(input)
                .Should().Be("A.B:C () (at Assets/A.cs:1)\nD.E:F () (at Assets/D.cs:2)");
        }
    }
}
