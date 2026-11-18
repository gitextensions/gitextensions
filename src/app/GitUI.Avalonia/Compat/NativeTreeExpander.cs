using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace GitUI.Compat;

/// <summary>
///  Lets a repository node approve its expansion before the chevron reflects the new state.
/// </summary>
internal sealed class NativeTreeExpander : ToggleButton
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="NativeTreeExpander"/> class.
    /// </summary>
    public NativeTreeExpander()
    {
    }

    protected override Type StyleKeyOverride => typeof(ToggleButton);

    protected override void Toggle()
    {
        if (TemplatedParent is NativeTreeViewItem item)
        {
            item.SetCurrentValue(TreeViewItem.IsExpandedProperty, !item.IsExpanded);
        }
        else
        {
            base.Toggle();
        }
    }
}
