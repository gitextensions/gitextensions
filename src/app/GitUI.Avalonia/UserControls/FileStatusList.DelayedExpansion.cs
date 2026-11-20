using Avalonia.Controls;

namespace GitUI;

partial class FileStatusList
{
    // Framework constraint: the expandable data node and the realized focus row are separate.
    private static void ExpandAll(object? node, TreeViewItem? focusedRow = null)
    {
        if (node is null)
        {
            return;
        }

        SetExpansion(node, expanded: true);
        focusedRow?.BringIntoView();
    }
}
