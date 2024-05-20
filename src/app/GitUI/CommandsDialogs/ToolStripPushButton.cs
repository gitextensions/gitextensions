using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public class ToolStripPushButton : ToolStripButton
    {
        private readonly TranslationString _push = new("Push");

        private readonly TranslationString _aheadCommitsToPush =
            new("{0} new commit(s) will be pushed");

        private readonly TranslationString _behindCommitsTointegrateOrForcePush =
            new("{0} commit(s) should be integrated (or will be lost if force pushed)");

        public void DisplayAheadBehindInformation(IDictionary<string, AheadBehindData>? aheadBehindData, string branchName, string shortcut)
        {
            if (string.IsNullOrWhiteSpace(branchName)
                || !AppSettings.ShowAheadBehindData
                || aheadBehindData?.TryGetValue(branchName, out AheadBehindData data) is not true)
            {
                ResetToDefaultState();
                ToolTipText = ToolTipText.UpdateSuffix(shortcut);
                return;
            }

            ImageAlign = ContentAlignment.MiddleLeft;
            AutoSize = true;
            Text = data.ToDisplay();
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            ToolTipText = GetToolTipText(data).UpdateSuffix(shortcut);

            if (!string.IsNullOrEmpty(data.BehindCount))
            {
                Image = Images.Unstage.AdaptLightness();
            }
        }

        /// <summary>
        /// Reset the contents keeping the size the same, to avoid toolbar resizing.
        /// </summary>
        public void ResetBeforeUpdate()
        {
            AutoSize = false;
            Text = "";
            ToolTipText = _push.Text;
        }

        public void ResetToDefaultState()
        {
            AutoSize = true;
            DisplayStyle = ToolStripItemDisplayStyle.Image;
            Image = Images.Push.AdaptLightness();
            ToolTipText = _push.Text;
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

            public string GetButtonText() => _button.Text;
            public int GetButtonWidth() => _button.Width;
        }
    }
}
