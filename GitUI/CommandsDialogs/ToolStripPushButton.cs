﻿using GitCommands;
using GitCommands.Git;
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

        private IAheadBehindDataProvider? _aheadBehindDataProvider;

        public void Initialize(IAheadBehindDataProvider? aheadBehindDataProvider)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            ResetToDefaultState();
        }

        public void DisplayAheadBehindInformation(IDictionary<string, AheadBehindData>? aheadBehindData, string branchName)
        {
            ResetToDefaultState();

            if (string.IsNullOrWhiteSpace(branchName) || !AppSettings.ShowAheadBehindData)
            {
                return;
            }

            if (aheadBehindData is null || aheadBehindData.Count < 1 || !aheadBehindData.ContainsKey(branchName))
            {
                return;
            }

            var data = aheadBehindData[branchName];
            Text = data.ToDisplay();
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            ToolTipText = GetToolTipText(data);

            if (!string.IsNullOrEmpty(data.BehindCount))
            {
                Image = Images.Unstage.AdaptLightness();
            }
        }

        private void ResetToDefaultState()
        {
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

            public string? GetToolTipText(AheadBehindData data) => _button.GetToolTipText(data);
        }
    }
}
