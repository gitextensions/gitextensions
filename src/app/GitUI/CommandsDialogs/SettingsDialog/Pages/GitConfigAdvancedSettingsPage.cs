using GitCommands;
using GitCommands.Git;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GitConfigAdvancedSettingsPage : GitConfigBaseSettingsPage
{
    private const string _gitConfigDocsBaseUrl = "https://git-scm.com/docs/git-config#Documentation/git-config.txt-";

    private const string _unsetText = "";

    private record GitSettingUiMapping(string GitSettingKey, Label DescriptionLabel, string[] Values)
    {
        public ComboBox MappedComboBox { get; } = new();
    }

    private readonly string[] _boolValues = ["false", "true"];

    private readonly List<GitSettingUiMapping> _gitSettings;

    public GitConfigAdvancedSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        _gitSettings =
        [
            new("pull.ff", lblPullFfOnly, [.. _boolValues, "only"]),
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

        lblUpdateRefs.Visible = GitVersion.Current.SupportUpdateRefs;

        FillColumns();

        Load += GitConfigAdvancedSettingsPage_Load;
    }

    private void FillColumns()
    {
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string gitSettingId = gitSetting.GitSettingKey.Replace(".", "").ToLowerInvariant();
            bool visible = gitSetting.DescriptionLabel.Visible;
            int row = tlpnlMain.GetRow(gitSetting.DescriptionLabel);

            ComboBox comboBox = gitSetting.MappedComboBox;
            comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox.Name = "_NO_TRANSLATE_cbx" + gitSettingId;
            comboBox.Items.Add(_unsetText);
            comboBox.Items.AddRange(gitSetting.Values);
            comboBox.Visible = visible;
            tlpnlMain.Controls.Add(comboBox, column: 1, row);

            string url = $"{_gitConfigDocsBaseUrl}{gitSettingId}";
            PictureBox helpIcon = new()
            {
                Anchor = AnchorStyles.Left,
                Cursor = Cursors.Hand,
                Image = Properties.Resources.information,
                Margin = new Padding(3),
                Name = "pic" + gitSettingId,
                Size = new Size(16, 16),
                Visible = visible
            };
            helpIcon.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(url);
            tlpnlMain.Controls.Add(helpIcon, column: 2, row);
        }
    }

    private void GitConfigAdvancedSettingsPage_Load(object? sender, EventArgs e)
    {
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            gitSetting.DescriptionLabel.Text += $" [{gitSetting.GitSettingKey}]";
        }
    }

    protected override void SettingsToPage()
    {
        Validates.NotNull(CurrentSettings);
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string value = CurrentSettings.GetValue(gitSetting.GitSettingKey) ?? _unsetText;
            gitSetting.MappedComboBox.Text = value;
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        Validates.NotNull(CurrentSettings);
        foreach (GitSettingUiMapping gitSetting in _gitSettings)
        {
            string value = gitSetting.MappedComboBox.Text;
            CurrentSettings.SetValue(gitSetting.GitSettingKey, value == _unsetText ? null : value);
        }

        base.PageToSettings();
    }
}
