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

        public void Log(string command, DateTime executionStartTimestamp, DateTime executionEndTimestamp)
        {
            lock (_logQueue)
            {
                if (_logQueue.Count >= LogLimit)
                {
                    _logQueue.Dequeue();
                }

                var commandLogEntry = new CommandLogEntry(command, executionStartTimestamp, executionEndTimestamp);
                _logQueue.Enqueue(commandLogEntry);
            }

            CommandsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, GetCommands().Select(cle => cle.ToString()));
        }
    }
}