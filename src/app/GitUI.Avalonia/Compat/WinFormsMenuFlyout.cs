using Avalonia.Controls;

namespace GitUI.Compat;

/// <summary>
///  Preserves ToolStrip character activation in an otherwise native menu flyout.
/// </summary>
public sealed class WinFormsMenuFlyout : MenuFlyout
{
    protected override Control CreatePresenter()
    {
        MenuFlyoutPresenter presenter = new(new MenuMnemonicInteractionHandler(true))
        {
            ItemsSource = Items,
        };
        presenter[!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty];
        presenter[!ItemsControl.ItemContainerThemeProperty] = this[!ItemContainerThemeProperty];
        return presenter;
    }
}
