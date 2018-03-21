using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class CommitData
    {
        public CommitData(string guid,
            string treeGuid, IReadOnlyList<string> parentGuids,
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
        public IReadOnlyList<string> ParentGuids { get; }
        public string Author { get; }
        public DateTimeOffset AuthorDate { get; }
        public string Committer { get; }
        public DateTimeOffset CommitDate { get; }

        // TODO mutable properties need review

        [CanBeNull, ItemNotNull]
        public IReadOnlyList<string> ChildrenGuids { get; set; }

        /// <summary>
        /// Gets and sets the commit message.
        /// </summary>
        public string Body { get; set; }
    }
}
