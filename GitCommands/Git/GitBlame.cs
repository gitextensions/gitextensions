﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GitExtensions.Core.Module;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class GitBlame
    {
        public IReadOnlyList<GitBlameLine> Lines { get; }

        public GitBlame(IReadOnlyList<GitBlameLine> lines)
        {
            Lines = lines;
        }
    }

    public sealed class GitBlameLine
    {
        [NotNull]
        public GitBlameCommit Commit { get; }
        public int FinalLineNumber { get; }
        public int OriginLineNumber { get; }
        [NotNull]
        public string Text { get; }

        public GitBlameLine([NotNull] GitBlameCommit commit, int finalLineNumber, int originLineNumber, [NotNull] string text)
        {
            Commit = commit;
            FinalLineNumber = finalLineNumber;
            OriginLineNumber = originLineNumber;
            Text = text;
        }
    }

    public sealed class GitBlameCommit
    {
        public ObjectId ObjectId { get; }
        public string Author { get; }
        public string AuthorMail { get; }
        public DateTime AuthorTime { get; }
        public string AuthorTimeZone { get; }
        public string Committer { get; }
        public string CommitterMail { get; }
        public DateTime CommitterTime { get; }
        public string CommitterTimeZone { get; }
        public string Summary { get; }
        public string FileName { get; }

        public GitBlameCommit(ObjectId objectId, string author, string authorMail, DateTime authorTime, string authorTimeZone, string committer, string committerMail, DateTime committerTime, string committerTimeZone, string summary, string fileName)
        {
            ObjectId = objectId;
            Author = author;
            AuthorMail = authorMail;
            AuthorTime = authorTime;
            AuthorTimeZone = authorTimeZone;
            Committer = committer;
            CommitterMail = committerMail;
            CommitterTime = committerTime;
            CommitterTimeZone = committerTimeZone;
            Summary = summary;
            FileName = fileName;
        }

        public override string ToString()
        {
            var s = new StringBuilder();

            s.Append("Author: ").AppendLine(Author);
            s.Append("Author date: ").AppendLine(AuthorTime.ToString(CultureInfo.CurrentCulture));
            if (Author != Committer || AuthorTime != CommitterTime)
            {
                s.Append("Committer: ").AppendLine(Committer);
                s.Append("Commit date: ").AppendLine(CommitterTime.ToString(CultureInfo.CurrentCulture));
            }

            s.Append("Commit hash: ").AppendLine(ObjectId.ToShortString());
            s.Append("Summary: ").AppendLine(Summary);
            s.AppendLine();
            s.Append("FileName: ").Append(FileName);

            return s.ToString();
        }
    }
}
