using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGridClasses
{
    class GitRefListsForRevision
    {
        private readonly IGitRef[] _allBranches;
        private readonly IGitRef[] _localBranches;
        private readonly IGitRef[] _branchesWithNoIdenticalRemotes;
        private readonly IGitRef[] _tags;

        public GitRefListsForRevision(GitRevision revision)
        {
            _allBranches = revision.Refs.Where(h => !h.IsTag && (h.IsHead || h.IsRemote)).ToArray();
            _localBranches = _allBranches.Where(b => !b.IsRemote).ToArray();
            _branchesWithNoIdenticalRemotes = _allBranches.Where(
                b => !b.IsRemote || !_localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName)).ToArray();

            _tags = revision.Refs.Where(h => h.IsTag).ToArray();
        }

        public IGitRef[] AllBranches
        {
            get { return _allBranches; }
        }

        public IGitRef[] LocalBranches
        {
            get { return _localBranches; }
        }

        public IGitRef[] BranchesWithNoIdenticalRemotes
        {
            get { return _branchesWithNoIdenticalRemotes; }
        }

        public IGitRef[] AllTags
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
