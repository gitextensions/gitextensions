using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GitExtUtils.FileLogging
{
    /// <summary>
    /// For unit testing
    /// </summary>
    public class LoggerStub : ILogger
    {
        public List<LogEntry> LogEntries = new List<LogEntry>();

        public void WaitShutdown()
        {
        }

        public void Log(LogSeverity severity, string message, Dictionary<string, string> extra = null)
        {
            LogEntries.Add(LogEntryCreator.Create(severity, message, extra));
        }

        public void Info(string message, Dictionary<string, string> extra = null)
        {
            LogEntries.Add(LogEntryCreator.Create(LogSeverity.Info, message, extra));
        }

        public void Error(string message, Dictionary<string, string> extra = null)
        {
            LogEntries.Add(LogEntryCreator.Create(LogSeverity.Error, message, extra));
        }

        public void Error(string message, Exception e, Dictionary<string, string> extra = null)
        {
            LogEntries.Add(LogEntryCreator.Create(LogSeverity.Error, message, e, extra));
        }

        public void LogGitCommandExecution(string message, string fileName, string arguments, string workingDir, int? processId,
            bool isOnMainThread, TimeSpan? duration, int? exitCode, StackTrace callStack)
        {
            LogEntries.Add(LogEntryCreator.Create(message, fileName, arguments, workingDir, processId, isOnMainThread, duration,
                exitCode, callStack));
        }
    }

    public class Logger : ILogger
    {
        private readonly LogWriter _writer;

        private readonly Task _writerTask;

        public const string MetaTypeKeyName = "_MetaType";

        private static ILogger _instance = new LoggerStub();

        public Logger(
            string gitExtensionsPath,
            ILogFormatter formatter,
            TimeSpan flushInterval,
            TimeSpan maxLogAge,
            int processId,
            CancellationToken cancellationToken)
        {
            if (!Directory.Exists(gitExtensionsPath))
            {
                throw new ArgumentException($"Non-existing folder '{gitExtensionsPath}'", nameof(gitExtensionsPath));
            }

            var logPath = Path.Combine(gitExtensionsPath, "gitextensions");
            var creator = new LogPathGenerator(logPath, "gitextensions", processId);
            _writer = new LogWriter(creator, formatter, flushInterval);

            Task.Run(new LogFileCleaner(this, creator, maxLogAge).Run);

            _writerTask = Task.Run(() => _writer.Run(cancellationToken));

            _instance = this;
        }

        public void WaitShutdown()
        {
            _writerTask?.Wait(TimeSpan.FromSeconds(2));
        }

        public void Log(LogSeverity severity, string message, Dictionary<string, string> extra = null)
        {
            _writer?.Enqueue(LogEntryCreator.Create(severity, message, extra));
        }

        public void Info(string message, Dictionary<string, string> extra = null)
        {
            _writer?.Enqueue(LogEntryCreator.Create(LogSeverity.Info, message, extra));
        }

        public void Error(string message, Dictionary<string, string> extra = null)
        {
            _writer?.Enqueue(LogEntryCreator.Create(LogSeverity.Error, message, extra));
        }

        public void Error(string message, Exception e, Dictionary<string, string> extra = null)
        {
            _writer?.Enqueue(LogEntryCreator.Create(LogSeverity.Error, message, e, extra));
        }

        /// <summary>
        /// shortcut specifically for logging git commands executions
        /// </summary>
        public void LogGitCommandExecution(
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
            _writer?.Enqueue(LogEntryCreator.Create(message, fileName, arguments, workingDir, processId, isOnMainThread, duration, exitCode, callStack));
        }

        public static void LogEntry(LogSeverity severity, string message, Dictionary<string, string> extra = null)
        {
            _instance.Log(severity, message, extra);
        }

        public static void LogInfo(string message, Dictionary<string, string> extra = null)
        {
            _instance.Info(message, extra);
        }

        public static void LogError(string message, Dictionary<string, string> extra = null)
        {
            _instance.Error(message, extra);
        }

        public static void LogError(string message, Exception e, Dictionary<string, string> extra = null)
        {
            _instance.Error(message, e, extra);
        }

        /// <summary>
        /// shortcut specifically for logging git commands executions
        /// </summary>
        public static void LogAGitCommandExecution(
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
            _instance.LogGitCommandExecution(message, fileName, arguments, workingDir, processId, isOnMainThread, duration, exitCode, callStack);
        }
    }
}