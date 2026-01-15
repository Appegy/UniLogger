using UnityEngine;

namespace Appegy.UniLogger
{
    [CreateAssetMenu(fileName = nameof(ULoggerConfigurator), menuName = "ULogger/Configurator")]
    public partial class ULoggerConfigurator : ScriptableObject
    {
        [SerializeField]
        private bool _autoConfigurationAtStart = true;
        [SerializeField]
        private UnityTargetConfig _unityTarget = new UnityTargetConfig();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoConfigureLogger()
        {
            var configurator = Resources.Load<ULoggerConfigurator>("ULoggerConfigurator");
            if (configurator == null || configurator._autoConfigurationAtStart == false) return;
            configurator.ConfigureLogger();
        }

        public void ConfigureLogger()
        {
            var unityFormatterOption = _unityTarget.Formatter;
            ULogger.Initialize(new Formatter(unityFormatterOption));
        }
    }
}