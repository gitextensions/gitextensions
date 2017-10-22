using System;
using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitCommands
{
    [DebuggerDisplay("GitItem( {FileName} )")]
    public class GitItem : IGitItem
    {
        public GitItem(string mode, string itemType, string guid, string name)
        {
            Mode = mode;
            ItemType = itemType;
            Guid = guid;
            FileName = Name = name;
        }


        public string Guid { get; }
        public string ItemType { get; }
        public string Name { get; }
        public string FileName { get; set; }
        public string Mode { get; }


        public bool IsBlob
        {
            get { return ItemType == "blob"; }
        }

        public bool IsCommit
        {
            get { return ItemType == "commit"; }
        }

        public bool IsTree
        {
            get { return ItemType == "tree"; }
        }
    }

    }
}
