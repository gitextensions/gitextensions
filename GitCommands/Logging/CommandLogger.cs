using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="executionStartTimestamp">The start time for the command</param>
        /// <param name="executionEndTimestamp">The end time for the command, ignored if this is start of a command</param>
        /// <param name="startCmd">The log entry for the start command, to be able to identify start/end pairs</param>
        /// <returns>The log entry reference, null if not added</returns>
        public CommandLogEntry Log(string command, DateTime executionStartTimestamp, DateTime executionEndTimestamp, CommandLogEntry startCmd = null, bool isStart = false)
        {
            if (!AppSettings.EnhancedGitLog)
            {
                if (isStart)
                {
                    return null;
                }

                startCmd = null;
            }

            CommandLogEntry commandLogEntry = null;
            lock (_logQueue)
            {
                if (_logQueue.Count >= LogLimit)
                {
                    _logQueue.Dequeue();
                }

                commandLogEntry = new CommandLogEntry(command, executionStartTimestamp, executionEndTimestamp, startCmd, isStart);
                _logQueue.Enqueue(commandLogEntry);
            }

            CommandsChanged?.Invoke(this, EventArgs.Empty);
            return commandLogEntry;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, GetCommands().Select(cle => cle.ToString()));
        }
    }
}