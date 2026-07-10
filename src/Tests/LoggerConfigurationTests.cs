using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class LoggerConfigurationTests
    {
        private sealed class SyncRecordingTarget : Target
        {
            public string LastMessage;

            public override bool RunSynchronously => true;

            protected internal override void Log(in LogEntry entry, string stackTrace)
            {
                LastMessage = entry.String;
            }

            protected internal override void LogException(Exception exception, in LogEntry entry)
            {
            }
        }

        [Test]
        public void WhenConfigured_ThanTargetsAreBuiltInOrderWithCorrectTypes()
        {
            var configuration = new LoggerConfiguration();
            configuration
                .WriteTo.Unity()
                .WriteTo.File(t => t.Path("Temp/logger-config-test.log"))
                .WriteTo.Memory(t => t.Capacity(1024));

            var targets = configuration.BuildTargets();

            targets.Should().HaveCount(3);
            targets[0].Should().BeOfType<UnityTarget>();
            targets[1].Should().BeOfType<FileTarget>();
            targets[2].Should().BeOfType<InMemoryTarget>();

            (targets[1] as IDisposable)?.Dispose();
        }

        [Test]
        public void WhenTargetAddedViaEscapeHatch_ThanItIsBuilt()
        {
            var custom = new SyncRecordingTarget();
            var configuration = new LoggerConfiguration();
            configuration.WriteTo.Target(custom);

            configuration.BuildTargets().Should().ContainSingle().Which.Should().BeSameAs(custom);
        }

        [Test]
        public void WhenFileTargetHasNoPath_ThanBuildThrows()
        {
            var configuration = new LoggerConfiguration();

            Action build = () => configuration.WriteTo.File(t => t.Format(FormatOptions.Tags));

            build.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void WhenMemoryTargetHasNoCapacity_ThanBuildThrows()
        {
            var configuration = new LoggerConfiguration();

            Action build = () => configuration.WriteTo.Memory(t => t.Format(FormatOptions.Tags));

            build.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void WhenConfigured_ThanTargetsAreRegisteredAndReceiveLogs()
        {
            var target = new SyncRecordingTarget();
            try
            {
                ULogger.Configure(c => c.WriteTo.Target(target));

                ULogger.GetTarget<SyncRecordingTarget>().Should().BeSameAs(target);

                ULogger.GetLogger("Cfg").Log("hello");

                target.LastMessage.Should().Be("hello");
            }
            finally
            {
                ULogger.Terminate();
            }
        }

        [Test]
        public void WhenTwoTargetsOfSameType_ThanConfigureThrows()
        {
            Action configure = () => ULogger.Configure(c => c
                .WriteTo.Target(new SyncRecordingTarget())
                .WriteTo.Target(new SyncRecordingTarget()));

            try
            {
                configure.Should().Throw<InvalidOperationException>();
            }
            finally
            {
                if (ULogger.GetTarget<SyncRecordingTarget>() != null)
                {
                    ULogger.Terminate();
                }
            }
        }

        [Test]
        public void WhenReconfigureThrows_ThanExistingLoggerIsKept()
        {
            var working = new SyncRecordingTarget();
            try
            {
                ULogger.Configure(c => c.WriteTo.Target(working));

                Action reconfigure = () => ULogger.Configure(c => c
                    .WriteTo.Memory(t => t.Capacity(0)));

                reconfigure.Should().Throw<InvalidOperationException>();

                ULogger.GetTarget<SyncRecordingTarget>().Should().BeSameAs(working);
                ULogger.GetLogger("Cfg").Log("still-here");
                working.LastMessage.Should().Be("still-here");
            }
            finally
            {
                ULogger.Terminate();
            }
        }
    }
}
