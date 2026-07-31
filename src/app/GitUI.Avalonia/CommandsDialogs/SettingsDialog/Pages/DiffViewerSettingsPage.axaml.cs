using System.Reflection;
using Avalonia.Controls;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class DiffViewerSettingsPage : SettingsPageWithHeader
{
    private const string DiffAppearanceToolTip = "Diff appearance: patch (default), Git word-diff or Difftastic.";
    private const string ShowAllDiffToolsToolTip = "Show all configured difftools in a dropdown.\nThe primary difftool can still be selected by clicking the main menu entry.";
    private const string GitColoringToolTip = "Use Git coloring engine to show moved code etc.\n";
    private const string ReverseGitColoringToolTip = "Color the background at changes (invert colors).";

    private readonly TranslationString _saveCurrentViewSettingsAsDefaultTooltip = new("""
        Saves all current view settings as the default for future sessions.
        Note: The checkboxes 'Remember the "xyz" preference' only affect the running instance
        as long as the default has not been saved. These preference values are held in memory
        and must be explicitly saved to become persistent defaults.
        """);

    public DiffViewerSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public DiffViewerSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        btnSaveCurrentViewSettingsAsDefault.Click += btnSaveCurrentViewSettingsAsDefault_Click;
        chkUseGitColoring.IsCheckedChanged += chkUseGitColoring_CheckedChanged;
        ConfigureTranslatedText();
        ConfigureToolTips();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(DiffViewerSettingsPage));

    protected override void SettingsToPage()
    {
        chkRememberIgnoreWhiteSpacePreference.IsChecked = AppSettings.RememberIgnoreWhiteSpacePreference;
        chkOmitUninterestingDiff.IsChecked = AppSettings.OmitUninterestingDiff;
        chkRememberShowEntireFilePreference.IsChecked = AppSettings.RememberShowEntireFilePreference;
        chkRememberDiffAppearancePreference.IsChecked = AppSettings.RememberDiffDisplayAppearance.Value;
        chkRememberShowNonPrintingCharsPreference.IsChecked = AppSettings.RememberShowNonPrintingCharsPreference;
        chkRememberNumberOfContextLines.IsChecked = AppSettings.RememberNumberOfContextLines;
        chkRememberShowSyntaxHighlightingInDiff.IsChecked = AppSettings.RememberShowSyntaxHighlightingInDiff;
        chkOpenSubmoduleDiffInSeparateWindow.IsChecked = AppSettings.OpenSubmoduleDiffInSeparateWindow;
        chkContScrollToNextFileOnlyWithAlt.IsChecked = AppSettings.AutomaticContinuousScroll;
        chkShowDiffForAllParents.IsChecked = AppSettings.ShowDiffForAllParents;
        chkShowAllCustomDiffTools.IsChecked = AppSettings.ShowAvailableDiffTools;
        VerticalRulerPosition.Value = AppSettings.DiffVerticalRulerPosition;
        chkUseGitColoring.IsChecked = AppSettings.UseGitColoring.Value;
        chkUseGEThemeGitColoring.IsChecked = AppSettings.ReverseGitColoring.Value;
        chkUseGEThemeGitColoring.IsEnabled = chkUseGitColoring.IsChecked == true;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.RememberIgnoreWhiteSpacePreference = chkRememberIgnoreWhiteSpacePreference.IsChecked == true;
        AppSettings.OmitUninterestingDiff = chkOmitUninterestingDiff.IsChecked == true;
        AppSettings.RememberShowEntireFilePreference = chkRememberShowEntireFilePreference.IsChecked == true;
        AppSettings.RememberDiffDisplayAppearance.Value = chkRememberDiffAppearancePreference.IsChecked == true;
        AppSettings.RememberShowNonPrintingCharsPreference = chkRememberShowNonPrintingCharsPreference.IsChecked == true;
        AppSettings.RememberNumberOfContextLines = chkRememberNumberOfContextLines.IsChecked == true;
        AppSettings.RememberShowSyntaxHighlightingInDiff = chkRememberShowSyntaxHighlightingInDiff.IsChecked == true;
        AppSettings.OpenSubmoduleDiffInSeparateWindow = chkOpenSubmoduleDiffInSeparateWindow.IsChecked == true;
        AppSettings.AutomaticContinuousScroll = chkContScrollToNextFileOnlyWithAlt.IsChecked == true;
        AppSettings.ShowDiffForAllParents = chkShowDiffForAllParents.IsChecked == true;
        AppSettings.ShowAvailableDiffTools = chkShowAllCustomDiffTools.IsChecked == true;
        AppSettings.DiffVerticalRulerPosition = Convert.ToInt32(VerticalRulerPosition.Value);
        AppSettings.UseGitColoring.Value = chkUseGitColoring.IsChecked == true;
        AppSettings.ReverseGitColoring.Value = chkUseGEThemeGitColoring.IsChecked == true;

        base.PageToSettings();
    }

    public override void AddTranslationItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddToolTip(nameof(chkRememberDiffAppearancePreference), DiffAppearanceToolTip);
        AddToolTip(nameof(chkShowAllCustomDiffTools), ShowAllDiffToolsToolTip);
        AddToolTip(nameof(chkUseGitColoring), GitColoringToolTip);
        AddToolTip(nameof(chkUseGEThemeGitColoring), ReverseGitColoringToolTip);

        void AddToolTip(string name, string source)
            => translation.AddTranslationItem(nameof(DiffViewerSettingsPage), name, "ToolTipText", source);
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);
        ConfigureTranslatedText();
        ConfigureToolTips(translation);
    }

    private void chkUseGitColoring_CheckedChanged(object? sender, EventArgs e)
        => chkUseGEThemeGitColoring.IsEnabled = chkUseGitColoring.IsChecked == true;

    private static void btnSaveCurrentViewSettingsAsDefault_Click(object? sender, EventArgs e)
    {
        foreach (FieldInfo staticAppSettingField in typeof(AppSettings).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (staticAppSettingField.GetValue(null) is IRuntimeSetting runtimeSetting)
            {
                runtimeSetting.Save();
            }
        }

        AppSettings.SaveSettings();
    }

    private void ConfigureTranslatedText()
    {
        chkShowDiffForAllParents.Content = TranslatedStrings.ShowDiffForAllParentsText;
        chkContScrollToNextFileOnlyWithAlt.Content = TranslatedStrings.ContScrollToNextFileOnlyWithAlt;
    }

    private void ConfigureToolTips(GitExtensions.Extensibility.Translations.ITranslation? translation = null)
    {
        ToolTip.SetToolTip(btnSaveCurrentViewSettingsAsDefault, _saveCurrentViewSettingsAsDefaultTooltip.Text);
        SetToolTip(chkRememberDiffAppearancePreference, nameof(chkRememberDiffAppearancePreference), DiffAppearanceToolTip);
        SetToolTip(chkShowDiffForAllParents, nameof(chkShowDiffForAllParents), TranslatedStrings.ShowDiffForAllParentsTooltip);
        SetToolTip(chkShowAllCustomDiffTools, nameof(chkShowAllCustomDiffTools), ShowAllDiffToolsToolTip);
        SetToolTip(chkUseGitColoring, nameof(chkUseGitColoring), GitColoringToolTip);
        SetToolTip(chkUseGEThemeGitColoring, nameof(chkUseGEThemeGitColoring), ReverseGitColoringToolTip);

        void SetToolTip(Control control, string name, string source)
        {
            string text = translation?.TranslateItem(
                nameof(DiffViewerSettingsPage),
                name,
                "ToolTipText",
                () => source) ?? source;
            ToolTip.SetToolTip(control, text);
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(DiffViewerSettingsPage page)
    {
        public CheckBox AutomaticContinuousScroll => page.chkContScrollToNextFileOnlyWithAlt;

        public CheckBox RememberIgnoreWhitespace => page.chkRememberIgnoreWhiteSpacePreference;

        public CheckBox RememberNonPrinting => page.chkRememberShowNonPrintingCharsPreference;

        public CheckBox RememberEntireFile => page.chkRememberShowEntireFilePreference;

        public CheckBox RememberDiffAppearance => page.chkRememberDiffAppearancePreference;

        public CheckBox RememberContextLines => page.chkRememberNumberOfContextLines;

        public CheckBox RememberSyntaxHighlighting => page.chkRememberShowSyntaxHighlightingInDiff;

        public CheckBox OmitUninterestingDiff => page.chkOmitUninterestingDiff;

        public CheckBox OpenSubmoduleSeparately => page.chkOpenSubmoduleDiffInSeparateWindow;

        public CheckBox ShowAllParents => page.chkShowDiffForAllParents;

        public CheckBox ShowAllDiffTools => page.chkShowAllCustomDiffTools;

        public NumericUpDown VerticalRulerPosition => page.VerticalRulerPosition;

        public CheckBox UseGitColoring => page.chkUseGitColoring;

        public CheckBox ReverseGitColoring => page.chkUseGEThemeGitColoring;

        public Button SaveCurrentViewSettingsAsDefault => page.btnSaveCurrentViewSettingsAsDefault;
    }
}
