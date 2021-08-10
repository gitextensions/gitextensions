using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    internal class GitRefListsForRevision
    {
        private readonly IGitRef[] _allBranches;
        private readonly IGitRef[] _localBranches;
        private readonly IGitRef[] _branchesWithNoIdenticalRemotes;
        private readonly IGitRef[] _tags;

        public GitRefListsForRevision(GitRevision revision)
            : this(new[] { revision ?? throw new ArgumentNullException(nameof(revision)) })
        {
        }

        public GitRefListsForRevision(IEnumerable<GitRevision> revisions)
        {
            _ = revisions ?? throw new ArgumentNullException(nameof(revisions));

            IGitRef[] refs = revisions.SelectMany(r => r.Refs).Where(r => r is not null).ToArray();

            _allBranches = refs.Where(h => !h.IsTag && (h.IsHead || h.IsRemote)).ToArray();
            _localBranches = _allBranches.Where(b => !b.IsRemote).ToArray();
            _branchesWithNoIdenticalRemotes = _allBranches.Where(b => !b.IsRemote ||
                                                                      !_localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName))
                                                          .ToArray();

            _tags = refs.Where(h => h.IsTag).ToArray();

            HasBranchesOrTags = _allBranches.Length > 0 | _tags.Length > 0;
        }

        public bool HasBranchesOrTags { get; }

        public IReadOnlyList<IGitRef> LocalBranches => _localBranches;

        public IReadOnlyList<IGitRef> AllBranches => _allBranches;

        public IReadOnlyList<IGitRef> AllTags => _tags;

        public IReadOnlyList<IGitRef> BranchesWithNoIdenticalRemotes => _branchesWithNoIdenticalRemotes;

        public IReadOnlyList<string> GetAllBranchNames() => _allBranches.Select(b => b.Name).ToList();

        public IReadOnlyList<string> GetAllTagNames() => AllTags.Select(t => t.Name).ToList();

        /// <summary>
        /// Returns the collection of branches and tags which can be deleted.
        /// </summary>
        public IReadOnlyList<IGitRef> GetDeletableRefs(string currentBranch) => _allBranches.Where(b => b.IsRemote || b.Name != currentBranch).Union(_tags).ToArray();

        /// <summary>
        /// Returns the collection of local branches which can be renamed.
        /// </summary>
        public IReadOnlyList<IGitRef> GetRenameableLocalBranches() => _localBranches.ToList();
    }
}
