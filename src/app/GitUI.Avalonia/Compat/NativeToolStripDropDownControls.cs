using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;

namespace GitUI.Compat;

/// <summary>
///  Keeps the MenuFlyout interaction routes with a presenter whose coordinates are already source-directed.
/// </summary>
public sealed class NativeToolStripDropDownMenuFlyout : MenuFlyout
{
    private readonly IMenuInteractionHandler? _interactionHandler;

    public NativeToolStripDropDownMenuFlyout()
    {
    }

    internal NativeToolStripDropDownMenuFlyout(IMenuInteractionHandler interactionHandler)
        => _interactionHandler = interactionHandler;

    protected override Control CreatePresenter()
    {
        NativeToolStripDropDownPresenter presenter = _interactionHandler is null
            ? new() : new(_interactionHandler);
        presenter.ItemsSource = Items;
        presenter[!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty];
        presenter[!ItemsControl.ItemContainerThemeProperty] = this[!ItemContainerThemeProperty];
        return presenter;
    }
}

/// <summary>
///  Presents the source's directional dropdown rectangles without a second popup-wide mirror.
/// </summary>
public sealed class NativeToolStripDropDownPresenter : MenuFlyoutPresenter
{
    public NativeToolStripDropDownPresenter()
    {
    }

    internal NativeToolStripDropDownPresenter(IMenuInteractionHandler interactionHandler)
        : base(interactionHandler)
    {
    }

    protected override Type StyleKeyOverride => typeof(MenuFlyoutPresenter);

    protected override bool BypassFlowDirectionPolicies => true;
}

/// <summary>
///  Keeps the separator in its source dropdown slot without a second RTL mirror.
/// </summary>
public sealed class NativeToolStripDropDownSeparator : Separator
{
    public NativeToolStripDropDownSeparator()
    {
    }

    protected override Type StyleKeyOverride => typeof(Separator);

    protected override bool BypassFlowDirectionPolicies => true;
}

/// <summary>
///  Keeps MenuItem's interaction implementation while its owned source layout supplies RTL coordinates.
/// </summary>
public sealed class NativeToolStripDropDownMenuItem : MenuItem
{
    public static readonly AttachedProperty<bool> UseSystemVisualStyleProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStripDropDownMenuItem, Control, bool>("UseSystemVisualStyle");

    public NativeToolStripDropDownMenuItem()
    {
    }

    internal bool UseSourceMnemonicRouting { get; set; }

    protected override Type StyleKeyOverride => typeof(MenuItem);

    protected override bool BypassFlowDirectionPolicies => true;

    public static bool GetUseSystemVisualStyle(Control item) => item.GetValue(UseSystemVisualStyleProperty);

    public static void SetUseSystemVisualStyle(Control item, bool value) => item.SetValue(UseSystemVisualStyleProperty, value);

    protected override void OnAccessKey(RoutedEventArgs e)
    {
        if (UseSourceMnemonicRouting)
        {
            // Only this owned popup replaces AccessText's first-marker routing.
            // Its existing presenter and always-visible underline policy remain.
            e.Handled = true;
            return;
        }

        base.OnAccessKey(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseSystemVisualStyleProperty)
        {
            PseudoClasses.Set(":source-system-renderer", GetUseSystemVisualStyle(this));
        }
    }
}

/// <summary>
///  Presents menu captions in an already direction-aware source rectangle.
/// </summary>
public sealed class NativeToolStripMenuCaption : ContentPresenter
{
    private bool _normalizing;

    public NativeToolStripMenuCaption()
    {
    }

    protected override Type StyleKeyOverride => typeof(ContentPresenter);

    protected override bool BypassFlowDirectionPolicies => true;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_normalizing && change.Property == ContentProperty && Content is string text)
        {
            string normalized = NormalizeCaption(text);
            if (normalized != text)
            {
                _normalizing = true;
                SetCurrentValue(ContentProperty, normalized);
                _normalizing = false;
            }
        }
    }

    private static string NormalizeCaption(string text)
    {
        // GDI parses every native prefix while AccessText removes only its first.
        // Keep the original Header/search/measurement model and first access key;
        // normalize only this owned presentation's additional prefix markers.
        StringBuilder result = new(text.Length);
        bool hasAccessKey = false;
        for (int index = 0; index < text.Length; index++)
        {
            char character = text[index];
            if (character != '_')
            {
                result.Append(character);
            }
            else if (index + 1 < text.Length && text[index + 1] == '_')
            {
                result.Append("__");
                index++;
            }
            else if (!hasAccessKey && index + 1 < text.Length)
            {
                result.Append('_');
                hasAccessKey = true;
            }
        }

        return result.ToString();
    }
}

/// <summary>
///  Presents the image in its source rectangle without automatically mirroring the image.
/// </summary>
public sealed class NativeToolStripMenuIconPresenter : ContentControl
{
    public NativeToolStripMenuIconPresenter()
    {
    }

    protected override Type StyleKeyOverride => typeof(ContentControl);

    protected override bool BypassFlowDirectionPolicies => true;
}

/// <summary>
///  Keeps native TextBox input while the control host owns its already direction-aware outer slot.
/// </summary>
public sealed class NativeToolStripMenuTextBox : TextBox
{
    public NativeToolStripMenuTextBox()
    {
    }

    protected override Type StyleKeyOverride => typeof(TextBox);

    protected override bool BypassFlowDirectionPolicies => true;
}
