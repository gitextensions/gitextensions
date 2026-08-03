using System.ComponentModel;
using Avalonia.Controls;
using GitExtensions.Extensibility.Translations.Xliff;

namespace GitUI.UserControls;

/// <summary>
///  A ComboBox control with built-in watermark text functionality.
/// </summary>
[ToolboxItem(true)]
[Description("A ComboBox control with built-in watermark text functionality.")]
[LocalizableProperties(nameof(Watermark))]
public sealed class WatermarkComboBox : ComboBox
{
    public WatermarkComboBox()
    {
        IsEditable = true;
        Watermark = string.Empty;
    }

    /// <summary>
    ///  Gets or sets the watermark text to display when the ComboBox is empty.
    /// </summary>
    [Category("Appearance")]
    [Description("The watermark text to display when the ComboBox is empty.")]
    [DefaultValue("")]
    public string Watermark
    {
        get;
        set
        {
            string val = value ?? string.Empty;
            if (field == val)
            {
                return;
            }

            field = val;
            PlaceholderText = val;
        }
    }

    /// <summary>
    ///  Gets a value indicating whether the watermark is currently visible.
    /// </summary>
    [Browsable(false)]
    public bool IsWatermarkVisible => !IsKeyboardFocusWithin && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Watermark);

    // Avalonia renders PlaceholderText without replacing Text, so BaseText and Text remain identical.
    internal string BaseText => Text ?? string.Empty;

    // parity-scaffolding: Exposes the original observable watermark boundary to focused tests.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(WatermarkComboBox comboBox)
    {
        internal string BaseText => comboBox.BaseText;
    }
}
