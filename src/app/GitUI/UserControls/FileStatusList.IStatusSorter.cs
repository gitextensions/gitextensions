using GitExtensions.Extensibility.Git;

namespace GitUI;

partial class FileStatusList
{
    internal interface IStatusSorter
    {
        TreeNode CreateTreeSortedByPath(IEnumerable<GitItemStatus> statuses, bool flat, bool mergeSingleItemsWithFolder, Func<GitItemStatus, TreeNode> createNode);
    }
}
