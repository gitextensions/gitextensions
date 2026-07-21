using Avalonia.Controls;
using GitCommands.Git;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigAdvancedSettingsPage : GitConfigBaseSettingsPage
{
    private record GitSettingUiMapping(string GitSettingKey, CheckBox MappedCheckbox);

    private readonly List<GitSettingUiMapping> _gitSettings;
    private bool _settingNamesAppended;

    public GitConfigAdvancedSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

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

        checkBoxUpdateRefs.IsVisible = GitVersion.Current.SupportUpdateRefs;
        Loaded += (_, _) => AppendSettingNames();
    }

    protected override void SettingsToPage()
    {
        if (CurrentSettings is null)
        {
            return;
        }

        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            gitSetting.MappedCheckbox.IsChecked = CurrentSettings.GetValue(gitSetting.GitSettingKey) switch
            {
                "true" or "yes" or "on" or "1" => true,
                "false" or "no" or "off" or "0" or "" => false,
                _ => null,
            };
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        if (CurrentSettings is null)
        {
            return;
        }

        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string? value = gitSetting.MappedCheckbox.IsChecked switch
            {
                true => "true",
                false => "false",
                _ => null,
            };
            CurrentSettings.SetValue(gitSetting.GitSettingKey, value);
        }

        base.PageToSettings();
    }

    private void AppendSettingNames()
    {
        if (_settingNamesAppended)
        {
            return;
        }

        _settingNamesAppended = true;
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            gitSetting.MappedCheckbox.Content = $"{gitSetting.MappedCheckbox.Content} [{gitSetting.GitSettingKey}]";
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GitConfigAdvancedSettingsPage page)
    {
        public IReadOnlyList<CheckBox> Settings => [.. page._gitSettings.Select(mapping => mapping.MappedCheckbox)];
    }
}
