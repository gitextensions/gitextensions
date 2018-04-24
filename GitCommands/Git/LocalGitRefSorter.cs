using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public sealed class LocalGitRefSorter
    {
        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> OrderedBranches(IReadOnlyList<TimestampedGitRefItem> gitRefs, BranchOrdering ordering)
        {
            return OrderedFilteredGitRefs(gitRefs, ordering, i => i.Ref.IsHead);
        }

        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> OrderedTags(IReadOnlyList<TimestampedGitRefItem> gitRefs, BranchOrdering ordering)
        {
            return OrderedFilteredGitRefs(gitRefs, ordering, i => i.Ref.IsTag);
        }

        [NotNull, ItemNotNull]
        private IReadOnlyList<IGitRef> OrderedFilteredGitRefs(IReadOnlyList<TimestampedGitRefItem> gitRefs, BranchOrdering ordering, [NotNull] Func<TimestampedGitRefItem, bool> filter)
        {
            return SortFunction(ordering)(gitRefs.Where(filter)).Select(i => i.Ref).ToArray();
        }

        [NotNull]
        private Func<IEnumerable<TimestampedGitRefItem>, IEnumerable<TimestampedGitRefItem>> SortFunction(BranchOrdering ordering)
        {
            switch (ordering)
            {
                case BranchOrdering.ByLastAccessDate:
                    return list => list.OrderByDescending(item => item.Date);
                case BranchOrdering.Alphabetically:
                    return list => list.OrderBy(item => item.Ref.CompleteName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null);
            }
        }
    }
}