using System.Collections.Generic;

namespace GitExtensions.Plugins.ReleaseNotesGenerator
{
    public class LogLine
    {
        public LogLine(string hash, string message)
        {
            Commit = hash;
            MessageLines = new List<string> { message };
        }

        public string Commit { get; }
        public IList<string> MessageLines { get; }
    }
}
