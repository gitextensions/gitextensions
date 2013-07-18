using GitCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.UserControls.RevisionGridClasses
{
    class GitRefListsForRevision
    {
        private GitRef[] _allBranches;
        private GitRef[] _localBranches;
        private GitRef[] _branchesWithNoIdenticalRemotes;
        private GitRef[] _tags;

        public GitRefListsForRevision(GitRevision revision)
        {
            _allBranches = revision.Refs.Where(h => !h.IsTag && (h.IsHead || h.IsRemote)).ToArray();
            _localBranches = _allBranches.Where(b => !b.IsRemote).ToArray();
            _branchesWithNoIdenticalRemotes = _allBranches.Where(
                b => !b.IsRemote || !_localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName)).ToArray();

            _tags = revision.Refs.Where(h => h.IsTag).ToArray();
        }

        public GitRef[] AllBranches
        {
            get { return _allBranches; }
        }

        public GitRef[] LocalBranches
        {
            get { return _localBranches; }
        }

        public GitRef[] BranchesWithNoIdenticalRemotes
        {
            get { return _branchesWithNoIdenticalRemotes; }
        }

        public GitRef[] AllTags
        {
            get { return _tags; }
        }

        public string[] GetAllBranchNames()
        {
            return AllBranches.Select(b => b.Name).ToArray();
        }

        public string[] GetAllNonRemoteBranchNames()
        {
            return AllBranches.Where(head => !head.IsRemote).Select(b => b.Name).ToArray();
        }

        public string[] GetAllTagNames()
        {
            return AllTags.Select(t => t.Name).ToArray();
        }
    }
}
