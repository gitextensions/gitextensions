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
    private bool _suppressTextChangedEvent;

    public WatermarkComboBox()
    {
        Watermark = string.Empty;
        InitializeWatermark();
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

            ActOpaqueToTextChanged(() =>
            {
                if (IsWatermarkVisible)
                {
                    HideWatermark(resetText: false);
                }

                base.Text = val;

                if (_isInitialized && string.IsNullOrEmpty(val) && !Focused)
                {
                    ShowWatermark();
                }
            });
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
        ActOpaqueToTextChanged(() => HideWatermark(resetText: true));
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);

        if (string.IsNullOrEmpty(base.Text))
        {
            ActOpaqueToTextChanged(() => ShowWatermark(leaving: true));
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        if (_suppressTextChangedEvent)
        {
            base.OnTextChanged(e);
            return;
        }

        string currentText = base.Text;

        if (IsWatermarkVisible && currentText != Watermark && !string.IsNullOrEmpty(currentText))
        {
            ActOpaqueToTextChanged(() => HideWatermark(resetText: false));
        }
        else if (!IsWatermarkVisible && string.IsNullOrEmpty(currentText) && !Focused)
        {
            ActOpaqueToTextChanged(() => ShowWatermark());
        }

        base.OnTextChanged(e);
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
            ShowWatermark();
        }

        if (IsWatermarkVisible)
        {
            ActOpaqueToTextChanged(() => base.Text = Watermark);
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

    /// <summary>
    /// Prevent text change events from causing recursive calls when updating the text.
    /// </summary>
    private void ActOpaqueToTextChanged(Action action)
    {
        if (_suppressTextChangedEvent)
        {
            return;
        }

        _suppressTextChangedEvent = true;

        try
        {
            action();
        }
        finally
        {
            _suppressTextChangedEvent = false;
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
