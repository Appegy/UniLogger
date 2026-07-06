using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.UniLogger
{
    public class NativeLogEventBridgeTests
    {
        [Test]
        public void WhenRaised_ThanThreadedSubscriberReceivesMessage()
        {
            string condition = null;
            string stack = null;
            var type = LogType.Log;
            Application.LogCallback probe = (c, s, t) =>
            {
                if (c == "BRIDGE_MSG")
                {
                    condition = c;
                    stack = s;
                    type = t;
                }
            };

            Application.logMessageReceivedThreaded += probe;
            try
            {
                NativeLogEventBridge.Raise(LogType.Warning, "BRIDGE_MSG", "BRIDGE_STACK", true);
            }
            finally
            {
                Application.logMessageReceivedThreaded -= probe;
            }

            condition.Should().Be("BRIDGE_MSG");
            stack.Should().Be("BRIDGE_STACK");
            type.Should().Be(LogType.Warning);
        }

        [Test]
        public void WhenRaisedOffMainThread_ThanOnlyThreadedSubscriberReceives()
        {
            var threaded = 0;
            var nonThreaded = 0;
            Application.LogCallback threadedProbe = (c, s, t) =>
            {
                if (c == "OFFMAIN_MSG") threaded++;
            };
            Application.LogCallback nonThreadedProbe = (c, s, t) =>
            {
                if (c == "OFFMAIN_MSG") nonThreaded++;
            };

            Application.logMessageReceivedThreaded += threadedProbe;
            Application.logMessageReceived += nonThreadedProbe;
            try
            {
                NativeLogEventBridge.Raise(LogType.Log, "OFFMAIN_MSG", null, false);
            }
            finally
            {
                Application.logMessageReceivedThreaded -= threadedProbe;
                Application.logMessageReceived -= nonThreadedProbe;
            }

            threaded.Should().Be(1);
            nonThreaded.Should().Be(0);
        }

        [Test]
        public void WhenSubscriberRaisesAgain_ThanReentrancyIsBlocked()
        {
            var count = 0;
            Application.LogCallback probe = (c, s, t) =>
            {
                if (c == "REENTER_MSG")
                {
                    count++;
                    NativeLogEventBridge.Raise(LogType.Log, "REENTER_MSG", null, true);
                }
            };

            Application.logMessageReceivedThreaded += probe;
            try
            {
                NativeLogEventBridge.Raise(LogType.Log, "REENTER_MSG", null, true);
            }
            finally
            {
                Application.logMessageReceivedThreaded -= probe;
            }

            count.Should().Be(1);
        }
    }
}
