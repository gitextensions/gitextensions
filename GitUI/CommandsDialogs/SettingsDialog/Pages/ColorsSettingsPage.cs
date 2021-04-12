using System;
using System.Linq;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ColorsSettingsPage : SettingsPageWithHeader
    {
        private int _updateThemeSettingsCounter;
        private readonly ThemeRepository _themeRepository = new();
        private readonly ThemePathProvider _themePathProvider = new();

        private static readonly TranslationString FormatBuiltinThemeName =
            new TranslationString("{0}");

        private static readonly TranslationString FormatUserDefinedThemeName =
            new TranslationString("{0}, user-defined");

        private static readonly TranslationString DefaultThemeName =
            new TranslationString("default");

        public ColorsSettingsPage()
        {
            InitializeComponent();
            Text = "Colors";
            sbOpenThemeFolder.AutoSize = false;

            _NO_TRANSLATE_cbSelectTheme.SelectedIndexChanged += ComboBoxTheme_SelectedIndexChanged;
            chkUseSystemVisualStyle.CheckedChanged += ChkUseSystemVisualStyle_CheckedChanged;
            chkColorblind.CheckedChanged += ChkColorblind_CheckedChanged;

            InitializeComplete();
        }

        private ThemeId SelectedThemeId
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
                    // - on first install; suppress MessageBox
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

        private string[] SelectedThemeVariations
        {
            get => chkColorblind.Checked
                ? new[] { ThemeVariations.Colorblind }
                : GitExtUtils.GitUI.Theming.ThemeVariations.None;

            set => chkColorblind.Checked = value.Contains(ThemeVariations.Colorblind);
        }

        public bool UseSystemVisualStyle
        {
            get => chkUseSystemVisualStyle.Checked;
            set => chkUseSystemVisualStyle.Checked = value;
        }

        private bool SettingsAreModified
        {
            get
            {
                if (SelectedThemeId != ThemeModule.Settings.Theme.Id)
                {
                    return true;
                }

                if (SelectedThemeId == ThemeId.Default)
                {
                    // UseSystemVisualStyle and ThemeVariations settings are only applicable with non-default theme.
                    // However, in order to preserve user preference, we do not reset these when
                    // user chooses the default theme from the menu, we only disable the checkboxes.

                    // This is why, when the default theme is selected, we should ignore any difference in
                    // UseSystemVisualStyle or ThemeVariations checkboxes from the actual theme settings.
                    // Their value is not applied and only kept to be applied when user chooses non-default theme
                    // again.
                    return false;
                }

                return UseSystemVisualStyle != ThemeModule.Settings.UseSystemVisualStyle ||
                    !SelectedThemeVariations.SequenceEqual(AppSettings.ThemeVariations);
            }
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

            BeginUpdateThemeSettings();
            _NO_TRANSLATE_cbSelectTheme.Items.Clear();
            _NO_TRANSLATE_cbSelectTheme.Items.Add(new FormattedThemeId(ThemeId.Default));
            _NO_TRANSLATE_cbSelectTheme.Items.AddRange(_themeRepository.GetThemeIds()
                .Select(id => new FormattedThemeId(id))
                .Cast<object>()
                .ToArray());
            SelectedThemeId = AppSettings.ThemeId;
            SelectedThemeVariations = AppSettings.ThemeVariations;
            UseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
            EndUpdateThemeSettings();
        }

        protected override void PageToSettings()
        {
            AppSettings.MulticolorBranches = MulticolorBranches.Checked;
            AppSettings.RevisionGraphDrawAlternateBackColor = chkDrawAlternateBackColor.Checked;
            AppSettings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            AppSettings.HighlightAuthoredRevisions = chkHighlightAuthored.Checked;
            AppSettings.UseSystemVisualStyle = UseSystemVisualStyle;
            AppSettings.ThemeId = SelectedThemeId;
            AppSettings.ThemeVariations = SelectedThemeVariations;
        }

        private void ComboBoxTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsThemeSettingsUpdating())
            {
                return;
            }

            BeginUpdateThemeSettings();
            UseSystemVisualStyle = SelectedThemeId == ThemeId.Default;
            if (SelectedThemeId == ThemeId.Default)
            {
                SelectedThemeVariations = ThemeVariations.None;
            }

            EndUpdateThemeSettings();
        }

        private void ChkUseSystemVisualStyle_CheckedChanged(object sender, EventArgs e)
        {
            UpdateThemeSettings();
        }

        private void ChkColorblind_CheckedChanged(object sender, EventArgs e)
        {
            UpdateThemeSettings();
        }

        private void UpdateThemeSettings()
        {
            BeginUpdateThemeSettings();
            EndUpdateThemeSettings();
        }

        private void BeginUpdateThemeSettings()
        {
            _updateThemeSettingsCounter++;
        }

        private bool IsThemeSettingsUpdating() =>
            _updateThemeSettingsCounter > 0;

        private void EndUpdateThemeSettings()
        {
            int counter = --_updateThemeSettingsCounter;
            if (counter < 0)
            {
                throw new InvalidOperationException($"{nameof(EndUpdateThemeSettings)} must be called after {nameof(BeginUpdateThemeSettings)}");
            }

            if (counter == 0)
            {
                lblRestartNeeded.Visible = SettingsAreModified;
                chkColorblind.Enabled =
                    chkUseSystemVisualStyle.Enabled = SelectedThemeId != ThemeId.Default;
            }

            if (SelectedThemeId != ThemeId.Default)
            {
                try
                {
                    Theme unused = _themeRepository.GetTheme(SelectedThemeId, SelectedThemeVariations);
                }
                catch (Exception ex)
                {
                    string variations = string.Concat(SelectedThemeVariations.Select(_ => "." + _));
                    string identifier = new FormattedThemeId(SelectedThemeId).ToString();
                    MessageBoxes.ShowError(this, $"Failed to load theme {identifier}{variations}: {ex}");
                }
            }
        }

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

        private void tsmiApplicationFolder_Click(object sender, EventArgs e)
            => OsShellUtil.SelectPathInFileExplorer(_themePathProvider.AppThemesDirectory);

        private void tsmiUserFolder_Click(object sender, EventArgs e)
            => OsShellUtil.SelectPathInFileExplorer(_themePathProvider.UserThemesDirectory);
    }
}
