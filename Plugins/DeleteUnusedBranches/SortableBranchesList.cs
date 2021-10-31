using System;
using System.Windows.Forms;
using GitUI;

namespace GitExtensions.Plugins.DeleteUnusedBranches
{
    /// <summary>
    /// Custom sortable binding branches list, use for support user-defined sorting in <see cref="DataGridView"/>.
    /// </summary>
    internal sealed class SortableBranchesList : SortableBindingList<Branch>
    {
        static SortableBranchesList()
        {
            AddSortableProperty(branch => branch.Date, (x, y) => DateTime.Compare(x.Date, y.Date));
            AddSortableProperty(branch => branch.Name, (x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
            AddSortableProperty(branch => branch.Author, (x, y) => string.Compare(x.Author, y.Author, StringComparison.CurrentCulture));
        }
    }
}
