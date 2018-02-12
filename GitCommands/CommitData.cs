using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GitCommands
{
    public class CommitData
    {
        public CommitData(string guid,
            string treeGuid, ReadOnlyCollection<string> parentGuids,
            string author, DateTimeOffset authorDate,
            string committer, DateTimeOffset commitDate,
            string body)
        {
            Guid = guid;
            TreeGuid = treeGuid;
            ParentGuids = parentGuids;
            Author = author;
            AuthorDate = authorDate;
            Committer = committer;
            CommitDate = commitDate;

            Body = body;
        }

        public string Guid { get; }
        public string TreeGuid { get; }
        public ReadOnlyCollection<string> ParentGuids { get; }
        public List<string> ChildrenGuids { get; set; }
        public string Author { get; }
        public DateTimeOffset AuthorDate { get; }
        public string Committer { get; }
        public DateTimeOffset CommitDate { get; }

        // TODO: this needs review, it shouldn't be mutable
        public string Body { get;  set; }

    }
}
