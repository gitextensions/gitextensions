using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using GitUI;

namespace GitCommands.Logging
{
    // TODO capture process exit code
    // TODO capture process working directory
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

        public void LogProcessEnd()
        {
            _entry.Duration = _stopwatch.Elapsed;
            _raiseCommandsChanged();
        }

        public void SetProcessId(int processId)
        {
            _entry.ProcessId = processId;
            _raiseCommandsChanged();
        }
    }

    public sealed class CommandLogEntry
    {
        public string FileName { get; }
        public string Arguments { get; }
        private DateTime StartedAt { get; }
        public bool IsOnMainThread { get; }
        public TimeSpan? Duration { get; internal set; }
        public int? ProcessId { get; set; }

        internal CommandLogEntry(string fileName, string arguments, DateTime startedAt, bool isOnMainThread)
        {
            FileName = fileName;
            Arguments = arguments;
            StartedAt = startedAt;
            IsOnMainThread = isOnMainThread;
        }

        public override string ToString()
        {
            var duration = Duration == null
                ? "running"
                : $"{((TimeSpan)Duration).TotalMilliseconds:0}ms";

            var fileName = FileName;

            if (fileName.EndsWith("git.exe"))
            {
                fileName = "git";
            }

            var pid = ProcessId == null ? "     " : $"{ProcessId,5}";

            return $"{StartedAt:HH:mm:ss.fff} {duration,7} {pid} {(IsOnMainThread ? "UI" : "  ")} {fileName} {Arguments}";
        }
    }

    public static class CommandLog
    {
        public static event Action CommandsChanged;

        private static ConcurrentQueue<CommandLogEntry> _queue = new ConcurrentQueue<CommandLogEntry>();

        public static IEnumerable<CommandLogEntry> Commands => _queue;

        public static ProcessOperation LogProcessStart(string fileName, string arguments = "")
        {
            const int MaxEntryCount = 500;

            var entry = new CommandLogEntry(fileName, arguments, DateTime.Now, ThreadHelper.JoinableTaskContext.IsOnMainThread);

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