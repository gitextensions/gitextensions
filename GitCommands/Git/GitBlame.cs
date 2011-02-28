using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace GitCommands
{
    public class GitBlame
    {
        public GitBlame()
        {
            Headers = new List<GitBlameHeader>();
            Lines = new List<GitBlameLine>();
        }
        public IList<GitBlameHeader> Headers { get; private set; }
        public IList<GitBlameLine> Lines { get; private set; }

        public GitBlameHeader FindHeaderForCommitGuid(string commitGuid)
        {
            return Headers.First(h => h.CommitGuid == commitGuid);
        }
    }

    public class GitBlameLine
    {
        //Line
        public string CommitGuid { get; set; }
        public string FinalLineNumber { get; set; }
        public string OriginLineNumber { get; set; }

        public string LineText { get; set; }
    }
    public class GitBlameHeader
    {
        //Header
        public string CommitGuid { get; set; }
        public string AuthorMail { get; set; }
        public string AuthorTime { get; set; }
        public string AuthorTimeZone { get; set; }
        public string Author { get; set; }
        public string CommitterMail { get; set; }
        public string CommitterTime { get; set; }
        public string CommitterTimeZone { get; set; }
        public string Committer { get; set; }
        public string Summary { get; set; }
        public string FileName { get; set; }

        public Color GetColor()
        {
            return Color.White;
        }
        public override string ToString()
        {
            StringBuilder toStringValue = new StringBuilder();
            toStringValue.AppendLine("Author: " + Author);
            //toStringValue.AppendLine("AuthorTime: " + AuthorTime);
            toStringValue.AppendLine("Committer: " + Committer);
            //toStringValue.AppendLine("CommitterTime: " + CommitterTime);
            toStringValue.AppendLine("Summary: " + Summary);
            toStringValue.AppendLine();
            toStringValue.AppendLine("FileName: " + FileName);

            return toStringValue.ToString().Trim();
        }
    }
}
