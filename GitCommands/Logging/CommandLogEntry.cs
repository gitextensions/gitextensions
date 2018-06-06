using System;

namespace GitCommands.Logging
{
    public class CommandLogEntry
    {
        private string Command { get; }
        private DateTime Timestamp { get; }
        public TimeSpan? Duration { get; set;  }
        private int CmdId { get; }

        /// <summary>
        /// Create a new log entry
        /// </summary>
        /// <param name="command">The (Git) command to log</param>
        /// <param name="counter">The log entry for the start command, to be able to identify start/end pairs</param>
        /// <param name="timestamp">The time for the log</param>
        public CommandLogEntry(string command, int counter, DateTime timestamp)
        {
            Command = command;
            Timestamp = timestamp;
            CmdId = counter;

            Duration = null;
        }

        public override string ToString()
        {
            string duration = Duration == null ?
                "(start)" :
                string.Format("{0:0}ms", ((TimeSpan)Duration).TotalMilliseconds);

            return $"{Timestamp:O} {duration:0,7} #{CmdId} {Command}";
        }
    }
}