using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs;

/// <summary>
///  Displays the current branch's push tracking state in the main toolbar.
/// </summary>
public sealed class ToolStripPushButton : IconButton
{
    private readonly TranslationString _push = new("Push");
    private readonly TranslationString _aheadCommitsToPush = new("{0} new commit(s) will be pushed");
    private readonly TranslationString _behindCommitsTointegrateOrForcePush = new("{0} commit(s) should be integrated (or will be lost if force pushed)");

    public ToolStripPushButton()
    {
        ResetToDefaultState();
    }

    public void DisplayAheadBehindInformation(
        IReadOnlyDictionary<string, AheadBehindData>? aheadBehindData,
        string branchName,
        string shortcut)
    {
        if (string.IsNullOrWhiteSpace(branchName)
            || !AppSettings.ShowAheadBehindData
            || aheadBehindData?.TryGetValue(branchName, out AheadBehindData data) is not true)
        {
            ResetToDefaultState();
            ToolTip.SetTip(this, _push.Text.UpdateSuffix(shortcut));
            return;
        }

        MinWidth = 0;
        Classes.Set("gitextensions-icon-only", false);
        Content = data.ToDisplay();
        ToolTip.SetTip(this, GetToolTipText(data).UpdateSuffix(shortcut));
        Icon = string.IsNullOrEmpty(data.BehindCount) ? Images.Push : Images.Unstage;
    }

    /// <summary>
    ///  Clears stale branch information while preserving the toolbar button's place.
    /// </summary>
    public void ResetBeforeUpdate()
    {
        MinWidth = Math.Max(MinWidth, Bounds.Width);
        Classes.Set("gitextensions-icon-only", false);
        Content = string.Empty;
        ToolTip.SetTip(this, _push.Text);
    }

    /// <summary>
    ///  Restores the ordinary icon-only Push presentation.
    /// </summary>
    public void ResetToDefaultState()
    {
        MinWidth = 0;
        Classes.Set("gitextensions-icon-only", true);
        Content = _push.Text;
        Icon = Images.Push;
        ToolTip.SetTip(this, _push.Text);
    }

    private string GetToolTipText(AheadBehindData data)
    {
        string tooltip = string.Empty;
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

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly ToolStripPushButton _button;

        public TestAccessor(ToolStripPushButton button)
        {
            _button = button;
        }

        public string GetButtonText() => _button.Content as string ?? string.Empty;
        public bool IsIconOnly() => _button.Classes.Contains("gitextensions-icon-only");
    }
}
