using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Appegy.UniLogger
{
    public class FileTarget : Target, IDisposable
    {
        private const string SequenceSeparator = "_";
        private const string SequenceFormat = "D3";

        private readonly string _directory;
        private readonly string _baseName;
        private readonly string _extension;
        private readonly long _fileSizeLimitBytes;
        private readonly int _retainedFileCountLimit;
        private readonly bool _autoFlush;
        private readonly Encoding _encoding;

        private StreamWriter _writer;
        private long _currentSize;
        private bool _disposed;

        public FileTarget(
            string path,
            long fileSizeLimitBytes = 0,
            int retainedFileCountLimit = 0,
            bool autoFlush = false,
            Formatter formatter = null,
            Filterer filterer = null)
            : base(formatter, filterer)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path must be provided.", nameof(path));

            var fullPath = Path.GetFullPath(path);
            _directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
            _baseName = Path.GetFileNameWithoutExtension(fullPath);
            _extension = Path.GetExtension(fullPath);
            _fileSizeLimitBytes = fileSizeLimitBytes;
            _retainedFileCountLimit = retainedFileCountLimit;
            _autoFlush = autoFlush;
            _encoding = new UTF8Encoding(false);

            OpenNextFile();
        }

        public string CurrentFilePath { get; private set; }

        protected internal override void Log(in LogEntry entry, string stackTrace)
        {
            if (_disposed || _writer == null) return;

            var message = entry.String;
            var hasStack = !string.IsNullOrEmpty(stackTrace);
            var bytes = _encoding.GetByteCount(message) + 1;
            if (hasStack) bytes += _encoding.GetByteCount(stackTrace) + 1;

            if (_fileSizeLimitBytes > 0 && _currentSize > 0 && _currentSize + bytes > _fileSizeLimitBytes)
            {
                OpenNextFile();
            }

            try
            {
                _writer.Write(message);
                if (hasStack)
                {
                    _writer.Write('\n');
                    _writer.Write(stackTrace);
                }
                _writer.Write('\n');
                _currentSize += bytes;
                if (_autoFlush) _writer.Flush();
            }
            catch
            {
                // never throw from logging
            }
        }

        protected internal override void LogException(Exception exception, in LogEntry entry)
        {
            Log(in entry, null);
        }

        protected internal override void Flush()
        {
            if (_disposed || _writer == null) return;
            try
            {
                _writer.Flush();
            }
            catch
            {
                // never throw from logging
            }
        }

        public string[] GetLogFiles()
        {
            var files = CollectLogFiles();
            files.Sort((a, b) => a.Key.CompareTo(b.Key));
            var result = new string[files.Count];
            for (var i = 0; i < files.Count; i++)
            {
                result[i] = files[i].Value;
            }
            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;
            CloseWriter();
            _disposed = true;
        }

        private void OpenNextFile()
        {
            CloseWriter();

            var sequence = NextSequenceNumber();
            CurrentFilePath = BuildPath(sequence);

            try
            {
                if (_directory.Length > 0 && !Directory.Exists(_directory))
                {
                    Directory.CreateDirectory(_directory);
                }
                var stream = File.Open(CurrentFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(stream, _encoding) { AutoFlush = false };
                _currentSize = stream.Length;
            }
            catch (Exception exception)
            {
                _writer = null;
                Debug.LogError($"FileTarget disabled, could not open '{CurrentFilePath}': {exception.Message}");
                return;
            }

            ApplyRetention();
        }

        private void CloseWriter()
        {
            if (_writer == null) return;
            try
            {
                _writer.Flush();
                _writer.Dispose();
            }
            catch
            {
                // never throw from logging
            }
            _writer = null;
        }

        private int NextSequenceNumber()
        {
            var max = -1;
            foreach (var file in CollectLogFiles())
            {
                if (file.Key > max) max = file.Key;
            }
            return max + 1;
        }

        private void ApplyRetention()
        {
            if (_retainedFileCountLimit <= 0) return;

            var files = CollectLogFiles();
            files.Sort((a, b) => b.Key.CompareTo(a.Key));
            for (var i = _retainedFileCountLimit; i < files.Count; i++)
            {
                try
                {
                    File.Delete(files[i].Value);
                }
                catch
                {
                    // a locked or already removed file must not break logging
                }
            }
        }

        private List<KeyValuePair<int, string>> CollectLogFiles()
        {
            var result = new List<KeyValuePair<int, string>>();
            if (_directory.Length > 0 && !Directory.Exists(_directory)) return result;

            var searchDirectory = _directory.Length > 0 ? _directory : ".";
            foreach (var file in Directory.GetFiles(searchDirectory, _baseName + SequenceSeparator + "*" + _extension))
            {
                var sequence = ParseSequence(Path.GetFileName(file));
                if (sequence >= 0) result.Add(new KeyValuePair<int, string>(sequence, file));
            }
            return result;
        }

        private int ParseSequence(string fileName)
        {
            var prefix = _baseName + SequenceSeparator;
            if (!fileName.StartsWith(prefix, StringComparison.Ordinal)) return -1;
            var withoutExtension = _extension.Length > 0 && fileName.EndsWith(_extension, StringComparison.Ordinal)
                ? fileName.Substring(0, fileName.Length - _extension.Length)
                : fileName;
            var digits = withoutExtension.Substring(prefix.Length);
            return int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var sequence) ? sequence : -1;
        }

        private string BuildPath(int sequence)
        {
            var fileName = _baseName + SequenceSeparator + sequence.ToString(SequenceFormat) + _extension;
            return _directory.Length > 0 ? Path.Combine(_directory, fileName) : fileName;
        }
    }
}
