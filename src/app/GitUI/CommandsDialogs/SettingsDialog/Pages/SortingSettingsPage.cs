using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class SortingSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _revisionSortWarningTooltip = new("Sorting revisions may delay rendering of the revision graph.");
        private readonly TranslationString _prioBranchNamesTooltip = new("Regex to prioritize branch names in the left panel and commit info.\n" +
            "The branches matching the pattern will be shown before the others.\n" +
            "Separate the priorities with ';'.");
        private readonly TranslationString _prioRemoteNamesTooltip = new("Regex to prioritize remote names in the left panel and commit info.\n" +
            "The remotes matching the pattern will be shown before the others.\n" +
            "Separate the priorities with ';'.");

        public SortingSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();

            FillComboBoxWithEnumValues<RevisionSortOrder>(_NO_TRANSLATE_cmbRevisionsSortBy);
            FillComboBoxWithEnumValues<GitRefsSortOrder>(_NO_TRANSLATE_cmbBranchesOrder);
            FillComboBoxWithEnumValues<GitRefsSortBy>(_NO_TRANSLATE_cmbBranchesSortBy);
        }

        private static void FillComboBoxWithEnumValues<T>(ComboBox comboBox) where T : Enum
        {
            comboBox.DisplayMember = nameof(ComboBoxItem<T>.Text);
            comboBox.ValueMember = nameof(ComboBoxItem<T>.Value);
            comboBox.DataSource = EnumHelper.GetValues<T>()
                .Select(e => new ComboBoxItem<T>(e.GetDescription(), e))
                .ToArray();
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            ToolTip.SetToolTip(RevisionSortOrderHelp, _revisionSortWarningTooltip.Text);
            ToolTip.SetToolTip(PrioBranchNamesHelp, _prioBranchNamesTooltip.Text);
            ToolTip.SetToolTip(PrioRemoteNamesHelp, _prioRemoteNamesTooltip.Text);
            RevisionSortOrderHelp.Size = DpiUtil.Scale(RevisionSortOrderHelp.Size);
            PrioBranchNamesHelp.Size = DpiUtil.Scale(PrioBranchNamesHelp.Size);
            PrioRemoteNamesHelp.Size = DpiUtil.Scale(PrioRemoteNamesHelp.Size);

            if (!IsSettingsLoaded)
            {
                SettingsToPage();
            }
        }

        protected override void SettingsToPage()
        {
            _NO_TRANSLATE_cmbRevisionsSortBy.SelectedIndex = (int)AppSettings.RevisionSortOrder.Value;
            _NO_TRANSLATE_cmbBranchesOrder.SelectedIndex = (int)AppSettings.RefsSortOrder;
            _NO_TRANSLATE_cmbBranchesSortBy.SelectedIndex = (int)AppSettings.RefsSortBy;
            txtPrioBranchNames.Text = AppSettings.PrioritizedBranchNames;
            txtPrioRemoteNames.Text = AppSettings.PrioritizedRemoteNames;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.RevisionSortOrder.Value = (RevisionSortOrder)_NO_TRANSLATE_cmbRevisionsSortBy.SelectedIndex;
            AppSettings.RevisionSortOrder.Save();
            AppSettings.RefsSortOrder = (GitRefsSortOrder)_NO_TRANSLATE_cmbBranchesOrder.SelectedIndex;
            AppSettings.RefsSortBy = (GitRefsSortBy)_NO_TRANSLATE_cmbBranchesSortBy.SelectedIndex;
            AppSettings.PrioritizedBranchNames = txtPrioBranchNames.Text;
            AppSettings.PrioritizedRemoteNames = txtPrioRemoteNames.Text;

            ResourceManager.TranslatedStrings.Reinitialize();
            TranslatedStrings.Reinitialize();

            base.PageToSettings();
        }

        private void RevisionSortOrderHelp_Click(object sender, EventArgs e)
            => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "sorting-sort-author-date"));
        private void PrioBranchNamesHelp_Click(object sender, EventArgs e)
            => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "sorting-sort-prioritized-branches"));
        private void PrioRemoteNamesHelp_Click(object sender, EventArgs e)
            => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "sorting-sort-prioritized-remotes"));

        private class ComboBoxItem<T>
        {
            public string Text { get; }
            public T Value { get; }

            public ComboBoxItem(string text, T value)
            {
                Text = text;
                Value = value;
            }
        }
    }
}
