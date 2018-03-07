using System;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI
{
    public interface IFindFilePredicateProvider
    {
        /// <summary>
        /// Returns the names of files that match the specified search pattern
        /// </summary>
        /// <param name="searchPattern">The search string to match against the pathes of files</param>
        Func<string, bool> Get([NotNull] string searchPattern, [NotNull] string workingDir);
    }

    public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
    {
        public Func<string, bool> Get(string searchPattern, string workingDir)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            if (workingDir == null)
            {
                throw new ArgumentNullException(nameof(workingDir));
            }

            var pattern = searchPattern.ToPosixPath();
            var dir = workingDir.ToPosixPath();

            if (pattern.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern.Substring(dir.Length).TrimStart('/');
                return fileName => fileName != null && fileName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
            }

            // Method Contains have no override with StringComparison parameter
            return fileName => fileName != null && fileName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
