using System;
using UnityEngine;

namespace Appegy.UniLogger
{
    [Serializable]
    public class UnityTargetConfig : LoggerTargetConfig
    {
        private const FormatOptions DefaultFormatterOptions = FormatOptions.Tags | FormatOptions.LogType;
        private const FormatOptions EditorFormatterOptions = FormatOptions.RichText | FormatOptions.Tags;

        [SerializeField]
        private bool _overrideForEditor = true;
        [SerializeField]
        private FormatOptions _editorFormatter = EditorFormatterOptions;

        public override FormatOptions Formatter => (Application.isEditor && _overrideForEditor) ? _editorFormatter : base.Formatter;

        public UnityTargetConfig()
            : base(DefaultFormatterOptions)
        {
        }
    }
}