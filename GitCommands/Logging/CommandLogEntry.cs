using System;

namespace GitCommands.Logging
{
    public class CommandLogEntry
    {
        private string Command { get; }
        private DateTime ExecutionStartTimestamp { get; }
        private DateTime ExecutionEndTimestamp { get; }
        private bool IsStart { get; }
        private uint CmdId { get; }
        private bool _enhanced;

        private static uint counter = 0;

        /// <summary>
        /// Create a new log entry
        /// </summary>
        /// <param name="command">The (Git) command to log</param>
        /// <param name="executionStartTimestamp">The start time for the command</param>
        /// <param name="executionEndTimestamp">The end time for the command, ignored if this is start of a command</param>
        /// <param name="startCmd">The log entry for the start command, to be able to identify start/end pairs</param>
        /// <param name="isStart">Set if this is start of a command</param>
        public CommandLogEntry(string command, DateTime executionStartTimestamp, DateTime executionEndTimestamp, CommandLogEntry startCmd = null, bool isStart = false)
        {
            Command = command;
            ExecutionStartTimestamp = executionStartTimestamp;
            ExecutionEndTimestamp = executionEndTimestamp;
            IsStart = isStart;

            // Start of commands will only be logged if enhanced logging is set,
            // so cmdNo (with reference to start) will only be not null for ends with enhanced logging
            _enhanced = isStart || startCmd != null;

            if (isStart)
            {
                counter++;
            }

            // If there is no previous log entry, use the counter
            CmdId = startCmd == null ?
                counter :
                startCmd.CmdId;
        }

        public override string ToString()
        {
            string cmd = !_enhanced ? "" : "#" + CmdId.ToString() + " ";
            string end;
            if (IsStart)
            {
                end = "#(start)";
            }
            else
            {
                var durationInMillis = (long)ExecutionEndTimestamp.Subtract(ExecutionStartTimestamp).TotalMilliseconds;
                end = string.Format("#(took {0} ms)", durationInMillis);
            }

            return string.Format("{0}    {1}{2}    {3}", ExecutionStartTimestamp.ToString("O"), cmd, Command, end);
        }
    }
}