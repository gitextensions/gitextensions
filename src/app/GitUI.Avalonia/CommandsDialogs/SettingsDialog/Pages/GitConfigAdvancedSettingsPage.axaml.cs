using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using GitCommands;
using GitCommands.Git;
using Image = Avalonia.Controls.Image;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigAdvancedSettingsPage : GitConfigBaseSettingsPage
{
    private const string _gitConfigDocsBaseUrl = "https://git-scm.com/docs/git-config#Documentation/git-config.txt-";

    private const string _unsetText = "";

    private sealed record GitSettingUiMapping(string GitSettingKey, Label DescriptionLabel, string[] Values)
    {
        public ComboBox MappedComboBox { get; } = new();
    }

    private readonly string[] _boolValues = ["false", "true"];

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
            new("pull.ff", lblPullFF, [.. _boolValues, "only"]),
            new("pull.rebase", lblPullRebase, [.. _boolValues, "merges", "interactive"]),
            new("fetch.prune", lblFetchPrune, _boolValues),
            new("merge.autostash", lblMergeAutoStash, _boolValues),
            new("rebase.autostash", lblRebaseAutostash, _boolValues),
            new("rebase.autosquash", lblRebaseAutosquash, _boolValues),
            new("rebase.updaterefs", lblUpdateRefs, _boolValues),
            new("rerere.enabled", lblReReReEnabled, _boolValues),
            new("rerere.autoupdate", lblReReReAutoUpdate, _boolValues),
            new("status.showuntrackedfiles", lblStatusShowUntrackedFiles, ["no", "normal", "all"]),
        ];

        lblUpdateRefs.IsVisible = GitVersion.Current.SupportUpdateRefs;

        FillColumns();

        Loaded += GitConfigAdvancedSettingsPage_Load;
    }

    private void FillColumns()
    {
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string gitSettingId = gitSetting.GitSettingKey.Replace(".", "").ToLowerInvariant();
            bool visible = gitSetting.DescriptionLabel.IsVisible;
            int row = Grid.GetRow(gitSetting.DescriptionLabel);

            ComboBox comboBox = gitSetting.MappedComboBox;
            AutomationProperties.SetName(comboBox, gitSetting.DescriptionLabel.Content?.ToString());
            comboBox.IsEditable = true;
            comboBox.Width = 121;
            comboBox.Margin = new Thickness(3);
            comboBox.Name = "_NO_TRANSLATE_cbx" + gitSettingId;
            comboBox.Items.Add(_unsetText);
            foreach (string value in gitSetting.Values)
            {
                comboBox.Items.Add(value);
            }

            comboBox.IsVisible = visible;
            Grid.SetColumn(comboBox, 1);
            Grid.SetRow(comboBox, row);
            tlpnlMain.Children.Add(comboBox);

            string url = $"{_gitConfigDocsBaseUrl}{gitSettingId}";
            Image helpIcon = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = new Cursor(StandardCursorType.Hand),
                Source = Properties.Images.Information,
                Margin = new Thickness(3),
                Name = "pic" + gitSettingId,
                Width = 16,
                Height = 16,
                IsVisible = visible
            };
            helpIcon.PointerPressed += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(url);
            Grid.SetColumn(helpIcon, 2);
            Grid.SetRow(helpIcon, row);
            tlpnlMain.Children.Add(helpIcon);
        }
    }

    private void GitConfigAdvancedSettingsPage_Load(object? sender, EventArgs e)
    {
        if (_settingNamesAppended)
        {
            return;
        }

        _settingNamesAppended = true;
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            gitSetting.DescriptionLabel.Content += $" [{gitSetting.GitSettingKey}]";
        }
    }

    protected override void SettingsToPage()
    {
        if (CurrentSettings is null)
        {
            return;
        }

        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string value = CurrentSettings.GetValue(gitSetting.GitSettingKey) ?? _unsetText;
            gitSetting.MappedComboBox.Text = value;
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
            string value = gitSetting.MappedComboBox.Text ?? _unsetText;
            CurrentSettings.SetValue(gitSetting.GitSettingKey, value == _unsetText ? null : value);
        }

        base.PageToSettings();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GitConfigAdvancedSettingsPage page)
    {
        public IReadOnlyList<ComboBox> Settings => [.. page._gitSettings.Select(mapping => mapping.MappedComboBox)];
    }
}
