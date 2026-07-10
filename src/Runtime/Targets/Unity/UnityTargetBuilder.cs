using JetBrains.Annotations;

namespace Appegy.UniLogger
{
    public sealed class UnityTargetBuilder
    {
        private FormatOptions _format = FormatOptions.None;
        private Filterer _filterer;

        public UnityTargetBuilder Format(FormatOptions options)
        {
            _format = options;
            return this;
        }

        public UnityTargetBuilder Filter([CanBeNull] Filterer filterer)
        {
            _filterer = filterer;
            return this;
        }

        internal UnityTarget Build()
        {
            var formatter = new Formatter(_format);
            var filterer = _filterer ?? Filterer.AllowAllTags();
            return new UnityTarget(formatter, filterer);
        }
    }
}
