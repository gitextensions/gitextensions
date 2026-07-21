using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitCommands;
using GitCommands.Utils;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class SortingSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _revisionSortWarningTooltip = new("Sorting revisions may delay rendering of the revision graph.");
    private readonly TranslationString _prioBranchNamesTooltip = new("Regex to prioritize branch names in the left panel and commit info.\n" +
        "The branches matching the pattern will be shown before the others.\n" +
        "Separate the priorities with ';'.");
    private readonly TranslationString _prioRemoteNamesTooltip = new("Regex to prioritize remote names in the left panel and commit info.\n" +
        "The remotes matching the pattern will be shown before the others.\n" +
        "Separate the priorities with ';'.");

    public SortingSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public SortingSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        FillComboBoxWithEnumValues<RevisionSortOrder>(_NO_TRANSLATE_cmbRevisionsSortBy);
        FillComboBoxWithEnumValues<GitRefsSortOrder>(_NO_TRANSLATE_cmbBranchesOrder);
        FillComboBoxWithEnumValues<GitRefsSortBy>(_NO_TRANSLATE_cmbBranchesSortBy);
        WireEvents();
        ConfigureToolTips();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(SortingSettingsPage));

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
        AppSettings.PrioritizedBranchNames = txtPrioBranchNames.Text ?? string.Empty;
        AppSettings.PrioritizedRemoteNames = txtPrioRemoteNames.Text ?? string.Empty;

        ResourceManager.TranslatedStrings.Reinitialize();
        TranslatedStrings.Reinitialize();

        base.PageToSettings();
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        ConfigureToolTips();
    }

    private void ConfigureToolTips()
    {
        ToolTip.SetToolTip(RevisionSortOrderHelp, CreateToolTip(_revisionSortWarningTooltip.Text));
        ToolTip.SetToolTip(PrioBranchNamesHelp, CreateToolTip(_prioBranchNamesTooltip.Text));
        ToolTip.SetToolTip(PrioRemoteNamesHelp, CreateToolTip(_prioRemoteNamesTooltip.Text));
    }

    private static TextBlock CreateToolTip(string text)
        => new()
        {
            MaxWidth = 420,
            Text = text,
            TextWrapping = TextWrapping.Wrap,
        };

    private static void FillComboBoxWithEnumValues<T>(ComboBox comboBox) where T : struct, Enum
    {
        ComboBoxItem<T>[] items = EnumHelper.GetValues<T>()
            .Select(value => new ComboBoxItem<T>(value.GetDescription(), value))
            .ToArray();
        comboBox.ItemsSource = items;
        comboBox.ItemTemplate = new FuncDataTemplate<ComboBoxItem<T>>(
            (item, _) => new TextBlock { Text = item?.Text },
            supportsRecycling: true);
    }

    private void WireEvents()
    {
        RevisionSortOrderHelp.Tapped += RevisionSortOrderHelp_Click;
        PrioBranchNamesHelp.Tapped += PrioBranchNamesHelp_Click;
        PrioRemoteNamesHelp.Tapped += PrioRemoteNamesHelp_Click;
    }

    private void RevisionSortOrderHelp_Click(object? sender, EventArgs e)
        => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "sorting-sort-author-date"));

    private void PrioBranchNamesHelp_Click(object? sender, EventArgs e)
        => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "sorting-sort-prioritized-branches"));

    private void PrioRemoteNamesHelp_Click(object? sender, EventArgs e)
        => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "sorting-sort-prioritized-remotes"));

    internal TestAccessor GetTestAccessor() => new(this);

    private sealed record ComboBoxItem<T>(string Text, T Value);

    internal readonly struct TestAccessor(SortingSettingsPage page)
    {
        public ComboBox RevisionsSortBy => page._NO_TRANSLATE_cmbRevisionsSortBy;

        public ComboBox BranchesSortBy => page._NO_TRANSLATE_cmbBranchesSortBy;

        public ComboBox BranchesOrder => page._NO_TRANSLATE_cmbBranchesOrder;

        public TextBox PrioritizedBranchNames => page.txtPrioBranchNames;

        public TextBox PrioritizedRemoteNames => page.txtPrioRemoteNames;

        public Control RevisionSortOrderHelp => page.RevisionSortOrderHelp;

        public Control PrioBranchNamesHelp => page.PrioBranchNamesHelp;

        public Control PrioRemoteNamesHelp => page.PrioRemoteNamesHelp;

        public IReadOnlyList<string> RevisionSortItems
            => page._NO_TRANSLATE_cmbRevisionsSortBy.ItemsSource?
                .OfType<ComboBoxItem<RevisionSortOrder>>()
                .Select(item => item.Text)
                .ToArray() ?? [];

        public IReadOnlyList<string> BranchSortItems
            => page._NO_TRANSLATE_cmbBranchesSortBy.ItemsSource?
                .OfType<ComboBoxItem<GitRefsSortBy>>()
                .Select(item => item.Text)
                .ToArray() ?? [];

        public IReadOnlyList<string> BranchOrderItems
            => page._NO_TRANSLATE_cmbBranchesOrder.ItemsSource?
                .OfType<ComboBoxItem<GitRefsSortOrder>>()
                .Select(item => item.Text)
                .ToArray() ?? [];
    }
}
