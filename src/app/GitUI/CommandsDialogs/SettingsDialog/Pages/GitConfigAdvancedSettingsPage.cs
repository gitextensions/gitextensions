using GitCommands;
using GitCommands.Git;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigAdvancedSettingsPage : GitConfigBaseSettingsPage
{
    private const string _gitConfigDocsBaseUrl = "https://git-scm.com/docs/git-config#Documentation/git-config.txt-";

    private record GitSettingUiMapping(string GitSettingKey, CheckBox MappedCheckbox, string CheckedValue = "true", string UncheckedValue = "false");
    private readonly List<GitSettingUiMapping> _gitSettings;

    public GitConfigAdvancedSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        _gitSettings =
        [
            new("pull.ff", chkPullFfOnly, CheckedValue: "only"),
            new("pull.rebase", chkPullRebase),
            new("fetch.prune", chkFetchPrune),
            new("merge.autostash", chkMergeAutoStash),
            new("rebase.autostash", chkRebaseAutostash),
            new("rebase.autosquash", chkRebaseAutosquash),
            new("rebase.updaterefs", chkUpdateRefs),
            new("rerere.enabled", chkReReReEnabled),
            new("rerere.autoupdate", chkReReReAutoUpdate),
            new("status.showuntrackedfiles", chkStatusShowUntrackedFiles, UncheckedValue: "no"),
        ];

        chkUpdateRefs.Visible = GitVersion.Current.SupportUpdateRefs;

        AddHelpColumn();

        Load += GitConfigAdvancedSettingsPage_Load;
    }

    private void AddHelpColumn()
    {
        tlpnlMain.ColumnCount += 1;
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());

        Bitmap questionIcon = new Icon(SystemIcons.Question, SystemInformation.SmallIconSize).ToBitmap();
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string gitSettingId = gitSetting.GitSettingKey.Replace(".", "").ToLowerInvariant();
            string url = $"{_gitConfigDocsBaseUrl}{gitSettingId}";
            PictureBox helpIcon = new()
            {
                Anchor = AnchorStyles.Left,
                Cursor = Cursors.Hand,
                Image = Properties.Resources.information,
                Margin = new Padding(3, 3, 3, 3),
                Name = "hlp" + gitSettingId,
                Size = new Size(16, 16),
                Visible = gitSetting.MappedCheckbox.Visible
            };
            helpIcon.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(url);
            tlpnlMain.Controls.Add(helpIcon, column: 1, tlpnlMain.GetRow(gitSetting.MappedCheckbox));
        }
    }

    private void GitConfigAdvancedSettingsPage_Load(object? sender, EventArgs e)
    {
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            gitSetting.MappedCheckbox.Text += $" [{gitSetting.GitSettingKey}]";
        }
    }

    protected override void SettingsToPage()
    {
        Validates.NotNull(CurrentSettings);
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string? value = CurrentSettings.GetValue(gitSetting.GitSettingKey);
            gitSetting.MappedCheckbox.CheckState
                = value == gitSetting.CheckedValue
                    ? CheckState.Checked
                    : value == gitSetting.UncheckedValue
                        ? CheckState.Unchecked
                        : value switch
                            {
                                "true" or "yes" or "on" or "1" => CheckState.Checked,
                                "false" or "no" or "off" or "0" or "" => CheckState.Unchecked,
                                _ => CheckState.Indeterminate
                            };
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        Validates.NotNull(CurrentSettings);
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string? value = gitSetting.MappedCheckbox.CheckState switch { CheckState.Checked => gitSetting.CheckedValue, CheckState.Unchecked => gitSetting.UncheckedValue, _ => null };
            CurrentSettings.SetValue(gitSetting.GitSettingKey, value);
        }

        base.PageToSettings();
    }
}
