using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.Models;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    partial class FormBrowse
    {
        // This file is dedicated to init logic for FormBrowse commit details panel that includes
        // the commit info panel, the diff panel, the commit file tree, etc.

        private void InitCommitDetails()
        {
            // set tab page images
            CommitInfoTabControl.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = DpiUtil.Scale(new Size(16, 16)),
                Images =
                {
                    { nameof(Images.CommitSummary), Images.CommitSummary },
                    { nameof(Images.FileTree), Images.FileTree },
                    { nameof(Images.Diff), Images.Diff },
                    { nameof(Images.Key), Images.Key },
                    { nameof(Images.Console), Images.Console },
                    { nameof(Images.GitCommandLog), Images.GitCommandLog }
                }
            }
            .FixImageTransparencyRegression();

            CommitInfoTabPage.ImageKey = nameof(Images.CommitSummary);
            DiffTabPage.ImageKey = nameof(Images.Diff);
            TreeTabPage.ImageKey = nameof(Images.FileTree);
            GpgInfoTabPage.ImageKey = nameof(Images.Key);

            if (!AppSettings.ShowGpgInformation.Value)
            {
                CommitInfoTabControl.SelectedIndexChanged -= CommitInfoTabControl_SelectedIndexChanged;
                CommitInfoTabControl.RemoveIfExists(GpgInfoTabPage);
                CommitInfoTabControl.SelectedIndexChanged += CommitInfoTabControl_SelectedIndexChanged;
            }

            FillBuildReport(revision: null); // Ensure correct page visibility

            // Populate terminal tab after translation within InitializeComplete
            FillTerminalTab();

            _outputHistoryController = AppSettings.ShowOutputHistoryAsTab.Value
                ? new OutputHistoryTabController(UICommands.GetRequiredService<IOutputHistoryModel>(), new OutputHistoryControl(),
                    parent: CommitInfoTabControl, tabCaption: _outputHistoryTabCaption.Text)
                : new OutputHistoryPanelController(UICommands.GetRequiredService<IOutputHistoryModel>(), new OutputHistoryControl(),
                    parent: toolPanel.ContentPanel, verticalSplitContainer: LeftSplitContainer, horizontalSplitContainer: revisionDiff.HorizontalSplitter);
        }

        private void FillBuildReport(GitRevision? revision)
        {
            _buildReportTabPageExtension ??= new BuildReportTabPageExtension(() => Module, CommitInfoTabControl, _buildReportTabCaption.Text);

            // Note: FillBuildReport will check if tab is visible and revision is OK
            _buildReportTabPageExtension.FillBuildReport(revision);
        }
    }
}
