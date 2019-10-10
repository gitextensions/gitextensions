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

        public LogPathGenerator(string logPath, string logName)
        {
            LogPath = logPath;
            _logName = logName;
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
            return $"{_logName}-*.log";
        }

        private Tuple<DateTime, string> Renew(DateTime now)
        {
            var today = now.Date;
            var fullPath = Path.Combine(LogPath, $"{_logName}-{today:yyyy-MM-dd}.log");
            return Tuple.Create(today, fullPath);
        }
    }
}