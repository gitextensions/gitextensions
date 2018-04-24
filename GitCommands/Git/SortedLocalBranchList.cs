using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public sealed class SortedLocalBranchList : SortedLocalGitRefList
    {
        public SortedLocalBranchList(
            [NotNull, ItemNotNull] IReadOnlyList<TimestampedGitRefItem> items,
            BranchOrdering ordering)
            : base(items, ordering, i => i.Ref.IsHead)
        {
        }
    }
}