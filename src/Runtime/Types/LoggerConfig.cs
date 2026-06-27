using System;
using System.Collections.Generic;

namespace Appegy.UniLogger
{
    public class LoggerConfig
    {
        private readonly TargetConfig _console;
        private readonly List<TargetConfig> _targets = new List<TargetConfig>();

        internal LoggerConfig(UnityTarget console)
        {
            _console = new TargetConfig(console);
        }

        public TargetConfig Console()
        {
            return _console;
        }

        public TargetConfig AddTarget<T>(T target) where T : Target
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            var type = target.GetType();
            foreach (var existing in _targets)
            {
                if (existing.Target.GetType() == type)
                {
                    throw new InvalidOperationException($"A target of type '{type.Name}' is already registered.");
                }
            }
            var config = new TargetConfig(target);
            _targets.Add(config);
            return config;
        }

        internal UnityTarget ConsoleTarget => (UnityTarget)_console.Target;

        internal Target[] BuildTargets()
        {
            var result = new Target[_targets.Count];
            for (var i = 0; i < _targets.Count; i++)
            {
                result[i] = (Target)_targets[i].Target;
            }
            return result;
        }
    }
}
