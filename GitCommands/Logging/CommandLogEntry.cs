using System;

namespace GitCommands.Logging
{
  public class CommandLogEntry
  {
    public string Command { get; }
    public DateTime ExecutionStartTimestamp { get; }
    public DateTime ExecutionEndTimestamp { get; }

    public CommandLogEntry(string command, DateTime executionStartTimestamp, DateTime executionEndTimestamp)
    {
      Command = command;
      ExecutionStartTimestamp = executionStartTimestamp;
      ExecutionEndTimestamp = executionEndTimestamp;
    }

    public override string ToString()
    {
      var durationInMillis = (long)ExecutionEndTimestamp.Subtract(ExecutionStartTimestamp).TotalMilliseconds;
      return string.Format("{0}    {1}    #(took {2} ms)", ExecutionStartTimestamp.ToString("O"), Command, durationInMillis);
    }
  }
}