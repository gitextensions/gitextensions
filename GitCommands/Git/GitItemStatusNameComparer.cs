using GitExtensions.Extensibility.Git;

namespace GitCommands.Git
{
    /// <summary>
    /// Compares the file names/>.
    /// </summary>
    public class GitItemStatusNameComparer : Comparer<GitItemStatus?>
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

            return x.CompareName(y);
        }
    }
}
