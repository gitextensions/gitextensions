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

        public string Guid { get; private set; }
        public string TreeGuid { get; private set; }
        public ReadOnlyCollection<string> ParentGuids { get; private set; }
        public List<string> ChildrenGuids { get; set; }
        public string Author { get; private set; }
        public DateTimeOffset AuthorDate { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset CommitDate { get; private set; }

        // TODO: this needs review, it shouldn't be mutable
        public string Body { get;  set; }

    }
}
