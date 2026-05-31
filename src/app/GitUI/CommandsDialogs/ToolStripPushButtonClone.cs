namespace GitUI.CommandsDialogs;

/// <summary>
/// An independent clone of a <see cref="ToolStripPushButton"/> that can live on a custom toolbar
/// simultaneously with the original on the Standard toolbar.
/// <para>
/// Unlike a plain <see cref="ToolStripButton"/> clone, this class carries its own
/// <see cref="ShowLabel"/> / <see cref="LabelText"/> state (Option 1 from the architecture notes),
/// so the Standard and each custom toolbar instance can independently show or hide the "Push" label
/// next to the ahead/behind counter.
/// </para>
/// <para>
/// Synchronisation is driven by TextChanged, DisplayStyleChanged and EnabledChanged events on
/// the original. On each notification, the clone strips the original's label suffix from Text
/// (using the original's own LabelText) and rebuilds its own Text using its independent
/// ShowLabel/LabelText state. DisplayStyle, Image, ToolTipText, AutoSize and Enabled are
/// forwarded as-is. Image is always mutated before Text/DisplayStyle in the original, so it
/// arrives via the Text/DisplayStyle notifications without a dedicated ImageChanged subscription.
/// </para>
/// </summary>
internal sealed class ToolStripPushButtonClone : ToolStripButton, IPushLabelItem
{
    private readonly ToolStripPushButton _original;
    private bool _showLabel;
    private string? _labelText;

    public ToolStripPushButtonClone(ToolStripPushButton original, bool showLabel)
    {
        _original = original;
        _showLabel = showLabel;
        _labelText = original.LabelText;

        Name = $"clone_{original.Name}";
        Image = original.Image;
        ToolTipText = original.ToolTipText;
        AutoSize = original.AutoSize;
        ImageAlign = original.ImageAlign;
        ImageTransparentColor = original.ImageTransparentColor;
        Enabled = original.Enabled;
        Visible = true;
        Tag = original;

        // Compute initial Text / DisplayStyle from original's current state.
        SyncFromOriginal();

        // Image is copied in SyncFromOriginal on every notification. No dedicated ImageChanged
        // subscription is needed because every Image mutation in ToolStripPushButton
        // (Images.Unstage in DisplayAheadBehindInformation, Images.Push in ResetToDefaultState)
        // is immediately followed by a Text or DisplayStyle change that fires one of the events
        // below. If that invariant ever breaks, add an internal ImageChanged event to
        // ToolStripPushButton and subscribe here.
        original.TextChanged += OnOriginalChanged;
        original.DisplayStyleChanged += OnOriginalChanged;
        original.EnabledChanged += OnOriginalChanged;
        Disposed += (_, _) =>
        {
            original.TextChanged -= OnOriginalChanged;
            original.DisplayStyleChanged -= OnOriginalChanged;
            original.EnabledChanged -= OnOriginalChanged;
        };
    }

    public bool ShowLabel
    {
        get => _showLabel;
        set
        {
            if (_showLabel == value)
            {
                return;
            }

            _showLabel = value;
            RefreshDisplayedText();
        }
    }

    public string? LabelText
    {
        get => _labelText;
        set
        {
            if (_labelText == value)
            {
                return;
            }

            _labelText = value;
            RefreshDisplayedText();
        }
    }

    // Forward clicks to the original so all registered handlers fire.
    protected override void OnClick(EventArgs e)
    {
        _original.PerformClick();
        base.OnClick(e);
    }

    private void OnOriginalChanged(object? sender, EventArgs e) => SyncFromOriginal();

    private void SyncFromOriginal()
    {
        Image = _original.Image;
        ToolTipText = _original.ToolTipText;
        AutoSize = _original.AutoSize;
        ImageAlign = _original.ImageAlign;
        Enabled = _original.Enabled;

        // Rebuild Text and DisplayStyle from the original's current text, but honour our own
        // ShowLabel setting rather than mirroring the original's text verbatim.
        // The original's Text is either empty (idle, no label), the label alone (idle, with label),
        // a counter like "4↑" (data, no label), or "4↑ Push" (data, with label).
        // Strip the suffix the original appended (using the original's own LabelText), then
        // re-apply the clone's own label so the two can differ independently.
        string originalText = _original.Text ?? string.Empty;
        string originalLabel = _original.LabelText ?? _original._push.Text;
        string cloneLabel = _labelText ?? _original._push.Text;
        string strippedCounter = originalText.EndsWith(" " + originalLabel, StringComparison.Ordinal)
            ? originalText[..^(originalLabel.Length + 1)]
            : originalText;

        // Detect whether the original is in its idle state (no ahead/behind data) using the
        // text alone. Conditions: counter is empty (idle, no label) or counter equals the label
        // (idle, with label — the "counter" is just the label text itself).
        // Do NOT use _original.DisplayStyle here: DisplayStyle is set after Text in
        // DisplayAheadBehindInformation, so it may still be Image when TextChanged fires even
        // though the counter has already been written to Text.
        bool originalIsIdle = string.IsNullOrEmpty(strippedCounter)
            || strippedCounter == originalLabel;

        if (originalIsIdle)
        {
            if (_showLabel)
            {
                Text = cloneLabel;
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }
            else
            {
                Text = string.Empty;
                DisplayStyle = ToolStripItemDisplayStyle.Image;
            }
        }
        else
        {
            // Counter data is present.
            Text = _showLabel ? $"{strippedCounter} {cloneLabel}" : strippedCounter;
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        }
    }

    // Called when this clone's own ShowLabel or LabelText change.
    private void RefreshDisplayedText()
    {
        // Recompute from the original's current state — SyncFromOriginal handles both the idle
        // and the ahead/behind-data cases.
        SyncFromOriginal();
    }
}
