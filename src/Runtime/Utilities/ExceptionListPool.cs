using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Appegy.UniLogger
{
    internal static class ExceptionListPool
    {
        private const int MaxCapacity = 64;

        private static readonly ConcurrentBag<List<Exception>> _objects = new();

        public static List<Exception> Get()
        {
            if (!_objects.TryTake(out var list))
            {
                list = new List<Exception>();
            }
            return list;
        }

        public static void Return(List<Exception> list)
        {
            if (list == null)
            {
                return;
            }
            list.Clear();
            if (list.Capacity <= MaxCapacity)
            {
                _objects.Add(list);
            }
        }
    }
}
