using System.Collections.Concurrent;
using System.Text;

namespace UnityEngine
{
    internal static class StringBuilderPool
    {
        private const int MaxCapacity = 2000;
        private const int InitialCapacity = 100;

        private static readonly ConcurrentBag<StringBuilder> _objects = new ConcurrentBag<StringBuilder>();

        public static StringBuilder GetBuilder()
        {
            if (!_objects.TryTake(out var builder))
            {
                builder = new StringBuilder(InitialCapacity);
            }
            return builder;
        }

        public static void ReturnBuilder(StringBuilder builder)
        {
            if (builder == null)
            {
                return;
            }
            builder.Length = 0;
            if (builder.Capacity > MaxCapacity)
            {
                builder.Capacity = MaxCapacity;
            }
            _objects.Add(builder);
        }
    }
}