using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Appegy.UniLogger
{
    public class Formatter
    {
        private const float TagColorSaturation = 0.7f;
        private const float TagColorValue = 0.8f;

        private static readonly Dictionary<string, string> _tagColorPrefixes = new Dictionary<string, string>();

        public FormatOptions FormatOptions { get; set; }

        private bool AnyFormat => FormatOptions != FormatOptions.None;
        private bool RichText => FormatOptions.HasFlagFast(FormatOptions.RichText);
        private bool ShowTime => FormatOptions.HasFlagFast(FormatOptions.Time);
        private bool ShowThread => FormatOptions.HasFlagFast(FormatOptions.Thread);
        private bool ShowTagName => FormatOptions.HasFlagFast(FormatOptions.Tags);
        private bool ShowType => FormatOptions.HasFlagFast(FormatOptions.LogType);

        public Formatter(FormatOptions options = FormatOptions.None)
        {
            FormatOptions = options;
        }

        public string Format(LogEntry line)
        {
            if (!AnyFormat)
            {
                return line.String;
            }

            StringBuilder builder = null;
            try
            {
                builder = StringBuilderPool.GetBuilder();
                AppendTime(line, builder);
                AppendType(line, builder);
                AppendThread(line, builder);
                AppendTags(line, builder);
                AppendMessage(line, builder);

                return builder.ToString();
            }
            finally
            {
                StringBuilderPool.ReturnBuilder(builder);
            }
        }

        private void AppendTime(LogEntry line, StringBuilder builder)
        {
            if (!ShowTime) return;
            if (RichText)
            {
                builder.Append("[<i><color=yellow>");
                builder.Append(line.LogTime.ToString("HH:mm:ss:fff"));
                builder.Append("</color></i>]");
            }
            else
            {
                builder.Append("[");
                builder.Append(line.LogTime.ToString("HH:mm:ss:fff"));
                builder.Append("]");
            }
        }

        private void AppendType(LogEntry line, StringBuilder builder)
        {
            if (!ShowType) return;
            if (RichText)
            {
                builder.Append("[<b><color=");
                builder.Append(line.LogLevel.ToMessageColor());
                builder.Append(">");
                builder.Append(line.LogLevel.ToShortString());
                builder.Append("</color></b>]");
            }
            else
            {
                builder.Append("[");
                builder.Append(line.LogLevel.ToShortString());
                builder.Append("]");
            }
        }

        private void AppendThread(LogEntry line, StringBuilder builder)
        {
            if (!ShowThread) return;
            builder.Append("[TH=");
            builder.Append(line.ThreadId);
            builder.Append("]");
        }

        private void AppendTags(LogEntry line, StringBuilder builder)
        {
            if (!ShowTagName) return;

            for (var i = 0; i < line.Tags.Count; i++)
            {
                var tag = line.Tags[i];
                if (RichText)
                {
                    builder.Append(GetTagColorPrefix(tag));
                    builder.Append("[");
                    builder.Append(tag);
                    builder.Append("]</color>");
                }
                else
                {
                    builder.Append("[");
                    builder.Append(tag);
                    builder.Append("]");
                }
            }
        }

        private void AppendMessage(LogEntry line, StringBuilder builder)
        {
            if (AnyFormat)
            {
                builder.Append(' ');
            }
            if (RichText && line.IsColored)
            {
                builder.Append("<color=#");
                builder.Append(ColorUtility.ToHtmlStringRGBA(line.Color));
                builder.Append(">");
                builder.Append(line.String);
                builder.Append("</color>");
            }
            else
            {
                builder.Append(line.String);
            }
        }

        private static string GetTagColorPrefix(string tag)
        {
            lock (_tagColorPrefixes)
            {
                if (_tagColorPrefixes.TryGetValue(tag, out var prefix)) return prefix;

                var hue = (StableHash(tag) & 0xFFFFFF) / (float)0x1000000;
                var color = Color.HSVToRGB(hue, TagColorSaturation, TagColorValue);
                prefix = "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">";

                _tagColorPrefixes[tag] = prefix;
                return prefix;
            }
        }

        private static uint StableHash(string tag)
        {
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;
            var hash = offsetBasis;
            for (var i = 0; i < tag.Length; i++)
            {
                hash ^= tag[i];
                hash *= prime;
            }
            return hash;
        }
    }
}