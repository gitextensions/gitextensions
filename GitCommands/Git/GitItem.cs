﻿using System.Diagnostics;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Git
{
    [DebuggerDisplay("GitItem( {" + nameof(FileName) + "} )")]
    public class GitItem : INamedGitItem
    {
        public GitItem(int mode, GitObjectType objectType, ObjectId objectId, string name)
        {
            Mode = mode;
            ObjectType = objectType;
            ObjectId = objectId;
            FileName = Name = name;
        }

        public ObjectId ObjectId { get; }
        public GitObjectType ObjectType { get; }
        public string Name { get; }
        public string FileName { get; set; }
        public int Mode { get; }

        public string Guid => ObjectId.ToString();
    }

    public enum GitObjectType
    {
        None = 0,
        Commit,
        Tree,
        Blob
    }
}
