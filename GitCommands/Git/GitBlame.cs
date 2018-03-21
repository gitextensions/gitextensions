using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{
    public class GitBlame
    {
        public IList<GitBlameHeader> Headers { get; } = new List<GitBlameHeader>();
        public IList<GitBlameLine> Lines { get; } = new List<GitBlameLine>();

        public GitBlameHeader FindHeaderForCommitGuid(string commitGuid)
        {
            return Headers.First(h => h.CommitGuid == commitGuid);
        }
    }

    public class GitBlameLine
    {
        public string CommitGuid { get; set; }
        public int FinalLineNumber { get; set; }
        public int OriginLineNumber { get; set; }
        public string LineText { get; set; }
    }

    public class GitBlameHeader
    {
        public string CommitGuid { get; set; }
        public string AuthorMail { get; set; }
        public DateTime AuthorTime { get; set; }
        public string AuthorTimeZone { get; set; }
        public string Author { get; set; }
        public string CommitterMail { get; set; }
        public DateTime CommitterTime { get; set; }
        public string CommitterTimeZone { get; set; }
        public string Committer { get; set; }
        public string Summary { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            StringBuilder toStringValue = new StringBuilder();
            toStringValue.AppendLine("Author: " + Author);
            toStringValue.AppendLine("AuthorTime: " + AuthorTime);
            toStringValue.AppendLine("Committer: " + Committer);
            toStringValue.AppendLine("CommitterTime: " + CommitterTime);
            toStringValue.AppendLine("Summary: " + Summary);
            toStringValue.AppendLine();
            toStringValue.AppendLine("FileName: " + FileName);

            return toStringValue.ToString().Trim();
        }
    }
}
