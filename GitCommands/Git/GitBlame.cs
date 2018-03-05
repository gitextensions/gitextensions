using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        // Line
        public string CommitGuid { get; set; }
        public int FinalLineNumber { get; set; }
        public int OriginLineNumber { get; set; }

        public string LineText { get; set; }
    }

    public class GitBlameHeader
    {
        // Header
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

        public Color GetColor()
        {
            int partLength = CommitGuid.Length / 3;
            return Color.FromArgb(GenerateIntFromString(CommitGuid.Substring(0, partLength)) % 55 + 200, GenerateIntFromString(CommitGuid.Substring(partLength, partLength)) % 55 + 200, GenerateIntFromString(CommitGuid.Substring(partLength)) % 55 + 200);

            // return Color.White;
        }

        private int GenerateIntFromString(string text)
        {
            int number = 0;
            foreach (char c in text)
            {
                number += (int)c;
            }
            return number;
        }

        public override string ToString()
        {
            StringBuilder toStringValue = new StringBuilder();
            toStringValue.AppendLine("Author: " + Author);
            toStringValue.AppendLine("AuthorTime: " + AuthorTime.ToString());
            toStringValue.AppendLine("Committer: " + Committer);
            toStringValue.AppendLine("CommitterTime: " + CommitterTime.ToString());
            toStringValue.AppendLine("Summary: " + Summary);
            toStringValue.AppendLine();
            toStringValue.AppendLine("FileName: " + FileName);

            return toStringValue.ToString().Trim();
        }

        public override bool Equals(object obj)
        {
            return this == (GitBlameHeader)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(GitBlameHeader x, GitBlameHeader y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.Author == y.Author && x.AuthorTime == y.AuthorTime &&
                x.Committer == y.Committer && x.CommitterTime == y.CommitterTime &&
                x.Summary == y.Summary && x.FileName == y.FileName;
        }

        public static bool operator !=(GitBlameHeader x, GitBlameHeader y)
        {
            return !(x == y);
        }
    }
}
