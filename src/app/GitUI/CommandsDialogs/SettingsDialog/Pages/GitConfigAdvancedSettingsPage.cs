using GitCommands.Git;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigAdvancedSettingsPage : GitConfigBaseSettingsPage
{
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

        Load += GitConfigAdvancedSettingsPage_Load;
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
