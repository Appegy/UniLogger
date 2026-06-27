using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Appegy.UniLogger
{
    internal sealed class LogDispatcher : IDisposable
    {
        private readonly ULoggerData _data;
        private readonly BlockingCollection<LogRecord> _queue = new();
        private readonly Thread _thread;
        private volatile bool _completed;

        public LogDispatcher(ULoggerData data)
        {
            _data = data;
            _thread = new Thread(WriteLoop)
            {
                Name = "UniLogger.Writer",
                IsBackground = true,
            };
            _thread.Start();
        }

        public void Enqueue(in LogRecord record)
        {
            if (_completed || _data.Targets.Length == 0) return;
            try
            {
                _queue.Add(record);
            }
            catch (InvalidOperationException)
            {
                // adding completed concurrently with terminate
            }
        }

        public void Dispose()
        {
            if (_completed) return;
            _completed = true;
            _queue.CompleteAdding();
            _thread.Join();
            _queue.Dispose();
        }

        private void WriteLoop()
        {
            while (true)
            {
                LogRecord record;
                try
                {
                    record = _queue.Take();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                do
                {
                    ULogger.Deliver(_data, record);
                }
                while (_queue.TryTake(out record, 0));

                FlushTargets();
            }

            FlushTargets();
        }

        private void FlushTargets()
        {
            var targets = _data.Targets;
            for (var i = 0; i < targets.Length; i++)
            {
                try
                {
                    targets[i].Flush();
                }
                catch
                {
                    // never fail the writer loop on a target flush
                }
            }
        }
    }
}
