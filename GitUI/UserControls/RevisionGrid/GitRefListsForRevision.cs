using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid
{
    internal class GitRefListsForRevision
    {
        private readonly IGitRef[] _allBranches;
        private readonly IGitRef[] _localBranches;
        private readonly IGitRef[] _branchesWithNoIdenticalRemotes;
        private readonly IGitRef[] _tags;

        public GitRefListsForRevision([NotNull] GitRevision revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            if (revision.Refs == null)
            {
                throw new ArgumentNullException(nameof(revision.Refs));
            }

            _allBranches = revision.Refs.Where(h => !h.IsTag && (h.IsHead || h.IsRemote)).ToArray();
            _localBranches = _allBranches.Where(b => !b.IsRemote).ToArray();
            _branchesWithNoIdenticalRemotes = _allBranches.Where(b => !b.IsRemote ||
                                                                      !_localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName))
                                                          .ToArray();

            _tags = revision.Refs.Where(h => h.IsTag).ToArray();
        }

        public IReadOnlyList<IGitRef> AllBranches => _allBranches;

        public IReadOnlyList<IGitRef> AllTags => _tags;

        public IReadOnlyList<IGitRef> BranchesWithNoIdenticalRemotes => _branchesWithNoIdenticalRemotes;

        public IReadOnlyList<string> GetAllBranchNames()
        {
            return _allBranches.Select(b => b.Name).ToList();
        }

        public IReadOnlyList<string> GetAllTagNames()
        {
            return AllTags.Select(t => t.Name).ToList();
        }

        /// <summary>
        /// Returns the collection of branches and tags which can be deleted.
        /// </summary>
        public IReadOnlyList<IGitRef> GetDeletableRefs(string currentBranch)
        {
            return _allBranches.Where(b => b.IsRemote || b.Name != currentBranch).Union(_tags).ToArray();
        }

        /// <summary>
        /// Returns the collection of local branches which can be renamed.
        /// </summary>
        public IReadOnlyList<IGitRef> GetRenameableLocalBranches()
        {
            return _localBranches.ToList();
        }
    }
}
