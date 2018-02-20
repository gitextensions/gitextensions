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
        Func<string, bool> Get([NotNull] string searchPattern);
    }

    public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
    {
        private readonly Func<string> _workingDirGetter;

        public FindFilePredicateProvider([NotNull] Func<string> workingDirGetter)
        {
            if (workingDirGetter == null)
                throw new ArgumentNullException(nameof(workingDirGetter));

            _workingDirGetter = workingDirGetter;
        }

        public Func<string, bool> Get(string searchPattern)
        {
            if (searchPattern == null)
                throw new ArgumentNullException(nameof(searchPattern));

            var pattern = searchPattern.ToPosixPath();
            var dir = (_workingDirGetter() ?? string.Empty).ToPosixPath();

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
