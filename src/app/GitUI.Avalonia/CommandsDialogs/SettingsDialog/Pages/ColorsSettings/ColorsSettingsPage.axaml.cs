using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class ColorsSettingsPage : SettingsPageWithHeader, IColorsSettingsPage
{
    private readonly ColorsSettingsPageController _controller;
    private readonly List<FormattedThemeId> _themeIds = [];

    private static readonly TranslationString FormatBuiltinThemeName = new("{0}");
    private static readonly TranslationString FormatUserDefinedThemeName = new("{0}, user-defined");

    private MenuFlyout cmsOpenThemeFolders => (MenuFlyout)sbOpenThemeFolder.Flyout!;

    public ColorsSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ColorsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        _controller = new ColorsSettingsPageController(this, new ThemeRepository(), new ThemePathProvider());
        WireEvents();
        InitializeComplete();
    }

    public ThemeId SelectedThemeId
    {
        get => (_NO_TRANSLATE_cbSelectTheme.SelectedItem as FormattedThemeId)?.ThemeId
            ?? ThemeId.WindowsAppColorModeId;
        set
        {
            int index = _themeIds.FindIndex(item => item.ThemeId == value);
            if (index < 0)
            {
                FormattedThemeId formattedThemeId = new(value);
                string theme = formattedThemeId.ToString();
                if (!string.IsNullOrWhiteSpace(theme))
                {
                    WinFormsShims.IWin32Window? owner = TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;
                    MessageBoxes.ShowError(owner, $"Theme not found: {theme}");
                }

                index = 0;
            }

            _NO_TRANSLATE_cbSelectTheme.SelectedIndex = index;
        }
    }

    public string[] SelectedThemeVariations
    {
        get => chkColorblind.IsChecked == true ? [ThemeVariations.Colorblind] : ThemeVariations.None;
        set => chkColorblind.IsChecked = value.Contains(ThemeVariations.Colorblind);
    }

    public bool UseSystemVisualStyle
    {
        get => chkUseSystemVisualStyle.IsChecked == true;
        set => chkUseSystemVisualStyle.IsChecked = value;
    }

    public bool LabelRestartIsNeededVisible
    {
        get => _NO_TRANSLATE_restartNeededPanel.IsVisible;
        set => _NO_TRANSLATE_restartNeededPanel.IsVisible = value;
    }

    public bool IsChoosingVisualStyleEnabled
    {
        get => chkUseSystemVisualStyle.IsEnabled;
        set => chkUseSystemVisualStyle.IsEnabled = value;
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ColorsSettingsPage));

    public void ShowThemeLoadingErrorMessage(ThemeId themeId, string[] variations, Exception ex)
    {
        Trace.WriteLine($"Failed to load theme {themeId.Name}: {ex}");
        string variationsText = string.Concat(variations.Select(variation => "." + variation));
        string identifier = new FormattedThemeId(themeId).ToString();
        AppSettings.ThemeId = ThemeId.DefaultLight;
        WinFormsShims.IWin32Window? owner = TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;
        MessageBoxes.ShowError(owner, $"Failed to load theme {identifier}{variationsText}: {ex.Message}"
            + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
    }

    public void PopulateThemeMenu(IEnumerable<ThemeId> themeIds)
    {
        _themeIds.Clear();
        _themeIds.AddRange(themeIds.Select(themeId => new FormattedThemeId(themeId)));
        _NO_TRANSLATE_cbSelectTheme.ItemsSource = null;
        _NO_TRANSLATE_cbSelectTheme.ItemsSource = _themeIds;
    }

    protected override void SettingsToPage()
    {
        MulticolorBranches.IsChecked = AppSettings.MulticolorBranches;
        chkDrawAlternateBackColor.IsChecked = AppSettings.RevisionGraphDrawAlternateBackColor;
        DrawNonRelativesGray.IsChecked = AppSettings.RevisionGraphDrawNonRelativesGray;
        DrawNonRelativesTextGray.IsChecked = AppSettings.RevisionGraphDrawNonRelativesTextGray;
        chkHighlightAuthored.IsChecked = AppSettings.HighlightAuthoredRevisions;
        chkFillRefLabels.IsChecked = AppSettings.FillRefLabels;
        _controller.ShowThemeSettings();

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.MulticolorBranches = MulticolorBranches.IsChecked == true;
        AppSettings.RevisionGraphDrawAlternateBackColor = chkDrawAlternateBackColor.IsChecked == true;
        AppSettings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.IsChecked == true;
        AppSettings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.IsChecked == true;
        AppSettings.HighlightAuthoredRevisions = chkHighlightAuthored.IsChecked == true;
        AppSettings.FillRefLabels = chkFillRefLabels.IsChecked == true;
        _controller.ApplyThemeSettings();

        base.PageToSettings();
    }

    private void WireEvents()
    {
        _NO_TRANSLATE_cbSelectTheme.SelectionChanged += (_, _) => _controller.HandleSelectedThemeChanged();
        chkUseSystemVisualStyle.IsCheckedChanged += (_, _) => _controller.HandleUseSystemVisualStyleChanged();
        chkColorblind.IsCheckedChanged += (_, _) => _controller.HandleUseColorblindVariationChanged();
        sbOpenThemeFolder.Click += (_, _) => sbOpenThemeFolder.Flyout?.ShowAt(sbOpenThemeFolder);
        tsmiApplicationFolder.Click += (_, _) => _controller.ShowAppThemesDirectory();
        tsmiUserFolder.Click += (_, _) => _controller.ShowUserThemesDirectory();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ColorsSettingsPage page)
    {
        public CheckBox MulticolorBranches => page.MulticolorBranches;

        public CheckBox DrawAlternateBackColor => page.chkDrawAlternateBackColor;

        public CheckBox DrawNonRelativesGray => page.DrawNonRelativesGray;

        public CheckBox DrawNonRelativesTextGray => page.DrawNonRelativesTextGray;

        public CheckBox HighlightAuthored => page.chkHighlightAuthored;

        public CheckBox FillRefLabels => page.chkFillRefLabels;

        public ComboBox SelectTheme => page._NO_TRANSLATE_cbSelectTheme;

        public SplitButton OpenThemeFolder => page.sbOpenThemeFolder;

        public MenuFlyout OpenThemeFolders => page.cmsOpenThemeFolders;

        public Control RestartNeeded => page._NO_TRANSLATE_restartNeededPanel;

        public CheckBox Colorblind => page.chkColorblind;

        public CheckBox UseSystemVisualStyle => page.chkUseSystemVisualStyle;
    }

    private sealed record FormattedThemeId(ThemeId ThemeId)
    {
        public override string ToString()
            => string.Format(ThemeId.IsBuiltin ? FormatBuiltinThemeName.Text : FormatUserDefinedThemeName.Text, ThemeId.Name);
    }
}
