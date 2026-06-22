using JetBrains.Annotations;

namespace Appegy.UniLogger
{
    public class UnityTarget : TargetBase
    {
        internal UnityTarget([NotNull] Formatter formatter, [NotNull] Filterer filterer) : base(formatter, filterer)
        {
        }
    }
}