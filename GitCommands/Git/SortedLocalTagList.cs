using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public sealed class SortedLocalTagList : SortedLocalGitRefList
    {
        public SortedLocalTagList(
            [NotNull, ItemNotNull] IReadOnlyList<TimestampedGitRefItem> items,
            BranchOrdering ordering)
            : base(items, ordering, i => i.Ref.IsTag)
        {
        }
    }
}