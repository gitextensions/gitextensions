﻿using System.Diagnostics;

namespace GitUI.BranchTreePanel
{
    [DebuggerDisplay("(Branch path) FullPath = {FullPath}")]
    internal sealed class BranchPathNode : BasePathNode
    {
        public BranchPathNode(Tree tree, string fullPath)
            : base(tree, fullPath)
        {
        }

        public override string ToString()
        {
            return $"{Name}{PathSeparator}";
        }

        public void DeleteAll()
        {
            var branches = Nodes.DepthEnumerator<LocalBranchNode>().Select(branch => branch.FullPath);
            UICommands.StartDeleteBranchDialog(ParentWindow(), branches);
        }

        public void CreateBranch()
        {
            var newBranchNamePrefix = FullPath + PathSeparator;
            UICommands.StartCreateBranchDialog(ParentWindow(), objectId: null, newBranchNamePrefix);
        }
    }
}
