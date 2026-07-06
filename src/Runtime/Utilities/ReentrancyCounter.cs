using System.Threading;

namespace Appegy.UniLogger
{
    internal static class ReentrancyCounter
    {
        private static readonly ThreadLocal<int> _depth = new();

        public static bool IsEntered => _depth.Value > 0;

        public static Scope Enter()
        {
            _depth.Value++;
            return default;
        }

        public readonly ref struct Scope
        {
            public void Dispose()
            {
                if (_depth.Value > 0)
                {
                    _depth.Value--;
                }
            }
        }
    }
}
