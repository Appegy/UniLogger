using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityEngine
{
    // TODO just use File.AppendAllText() instead of Flush
    // TODO work still in progress
    public class FileTarget : Target
    {
        private readonly object _syncRoot = new object();

        private readonly string _logFileDirectory;
        private readonly string _fileName;
        private readonly string _fileExtension;
        private readonly long _fileSizeLimit;
        private readonly int _bufferSize;
        private readonly bool _autoFlush;
        private readonly DateTime _creationTime;

        private FileInfo _logFileInfo;
        private StreamWriter _writer;

        private long _currentFileSize;
        private int _currentFileIndex;

        public FileTarget(string logFilePath, long fileSizeLimit, TimeSpan liveTime, int bufferSize = 1024, bool autoFlush = false, Formatter formatter = null, Filterer filterer = null)
            : base(formatter, filterer)
        {
            _fileName = Path.GetFileNameWithoutExtension(logFilePath);
            _fileExtension = Path.GetExtension(logFilePath);
            _logFileDirectory = Path.GetDirectoryName(logFilePath);
            _fileSizeLimit = fileSizeLimit;
            _bufferSize = bufferSize;
            _autoFlush = autoFlush;

            if (!string.IsNullOrEmpty(_logFileDirectory) && !Directory.Exists(_logFileDirectory))
            {
                Directory.CreateDirectory(_logFileDirectory);
            }

            _currentFileIndex = -1;
            _creationTime = DateTime.UtcNow;
            RemoveOldLogs(liveTime);
            CreateLogFile();
        }

        private void RemoveOldLogs(TimeSpan liveTime)
        {
            var files = GetLogFilePaths()
                .Select(p => new FileInfo(p))
                .Where(f => DateTime.UtcNow - f.LastWriteTimeUtc > liveTime);
            foreach (var file in files)
            {
                Delete(file);
            }
        }

        public override void Log(string message, string stackTrace)
        {
            var sizeOfLine = message.Length + (stackTrace?.Length ?? 0); // because sizeof(char) for UTF8 encoding is 1 byte. UTF-16 encoding is used by default in .NET
            if (_currentFileSize + sizeOfLine > _fileSizeLimit)
            {
                Flush();
                CreateLogFile();
                _currentFileSize = 0;
            }

            lock (_syncRoot)
            {
                _writer.Write(message);
                _currentFileSize += message.Length;

                if (!string.IsNullOrEmpty(stackTrace))
                {
                    _writer.Write("\r\n");
                    _writer.Write(stackTrace);
                    _currentFileSize += stackTrace.Length + 2;
                }
                _writer.Write("\r\n");
                _currentFileSize += 2;
            }
        }

        public override void Flush()
        {
            lock (_syncRoot)
            {
                try
                {
                    _writer.Flush();
                }
                catch
                {
                    GC.SuppressFinalize(_writer);
                    GC.SuppressFinalize(_writer.BaseStream);
                }
            }
        }

        public string[] GetLogFilePaths()
        {
            return Directory.GetFiles(_logFileDirectory);
        }

        public byte[] GetFileContent(FileInfo fileInfo)
        {
            if (IsCurrentLog(fileInfo))
            {
                lock (_syncRoot)
                {
                    Flush();

                    return GetFileContentSafe(fileInfo);
                }
            }
            else
            {
                return GetFileContentSafe(fileInfo);
            }
        }

        public void Delete(FileInfo fileInfo)
        {
            if (!IsCurrentLog(fileInfo))
            {
                File.Delete(fileInfo.FullName);
            }
        }

        public bool IsCurrentLog(FileInfo fileInfo)
        {
            return _logFileInfo != null && _logFileInfo.FullName == fileInfo.FullName;
        }

        private void CreateLogFile()
        {
            _currentFileIndex++;

            var offset = DateTime.UtcNow - _creationTime;
            int minutes = (int)Math.Floor(offset.TotalMinutes);

            var logFilePath = Path.Combine(_logFileDirectory, $"{_fileName}+{minutes}m[{_currentFileIndex}]{_fileExtension}");

            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            try
            {
                _writer?.Dispose();
                var file = File.Open(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(file, Encoding.UTF8, _bufferSize)
                {
                    AutoFlush = _autoFlush
                };
            }
            catch
            {
                _writer = StreamWriter.Null;
            }

            _logFileInfo = new FileInfo(logFilePath);
        }

        private byte[] GetFileContentSafe(FileInfo f)
        {
            string copyName = f.FullName + ".copy";
            byte[] bytes = new byte[f.Length];
            File.Copy(f.FullName, copyName);
            using (var stream = File.OpenRead(copyName))
            {
                stream.Read(bytes, 0, bytes.Length);
            }

            File.Delete(copyName);

            return bytes;
        }
    }
}