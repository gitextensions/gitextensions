using System;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls
{
    public sealed class FileStatusItem
    {
        public FileStatusItem(GitRevision? firstRev, GitRevision secondRev, GitItemStatus item, ObjectId? baseA = null, ObjectId? baseB = null)
        {
            FirstRevision = firstRev;
            SecondRevision = secondRev ?? throw new ArgumentNullException(nameof(secondRev));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            BaseA = baseA;
            BaseB = baseB;
        }

        /// <summary>
        /// First (Parent or A in diff)
        /// Can be null for the initial commit.
        /// </summary>
        public GitRevision? FirstRevision { get; }

        /// <summary>
        /// Selected (current or B in diff)
        /// The revision selected, the primary for which info exists.
        /// </summary>
        public GitRevision SecondRevision { get; }

        /// <summary>
        /// If ranges are selected, the first commit (base) for <see cref="FirstRevision"/> (head).
        /// </summary>
        public ObjectId? BaseA { get; }

        /// <summary>
        /// If ranges are selected, the first commit (base) for <see cref="SecondRevision"/> (head).
        /// </summary>
        public ObjectId? BaseB { get; }

        /// <summary>
        /// The status item in the list.
        /// </summary>
        public GitItemStatus Item { get; }

        public override string ToString() => Item.ToString();
    }
}
