using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace GitCommands.Git
{
    public interface IGitRevisionTester
    {
        /// <summary>
        /// Finds if all of the first selected are parents to the selected revision
        /// </summary>
        /// <param name="firstSelected">The first selected revisions (A)</param>
        /// <param name="selectedRevision">The currently (last) selected revision (B)</param>
        /// <returns>
        /// True if one of the first selected is parent
        /// </returns>
        bool AllFirstAreParentsToSelected(IEnumerable<GitRevision> firstSelected, GitRevision selectedRevision);

        /// <summary>
        /// Finds if any of the git items exists as a file.
        /// </summary>
        /// <param name="selectedItemsWithParent">List of items to resolve and check</param>
        /// <returns>
        /// True if at least one file exists.
        /// </returns>
        bool AnyLocalFileExists(IEnumerable<GitItemStatus> selectedItemsWithParent);

        bool Matches(GitRevision revision, string criteria);
    }

    public class GitRevisionTester : IGitRevisionTester
    {
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFileSystem _fileSystem;

        public GitRevisionTester(IFullPathResolver fullPathResolver, IFileSystem fileSystem = null)
        {
            _fullPathResolver = fullPathResolver;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        /// <inheritdoc />
        public bool AllFirstAreParentsToSelected(IEnumerable<GitRevision> firstSelected, GitRevision selectedRevision)
        {
            if (selectedRevision?.ParentIds == null || firstSelected == null)
            {
                return false;
            }

            foreach (var item in firstSelected.Select(r => r.ObjectId))
            {
                if (!selectedRevision.ParentIds.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public bool AnyLocalFileExists(IEnumerable<GitItemStatus> selectedItemsWithParent)
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

        /// <inheritdoc />
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

            return revision.Author?.IndexOf(criteria, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   revision.Subject?.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}