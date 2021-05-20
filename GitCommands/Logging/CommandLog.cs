using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GitUI;

namespace GitCommands.Logging
{
    // TODO capture number of input bytes
    // TODO capture number of standard output bytes
    // TODO capture number of standard error bytes

    public sealed class ProcessOperation
    {
        private readonly CommandLogEntry _entry;
        private readonly Stopwatch _stopwatch;
        private readonly Action _raiseCommandsChanged;

        internal ProcessOperation(CommandLogEntry entry, Stopwatch stopwatch, Action raiseCommandsChanged)
        {
            _entry = entry;
            _stopwatch = stopwatch;
            _raiseCommandsChanged = raiseCommandsChanged;
        }

        public void LogProcessEnd(int exitCode) => LogProcessEnd(exitCode, ex: null);

        public void LogProcessEnd(Exception ex) => LogProcessEnd(exitCode: null, ex);

        public void SetProcessId(int processId)
        {
            try
            {
                _entry.ProcessId = processId;
                _raiseCommandsChanged();
            }
            catch (Exception)
            {
                // do not disturb the actual operation
            }
        }

        public void NotifyDisposed()
        {
            if (_entry.Duration is null)
            {
                LogProcessEnd();
            }
        }

        private void LogProcessEnd(int? exitCode = null, Exception? ex = null)
        {
            try
            {
                _entry.Duration = _stopwatch.Elapsed;
                _entry.ExitCode = exitCode;
                _entry.Exception = ex;
                _raiseCommandsChanged();
            }
            catch (Exception)
            {
                // do not disturb the actual operation
            }
        }
    }

    public sealed class CommandLogEntry
    {
        public string FileName { get; }
        public string Arguments { get; }
        public string WorkingDir { get; }
        private DateTime StartedAt { get; }
        public bool IsOnMainThread { get; }
        public TimeSpan? Duration { get; internal set; }
        public int? ProcessId { get; set; }
        public int? ExitCode { get; set; }
        public Exception? Exception { get; set; }
        public StackTrace? CallStack { get; set; }

        internal CommandLogEntry(string fileName, string arguments, string workingDir, DateTime startedAt, bool isOnMainThread)
        {
            FileName = fileName;
            Arguments = arguments;
            WorkingDir = workingDir;
            StartedAt = startedAt;
            IsOnMainThread = isOnMainThread;
        }

        public string ColumnLine
        {
            get
            {
                var duration = Duration is null
                    ? "running"
                    : $"{((TimeSpan)Duration).TotalMilliseconds:0,0} ms";

                var fileName = FileName;

                if (fileName.EndsWith("git.exe"))
                {
                    fileName = "git";
                }

                var exit = ExitCode is not null ? $"{ExitCode}" : Exception is not null ? "exc" : string.Empty;
                var ex = Exception is not null ? $"  {Exception.GetType().Name}: {Exception.Message}" : string.Empty;

                return $"{StartedAt:HH:mm:ss.fff} {duration,9} {ProcessId,5} {(IsOnMainThread ? "UI" : "  ")} {exit,3} {fileName} {Arguments}{ex}";
            }
        }

        public string FullLine(string sep)
        {
            var duration = Duration is null ? "" : $"{((TimeSpan)Duration).TotalMilliseconds:0}";

            var fileName = FileName;

            if (fileName.EndsWith("git.exe"))
            {
                fileName = "git";
            }

            var exit = ExitCode is not null ? $"{ExitCode}" : Exception is not null ? "exc" : string.Empty;
            var ex = Exception is not null ? $"{Environment.NewLine}{Exception}" : string.Empty;
            var callStack = CallStack is not null ? $"{Environment.NewLine}{CallStack}" : string.Empty;

            return $"{StartedAt:O}{sep}{duration}{sep}{ProcessId}{sep}{(IsOnMainThread ? "UI" : "")}{sep}{exit}{sep}{fileName}{sep}{Arguments}{sep}{WorkingDir}{callStack}{ex}";
        }

        public string Detail
        {
            get
            {
                StringBuilder s = new();

                s.Append("File name:   ").AppendLine(FileName);
                s.Append("Arguments:   ").AppendLine(Arguments);
                s.Append("Working dir: ").AppendLine(WorkingDir);
                s.Append("Process ID:  ").AppendLine(ProcessId is not null ? $"{ProcessId}" : "unknown");
                s.Append("Started at:  ").AppendLine($"{StartedAt:O}");
                s.Append("UI Thread?:  ").AppendLine($"{IsOnMainThread}");
                s.Append("Duration:    ").AppendLine(Duration is not null ? $"{Duration.Value.TotalMilliseconds:0.###} ms" : "still running");
                s.Append("Exit code:   ").AppendLine(ExitCode is not null ? $"{ExitCode}" : Exception is not null ? $"{Exception}" : "unknown");
                s.Append("Call stack:  ").Append(CallStack is not null ? $"{Environment.NewLine}{CallStack}" : "not captured");

                return s.ToString();
            }
        }
    }

    public static class CommandLog
    {
        public static event Action? CommandsChanged;

        private static ConcurrentQueue<CommandLogEntry> _queue = new();

        public static IEnumerable<CommandLogEntry> Commands => _queue;

        public static ProcessOperation LogProcessStart(string fileName, string arguments = "", string workDir = "")
        {
            const int MaxEntryCount = 500;

            CommandLogEntry entry = new(fileName, arguments, workDir, DateTime.Now, ThreadHelper.JoinableTaskContext.IsOnMainThread);

            if (AppSettings.LogCaptureCallStacks)
            {
                entry.CallStack = new StackTrace();
            }

            _queue.Enqueue(entry);

            // Trim extra items
            while (_queue.Count >= MaxEntryCount)
            {
                _queue.TryDequeue(out _);
            }

            CommandsChanged?.Invoke();

            return new ProcessOperation(entry, Stopwatch.StartNew(), () => CommandsChanged?.Invoke());
        }

        public static void Clear()
        {
            _queue = new ConcurrentQueue<CommandLogEntry>();
            CommandsChanged?.Invoke();
        }
    }
}
