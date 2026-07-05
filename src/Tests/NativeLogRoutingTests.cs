using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class NativeLogRoutingTests
    {
        private sealed class CapturingTarget : Target
        {
            public int Received;

            protected internal override void Log(in LogEntry entry, string stackTrace)
            {
                Received++;
            }

            protected internal override void LogException(Exception exception, in LogEntry entry)
            {
            }
        }

        private sealed class NonCapturingTarget : Target
        {
            public int Received;

            public override bool CapturesNativeLogs => false;

            protected internal override void Log(in LogEntry entry, string stackTrace)
            {
                Received++;
            }

            protected internal override void LogException(Exception exception, in LogEntry entry)
            {
            }
        }

        private static LogRecord NativeRecord(string message)
        {
            var entry = new LogEntry(new[] { Tags.Unsorted }, LogLevel.Log, message, default, null);
            return new LogRecord(in entry, null, nativeBroadcast: true);
        }

        private static LogRecord RegularRecord(string message)
        {
            var entry = new LogEntry(new[] { Tags.Unsorted }, LogLevel.Log, message, default, null);
            return new LogRecord(in entry, null);
        }

        [Test]
        public void WhenNativeLogDelivered_ThanOnlyCapturingTargetsReceive()
        {
            var data = new ULoggerData();
            var capturing = new CapturingTarget();
            var nonCapturing = new NonCapturingTarget();
            data.AddTarget(capturing);
            data.AddTarget(nonCapturing);

            ULogger.Deliver(data, NativeRecord("native"));

            capturing.Received.Should().Be(1);
            nonCapturing.Received.Should().Be(0);
        }

        [Test]
        public void WhenRegularLogDelivered_ThanAllTargetsReceive()
        {
            var data = new ULoggerData();
            var capturing = new CapturingTarget();
            var nonCapturing = new NonCapturingTarget();
            data.AddTarget(capturing);
            data.AddTarget(nonCapturing);

            ULogger.Deliver(data, RegularRecord("regular"));

            capturing.Received.Should().Be(1);
            nonCapturing.Received.Should().Be(1);
        }

        [Test]
        public void WhenTargetDefault_ThanCapturesNativeLogs()
        {
            new CapturingTarget().CapturesNativeLogs.Should().BeTrue();
        }
    }
}
