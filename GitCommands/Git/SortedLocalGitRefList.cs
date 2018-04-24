using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public abstract class SortedLocalGitRefList
    {
        [NotNull, ItemNotNull] private readonly IReadOnlyList<TimestampedGitRefItem> _items;
        private readonly BranchOrdering _ordering;
        [NotNull] private readonly Func<TimestampedGitRefItem, bool> _filter;

        protected SortedLocalGitRefList(
            [NotNull, ItemNotNull] IReadOnlyList<TimestampedGitRefItem> items,
            BranchOrdering ordering,
            [NotNull] Func<TimestampedGitRefItem, bool> filter)
        {
            _items = items;
            _filter = filter;
            _ordering = ordering;
        }

        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> Items => SortFunction(_ordering)(_items.Where(_filter)).Select(i => i.Ref).ToArray();

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