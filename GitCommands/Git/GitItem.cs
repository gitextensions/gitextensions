using System;
using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    [DebuggerDisplay("GitItem( {FileName} )")]
    public class GitItem : IGitItem
    {
        public GitItem(string mode, string objectType, string guid, string name)
        {
            Mode = mode;
            GitObjectType type;
            Enum.TryParse(objectType, true, out type);
            ObjectType = type;
            Guid = guid;
            FileName = Name = name;
        }


        public string Guid { get; }
        public GitObjectType ObjectType { get; }
        public string Name { get; }
        public string FileName { get; set; }
        public string Mode { get; }
    }

    public enum GitObjectType
    {
        None = 0,
        Commit,
        Tree,
        Blob
    }
}
