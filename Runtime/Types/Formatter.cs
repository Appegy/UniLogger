using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
    public class Formatter
    {

        private static readonly Dictionary<Tag, Color> _coloredCategories = new Dictionary<Tag, Color>();

        private readonly bool _richText;
        private readonly bool _showTime;
        private readonly bool _showThread;
        private readonly bool _showTagCategory;
        private readonly bool _showTagName;
        private readonly bool _showType;

        public Formatter(FormatOptions options = FormatOptions.None)
        {
            _richText = options.HasFlag(FormatOptions.RichText);
            _showTime = options.HasFlag(FormatOptions.Time);
            _showThread = options.HasFlag(FormatOptions.Thread);
            _showTagCategory = options.HasFlag(FormatOptions.TagCategory);
            _showTagName = options.HasFlag(FormatOptions.TagName);
            _showType = options.HasFlag(FormatOptions.LogType);
        }

        public string Format(LogEntry line)
        {
            StringBuilder builder = null;
            try
            {
                builder = StringBuilderPool.GetBuilder();
                AppendTime(line, builder);
                AppendType(line, builder);
                AppendThread(line, builder);
                AppendTag(line, builder);
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

        private void AppendTag(LogEntry line, StringBuilder builder)
        {
            if (!_showTagCategory && !_showTagName) return;
            if (_richText)
            {
                builder.Append("<color=#");
                builder.Append(ColorUtility.ToHtmlStringRGBA(GetColor(line.Tag)));
                builder.Append(">");
                builder.Append("[");
                if (_showTagCategory) builder.Append(line.Tag.Category);
                if (_showTagCategory && _showTagName) builder.Append(":");
                if (_showTagName) builder.Append(line.Tag.Name);
                builder.Append("]</color> ");
            }
            else
            {
                builder.Append("[");
                if (_showTagCategory) builder.Append(line.Tag.Category);
                if (_showTagCategory && _showTagName) builder.Append(":");
                if (_showTagName) builder.Append(line.Tag.Name);
                builder.Append("] ");
            }
        }

        private void AppendMessage(LogEntry line, StringBuilder builder)
        {
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

        private static Color GetColor(Tag tag)
        {
            Color color;
            lock (_coloredCategories)
            {
                if (_coloredCategories.TryGetValue(tag, out color)) return color;
                var seed = tag.ToLongString().GetHashCode();
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