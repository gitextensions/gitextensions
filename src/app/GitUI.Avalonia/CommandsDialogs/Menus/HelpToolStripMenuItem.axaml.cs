using Avalonia.Controls;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.Menus;

internal partial class HelpToolStripMenuItem : ToolStripMenuItemEx
{
    public HelpToolStripMenuItem()
    {
        InitializeComponent();

        ((Image)translateToolStripMenuItem.Icon!).Source = Images.Translate.AdaptLightness();
        SubmenuOpened += this_DropDownOpening;
        userManualToolStripMenuItem.Click += UserManualToolStripMenuItemClick;
        changelogToolStripMenuItem.Click += ChangelogToolStripMenuItemClick;
        translateToolStripMenuItem.Click += TranslateToolStripMenuItemClick;
        donateToolStripMenuItem.Click += DonateToolStripMenuItemClick;
        tsmiTelemetryEnabled.Click += TsmiTelemetryEnabled_Click;
        reportAnIssueToolStripMenuItem.Click += reportAnIssueToolStripMenuItem_Click;
        checkForUpdatesToolStripMenuItem.Click += checkForUpdatesToolStripMenuItem_Click;
        aboutToolStripMenuItem.Click += AboutToolStripMenuItemClick;
        InputAccessibility.Apply(this);
    }

    private void this_DropDownOpening(object? sender, EventArgs e)
    {
        tsmiTelemetryEnabled.IsChecked = AppSettings.TelemetryEnabled ?? false;
    }

    private void AboutToolStripMenuItemClick(object? sender, EventArgs e)
    {
        using FormAbout frm = new();
        frm.ShowDialog(OwnerForm);
    }

    private void ChangelogToolStripMenuItemClick(object? sender, EventArgs e)
    {
        using FormChangeLog frm = new();
        frm.ShowDialog(OwnerForm);
    }

    private void DonateToolStripMenuItemClick(object? sender, EventArgs e)
    {
        using FormDonate frm = new();
        frm.ShowDialog(OwnerForm);
    }

    private void checkForUpdatesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        (UICommands.GetService(typeof(IUpdateCheckService)) as IUpdateCheckService)
            ?.SearchForUpdatesAndShow(OwnerForm!, alwaysShow: true);
    }

    private void reportAnIssueToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        UserEnvironmentInformation.CopyInformation();
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/issues");
    }

    private void TranslateToolStripMenuItemClick(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
    }

    private void TsmiTelemetryEnabled_Click(object? sender, EventArgs e)
    {
        UICommands.StartGeneralSettingsDialog(OwnerForm);
    }

    private void UserManualToolStripMenuItemClick(object? sender, EventArgs e)
    {
        // Point to the default documentation, will work also if the old doc version is removed
        OsShellUtil.OpenUrlInDefaultBrowser(AppSettings.DocumentationBaseUrl);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(HelpToolStripMenuItem menu)
    {
        public MenuItem UserManualMenuItem => menu.userManualToolStripMenuItem;
        public MenuItem TranslateMenuItem => menu.translateToolStripMenuItem;
        public MenuItem TelemetryMenuItem => menu.tsmiTelemetryEnabled;
        public MenuItem ReportIssueMenuItem => menu.reportAnIssueToolStripMenuItem;
        public MenuItem CheckUpdatesMenuItem => menu.checkForUpdatesToolStripMenuItem;
    }
}
