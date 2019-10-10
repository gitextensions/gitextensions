using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GitExtUtils.FileLogging
{
    public interface ILogger
    {
        void WaitShutdown();

        void Log(LogSeverity severity, string message, Dictionary<string, string> extra = null);

        void Info(string message, Dictionary<string, string> extra = null);

        void Error(string message, Dictionary<string, string> extra = null);

        void Error(string message, Exception e, Dictionary<string, string> extra = null);

        void LogGitCommandExecution(
            string message,
            string fileName,
            string arguments,
            string workingDir,
            int? processId,
            bool isOnMainThread,
            TimeSpan? duration,
            int? exitCode,
            StackTrace callStack);
    }
}