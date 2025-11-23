#nullable enable

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
    private readonly Color _watermarkColor = SystemColors.GrayText;
    private Font? _watermarkFont;
    private Font? _originalFont;
    private Color _originalForeColor;
    private bool _isInitialized;
    private bool _suppressEvents;

    public WatermarkComboBox()
    {
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

            if (IsWatermarkVisible)
            {
                ShowWatermark();
            }
        }
    }

    /// <summary>
    ///  Gets a value indicating whether the watermark is currently visible.
    /// </summary>
    [Browsable(false)]
    public bool IsWatermarkVisible { get; private set; }

    /// <summary>
    ///  Gets or sets the text associated with this control, handling watermark display automatically.
    /// </summary>
    [AllowNull]
    public override string Text
    {
        get => IsWatermarkVisible ? string.Empty : base.Text;
        set
        {
            string val = value ?? string.Empty;
            if (IsWatermarkVisible && val == Watermark)
            {
                return;
            }

            if (IsWatermarkVisible)
            {
                HideWatermark(resetText: false);
            }

            base.Text = val;

            if (ShouldWatermarkBeVisible(val))
            {
                ShowWatermark();
            }
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        InitializeWatermark();

        if (ShouldWatermarkBeVisible(base.Text))
        {
            ShowWatermark();
        }
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        HideWatermark(resetText: true);
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);

        if (ShouldWatermarkBeVisible(base.Text))
        {
            ShowWatermark(leaving: true);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        if (_suppressEvents)
        {
            return;
        }

        base.OnTextChanged(e);

        if (ShouldWatermarkBeVisible(base.Text))
        {
            ShowWatermark();
        }
        else
        {
            HideWatermark(resetText: false);
        }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);

        Font currentFont = Font;
        if (!IsWatermarkVisible && _isInitialized && !currentFont.Equals(_watermarkFont) && !currentFont.Equals(_originalFont))
        {
            _originalFont = currentFont;
            UpdateWatermarkFont();
        }
    }

    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);

        if (!IsWatermarkVisible && _isInitialized && ForeColor != _watermarkColor)
        {
            _originalForeColor = ForeColor;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _watermarkFont?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeWatermark()
    {
        _isInitialized = true;

        _originalFont = Font;
        _originalForeColor = ForeColor;
        UpdateWatermarkFont();
    }

    private void UpdateWatermarkFont()
    {
        _watermarkFont?.Dispose();
        _watermarkFont = new Font((Font)(_originalFont ?? Font).Clone(), FontStyle.Italic);
    }

    private void ShowWatermark(bool leaving = false)
    {
        if ((IsWatermarkVisible && base.Text == Watermark) || !_isInitialized || (!leaving && Focused) || string.IsNullOrEmpty(Watermark))
        {
            return;
        }

        IsWatermarkVisible = true;

        ActWithEventsSuppressed(() =>
        {
            if (_watermarkFont is not null)
            {
                Font = _watermarkFont;
            }

            ForeColor = _watermarkColor;
            base.Text = Watermark;
        });
    }

    private void HideWatermark(bool resetText)
    {
        if (!IsWatermarkVisible || !_isInitialized)
        {
            return;
        }

        IsWatermarkVisible = false;

        ActWithEventsSuppressed(() =>
        {
            if (resetText)
            {
                base.Text = string.Empty;
            }

            if (_originalFont is not null)
            {
                Font = _originalFont;
            }

            ForeColor = _originalForeColor;
        });
    }

    private static bool ShouldWatermarkBeVisible(string text)
    {
        return string.IsNullOrEmpty(text);
    }

    /// <summary>
    ///  Prevent text change events.
    /// </summary>
    private void ActWithEventsSuppressed(Action action)
    {
        try
        {
            _suppressEvents = true;
            action();
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    internal string BaseText => base.Text;

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly WatermarkComboBox _comboBox;

        internal TestAccessor(WatermarkComboBox comboBox)
        {
            _comboBox = comboBox;
        }

        internal string BaseText => _comboBox.BaseText;
    }
}
