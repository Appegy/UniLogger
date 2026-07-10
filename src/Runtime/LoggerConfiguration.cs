using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Appegy.UniLogger
{
    public sealed class LoggerConfiguration
    {
        private readonly List<Target> _targets = new();

        public LoggerConfiguration()
        {
            WriteTo = new LoggerSinkConfiguration(this);
        }

        public LoggerSinkConfiguration WriteTo { get; }

        internal IReadOnlyList<Target> BuildTargets() => _targets;

        internal LoggerConfiguration Add(Target target)
        {
            _targets.Add(target);
            return this;
        }
    }

    public sealed class LoggerSinkConfiguration
    {
        private readonly LoggerConfiguration _configuration;

        internal LoggerSinkConfiguration(LoggerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoggerConfiguration Unity([CanBeNull] Action<UnityTargetBuilder> configure = null)
        {
            var builder = new UnityTargetBuilder();
            configure?.Invoke(builder);
            return _configuration.Add(builder.Build());
        }

        public LoggerConfiguration File([NotNull] Action<FileTargetBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var builder = new FileTargetBuilder();
            configure.Invoke(builder);
            return _configuration.Add(builder.Build());
        }

        public LoggerConfiguration Memory([NotNull] Action<MemoryTargetBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var builder = new MemoryTargetBuilder();
            configure.Invoke(builder);
            return _configuration.Add(builder.Build());
        }

        public LoggerConfiguration Target([NotNull] Target target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return _configuration.Add(target);
        }
    }
}
