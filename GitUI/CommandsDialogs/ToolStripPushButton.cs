using System;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public class ToolStripPushButton : ToolStripButton
    {
        private readonly TranslationString _push = new TranslationString("Push");

        private readonly TranslationString _aheadCommitsToPush =
            new TranslationString("{0} new commit(s) will be pushed");

        private readonly TranslationString _behindCommitsTointegrateOrForcePush =
            new TranslationString("{0} commit(s) should be integrated (or will be lost if force pushed)");

        private IAheadBehindDataProvider _aheadBehindDataProvider;
        private bool _supportsAheadBehindData;

        public void Initialize(IAheadBehindDataProvider aheadBehindDataProvider, bool supportsAheadBehindData)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _supportsAheadBehindData = supportsAheadBehindData;
            ResetToDefaultState();
        }

        public void DisplayAheadBehindInformation(string branchName)
        {
            if (!_supportsAheadBehindData || !AppSettings.ShowAheadBehindData)
            {
                return;
            }

            ResetToDefaultState();

            var aheadBehindData = _aheadBehindDataProvider.GetData(branchName);
            if (aheadBehindData == null || aheadBehindData.Count < 1 || !aheadBehindData.ContainsKey(branchName))
            {
                return;
            }

            var data = aheadBehindData[branchName];

            if (AppSettings.ShowAheadBehindData)
            {
                Text = data.ToDisplay();
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }

            ToolTipText = GetToolTipText(data);

            if (!string.IsNullOrEmpty(data.BehindCount))
            {
                Image = Images.Unstage;
            }
        }

        private void ResetToDefaultState()
        {
            DisplayStyle = ToolStripItemDisplayStyle.Image;
            Image = Images.Push;
            ToolTipText = _push.Text;
        }

        private string GetToolTipText(AheadBehindData data)
        {
            string tooltip = null;
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
            => new TestAccessor(this);

        public readonly struct TestAccessor
        {
            private readonly ToolStripPushButton _button;

            public TestAccessor(ToolStripPushButton button)
            {
                _button = button;
            }

            public string GetToolTipText(AheadBehindData data) => _button.GetToolTipText(data);
        }
    }
}
