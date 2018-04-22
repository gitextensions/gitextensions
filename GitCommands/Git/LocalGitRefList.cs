using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    /// <summary>
    /// Parse the output of <c>git for-each-ref</c> with the specific format
    /// <c>"%(committerdate:raw)%(taggerdate:raw) %(objectname) %(refname)"</c>
    /// This format allows us to sort both annotated and lightweight tags by date
    /// </summary>
    public sealed class LocalGitRefList
    {
        [NotNull, ItemNotNull]
        private readonly Item[] _items;

        public LocalGitRefList([NotNull]string commandOutput)
        {
            // TODO handle git error message
            var lines = commandOutput.SplitLines();
            _items = lines.Select(l => new Item(l)).ToArray();
        }

        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> OrderedBranches(BranchOrdering ordering)
        {
            return OrderedFilteredGitRefs(ordering, i => i.Ref.IsHead);
        }

        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> OrderedTags(BranchOrdering ordering)
        {
            return OrderedFilteredGitRefs(ordering, i => i.Ref.IsTag);
        }

        [NotNull, ItemNotNull]
        private IReadOnlyList<IGitRef> OrderedFilteredGitRefs(BranchOrdering ordering, [NotNull] Func<Item, bool> filter)
        {
            return SortFunction(ordering)(_items.Where(filter)).Select(i => i.Ref).ToArray();
        }

        [NotNull]
        private Func<IEnumerable<Item>, IEnumerable<Item>> SortFunction(BranchOrdering ordering)
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

        public sealed class Item
        {
            public Item([NotNull]string line)
            {
                var columns = line.Split(new[] { ' ' }, 4);

                Date = DateTimeUtils.ParseUnixTime(columns[0]);

                var guid = columns[2];
                var completeName = columns[3];
                Ref = new GitRef(null, guid, completeName);
            }

            public DateTime Date { get; }
            [NotNull] public GitRef Ref { get; }
        }
    }
}