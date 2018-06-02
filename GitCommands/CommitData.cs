using System;
using System.Collections.Generic;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class CommitData
    {
        public CommitData(
            ObjectId guid,
            ObjectId treeGuid,
            IReadOnlyList<string> parentGuids,
            string author,
            DateTime authorDate,
            string committer,
            DateTime commitDate,
            string body)
        {
            Guid = guid;
            TreeGuid = treeGuid;
            ParentGuids = parentGuids;
            Author = author;
            AuthorDate = authorDate.ToDateTimeOffset();
            Committer = committer;
            CommitDate = commitDate.ToDateTimeOffset();
            Body = body;
        }

        public ObjectId Guid { get; }
        public ObjectId TreeGuid { get; }
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
