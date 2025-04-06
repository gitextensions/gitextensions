namespace GitUI.Editor;

/// <summary>
///  Manages watermark text display for a ComboBox control.
/// </summary>
internal sealed class ComboBoxWatermarkManager : IDisposable
{
    private readonly ComboBox _comboBox;
    private readonly string _watermarkText;
    private Font _watermarkFont;
    private readonly Color _watermarkColor = SystemColors.GrayText;
    private Font _originalFont;
    private Color _originalForeColor;
    private bool _isWatermarkActive;
    private bool _isDisposed;

    /// <param name="comboBox">The ComboBox control to manage.</param>
    /// <param name="watermarkText">The watermark text to display.</param>
    public ComboBoxWatermarkManager(ComboBox comboBox, string watermarkText)
    {
        _comboBox = comboBox ?? throw new ArgumentNullException(nameof(comboBox));
        _watermarkText = watermarkText ?? string.Empty;

        _originalFont = _comboBox.Font;
        _originalForeColor = _comboBox.ForeColor;
        _watermarkFont = new Font(_originalFont, FontStyle.Italic);

        if (string.IsNullOrEmpty(_comboBox.Text))
        {
            ShowWatermark();
        }

        _comboBox.Enter += ComboBox_Enter;
        _comboBox.LostFocus += ComboBox_LostFocus;
        _comboBox.TextChanged += ComboBox_TextChanged; // Handle programmatic changes or pasting
        _comboBox.FontChanged += ComboBox_FontChanged; // Keep original font updated
        _comboBox.ForeColorChanged += ComboBox_ForeColorChanged; // Keep original color updated
    }

    public string ComboBoxText
    {
        get
        {
            return _isWatermarkActive
                ? string.Empty
                : _comboBox.Text;
        }
    }

    public bool WatermarkActive => _isWatermarkActive;

    private void ComboBox_Enter(object sender, EventArgs e)
    {
        ActOpaqueToTextChanged(static wmm => wmm.HideWatermarkInternal(resetText: true));
    }

    private void ComboBox_LostFocus(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_comboBox.Text))
        {
            ActOpaqueToTextChanged(static wmm => wmm.ShowWatermark());
        }
    }

    private void ComboBox_TextChanged(object sender, EventArgs e)
    {
        string comboBoxText = _comboBox.Text;

        // If text changes while watermark is active (e.g., programmatically), hide watermark
        if (_isWatermarkActive && comboBoxText != _watermarkText && comboBoxText != "")
        {
            ActOpaqueToTextChanged(static wmm => wmm.HideWatermarkInternal(resetText: false));
        }
        else if (!_isWatermarkActive && comboBoxText == "")
        {
            ActOpaqueToTextChanged(static wmm => wmm.ShowWatermark());
        }
    }

    private void ComboBox_FontChanged(object sender, EventArgs e)
    {
        // If the font changes while the watermark is not active, update the original font.
        if (!_isWatermarkActive && !_isDisposed && _comboBox.Font != _watermarkFont)
        {
            _originalFont = _comboBox.Font;

            // Recreate watermark font based on new original font
            _watermarkFont?.Dispose(); // Dispose previous watermark font if exists
            _watermarkFont = new Font(_originalFont, FontStyle.Italic);
        }
    }

    private void ComboBox_ForeColorChanged(object sender, EventArgs e)
    {
        // If the color changes while the watermark is not active, update the original color.
        if (!_isWatermarkActive && !_isDisposed && _comboBox.ForeColor != _watermarkColor)
        {
            _originalForeColor = _comboBox.ForeColor;
        }
    }

    /// <summary>
    ///  Displays the watermark text in the ComboBox.
    /// </summary>
    public void ShowWatermark()
    {
        if (_isWatermarkActive || _isDisposed || _comboBox.Focused)
        {
            return;
        }

        _isWatermarkActive = true;
        _comboBox.Font = _watermarkFont;
        _comboBox.ForeColor = _watermarkColor;
        _comboBox.Text = _watermarkText;
    }

    /// <summary>
    ///  Hides the watermark text and restores the original appearance.
    /// </summary>
    public void HideWatermark()
    {
        HideWatermarkInternal(resetText: true);
    }

    private void HideWatermarkInternal(bool resetText)
    {
        if (!_isWatermarkActive || _isDisposed)
        {
            return;
        }

        _isWatermarkActive = false;

        if (resetText)
        {
            _comboBox.Text = string.Empty;
        }

        _comboBox.Font = _originalFont;
        _comboBox.ForeColor = _originalForeColor;
    }

    /// <summary>
    ///  Releases the resources used by the <see cref="ComboBoxWatermarkManager"/>.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _comboBox.Enter -= ComboBox_Enter;
        _comboBox.LostFocus -= ComboBox_LostFocus;
        _comboBox.TextChanged -= ComboBox_TextChanged;
        _comboBox.FontChanged -= ComboBox_FontChanged;
        _comboBox.ForeColorChanged -= ComboBox_ForeColorChanged;

        _watermarkFont?.Dispose();

        // Restore original state if watermark was active on dispose
        if (_isWatermarkActive)
        {
            _comboBox.Text = string.Empty;
            _comboBox.Font = _originalFont;
            _comboBox.ForeColor = _originalForeColor;
        }
    }

    private void ActOpaqueToTextChanged(Action<ComboBoxWatermarkManager> action)
    {
        // Temporarily unsubscribe to prevent recursion when ShowWatermark changes text
        _comboBox.TextChanged -= ComboBox_TextChanged;
        action(this);

        // Re-subscribe if not disposed
        if (!_isDisposed)
        {
            _comboBox.TextChanged += ComboBox_TextChanged;
        }
    }
}
