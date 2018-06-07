using System;

namespace GitCommands.Logging
{
    public class CommandLogEntry
    {
        private string Command { get; }
        private DateTime Timestamp { get; }
        public TimeSpan? Duration { get; set;  }

        /// <summary>
        /// Create a new log entry
        /// </summary>
        /// <param name="command">The (Git) command to log</param>
        /// <param name="timestamp">The time for the log</param>
        public CommandLogEntry(string command, DateTime timestamp)
        {
            Command = command;
            Timestamp = timestamp;

            Duration = null;
        }

        public override string ToString()
        {
            string duration = Duration == null ?
                "started" :
                string.Format("{0:0}ms", ((TimeSpan)Duration).TotalMilliseconds);

            return $"{Timestamp:HH:mm:ss.fffffff} {duration,7} {Command}";
        }
    }
}