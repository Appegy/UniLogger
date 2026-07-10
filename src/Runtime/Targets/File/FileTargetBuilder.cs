using System;
using JetBrains.Annotations;

namespace Appegy.UniLogger
{
    public sealed class FileTargetBuilder
    {
        private string _path;
        private FormatOptions _format = FormatOptions.None;
        private Filterer _filterer;
        private long _rollEveryBytes;
        private int _retainedFileCount;
        private bool _autoFlush;

        public FileTargetBuilder Path([NotNull] string path)
        {
            _path = path;
            return this;
        }

        public FileTargetBuilder Format(FormatOptions options)
        {
            _format = options;
            return this;
        }

        public FileTargetBuilder Filter([CanBeNull] Filterer filterer)
        {
            _filterer = filterer;
            return this;
        }

        public FileTargetBuilder RollEvery(long bytes)
        {
            _rollEveryBytes = bytes;
            return this;
        }

        public FileTargetBuilder Keep(int count)
        {
            _retainedFileCount = count;
            return this;
        }

        public FileTargetBuilder AutoFlush(bool enabled = true)
        {
            _autoFlush = enabled;
            return this;
        }

        internal FileTarget Build()
        {
            if (string.IsNullOrEmpty(_path))
            {
                throw new InvalidOperationException("File target requires a path. Use .Path(...).");
            }
            var formatter = new Formatter(_format);
            var filterer = _filterer ?? Filterer.AllowAllTags();
            return new FileTarget(_path, _rollEveryBytes, _retainedFileCount, _autoFlush, formatter, filterer);
        }
    }
}
