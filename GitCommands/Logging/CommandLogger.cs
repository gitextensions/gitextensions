using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GitCommands.Logging
{
    public sealed class CommandLogger
    {
        private const int LogLimit = 500;
        private readonly Queue<CommandLogEntry> _logQueue = new Queue<CommandLogEntry>(LogLimit);

        public event EventHandler CommandsChanged;

        public CommandLogEntry[] GetCommands()
        {
            lock (_logQueue)
            {
                return _logQueue.ToArray();
            }
        }

        /// <summary>
        /// Add the command to the log fifo queue
        /// </summary>
        /// <param name="command">The (Git) command to log</param>
        /// <param name="counter">The log entry for the start command, to be able to identify start/end pairs</param>
        /// <param name="timestamp">The time for the log</param>
        /// <returns>The log entry reference, null if not added</returns>
        public CommandLogEntry LogEntry(string command, int counter, DateTime timestamp)
        {
            CommandLogEntry commandLogEntry = null;
            lock (_logQueue)
            {
                if (_logQueue.Count >= LogLimit)
                {
                    _logQueue.Dequeue();
                }

                commandLogEntry = new CommandLogEntry(command, counter, timestamp);
                _logQueue.Enqueue(commandLogEntry);
            }

            CommandsChanged?.Invoke(this, EventArgs.Empty);
            return commandLogEntry;
        }

        public void LogEnd()
        {
            CommandsChanged?.Invoke(this, EventArgs.Empty);
        }

        public class GitLogEntry
        {
            public GitLogEntry(CommandLogger log, string fileName, string arguments)
            {
                string command = System.IO.Path.GetFileName(fileName) + " " + arguments;
                int counter = Interlocked.Increment(ref globalCounter);
                _log = log;

                _stopwatch = new Stopwatch();
                _entry = _log.LogEntry(command, counter, DateTime.Now);
                _stopwatch.Start();
            }

            public void LogEnd()
            {
                _stopwatch.Stop();
                _entry.Duration = _stopwatch.Elapsed;
                _log.LogEnd();
            }

            private CommandLogger _log;
            private Stopwatch _stopwatch;
            private CommandLogEntry _entry;

            private static int globalCounter = 0;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, GetCommands().Select(cle => cle.ToString()));
        }

        public GitLogEntry Log(string fileName, string arguments = "")
        {
            return new GitLogEntry(this, fileName, arguments);
        }
    }
}