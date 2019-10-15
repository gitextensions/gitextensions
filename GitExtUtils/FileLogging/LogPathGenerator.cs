using System;
using System.IO;

namespace GitExtUtils.FileLogging
{
    public class LogPathGenerator
    {
        /// <summary>
        /// The folder for log files
        /// </summary>
        public readonly string LogPath;

        private Tuple<DateTime, string> _fileNameCache = new Tuple<DateTime, string>(DateTime.MinValue, null);
        private readonly string _logName;
        private readonly int _processId;

        public LogPathGenerator(string logPath, string logName, int processId)
        {
            LogPath = logPath;
            _logName = logName;
            _processId = processId;
        }

        public string GenerateFullPath(DateTime now)
        {
            if (now.Date != _fileNameCache.Item1)
            {
                _fileNameCache = Renew(now);
            }

            return _fileNameCache.Item2;
        }

        public string GetFilenamePattern()
        {
            return $"{_logName}-*-*.log";
        }

        private Tuple<DateTime, string> Renew(DateTime now)
        {
            var today = now.Date;
            var fullPath = Path.Combine(LogPath, $"{_logName}-{today:yyyyMMdd}-{_processId}.log");
            return Tuple.Create(today, fullPath);
        }
    }
}