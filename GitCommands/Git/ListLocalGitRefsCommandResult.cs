using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    /// <summary>
    /// Parse the output of <c>git for-each-ref</c> with the specific format
    /// <c>"%(committerdate:raw)%(taggerdate:raw) %(objectname) %(refname)"</c>
    /// This format allows us to sort both annotated and lightweight tags by date
    /// </summary>
    public sealed class ListLocalGitRefsCommandResult
    {
        [NotNull, ItemNotNull] public IReadOnlyList<TimestampedGitRefItem> Items { get; }

        public ListLocalGitRefsCommandResult([NotNull] GitCommandResult result)
        {
            Items = result.Lines.Select(l => new TimestampedGitRefItem(l)).ToArray();
        }
    }
}