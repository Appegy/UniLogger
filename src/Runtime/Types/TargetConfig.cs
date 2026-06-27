using System;

namespace Appegy.UniLogger
{
    public class TargetConfig
    {
        internal TargetBase Target { get; }

        internal TargetConfig(TargetBase target)
        {
            Target = target;
        }

        public TargetConfig With(Formatter formatter)
        {
            Target.Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            return this;
        }

        public TargetConfig With(FormatOptions options)
        {
            Target.Formatter = new Formatter(options);
            return this;
        }

        public TargetConfig With(Filterer filterer)
        {
            Target.Filterer = filterer ?? throw new ArgumentNullException(nameof(filterer));
            return this;
        }
    }
}
