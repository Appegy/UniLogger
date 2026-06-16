using System.Collections.Generic;
using System.Text;

namespace UnityEngine
{
    public class Formatter
    {
        private static readonly Dictionary<string, Color> _coloredCategories = new Dictionary<string, Color>();

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

            foreach (var tag in line.Tags)
            {
                if (RichText)
                {
                    builder.Append("<color=#");
                    builder.Append(ColorUtility.ToHtmlStringRGBA(GetColor(tag)));
                    builder.Append(">");
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

        private static Color GetColor(string tag)
        {
            // TODO Create Tag class with Name and Color and cache Color on logger creation
            Color color;
            lock (_coloredCategories)
            {
                if (_coloredCategories.TryGetValue(tag, out color)) return color;
                var seed = tag.GetHashCode();
                var rnd = new System.Random(seed);

                // TODO find convenient parameters for generating colors for both light and dark themes
                var h = GetRandomNumberInRange(rnd, 0.4f, 0.6f);
                var s = GetRandomNumberInRange(rnd, 0.9f, 1.0f);
                var v = GetRandomNumberInRange(rnd, 0.0f, 1.0f);

                color = Color.HSVToRGB(h, s, v);
                _coloredCategories[tag] = color;
            }
            return color;
        }

        private static float GetRandomNumberInRange(System.Random rnd, float minNumber, float maxNumber)
        {
            return (float)(rnd.NextDouble() * (maxNumber - minNumber) + minNumber);
        }
    }
}