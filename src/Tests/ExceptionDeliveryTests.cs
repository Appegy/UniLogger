using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class ExceptionDeliveryTests
    {
        private sealed class RecordingTarget : Target
        {
            public string LastMessage;
            public Exception LastException;
            public string LastExceptionMessage;

            public RecordingTarget(Formatter formatter = null, Filterer filterer = null)
                : base(formatter, filterer)
            {
            }

            protected internal override void Log(in LogEntry entry, string stackTrace)
            {
                LastMessage = entry.String;
            }

            protected internal override void LogException(Exception exception, in LogEntry entry)
            {
                LastException = exception;
                LastExceptionMessage = entry.String;
            }
        }

        private static LogRecord ExceptionRecord(Exception exception, string message)
        {
            var entry = new LogEntry(null, LogLevel.Error, message, default, null);
            return new LogRecord(exception, in entry);
        }

        [Test]
        public void WhenExceptionDelivered_ThanTargetReceivesSameInstanceAndMessage()
        {
            var data = new ULoggerData();
            var target = new RecordingTarget();
            data.AddTarget(target);

            var exception = new InvalidOperationException("boom");
            ULogger.Deliver(data, ExceptionRecord(exception, "formatted text"));

            target.LastException.Should().BeSameAs(exception);
            target.LastExceptionMessage.Should().Be("formatted text");
        }

        [Test]
        public void WhenExceptionDelivered_ThanTagFilterIsBypassed()
        {
            var data = new ULoggerData();
            var target = new RecordingTarget(filterer: new Filterer(false));
            data.AddTarget(target);

            var exception = new InvalidOperationException("boom");
            ULogger.Deliver(data, ExceptionRecord(exception, "formatted text"));

            target.LastException.Should().BeSameAs(exception);
        }

        [Test]
        public void WhenExceptionDelivered_ThanRegularLogIsNotTouched()
        {
            var data = new ULoggerData();
            var target = new RecordingTarget();
            data.AddTarget(target);

            ULogger.Deliver(data, ExceptionRecord(new Exception("boom"), "formatted text"));

            target.LastMessage.Should().BeNull();
        }
    }
}
