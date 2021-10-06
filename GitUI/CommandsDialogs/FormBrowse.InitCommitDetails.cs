using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;
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
                    { nameof(Images.Console), Images.Console }
                }
            };

            CommitInfoTabPage.ImageKey = nameof(Images.CommitSummary);
            DiffTabPage.ImageKey = nameof(Images.Diff);
            TreeTabPage.ImageKey = nameof(Images.FileTree);
            GpgInfoTabPage.ImageKey = nameof(Images.Key);

            if (!AppSettings.ShowGpgInformation.Value)
            {
                CommitInfoTabControl.RemoveIfExists(GpgInfoTabPage);
            }

            FillBuildReport(revision: null); // Ensure correct page visibility

            // Populate terminal tab after translation within InitializeComplete
            FillTerminalTab();
        }

        private void FillBuildReport(GitRevision? revision)
        {
            _buildReportTabPageExtension ??= new BuildReportTabPageExtension(() => Module, CommitInfoTabControl, _buildReportTabCaption.Text);

            // Note: FillBuildReport will check if tab is visible and revision is OK
            _buildReportTabPageExtension.FillBuildReport(revision);
        }
    }
}
