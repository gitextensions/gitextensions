using Avalonia.Controls;

namespace GitUI.UserControls;

/// <summary>
/// Native Avalonia counterpart for repository-host list views.
/// </summary>
/// <remarks>
/// Avalonia owns virtualization, scrolling, focus, and selection painting; repository-host forms
/// supply their source columns through native item templates and matching header controls.
/// </remarks>
public class NativeListView : ListBox
{
    protected override Type StyleKeyOverride => typeof(ListBox);
}
