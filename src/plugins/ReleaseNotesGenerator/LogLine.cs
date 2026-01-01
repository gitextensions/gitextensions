namespace GitExtensions.Plugins.ReleaseNotesGenerator;

public class LogLine
{
    public LogLine(string hash, string message)
    {
        Commit = hash;
        MessageLines = [message];
    }

    public string Commit { get; }
    public IList<string> MessageLines { get; }
}
