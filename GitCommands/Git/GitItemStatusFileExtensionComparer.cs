using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands.Git
{
    /// <summary>
    /// Compares the file extension of <see cref="GitItemStatus.Name"/> and then by <see cref="GitItemStatus.CompareTo(GitItemStatus)"/>.
    /// </summary>
    public class GitItemStatusFileExtensionComparer : Comparer<GitItemStatus>
    {
        public override int Compare(GitItemStatus x, GitItemStatus y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var lhsPath = GetPrimarySortingPath(x);
            var rhsPath = GetPrimarySortingPath(y);
            var lhsExt = Path.GetExtension(lhsPath);
            var rhsExt = Path.GetExtension(rhsPath);

            var comparisonResult = StringComparer.InvariantCulture.Compare(lhsExt, rhsExt);
            if (comparisonResult == 0)
            {
                // originally used Comparer<GitItemStatus>.Default.Compare(x, y) but this would
                // produce incorrect results when the oldName was the only available path on an item.
                return StringComparer.InvariantCulture.Compare(lhsPath, rhsPath);
            }

            return comparisonResult;
        }

        private static string GetPrimarySortingPath(GitItemStatus itemStatus)
        {
            return !string.IsNullOrEmpty(itemStatus.Name)
                ? itemStatus.Name
                : itemStatus.OldName;
        }
    }
}
