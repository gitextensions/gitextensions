#nullable enable

using GitCommands.Git;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigAdvancedSettingsPage : GitConfigBaseSettingsPage
{
    private record GitSettingUiMapping(string GitSettingKey, CheckBox MappedCheckbox);
    private readonly List<GitSettingUiMapping> _gitSettings;

    public GitConfigAdvancedSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        _gitSettings =
        [
            new("pull.rebase", checkBoxPullRebase),
            new("fetch.prune", checkBoxFetchPrune),
            new("merge.autostash", checkboxMergeAutoStash),
            new("rebase.autostash", checkBoxRebaseAutostash),
            new("rebase.autosquash", checkBoxRebaseAutosquash),
            new("rebase.updaterefs", checkBoxUpdateRefs),
            new("rerere.enabled", checkBoxReReReEnabled),
            new("rerere.autoupdate", checkBoxReReReAutoUpdate),
        ];

        checkBoxUpdateRefs.Visible = GitVersion.Current.SupportUpdateRefs;

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
            gitSetting.MappedCheckbox.CheckState = CurrentSettings.GetValue(gitSetting.GitSettingKey) switch
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
            CurrentSettings.SetValue(gitSetting.GitSettingKey, gitSetting.MappedCheckbox.CheckState switch { CheckState.Checked => "true", CheckState.Unchecked => "false", _ => null });
        }

        base.PageToSettings();
    }
}
