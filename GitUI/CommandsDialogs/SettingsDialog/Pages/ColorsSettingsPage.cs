using System;
using System.Diagnostics;
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
                var formattedThemeId = new FormattedThemeId(value);
                int index = _NO_TRANSLATE_cbSelectTheme.Items.IndexOf(formattedThemeId);
                if (index < 0)
                {
                    // Handle case when selected theme is missing gracefully.
                    // It may happen in a following scenario:
                    // - user creates custom theme and selects it in this settings page
                    // - user saves app settings
                    // - user deletes the file with custom theme
                    Trace.WriteLine("Theme not found: " + formattedThemeId);
                    index = 0;
                }

                _NO_TRANSLATE_cbSelectTheme.SelectedIndex = index;
            }
        }

        public bool UseSystemVisualStyle
        {
            get => chkUseSystemVisualStyle.Checked;
            set => chkUseSystemVisualStyle.Checked = value;
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
            var themeRepository = new ThemeRepository(new ThemePersistence());
            _NO_TRANSLATE_cbSelectTheme.Items.Clear();
            _NO_TRANSLATE_cbSelectTheme.Items.Add(new FormattedThemeId(ThemeId.Default));
            _NO_TRANSLATE_cbSelectTheme.Items.AddRange(themeRepository.GetThemeIds()
                .Select(id => new FormattedThemeId(id))
                .Cast<object>()
                .ToArray());
            SelectedThemeId = AppSettings.ThemeId;
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
        }

        private void ComboBoxTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsThemeSettingsUpdating())
            {
                return;
            }

            BeginUpdateThemeSettings();
            UseSystemVisualStyle = SelectedThemeId == ThemeId.Default;
            EndUpdateThemeSettings();
        }

        private void ChkUseSystemVisualStyle_CheckedChanged(object sender, EventArgs e)
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
                bool settingsChanged =
                    UseSystemVisualStyle != ThemeModule.Settings.UseSystemVisualStyle ||
                    SelectedThemeId != ThemeModule.Settings.Theme.Id;

                lblRestartNeeded.Visible = settingsChanged;
                chkUseSystemVisualStyle.Enabled = SelectedThemeId != ThemeId.Default;
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
    }
}
