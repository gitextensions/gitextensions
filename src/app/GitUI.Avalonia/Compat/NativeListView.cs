using Avalonia.Controls;
using Avalonia.Interactivity;
using GitUI.Compat.WinFormsControls;

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
    public IList<ColumnHeader> Columns { get; } = [];

    /// <summary>
    /// Raised when a retained group-header task link is activated.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? GroupTaskLinkClick;

    protected override Type StyleKeyOverride => typeof(ListBox);

    public void AddColumns(params ColumnHeader[] columns)
    {
        for (int index = 0; index < columns.Length; index++)
        {
            columns[index].DisplayIndex = index;
            Columns.Add(columns[index]);
        }
    }

    public void RaiseGroupTaskLinkClick(object? sender, RoutedEventArgs e)
        => GroupTaskLinkClick?.Invoke(sender ?? this, e);
}
