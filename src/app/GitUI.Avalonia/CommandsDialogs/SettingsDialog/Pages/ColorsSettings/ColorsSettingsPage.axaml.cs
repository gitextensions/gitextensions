using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ColorsSettingsPage : SettingsPageWithHeader, IColorsSettingsPage
{
    private readonly ColorsSettingsPageController _controller;

    private static readonly TranslationString FormatBuiltinThemeName =
        new("{0}");

    private static readonly TranslationString FormatUserDefinedThemeName =
        new("{0}, user-defined");

    private MenuFlyout cmsOpenThemeFolders => (MenuFlyout)sbOpenThemeFolder.Flyout!;

    public ColorsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();

        _NO_TRANSLATE_cbSelectTheme.SelectionChanged += ComboBoxTheme_SelectedIndexChanged;
        chkUseSystemVisualStyle.IsCheckedChanged += ChkUseSystemVisualStyle_CheckedChanged;
        chkColorblind.IsCheckedChanged += ChkColorblind_CheckedChanged;
        sbOpenThemeFolder.Click += SbOpenThemeFolder_Click;
        tsmiApplicationFolder.Click += tsmiApplicationFolder_Click;
        tsmiUserFolder.Click += tsmiUserFolder_Click;
        _controller = new ColorsSettingsPageController(this, new ThemeRepository(), new ThemePathProvider());
        InitializeComplete();
    }

    public ColorsSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ThemeId SelectedThemeId
    {
        get
        {
            return ((FormattedThemeId)_NO_TRANSLATE_cbSelectTheme.SelectedItem!).ThemeId;
        }
        set
        {
            FormattedThemeId formattedThemeId = new(value);
            int index = _NO_TRANSLATE_cbSelectTheme.Items.IndexOf(formattedThemeId);
            if (index < 0)
            {
                // Handle case when selected theme is missing gracefully.
                // It may happen in a following scenario:
                // - user creates custom theme and selects it in this settings page
                // - user saves app settings
                // - user deletes the file with custom theme
                // on first install; suppress MessageBox
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
        get => chkColorblind.IsChecked == true
            ? [ThemeVariations.Colorblind]
            : ThemeVariations.None;

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

    public void ShowThemeLoadingErrorMessage(ThemeId themeId, string[] variations, Exception ex)
    {
        Trace.WriteLine($"Failed to load theme {themeId.Name}: {ex}");
        string variationsStr = string.Concat(variations.Select(_ => "." + _));
        string identifier = new FormattedThemeId(themeId).ToString();
        AppSettings.ThemeId = ThemeId.DefaultLight;
        WinFormsShims.IWin32Window? owner = TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;
        MessageBoxes.ShowError(owner, $"Failed to load theme {identifier}{variationsStr}: {ex.Message}"
            + $"{Environment.NewLine}{Environment.NewLine}See also https://github.com/gitextensions/gitextensions/wiki/Dark-Mode");
    }

    public override void OnPageShown()
    {
        base.OnPageShown();

        // Avalonia settings pages are shown inside a header control rather than receiving the
        // WinForms OnRuntimeLoad callback.
        if (!IsSettingsLoaded)
        {
            LoadSettings();
        }
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

    public void PopulateThemeMenu(IEnumerable<ThemeId> themeIds)
    {
        _NO_TRANSLATE_cbSelectTheme.ItemsSource = themeIds
            .Select(id => new FormattedThemeId(id))
            .ToArray();
    }

    private void ComboBoxTheme_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Avalonia reports the transient empty selection while ItemsSource is replaced.
        if (_NO_TRANSLATE_cbSelectTheme.SelectedItem is not null)
        {
            _controller.HandleSelectedThemeChanged();
        }
    }

    private void ChkUseSystemVisualStyle_CheckedChanged(object? sender, EventArgs e) =>
        _controller.HandleUseSystemVisualStyleChanged();

    private void ChkColorblind_CheckedChanged(object? sender, EventArgs e) =>
        _controller.HandleUseColorblindVariationChanged();

    private void tsmiApplicationFolder_Click(object? sender, EventArgs e)
        => _controller.ShowAppThemesDirectory();

    private void tsmiUserFolder_Click(object? sender, EventArgs e) =>
        _controller.ShowUserThemesDirectory();

    private void SbOpenThemeFolder_Click(object? sender, EventArgs e) =>
        sbOpenThemeFolder.ShowDropDown();

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ColorsSettingsPage));

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

    private readonly struct FormattedThemeId
    {
        public FormattedThemeId(ThemeId themeId)
        {
            ThemeId = themeId;
        }

        public ThemeId ThemeId { get; }

        public override bool Equals(object? obj) =>
            obj is FormattedThemeId other && Equals(other);

        public override readonly int GetHashCode() =>
            ThemeId.GetHashCode();

        public static bool operator ==(FormattedThemeId left, FormattedThemeId right) =>
            left.Equals(right);

        public static bool operator !=(FormattedThemeId left, FormattedThemeId right) =>
            !left.Equals(right);

        public override readonly string ToString()
        {
            if (ThemeId.IsBuiltin)
            {
                return string.Format(FormatBuiltinThemeName.Text, ThemeId.Name);
            }

            return string.Format(FormatUserDefinedThemeName.Text, ThemeId.Name);
        }

        private readonly bool Equals(FormattedThemeId other) =>
            ThemeId.Equals(other.ThemeId);
    }
}
