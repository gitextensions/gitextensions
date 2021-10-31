using System;
using System.Windows.Forms;
using GitUI;

namespace GitExtensions.Plugins.FindLargeFiles
{
    /// <summary>
    /// Custom sortable binding branches list, use for support user-defined sorting in <see cref="DataGridView"/>.
    /// </summary>
    internal sealed class SortableObjectsList : SortableBindingList<GitObject>
    {
        static SortableObjectsList()
        {
            AddSortableProperty(branch => branch.LastCommitDate, (x, y) => DateTime.Compare(x.LastCommitDate, y.LastCommitDate));
            AddSortableProperty(branch => branch.Path, (x, y) => string.Compare(x.Path, y.Path, StringComparison.CurrentCulture));
            AddSortableProperty(branch => branch.SHA, (x, y) => string.Compare(x.SHA, y.SHA));
            AddSortableProperty(branch => branch.CommitCount, (x, y) => x.CommitCount.CompareTo(y.CommitCount));
            AddSortableProperty(branch => branch.Size, (x, y) => x.SizeInBytes.CompareTo(y.SizeInBytes));
            AddSortableProperty(branch => branch.CompressedSize, (x, y) => x.CompressedSizeInBytes.CompareTo(y.CompressedSizeInBytes));
        }
    }
}
