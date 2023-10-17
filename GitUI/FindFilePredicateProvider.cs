using GitCommands;

namespace GitUI
{
    public interface IFindFilePredicateProvider
    {
        /// <summary>
        /// Returns the names of files that match the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the paths of files.</param>
        Func<string?, bool> Get(string searchPattern, string workingDir);
    }

    public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
    {
        public Func<string?, bool> Get(string searchPattern, string workingDir)
        {
            if (searchPattern is null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            if (workingDir is null)
            {
                throw new ArgumentNullException(nameof(workingDir));
            }

            string pattern = searchPattern.ToPosixPath();
            string dir = workingDir.ToPosixPath();

            if (pattern.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern[dir.Length..].TrimStart('/');
                return fileName => fileName?.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) is true;
            }

            // Method Contains have no override with StringComparison parameter
            return fileName => fileName?.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) is >= 0;
        }
    }
}
