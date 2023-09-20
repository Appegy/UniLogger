using System.Collections.Generic;
using System.Text;

namespace UnityEngine
{
    public class Formatter
    {
        private static readonly Dictionary<string, Color> _coloredCategories = new Dictionary<string, Color>();

        private readonly bool _anyFormat;
        private readonly bool _richText;
        private readonly bool _showTime;
        private readonly bool _showThread;
        private readonly bool _showTagName;
        private readonly bool _showType;

        public Formatter(FormatOptions options = FormatOptions.None)
        {
            _anyFormat = options != FormatOptions.None;
            _richText = options.HasFlag(FormatOptions.RichText);
            _showTime = options.HasFlag(FormatOptions.Time);
            _showThread = options.HasFlag(FormatOptions.Thread);
            _showTagName = options.HasFlag(FormatOptions.Tags);
            _showType = options.HasFlag(FormatOptions.LogType);
        }

        public string Format(LogEntry line)
        {
            if (!_anyFormat)
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
            if (!_showTime) return;
            if (_richText)
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
            if (!_showType) return;
            if (_richText)
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
            if (!_showThread) return;
            builder.Append("[TH=");
            builder.Append(line.ThreadId);
            builder.Append("]");
        }

        private void AppendTags(LogEntry line, StringBuilder builder)
        {
            if (!_showTagName) return;

            foreach (var tag in line.Tags)
            {
                if (_richText)
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
            if (_anyFormat)
            {
                builder.Append(' ');
            }
            if (_richText && line.IsColored)
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
                var r = GetRandomNumberInRange(rnd, 0.5f, 0.97f);
                var g = GetRandomNumberInRange(rnd, 0.5f, 0.97f);
                var b = GetRandomNumberInRange(rnd, 0.5f, 0.97f);
                color = new Color(r, g, b);
                _coloredCategories[tag] = color;
            }
            return color;
        }

        private static float GetRandomNumberInRange(System.Random rnd, float minNumber, float maxNumber)
        {
            return (float)rnd.NextDouble() * (maxNumber - minNumber) + minNumber;
        }
    }
}