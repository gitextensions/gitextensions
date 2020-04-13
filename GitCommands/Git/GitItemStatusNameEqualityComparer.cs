using System.Collections.Generic;

namespace GitCommands.Git
{
    /// <summary>
    /// Compares the file names/>.
    /// </summary>
    public class GitItemStatusNameEqualityComparer : EqualityComparer<GitItemStatus>
    {
        public override bool Equals(GitItemStatus x, GitItemStatus y)
        {
            return x?.Name == y?.Name;
        }

        public override int GetHashCode(GitItemStatus obj)
        {
            return obj?.Name?.GetHashCode() ?? 0;
        }
    }
}
