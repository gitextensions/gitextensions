using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public class ToolStripPushButton : ToolStripButton, IPushLabelItem
{
    internal readonly TranslationString _push = new("Push");

    private readonly TranslationString _aheadCommitsToPush =
        new("{0} new commit(s) will be pushed");

    private readonly TranslationString _behindCommitsTointegrateOrForcePush =
        new("{0} commit(s) should be integrated (or will be lost if force pushed)");

    private bool _showLabel;
    private string? _labelText;
    private bool _hasAheadBehindData;

    // The label suffix currently appended to Text (if any), tracked so RefreshDisplayedText can
    // strip the previously displayed suffix even after LabelText has already been changed to a new value.
    private string? _lastDisplayedLabel;

    // Whether the user has enabled the text label for this button ("For this icon" / "For all icons").
    // When true, the label text ("Push") is appended after the ahead/behind counter.
    // Updating this property immediately refreshes the displayed text.
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

    // The label text to display next to the ahead/behind counter when <see cref="ShowLabel"/> is true.
    // Defaults to the translated "Push" string if not set.
    // Updating this property immediately refreshes the displayed text.
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

    /// <summary>
    /// Rebuilds <see cref="ToolStripItem.Text"/> from the current ahead/behind data and label
    /// preferences without re-querying git. Called when <see cref="ShowLabel"/> or
    /// <see cref="LabelText"/> change so that clones (which mirror Text via TextChanged) update
    /// immediately without waiting for the next branch refresh.
    /// </summary>
    private void RefreshDisplayedText()
    {
        // No ahead/behind data is currently shown (idle state): just update Text/DisplayStyle to
        // reflect the new ShowLabel setting; the next DisplayAheadBehindInformation call will
        // repopulate the counter.
        if (!_hasAheadBehindData)
        {
            if (_showLabel)
            {
                Text = _labelText ?? _push.Text;
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }
            else
            {
                Text = string.Empty;
                DisplayStyle = ToolStripItemDisplayStyle.Image;
            }

            return;
        }

        // Ahead/behind data is currently displayed: rebuild the text preserving the counter.
        // Text is either "X↑ Y↓" (no label) or "X↑ Y↓ Push" (with label).
        // Strip the previously displayed label suffix (not the new one - LabelText may have
        // already changed by the time this runs), then re-append according to ShowLabel.
        string label = _labelText ?? _push.Text;
        string current = Text ?? string.Empty;
        string strippedCounter = _lastDisplayedLabel is not null && current.EndsWith(" " + _lastDisplayedLabel, StringComparison.Ordinal)
            ? current[..^(_lastDisplayedLabel.Length + 1)]
            : current;

        Text = _showLabel ? $"{strippedCounter} {label}" : strippedCounter;
        _lastDisplayedLabel = _showLabel ? label : null;
    }

    public void DisplayAheadBehindInformation(IReadOnlyDictionary<string, AheadBehindData>? aheadBehindData, string branchName, string shortcut)
    {
        if (string.IsNullOrWhiteSpace(branchName)
            || !AppSettings.ShowAheadBehindData
            || aheadBehindData?.TryGetValue(branchName, out AheadBehindData data) is not true)
        {
            ResetToDefaultState(shortcut);
            return;
        }

        _hasAheadBehindData = true;
        ImageAlign = ContentAlignment.MiddleLeft;
        AutoSize = true;
        ToolTipText = GetToolTipText(data)!.UpdateSuffix(shortcut);
        if (!string.IsNullOrEmpty(data.BehindCount))
        {
            Image = Images.Unstage;
        }

        string aheadBehind = data.ToDisplay();
        string label = LabelText ?? _push.Text;
        Text = ShowLabel ? $"{aheadBehind} {label}" : aheadBehind;
        _lastDisplayedLabel = ShowLabel ? label : null;
        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
    }

    /// <summary>
    /// Reset the contents keeping the size the same, to avoid toolbar resizing.
    /// </summary>
    public void ResetBeforeUpdate()
    {
        AutoSize = false;
        ToolTipText = _push.Text;
    }

    public void ResetToDefaultState()
        => ResetToDefaultState(shortcut: null);

    private void ResetToDefaultState(string? shortcut)
    {
        _hasAheadBehindData = false;
        _lastDisplayedLabel = null;
        AutoSize = true;
        Image = Images.Push;
        ToolTipText = _push.Text.UpdateSuffix(shortcut ?? string.Empty);
        if (ShowLabel)
        {
            Text = LabelText ?? _push.Text;
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        }
        else
        {
            Text = string.Empty;
            DisplayStyle = ToolStripItemDisplayStyle.Image;
        }
    }

    private string? GetToolTipText(AheadBehindData data)
    {
        string? tooltip = null;
        if (!string.IsNullOrEmpty(data.AheadCount))
        {
            tooltip = string.Format(_aheadCommitsToPush.Text, data.AheadCount);
        }

        if (!string.IsNullOrEmpty(data.BehindCount))
        {
            if (!string.IsNullOrEmpty(tooltip))
            {
                tooltip += Environment.NewLine;
            }

            tooltip += string.Format(_behindCommitsTointegrateOrForcePush.Text, data.BehindCount);
        }

        return tooltip;
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly ToolStripPushButton _button;

        public TestAccessor(ToolStripPushButton button)
        {
            _button = button;
        }

        public string GetButtonText() => _button.Text!;
        public int GetButtonWidth() => _button.Width;
    }
}
