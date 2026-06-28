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

            public RecordingTarget(Formatter formatter = null, Filterer filterer = null)
                : base(formatter, filterer)
            {
            }

            protected internal override void Log(string message, string stackTrace)
            {
                LastMessage = message;
            }

            protected internal override void LogException(Exception exception)
            {
                LastException = exception;
            }
        }

        [Test]
        public void WhenExceptionDelivered_ThanTargetReceivesSameInstance()
        {
            var data = new ULoggerData();
            var target = new RecordingTarget();
            data.AddTarget(target);

            var exception = new InvalidOperationException("boom");
            ULogger.Deliver(data, new LogRecord(exception));

            target.LastException.Should().BeSameAs(exception);
        }

        [Test]
        public void WhenExceptionDelivered_ThanTagFilterIsBypassed()
        {
            var data = new ULoggerData();
            var target = new RecordingTarget(filterer: new Filterer(false));
            data.AddTarget(target);

            var exception = new InvalidOperationException("boom");
            ULogger.Deliver(data, new LogRecord(exception));

            target.LastException.Should().BeSameAs(exception);
        }

        [Test]
        public void WhenExceptionDelivered_ThanRegularLogIsNotTouched()
        {
            var data = new ULoggerData();
            var target = new RecordingTarget();
            data.AddTarget(target);

            ULogger.Deliver(data, new LogRecord(new Exception("boom")));

            target.LastMessage.Should().BeNull();
        }
    }
}
