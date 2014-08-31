using System;

namespace GitCommands.Logging
{
  public class CommandLogEntry
  {
    public string Command { get; private set; }
    public DateTime ExecutionStartTimestamp { get; private set; }
    public DateTime ExecutionEndTimestamp { get; private set; }

    public CommandLogEntry (string command, DateTime executionStartTimestamp, DateTime executionEndTimestamp)
    {
      Command = command;
      ExecutionStartTimestamp = executionStartTimestamp;
      ExecutionEndTimestamp = executionEndTimestamp;
    }

    public override string ToString ()
    {
      var durationInMillis = (long) ExecutionEndTimestamp.Subtract (ExecutionStartTimestamp).TotalMilliseconds;
      return string.Format ("{0}    {1}    #(took {2} ms)", ExecutionStartTimestamp.ToString ("O"), Command, durationInMillis);
    }
  }
}