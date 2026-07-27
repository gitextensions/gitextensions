using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AdvancedSettingsPage : SettingsPageWithHeader
{
    private readonly AutoNormaliseSymbolItem[] _autoNormaliseSymbols =
    [
        new("_", "_"),
        new("-", "-"),
        new("(none)", ""),
    ];

    public AdvancedSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public AdvancedSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();

        cboAutoNormaliseSymbol.ItemsSource = _autoNormaliseSymbols;
        cboAutoNormaliseSymbol.SelectedIndex = 0;
    }

    protected override void SettingsToPage()
    {
        chkAlwaysShowCheckoutDlg.IsChecked = AppSettings.AlwaysShowCheckoutBranchDlg;
        chkUseLocalChangesAction.IsChecked = AppSettings.UseDefaultCheckoutBranchAction;
        chkDontSHowHelpImages.IsChecked = AppSettings.DontShowHelpImages;
        chkAlwaysShowAdvOpt.IsChecked = AppSettings.AlwaysShowAdvOpt;
        chkCheckForUpdates.IsChecked = AppSettings.CheckForUpdates;
        chkCheckForRCVersions.IsChecked = AppSettings.CheckForReleaseCandidates;
        chkConsoleEmulator.IsChecked = AppSettings.UseConsoleEmulatorForCommands.Value;
        chkAutoNormaliseBranchName.IsChecked = AppSettings.AutoNormaliseBranchName;
        cboAutoNormaliseSymbol.IsEnabled = chkAutoNormaliseBranchName.IsChecked == true;
        cboAutoNormaliseSymbol.SelectedItem = _autoNormaliseSymbols
            .FirstOrDefault(item => item.Value == AppSettings.AutoNormaliseSymbol)
            ?? _autoNormaliseSymbols[0];
        chkCommitAndPushForcedWhenAmend.IsChecked = AppSettings.CommitAndPushForcedWhenAmend;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.IsChecked == true;
        AppSettings.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.IsChecked == true;
        AppSettings.DontShowHelpImages = chkDontSHowHelpImages.IsChecked == true;
        AppSettings.AlwaysShowAdvOpt = chkAlwaysShowAdvOpt.IsChecked == true;
        AppSettings.CheckForUpdates = chkCheckForUpdates.IsChecked == true;
        AppSettings.CheckForReleaseCandidates = chkCheckForRCVersions.IsChecked == true;
        AppSettings.UseConsoleEmulatorForCommands.Value = chkConsoleEmulator.IsChecked == true;
        AppSettings.AutoNormaliseBranchName = chkAutoNormaliseBranchName.IsChecked == true;
        AppSettings.AutoNormaliseSymbol =
            (cboAutoNormaliseSymbol.SelectedItem as AutoNormaliseSymbolItem)?.Value ?? "_";
        AppSettings.CommitAndPushForcedWhenAmend = chkCommitAndPushForcedWhenAmend.IsChecked == true;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));

    private void WireEvents()
    {
        chkAutoNormaliseBranchName.IsCheckedChanged += (_, _) =>
            cboAutoNormaliseSymbol.IsEnabled = chkAutoNormaliseBranchName.IsChecked == true;
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(AdvancedSettingsPage),
            "$this",
            "Text",
            Text ?? "Advanced");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string neutralText = Text ?? "Advanced";
        Text = translation.TranslateItem(
            nameof(AdvancedSettingsPage),
            "$this",
            "Text",
            () => neutralText) ?? neutralText;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(AdvancedSettingsPage page)
    {
        public CheckBox AlwaysShowCheckoutDialog => page.chkAlwaysShowCheckoutDlg;

        public CheckBox UseLocalChangesAction => page.chkUseLocalChangesAction;

        public CheckBox DontShowHelpImages => page.chkDontSHowHelpImages;

        public CheckBox AlwaysShowAdvancedOptions => page.chkAlwaysShowAdvOpt;

        public CheckBox CheckForUpdates => page.chkCheckForUpdates;

        public CheckBox CheckForReleaseCandidates => page.chkCheckForRCVersions;

        public CheckBox ConsoleEmulator => page.chkConsoleEmulator;

        public Control ConsoleEmulatorRow => page._NO_TRANSLATE_ConsoleEmulatorRow;

        public CheckBox AutoNormaliseBranchName => page.chkAutoNormaliseBranchName;

        public ComboBox AutoNormaliseSymbol => page.cboAutoNormaliseSymbol;

        public CheckBox CommitAndPushForcedWhenAmend => page.chkCommitAndPushForcedWhenAmend;

        public Control UpdatesGroup => page.grpUpdates;
    }

    private sealed record AutoNormaliseSymbolItem(string DisplayName, string Value)
    {
        public override string ToString() => DisplayName;
    }
}
