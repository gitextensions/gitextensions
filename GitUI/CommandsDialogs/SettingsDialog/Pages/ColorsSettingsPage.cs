using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
                int index = _NO_TRANSLATE_cbSelectTheme.Items.IndexOf(new FormattedThemeId(value));
                if (index < 0)
                {
                    throw new ArgumentException("Selected theme was not added to ComboBox");
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

            // align 1st columns across all tables
            tlpnlRevisionGraph.AdjustWidthToSize(0, MulticolorBranches, lblColorLineRemoved);
            tlpnlDiffView.AdjustWidthToSize(0, MulticolorBranches, lblColorLineRemoved);

            // align 2nd columns across all tables
            var colorControls = tlpnlRevisionGraph.Controls.Cast<Control>().Where(c => tlpnlRevisionGraph.GetColumn(c) == 1)
                .Union(tlpnlDiffView.Controls.Cast<Control>().Where(c => tlpnlDiffView.GetColumn(c) == 1))
                .ToArray();
            tlpnlRevisionGraph.AdjustWidthToSize(1, colorControls);
            tlpnlDiffView.AdjustWidthToSize(1, colorControls);
        }

        protected override void SettingsToPage()
        {
            MulticolorBranches.Checked = AppSettings.MulticolorBranches;
            MulticolorBranches_CheckedChanged(null, null);

            chkDrawAlternateBackColor.Checked = AppSettings.RevisionGraphDrawAlternateBackColor;
            DrawNonRelativesGray.Checked = AppSettings.RevisionGraphDrawNonRelativesGray;
            DrawNonRelativesTextGray.Checked = AppSettings.RevisionGraphDrawNonRelativesTextGray;
            chkHighlightAuthored.Checked = AppSettings.HighlightAuthoredRevisions;

            _NO_TRANSLATE_ColorHighlightAuthoredLabel.BackColor = AppSettings.HighlightAuthoredRevisions
                ? AppSettings.AuthoredRevisionsHighlightColor
                : Color.LightYellow;
            _NO_TRANSLATE_ColorHighlightAuthoredLabel.Text =
                AppSettings.AuthoredRevisionsHighlightColor.Name;
            _NO_TRANSLATE_ColorHighlightAuthoredLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorGraphLabel.BackColor = AppSettings.GraphColor;
            _NO_TRANSLATE_ColorGraphLabel.Text = AppSettings.GraphColor.Name;
            _NO_TRANSLATE_ColorGraphLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorTagLabel.BackColor = AppSettings.TagColor;
            _NO_TRANSLATE_ColorTagLabel.Text = AppSettings.TagColor.Name;
            _NO_TRANSLATE_ColorTagLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorBranchLabel.BackColor = AppSettings.BranchColor;
            _NO_TRANSLATE_ColorBranchLabel.Text = AppSettings.BranchColor.Name;
            _NO_TRANSLATE_ColorBranchLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = AppSettings.RemoteBranchColor;
            _NO_TRANSLATE_ColorRemoteBranchLabel.Text = AppSettings.RemoteBranchColor.Name;
            _NO_TRANSLATE_ColorRemoteBranchLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorOtherLabel.BackColor = AppSettings.OtherTagColor;
            _NO_TRANSLATE_ColorOtherLabel.Text = AppSettings.OtherTagColor.Name;
            _NO_TRANSLATE_ColorOtherLabel.SetForeColorForBackColor();

            _NO_TRANSLATE_ColorAddedLineLabel.BackColor = AppSettings.DiffAddedColor;
            _NO_TRANSLATE_ColorAddedLineLabel.Text = AppSettings.DiffAddedColor.Name;
            _NO_TRANSLATE_ColorAddedLineLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = AppSettings.DiffAddedExtraColor;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = AppSettings.DiffAddedExtraColor.Name;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.SetForeColorForBackColor();

            _NO_TRANSLATE_ColorRemovedLine.BackColor = AppSettings.DiffRemovedColor;
            _NO_TRANSLATE_ColorRemovedLine.Text = AppSettings.DiffRemovedColor.Name;
            _NO_TRANSLATE_ColorRemovedLine.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = AppSettings.DiffRemovedExtraColor;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = AppSettings.DiffRemovedExtraColor.Name;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorSectionLabel.BackColor = AppSettings.DiffSectionColor;
            _NO_TRANSLATE_ColorSectionLabel.Text = AppSettings.DiffSectionColor.Name;
            _NO_TRANSLATE_ColorSectionLabel.SetForeColorForBackColor();
            _NO_TRANSLATE_ColorHighlightAllOccurrencesLabel.BackColor = AppSettings.HighlightAllOccurencesColor;
            _NO_TRANSLATE_ColorHighlightAllOccurrencesLabel.Text = AppSettings.HighlightAllOccurencesColor.Name;
            _NO_TRANSLATE_ColorHighlightAllOccurrencesLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorHighlightAllOccurrencesLabel.BackColor);

            BeginUpdateThemeSettings();
            var themeRepository = new ThemeRepository(new ThemePersistence());
            _NO_TRANSLATE_cbSelectTheme.Items.Clear();
            _NO_TRANSLATE_cbSelectTheme.Items.Add(new FormattedThemeId(ThemeId.Default));
            _NO_TRANSLATE_cbSelectTheme.Items.AddRange(themeRepository.GetThemeIds()
                .Select(id => new FormattedThemeId(id))
                .Cast<object>()
                .ToArray());

            UseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
            SelectedThemeId = new ThemeId(AppSettings.UIThemeName, AppSettings.UIThemeIsBuiltin);
            EndUpdateThemeSettings();
        }

        protected override void PageToSettings()
        {
            AppSettings.MulticolorBranches = MulticolorBranches.Checked;
            AppSettings.RevisionGraphDrawAlternateBackColor = chkDrawAlternateBackColor.Checked;
            AppSettings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            AppSettings.HighlightAuthoredRevisions = chkHighlightAuthored.Checked;
            AppSettings.AuthoredRevisionsHighlightColor = chkHighlightAuthored.Checked ? _NO_TRANSLATE_ColorHighlightAuthoredLabel.BackColor : Color.LightYellow;

            AppSettings.GraphColor = _NO_TRANSLATE_ColorGraphLabel.BackColor;
            AppSettings.TagColor = _NO_TRANSLATE_ColorTagLabel.BackColor;
            AppSettings.BranchColor = _NO_TRANSLATE_ColorBranchLabel.BackColor;
            AppSettings.RemoteBranchColor = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor;
            AppSettings.OtherTagColor = _NO_TRANSLATE_ColorOtherLabel.BackColor;

            AppSettings.DiffAddedColor = _NO_TRANSLATE_ColorAddedLineLabel.BackColor;
            AppSettings.DiffRemovedColor = _NO_TRANSLATE_ColorRemovedLine.BackColor;
            AppSettings.DiffAddedExtraColor = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor;
            AppSettings.DiffRemovedExtraColor = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor;
            AppSettings.DiffSectionColor = _NO_TRANSLATE_ColorSectionLabel.BackColor;
            AppSettings.HighlightAllOccurencesColor = _NO_TRANSLATE_ColorHighlightAllOccurrencesLabel.BackColor;

            AppSettings.UseSystemVisualStyle = UseSystemVisualStyle;
            AppSettings.UIThemeName = SelectedThemeId.Name;
            AppSettings.UIThemeIsBuiltin = SelectedThemeId.IsBuiltin;
        }

        private void MulticolorBranches_CheckedChanged(object sender, EventArgs e)
        {
            if (MulticolorBranches.Checked)
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = false;
            }
            else
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = true;
            }
        }

        private void ColorLabel_Click(object sender, EventArgs e)
        {
            var label = (Label)sender;

            using (var colorDialog = new ColorDialog { Color = label.BackColor })
            {
                colorDialog.ShowDialog(this);
                label.BackColor = colorDialog.Color;
                label.Text = colorDialog.Color.Name;
                label.SetForeColorForBackColor();
            }
        }

        private void ChkHighlightAuthored_CheckedChanged(object sender, EventArgs e)
        {
            lblColorHighlightAuthored.Enabled = chkHighlightAuthored.Checked;
            _NO_TRANSLATE_ColorHighlightAuthoredLabel.Enabled = chkHighlightAuthored.Checked;
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
