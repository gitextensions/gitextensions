#define CONTRACTS_FULL

using System.Diagnostics.Contracts;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public class RememberFileContextMenuController
    {
        // Singleton accessor
        public static RememberFileContextMenuController Default { get; } = new();

        /// <summary>
        /// The remembered file status item, to diff with other files and commits.
        /// </summary>
        public FileStatusItem? RememberedDiffFileItem { get; set; }

        // Note that the methods in this class are without side effects (static)

        /// <summary>
        /// Get a FileStatusItem that can be used to compare with.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <param name="rev">The Git revision.</param>
        /// <returns>The FileStatusItem.</returns>
        [Pure]
        public FileStatusItem CreateFileStatusItem(string name, GitRevision rev)
        {
            GitItemStatus gis = new(name) { IsNew = true };
            FileStatusItem fsi = new(null, rev, gis);
            return fsi;
        }

        /// <summary>
        /// Check if this file and commit can be used as the first item in a diff to another item
        /// It must be possible to describe the item as a Git commitish.
        /// </summary>
        /// <param name="item">The file status item.</param>
        /// <param name="isSecondRevision">true if second revision can be used as the first item.</param>
        /// <returns>If the item can be used.</returns>
        [Pure]
        public bool ShouldEnableFirstItemDiff(FileStatusItem? item, bool isSecondRevision)
        {
            // First item must be a git reference existing in the revision, i.e. other than work tree
            return ShouldEnableSecondItemDiff(item, isSecondRevision: isSecondRevision)
                   && (isSecondRevision ? item?.SecondRevision : item?.FirstRevision)?.ObjectId != ObjectId.WorkTreeId;
        }

        [Pure]
        public bool ShouldEnableFirstItemDiff(FileStatusItem item)
            => ShouldEnableFirstItemDiff(item, isSecondRevision: false)
               || ShouldEnableFirstItemDiff(item, isSecondRevision: true);

        /// <summary>
        /// Check if this file and commit can be used as the second item in a diff to another item.
        /// </summary>
        /// <param name="item">The file status item.</param>
        /// <param name="isSecondRevision">true if second revision can be used as the second item.</param>
        /// <returns>If the item can be used.</returns>
        [Pure]
        public bool ShouldEnableSecondItemDiff(FileStatusItem? item, bool isSecondRevision)
        {
            // Git reference in this revision or work tree file existing on the file system
            return item is not null
                   && !item.Item.IsSubmodule
                   && (isSecondRevision ? !item.Item.IsDeleted : !item.Item.IsNew);
        }

        [Pure]
        public bool ShouldEnableSecondItemDiff(FileStatusItem? item)
            => ShouldEnableSecondItemDiff(item, isSecondRevision: false)
               || ShouldEnableSecondItemDiff(item, isSecondRevision: true);

        /// <summary>
        /// A Git commitish representation of an object
        /// https://git-scm.com/docs/gitrevisions#_specifying_revisions.
        /// </summary>
        /// <param name="getFileBlobHash">the Git module function to get the blob.</param>
        /// <param name="item">the item.</param>
        /// <param name="isSecondRevision">true if second revision is used.</param>
        /// <returns>A Git commitish.</returns>
        [Pure]
        public string? GetGitCommit(Func<string, ObjectId, ObjectId?>? getFileBlobHash, FileStatusItem? item, bool isSecondRevision)
        {
            if (item is null)
            {
                return null;
            }

            var name = (!isSecondRevision && !string.IsNullOrWhiteSpace(item.Item.OldName)
                    ? item.Item.OldName
                    : item.Item.Name)
                ?.ToPosixPath();
            var id = (isSecondRevision ? item.SecondRevision : item.FirstRevision)?.ObjectId;
            if (string.IsNullOrWhiteSpace(name) || id is null)
            {
                return null;
            }

            if (id == ObjectId.WorkTreeId)
            {
                // A file system file
                return name;
            }

            if (id == ObjectId.IndexId)
            {
                // Must be referenced by blob - no commit. File name presented in difftool will be blob or the other file
                return item.Item.TreeGuid is not null
                    ? item.Item.TreeGuid.ToString()
                    : getFileBlobHash?.Invoke(name, id)?.ToString();
            }

            // commit:path
            return $"{id}:{name}";
        }
    }
}
