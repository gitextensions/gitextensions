using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GitExtUtils.FileLogging
{
    internal class LogEntryCreator
    {
        public static LogEntry Create(LogSeverity severity, string message, Dictionary<string, string> extra = null)
        {
            extra = extra ?? new Dictionary<string, string>();
            if (!extra.ContainsKey(Logger.MetaTypeKeyName))
            {
                extra.Add(Logger.MetaTypeKeyName, "Application");
            }

            var entry = new LogEntry()
            {
                Severity = severity,
                Time = DateTime.Now,
                Message = message,
                ExtraInfo = extra,
            };

            return entry;
        }

        public static LogEntry Create(LogSeverity severity, string message, Exception e, Dictionary<string, string> extra = null)
        {
            extra = extra ?? new Dictionary<string, string>();

            if (e != null && !extra.ContainsKey("Stacktrace"))
            {
                extra.Add("Stacktrace", e.ToStringDemystified());
            }

            return Create(LogSeverity.Error, message, extra);
        }

        public static LogEntry Create(
            string message,
            string fileName,
            string arguments,
            string workingDir,
            int? processId,
            bool isOnMainThread,
            TimeSpan? duration,
            int? exitCode,
            StackTrace callStack)
        {
            var extra = new Dictionary<string, string>()
            {
                { Logger.MetaTypeKeyName, "GitExeCommand" },
                { "Filename", fileName },
                { "Arguments", arguments },
                { "WorkingDir", workingDir },
                { "ProcessId", processId?.ToString() },
                { "IsOnMainThread", isOnMainThread ? "UI" : "  " },
                { "Duration", duration == null ? "" : $"{(int)duration.Value.TotalMilliseconds:##,#} ms" },
                { "ExitCode", exitCode == null ? "" : exitCode.ToString() },
                { "CallStack", callStack?.ToString() },
            };

            return Create(LogSeverity.Info, message, extra);
        }
    }
}