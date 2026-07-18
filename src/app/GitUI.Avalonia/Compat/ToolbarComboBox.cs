using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
/// An editable toolbar combo retaining the Fluent ComboBox behavior with a compact desktop
/// drop-down column that cannot be changed through an Avalonia style setter.
/// </summary>
public class ToolbarComboBox : ComboBox
{
    public ToolbarComboBox()
    {
        Classes.Add("gitextensions-toolbar-input");
    }

    protected override Type StyleKeyOverride => typeof(ComboBox);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        TextBox? editor = e.NameScope.Find<TextBox>("PART_EditableTextBox");
        Grid? layout = editor?.GetVisualParent<Grid>();
        if (layout is not null && layout.ColumnDefinitions.Count > 1)
        {
            layout.ColumnDefinitions[1].Width = new GridLength(16);
        }
    }
}
