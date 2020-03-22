using System;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.UserControls
{
    public sealed class FileStatusItem
    {
        public FileStatusItem([CanBeNull] GitRevision firstRev, [NotNull] GitRevision secondRev, [NotNull] GitItemStatus item)
        {
            FirstRevision = firstRev;
            SecondRevision = secondRev ?? throw new ArgumentNullException(nameof(secondRev));
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        /// <summary>
        /// First (Parent or A in diff)
        /// Can be null for the initial commit
        /// </summary>
        [CanBeNull]
        public GitRevision FirstRevision { get; }

        /// <summary>
        /// Selected (current or B in diff)
        /// The revision selected, the primary for which info exists
        /// </summary>
        [NotNull]
        public GitRevision SecondRevision { get; }

        /// <summary>
        /// The status item in the list
        /// </summary>
        [NotNull]
        public GitItemStatus Item { get; }

        public override string ToString() => Item.ToString();
    }
}
