#nullable enable

using System.ComponentModel;

namespace GitUI.UserControls;

/// <summary>
///  A ComboBox control with built-in watermark text functionality.
/// </summary>
[ToolboxItem(true)]
[Description("A ComboBox control with built-in watermark text functionality.")]
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

            if (_isInitialized)
            {
                UpdateWatermarkDisplay();
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
                ActWithEventsSuppressed(() =>
                {
                    HideWatermark(resetText: false);
                });
            }

            base.Text = val;

            if (_isInitialized && ShouldWatermarkBeVisible(val) && !Focused)
            {
                ActWithEventsSuppressed(() =>
                {
                    ShowWatermark();
                });
            }
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        InitializeWatermark();
        UpdateWatermarkDisplay();
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        ActWithEventsSuppressed(() => HideWatermark(resetText: true));
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);

        if (ShouldWatermarkBeVisible(base.Text))
        {
            ActWithEventsSuppressed(() => ShowWatermark(leaving: true));
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        if (_suppressEvents)
        {
            return;
        }

        base.OnTextChanged(e);

        if (ShouldWatermarkBeVisible(Text))
        {
            ActWithEventsSuppressed(() => ShowWatermark());
        }
        else
        {
            ActWithEventsSuppressed(() => HideWatermark(resetText: false));
        }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        if (_suppressEvents)
        {
            return;
        }

        _originalFont = Font;
        UpdateWatermarkFont();
        if (IsWatermarkVisible)
        {
            ActWithEventsSuppressed(() => ShowWatermark());
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
        if (!_isInitialized)
        {
            return;
        }

        _originalFont = Font;
        _originalForeColor = ForeColor;
        UpdateWatermarkFont();
    }

    private void UpdateWatermarkFont()
    {
        _watermarkFont?.Dispose();
        _watermarkFont = _originalFont is not null
            ? new Font(_originalFont, FontStyle.Italic)
            : new Font(Font, FontStyle.Italic);
    }

    private void UpdateWatermarkDisplay()
    {
        if (string.IsNullOrEmpty(base.Text) && !Focused)
        {
            ActWithEventsSuppressed(() => ShowWatermark());
        }

        if (IsWatermarkVisible)
        {
            ActWithEventsSuppressed(() => base.Text = Watermark);
        }
    }

    private void ShowWatermark(bool leaving = false)
    {
        if (IsWatermarkVisible || !_isInitialized || (!leaving && Focused) || string.IsNullOrEmpty(Watermark))
        {
            return;
        }

        IsWatermarkVisible = true;

        if (_watermarkFont is not null)
        {
            Font = _watermarkFont;
        }

        ForeColor = _watermarkColor;
        base.Text = Watermark;
    }

    private void HideWatermark(bool resetText)
    {
        if (!IsWatermarkVisible || !_isInitialized)
        {
            return;
        }

        IsWatermarkVisible = false;

        if (resetText)
        {
            base.Text = string.Empty;
        }

        if (_originalFont is not null)
        {
            Font = _originalFont;
        }

        ForeColor = _originalForeColor;
    }

    private static bool ShouldWatermarkBeVisible(string text)
    {
        return string.IsNullOrEmpty(text);
    }

    /// <summary>
    ///  Prevent text and font change events.
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
