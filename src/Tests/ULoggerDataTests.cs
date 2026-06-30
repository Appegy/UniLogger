using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class ULoggerDataTests
    {
        private sealed class AsyncTarget : Target
        {
            protected internal override void Log(in LogEntry entry, string stackTrace)
            {
            }

            protected internal override void LogException(Exception exception, in LogEntry entry)
            {
            }
        }

        private sealed class SyncTarget : Target
        {
            public override bool RunSynchronously => true;

            protected internal override void Log(in LogEntry entry, string stackTrace)
            {
            }

            protected internal override void LogException(Exception exception, in LogEntry entry)
            {
            }
        }

        [Test]
        public void WhenTargetsAdded_ThanPartitionedBySyncFlag()
        {
            var data = new ULoggerData();
            var sync = new SyncTarget();
            var async = new AsyncTarget();

            data.AddTarget(sync);
            data.AddTarget(async);

            data.Targets.Should().HaveCount(2);
            data.SyncTargets.Should().ContainSingle().Which.Should().BeSameAs(sync);
            data.AsyncTargets.Should().ContainSingle().Which.Should().BeSameAs(async);
        }

        [Test]
        public void WhenSameTypeAddedTwice_ThanSecondIsRejected()
        {
            var data = new ULoggerData();

            data.AddTarget(new AsyncTarget()).Should().BeTrue();
            data.AddTarget(new AsyncTarget()).Should().BeFalse();
            data.Targets.Should().HaveCount(1);
        }
    }
}
