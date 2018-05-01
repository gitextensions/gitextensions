using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class GitRefListsForRevision
    {
        private readonly IGitRef[] _allBranches;
        private readonly IGitRef[] _localBranches;
        private readonly IGitRef[] _branchesWithNoIdenticalRemotes;
        private readonly IGitRef[] _tags;

        public GitRefListsForRevision(GitRevision revision)
        {
            _allBranches = revision.Refs.Where(h => !h.IsTag && (h.IsHead || h.IsRemote)).ToArray();
            _localBranches = _allBranches.Where(b => !b.IsRemote).ToArray();
            _branchesWithNoIdenticalRemotes = _allBranches.Where(b => !b.IsRemote ||
                                                                      !_localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName))
                                                          .ToArray();

            _tags = revision.Refs.Where(h => h.IsTag).ToArray();
        }

        public IEnumerable<IGitRef> AllBranches => _allBranches.AsEnumerable();

        public IEnumerable<IGitRef> AllTags => _tags.AsEnumerable();

        public IEnumerable<IGitRef> BranchesWithNoIdenticalRemotes => _branchesWithNoIdenticalRemotes.AsEnumerable();

        public string[] GetAllBranchNames()
        {
            return _allBranches.Select(b => b.Name).ToArray();
        }

        public string[] GetAllTagNames()
        {
            return AllTags.Select(t => t.Name).ToArray();
        }

        /// <summary>
        /// Returns the collection of local branches and tags which can be deleted.
        /// </summary>
        public IGitRef[] GetDeletableLocalRefs(string currentBranch)
        {
            return _localBranches.Where(b => b.Name != currentBranch).Union(_tags).ToArray();
        }

        /// <summary>
        /// Returns the collection of local branches which can be renamed.
        /// </summary>
        public IGitRef[] GetRenameableLocalBranches()
        {
            return _localBranches.ToArray();
        }
    }
}
