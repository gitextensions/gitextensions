using GitExtensions.Extensibility.Git;

namespace GitCommands.Git
{
    /// <summary>
    /// Compares the file extension of <see cref="GitItemStatus.Name"/> and then by path/>.
    /// </summary>
    public class GitItemStatusFileExtensionComparer : Comparer<GitItemStatus?>
    {
        public override int Compare(GitItemStatus? x, GitItemStatus? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            string lhsPath = GetPrimarySortingPath(x);
            string rhsPath = GetPrimarySortingPath(y);
            string lhsExt = Path.GetExtension(lhsPath);
            string rhsExt = Path.GetExtension(rhsPath);

            int comparisonResult = StringComparer.InvariantCulture.Compare(lhsExt, rhsExt);
            if (comparisonResult == 0)
            {
                // originally used Comparer<GitItemStatus>.Default.Compare(x, y) but this would
                // produce incorrect results when the oldName was the only available path on an item.
                return StringComparer.InvariantCulture.Compare(lhsPath, rhsPath);
            }

            return comparisonResult;
        }

        private static string? GetPrimarySortingPath(GitItemStatus itemStatus)
        {
            return !string.IsNullOrEmpty(itemStatus.Name)
                ? itemStatus.Name
                : itemStatus.OldName;
        }
    }
}
