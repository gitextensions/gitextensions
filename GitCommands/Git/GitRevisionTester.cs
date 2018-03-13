using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace GitCommands.Git
{
    public interface IGitRevisionTester
    {
        bool IsFirstParent(GitRevision revision, IEnumerable<GitRevision> selectedItemParents);
        bool LocalRevisionExists(IEnumerable<GitItemStatus> selectedItemsWithParent);
        bool Matches(GitRevision revision, string criteria);
    }

    public class GitRevisionTester : IGitRevisionTester
    {
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFileSystem _fileSystem;

        public GitRevisionTester(IFullPathResolver fullPathResolver, IFileSystem fileSystem)
        {
            _fullPathResolver = fullPathResolver;
            _fileSystem = fileSystem;
        }

        public GitRevisionTester(IFullPathResolver fullPathResolver)
            : this(fullPathResolver, new FileSystem())
        {
        }

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

        public bool LocalRevisionExists(IEnumerable<GitItemStatus> selectedItemsWithParent)
        {
            if (selectedItemsWithParent == null)
            {
                return false;
            }

            var items = selectedItemsWithParent as List<GitItemStatus> ?? selectedItemsWithParent.ToList();
            bool localExists = items.Any(item => !item.IsTracked);
            if (localExists)
            {
                return true;
            }

            // enable *<->Local items only when (any) local file exists
            foreach (var item in items)
            {
                string filePath = _fullPathResolver.Resolve(item.Name);
                if (_fileSystem.File.Exists(filePath))
                {
                    return true;
                }
            }

            return false;
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