using System;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public class RememberFileContextMenuController
    {
        /// <summary>
        /// The remembered file status item, to diff with other files and commits
        /// </summary>
        public FileStatusItem RememberedDiffFileItem { get; set; }

        // Note that the methods in this class are without side effects (static)

        /// <summary>
        /// Check if this file and commit can be used as the first item in a diff to another item
        /// It must be possible to describe the item as a Git commitish
        /// </summary>
        /// <param name="item">The file status item</param>
        /// <param name="isSecondRevision">true if second revision can be used as the first item</param>
        /// <returns>If the item can be used</returns>
        public bool ShouldEnableFirstItemDiff(FileStatusItem item, bool isSecondRevision)
        {
            // First item must be a git reference existing in the revision, i.e. other than work tree
            return ShouldEnableSecondItemDiff(item, isSecondRevision: isSecondRevision)
                   && (isSecondRevision ? item.SecondRevision : item.FirstRevision)?.ObjectId != ObjectId.WorkTreeId;
        }

        public bool ShouldEnableFirstItemDiff(FileStatusItem item)
            => ShouldEnableFirstItemDiff(item, isSecondRevision: false)
               || ShouldEnableFirstItemDiff(item, isSecondRevision: true);

        /// <summary>
        /// Check if this file and commit can be used as the second item in a diff to another item
        /// </summary>
        /// <param name="item">The file status item</param>
        /// <param name="isSecondRevision">true if second revision can be used as the second item</param>
        /// <returns>If the item can be used</returns>
        public bool ShouldEnableSecondItemDiff(FileStatusItem item, bool isSecondRevision)
        {
            // Git reference in this revision or work tree file existing on the file system
            return item != null
                   && !item.Item.IsSubmodule
                   && (isSecondRevision ? !item.Item.IsDeleted : !item.Item.IsNew);
        }

        public bool ShouldEnableSecondItemDiff(FileStatusItem item)
            => ShouldEnableSecondItemDiff(item, isSecondRevision: false)
               || ShouldEnableSecondItemDiff(item, isSecondRevision: true);

        /// <summary>
        /// A Git commitish representation of an object
        /// https://git-scm.com/docs/gitrevisions#_specifying_revisions
        /// </summary>
        /// <param name="getFileBlobHash">the Git module function to get the blob</param>
        /// <param name="item">the item</param>
        /// <param name="isSecondRevision">true if second revision is used</param>
        /// <returns>A Git commitish</returns>
        public string GetGitCommit([CanBeNull] Func<string, ObjectId, ObjectId> getFileBlobHash, [CanBeNull] FileStatusItem item, bool isSecondRevision)
        {
            if (item == null)
            {
                return null;
            }

            var name = !isSecondRevision && !string.IsNullOrWhiteSpace(item.Item.OldName) ? item.Item.OldName : item.Item.Name;
            var id = (isSecondRevision ? item.SecondRevision : item.FirstRevision)?.ObjectId;
            if (string.IsNullOrWhiteSpace(name) || id == null)
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
                return item.Item.TreeGuid != null
                    ? item.Item.TreeGuid.ToString()
                    : getFileBlobHash?.Invoke(name, id)?.ToString();
            }

            // commit:path
            return $"{id}:{name}";
        }
    }
}
