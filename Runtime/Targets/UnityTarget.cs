using JetBrains.Annotations;

namespace UnityEngine
{
    public class UnityTarget : TargetBase
    {
        internal UnityTarget([NotNull] Formatter formatter, [NotNull] Filterer filterer) : base(formatter, filterer)
        {
        }
    }
}