using System;
using System.Linq;

namespace GitCommands.Git
{
    public interface IGitRevisionTester
    {
        bool Matches(GitRevision revision, string criteria);
    }

    public class GitRevisionTester : IGitRevisionTester
    {
        public bool Matches(GitRevision revision, string criteria)
        {
            if (revision == null || string.IsNullOrWhiteSpace(criteria))
            {
                // don't throw exception for performance reasons
                return false;
            }

            if (revision.Refs.Where(gitHead => !string.IsNullOrWhiteSpace(gitHead.Name))
                             .Any(gitHead => gitHead.Name.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            if (criteria.Length > 2 && revision.Guid.StartsWith(criteria, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return revision.Author?.StartsWith(criteria, StringComparison.CurrentCultureIgnoreCase) == true ||
                   revision.Subject?.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}