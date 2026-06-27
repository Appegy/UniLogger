using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Appegy.UniLogger
{
    internal sealed class LogDispatcher : IDisposable
    {
        private readonly struct Item
        {
            public readonly LogRecord Record;
            public readonly ManualResetEventSlim FlushSignal;

            private Item(LogRecord record, ManualResetEventSlim flushSignal)
            {
                Record = record;
                FlushSignal = flushSignal;
            }

            public bool IsFlush => FlushSignal != null;

            public static Item Log(in LogRecord record) => new Item(record, null);
            public static Item Flush(ManualResetEventSlim signal) => new Item(default, signal);
        }

        private readonly ULoggerData _data;
        private readonly BlockingCollection<Item> _queue = new BlockingCollection<Item>();
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
                _queue.Add(Item.Log(record));
            }
            catch (InvalidOperationException)
            {
                // adding completed concurrently with terminate
            }
        }

        public void Flush()
        {
            if (_completed) return;
            using var signal = new ManualResetEventSlim(false);
            try
            {
                _queue.Add(Item.Flush(signal));
            }
            catch (InvalidOperationException)
            {
                return;
            }
            signal.Wait();
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
            var pendingFlushes = new List<ManualResetEventSlim>();
            while (true)
            {
                Item item;
                try
                {
                    item = _queue.Take();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                do
                {
                    if (item.IsFlush)
                    {
                        pendingFlushes.Add(item.FlushSignal);
                    }
                    else
                    {
                        ULogger.Deliver(_data, item.Record);
                    }
                }
                while (_queue.TryTake(out item, 0));

                FlushTargets();
                for (var i = 0; i < pendingFlushes.Count; i++)
                {
                    pendingFlushes[i].Set();
                }
                pendingFlushes.Clear();
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
