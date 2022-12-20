using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ColorsSettingsPage : SettingsPageWithHeader, IColorsSettingsPage
    {
        private readonly ColorsSettingsPageController _controller;

        private static readonly TranslationString FormatBuiltinThemeName =
            new("{0}");

        private static readonly TranslationString FormatUserDefinedThemeName =
            new("{0}, user-defined");

        private static readonly TranslationString DefaultThemeName =
            new("default");

        public ColorsSettingsPage()
        {
            InitializeComponent();
            Text = "Colors";
            sbOpenThemeFolder.AutoSize = false;

            _NO_TRANSLATE_cbSelectTheme.SelectedIndexChanged += ComboBoxTheme_SelectedIndexChanged;
            chkUseSystemVisualStyle.CheckedChanged += ChkUseSystemVisualStyle_CheckedChanged;
            chkColorblind.CheckedChanged += ChkColorblind_CheckedChanged;
            _controller = new ColorsSettingsPageController(this, new ThemeRepository(), new ThemePathProvider());
            InitializeComplete();
        }

        public ThemeId SelectedThemeId
        {
            get
            {
                return ((FormattedThemeId)_NO_TRANSLATE_cbSelectTheme.SelectedItem).ThemeId;
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
                        MessageBoxes.ShowError(FindForm(), $"Theme not found: {theme}");
                    }

                    index = 0;
                }

                _NO_TRANSLATE_cbSelectTheme.SelectedIndex = index;
            }
        }

        public string[] SelectedThemeVariations
        {
            get => chkColorblind.Checked
                ? new[] { ThemeVariations.Colorblind }
                : ThemeVariations.None;

            set => chkColorblind.Checked = value.Contains(ThemeVariations.Colorblind);
        }

        public bool UseSystemVisualStyle
        {
            get => chkUseSystemVisualStyle.Checked;
            set => chkUseSystemVisualStyle.Checked = value;
        }

        public bool LabelRestartIsNeededVisible
        {
            get => lblRestartNeeded.Visible;
            set => lblRestartNeeded.Visible = value;
        }

        public bool IsChoosingVisualStyleEnabled
        {
            get => chkUseSystemVisualStyle.Enabled;
            set => chkUseSystemVisualStyle.Enabled = value;
        }

        public void ShowThemeLoadingErrorMessage(ThemeId themeId, string[] variations, Exception ex)
        {
            string variationsStr = string.Concat(variations.Select(_ => "." + _));
            string identifier = new FormattedThemeId(themeId).ToString();
            MessageBoxes.ShowError(this, $"Failed to load theme {identifier}{variationsStr}: {ex}");
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            if (!IsSettingsLoaded)
            {
                SettingsToPage();
            }
        }

        protected override void SettingsToPage()
        {
            MulticolorBranches.Checked = AppSettings.MulticolorBranches;
            chkDrawAlternateBackColor.Checked = AppSettings.RevisionGraphDrawAlternateBackColor;
            DrawNonRelativesGray.Checked = AppSettings.RevisionGraphDrawNonRelativesGray;
            DrawNonRelativesTextGray.Checked = AppSettings.RevisionGraphDrawNonRelativesTextGray;
            chkHighlightAuthored.Checked = AppSettings.HighlightAuthoredRevisions;
            chkFillRefLabels.Checked = AppSettings.FillRefLabels;
            _controller.ShowThemeSettings();

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.MulticolorBranches = MulticolorBranches.Checked;
            AppSettings.RevisionGraphDrawAlternateBackColor = chkDrawAlternateBackColor.Checked;
            AppSettings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            AppSettings.HighlightAuthoredRevisions = chkHighlightAuthored.Checked;
            AppSettings.FillRefLabels = chkFillRefLabels.Checked;
            _controller.ApplyThemeSettings();

            base.PageToSettings();
        }

        public void PopulateThemeMenu(IEnumerable<ThemeId> themeIds)
        {
            _NO_TRANSLATE_cbSelectTheme.Items.Clear();
            var formattedThemeIds = themeIds
                .Select(id => new FormattedThemeId(id))
                .Cast<object>()
                .ToArray();
            _NO_TRANSLATE_cbSelectTheme.Items.AddRange(formattedThemeIds);
        }

        private void ComboBoxTheme_SelectedIndexChanged(object sender, EventArgs e) =>
            _controller.HandleSelectedThemeChanged();

        private void ChkUseSystemVisualStyle_CheckedChanged(object sender, EventArgs e) =>
            _controller.HandleUseSystemVisualStyleChanged();

        private void ChkColorblind_CheckedChanged(object sender, EventArgs e) =>
            _controller.HandleUseColorblindVariationChanged();

        private void tsmiApplicationFolder_Click(object sender, EventArgs e)
            => _controller.ShowAppThemesDirectory();

        private void tsmiUserFolder_Click(object sender, EventArgs e) =>
            _controller.ShowUserThemesDirectory();

        private struct FormattedThemeId
        {
            public FormattedThemeId(ThemeId themeId)
            {
                ThemeId = themeId;
            }

            public ThemeId ThemeId { get; }

            public override bool Equals(object obj) =>
                obj is FormattedThemeId other && Equals(other);

            public override int GetHashCode() =>
                ThemeId.GetHashCode();

            public static bool operator ==(FormattedThemeId left, FormattedThemeId right) =>
                left.Equals(right);

            public static bool operator !=(FormattedThemeId left, FormattedThemeId right) =>
                !left.Equals(right);

            public override string ToString()
            {
                if (ThemeId == ThemeId.Default)
                {
                    return DefaultThemeName.Text;
                }

                if (ThemeId.IsBuiltin)
                {
                    return string.Format(FormatBuiltinThemeName.Text, ThemeId.Name);
                }

                return string.Format(FormatUserDefinedThemeName.Text, ThemeId.Name);
            }

            private bool Equals(FormattedThemeId other) =>
                ThemeId.Equals(other.ThemeId);
        }
    }
}
