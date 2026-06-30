using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class StackTraceCleanerTests
    {
        [Test]
        public void WhenUniLoggerAndUnityLogFramesPresent_ThanTheyAreDropped()
        {
            var stack =
                "UnityEngine.StackTraceUtility:ExtractStackTrace ()\n" +
                "Appegy.UniLogger.ULogger:SendLog (Appegy.UniLogger.LogLevel,string)\n" +
                "Appegy.UniLogger.ExtendedULogger:Log (Appegy.UniLogger.ULogger,string)\n" +
                "MyGame.Player:Start () (at Assets/Player.cs:10)";

            var cleaned = StackTraceCleaner.RemoveNoiseFrames(stack);

            cleaned.Should().Be("MyGame.Player:Start () (at Assets/Player.cs:10)");
        }

        [Test]
        public void WhenAssertionFramesPresent_ThanTheyAreDropped()
        {
            var stack =
                "UnityEngine.Assertions.Assert:Fail (System.String, System.String)\n" +
                "UnityEngine.Assertions.Assert:IsTrue (System.Boolean, System.String)\n" +
                "MyGame.Player:Start () (at Assets/Player.cs:11)";

            StackTraceCleaner.RemoveNoiseFrames(stack)
                .Should().Be("MyGame.Player:Start () (at Assets/Player.cs:11)");
        }

        [Test]
        public void WhenAsyncMachineryFramesPresent_ThanTheyAreDropped()
        {
            var stack =
                "MyGame.Job:MoveNext () (at Assets/Job.cs:22)\n" +
                "System.Runtime.CompilerServices.AsyncMethodBuilderCore/MoveNextRunner:InvokeMoveNext (object)\n" +
                "System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext)\n" +
                "System.Threading.Tasks.SynchronizationContextAwaitTaskContinuation+<>c:<.cctor>b__7_0 (object)\n" +
                "UnityEngine.UnitySynchronizationContext/WorkRequest:Invoke () (at /home/bokken/UnitySynchronizationContext.cs:156)\n" +
                "UnityEngine.UnitySynchronizationContext:Exec () (at /home/bokken/UnitySynchronizationContext.cs:84)";

            StackTraceCleaner.RemoveNoiseFrames(stack)
                .Should().Be("MyGame.Job:MoveNext () (at Assets/Job.cs:22)");
        }

        [Test]
        public void WhenPrefixIsSharedButTypeDiffers_ThanFrameIsKept()
        {
            var stack = "Appegy.UniLogger.ULoggerInitializer:Configure () (at Assets/Init.cs:5)";

            StackTraceCleaner.RemoveNoiseFrames(stack).Should().Be(stack);
        }

        [Test]
        public void WhenNoNoiseFrames_ThanStackIsUnchanged()
        {
            var stack = "MyGame.Player:Start () (at Assets/Player.cs:10)\nMyGame.Player:Awake () (at Assets/Player.cs:5)";

            StackTraceCleaner.RemoveNoiseFrames(stack).Should().Be(stack);
        }

        [Test]
        public void WhenStackIsNullOrEmpty_ThanReturnedAsIs()
        {
            StackTraceCleaner.RemoveNoiseFrames(null).Should().BeNull();
            StackTraceCleaner.RemoveNoiseFrames(string.Empty).Should().BeEmpty();
        }
    }
}
