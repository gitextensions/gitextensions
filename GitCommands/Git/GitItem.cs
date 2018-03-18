using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    [DebuggerDisplay("GitItem( {" + nameof(FileName) + "} )")]
    public class GitItem : IGitItem
    {
        public GitItem(int mode, GitObjectType objectType, string guid, string name)
        {
            Mode = mode;
            ObjectType = objectType;
            Guid = guid;
            FileName = Name = name;
        }

        public string Guid { get; }
        public GitObjectType ObjectType { get; }
        public string Name { get; }
        public string FileName { get; set; }
        public int Mode { get; }
    }

    public enum GitObjectType
    {
        None = 0,
        Commit,
        Tree,
        Blob
    }
}