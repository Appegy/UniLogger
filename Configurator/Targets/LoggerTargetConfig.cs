using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    [Serializable]
    public abstract class LoggerTargetConfig
    {
        [SerializeField]
        private FormatOptions _formatter;

        public virtual FormatOptions Formatter => _formatter;

        protected LoggerTargetConfig(FormatOptions formatter)
        {
            _formatter = formatter;
        }
    }
}