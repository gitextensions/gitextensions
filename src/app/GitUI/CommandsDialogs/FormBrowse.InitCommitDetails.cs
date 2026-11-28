using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse commit details panel that includes
    // the commit info panel, the diff panel, the commit file tree, etc.

    private const string _selectedTabSettingName = "CommitInfoTabControl_SelectedTab";

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

        if (!AppSettings.ShowGpgInformation)
        {
            CommitInfoTabControl.SelectedIndexChanged -= CommitInfoTabControl_SelectedIndexChanged;
            CommitInfoTabControl.RemoveIfExists(GpgInfoTabPage);
            CommitInfoTabControl.SelectedIndexChanged += CommitInfoTabControl_SelectedIndexChanged;
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

    private void RestoreSelectedTab()
    {
        // The file history and blame modes select the tab to show on their own
        if (_isFileHistoryMode)
        {
            return;
        }

        string? tabName = _settingsSource.GetString(_selectedTabSettingName, defaultValue: null);

        // The other tabs depend on the selected revision, on the plugins or on starting a shell and are not restored.
        // The "Commit" tab does not exist if the commit info is shown beside the revision grid.
        TabPage[] restorableTabPages = [CommitInfoTabPage, DiffTabPage, TreeTabPage];
        TabPage? tabPage = restorableTabPages.FirstOrDefault(page => page.Name == tabName);
        if (tabPage is null || !CommitInfoTabControl.TabPages.Contains(tabPage))
        {
            return;
        }

        // The tab is filled once the revisions are loaded; the event handler would move the focus away from the revision grid
        CommitInfoTabControl.SelectedIndexChanged -= CommitInfoTabControl_SelectedIndexChanged;
        CommitInfoTabControl.SelectedTab = tabPage;
        CommitInfoTabControl.SelectedIndexChanged += CommitInfoTabControl_SelectedIndexChanged;
    }

    private void SaveSelectedTab()
    {
        // Do not let the tab enforced by the file history and blame modes replace the one chosen by the user
        if (_isFileHistoryMode)
        {
            return;
        }

        _settingsSource.SetString(_selectedTabSettingName, CommitInfoTabControl.SelectedTab?.Name);
    }
}
