using GitCommands;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse commit details panel that includes
    // the commit info panel, the diff panel, the commit file tree, etc.

    private void InitCommitDetails()
    {
        // set tab page images
        CommitInfoTabPage.Icon = Images.CommitSummary;
        DiffTabPage.Icon = Images.Diff;
        TreeTabPage.Icon = Images.FileTree;
        GpgInfoTabPage.Icon = Images.Key;

        if (!AppSettings.ShowGpgInformation.Value)
        {
            GpgInfoTabPage.IsVisible = false;
        }

        FillBuildReport(revision: null); // Ensure correct page visibility

        // Populate terminal tab after translation within InitializeComplete
        FillTerminalTab();
    }

    private void FillBuildReport(GitRevision? revision)
    {
        _buildReportTabPageExtension ??= new BuildReportTabPageExtension(
            () => Module,
            CommitInfoTabControl,
            _buildReportTabCaption.Text);

        // Note: FillBuildReport will check if tab is visible and revision is OK
        _buildReportTabPageExtension.FillBuildReport(revision);
    }
}
