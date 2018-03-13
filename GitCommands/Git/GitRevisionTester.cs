using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands.Git
{
    public interface IGitRevisionTester
    {
        bool IsFirstParent(GitRevision revision, IEnumerable<GitRevision> selectedItemParents);
        bool Matches(GitRevision revision, string criteria);
    }

    public class GitRevisionTester : IGitRevisionTester
    {
        public bool IsFirstParent(GitRevision revision, IEnumerable<GitRevision> selectedItemParents)
        {
            if (revision?.ParentGuids == null)
            {
                return false;
            }

            // TODO: the logic looks very odd....
            foreach (var item in selectedItemParents.Select(r => r.Guid))
            {
                if (!revision.ParentGuids.Contains(item, StringComparer.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

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