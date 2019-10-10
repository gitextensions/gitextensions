using System;
using System.Collections.Generic;

namespace GitExtUtils.FileLogging
{
    /// <summary>
    /// Tiniest definition for now
    /// </summary>
    public class LogEntry
    {
        public DateTime Time { get; set; }
        public LogSeverity Severity { get; set; }
        public string Message { get; set; }

        public Dictionary<string, string> ExtraInfo { get; set; }
    }
}