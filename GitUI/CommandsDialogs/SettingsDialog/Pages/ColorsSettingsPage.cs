using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ColorsSettingsPage : SettingsPageWithHeader
    {
        private bool _syncingTheme;

        public ColorsSettingsPage()
        {
            InitializeComponent();
            InitializeComplete();
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

            ThemeModule.Controller.ThemeChanged += Theme_Changed;
            ThemeModule.Controller.ColorChanged += Theme_ColorChanged;
        }

        protected override void SettingsToPage()
        {
            MulticolorBranches.Checked = AppSettings.MulticolorBranches;
            MulticolorBranches_CheckedChanged(null, null);

            chkDrawAlternateBackColor.Checked = AppSettings.RevisionGraphDrawAlternateBackColor;
            DrawNonRelativesGray.Checked = AppSettings.RevisionGraphDrawNonRelativesGray;
            DrawNonRelativesTextGray.Checked = AppSettings.RevisionGraphDrawNonRelativesTextGray;
            chkHighlightAuthored.Checked = AppSettings.HighlightAuthoredRevisions;
            chkUseSystemVisualStyle.Checked = AppSettings.UseSystemVisualStyle;

            _syncingTheme = true;
            try
            {
                UpdateComboBoxTheme(AppSettings.UIThemeName);
            }
            finally
            {
                _syncingTheme = false;
            }

            ThemeModule.Controller.SetTheme(AppSettings.UIThemeName);
            UpdateAppColors();
            UpdateRestartWarningVisibility();
        }

        private void UpdateAppColors()
        {
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
        }

        protected override void PageToSettings()
        {
            AppSettings.UseSystemVisualStyle = chkUseSystemVisualStyle.Checked;
            AppSettings.UIThemeName = (string)_NO_TRANSLATE_cbSelectTheme.SelectedItem;

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

        private void BtnTheme_Click(object sender, EventArgs e)
        {
            ThemeModule.ShowEditor();
        }

        private void BtnResetTheme_Click(object sender, EventArgs e)
        {
            ThemeModule.Controller.ResetTheme();
            chkUseSystemVisualStyle.Checked = true;
        }

        private void ComboBoxTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            var menu = (ComboBox)sender;
            if (menu.SelectedIndex < 0)
            {
                return;
            }

            if (_syncingTheme)
            {
                return;
            }

            _syncingTheme = true;
            try
            {
                ThemeModule.Controller.SetTheme((string)menu.SelectedItem);
            }
            finally
            {
                _syncingTheme = false;
            }
        }

        private void ChkUseSystemVisualStyle_CheckedChanged(object sender, EventArgs e)
        {
            ThemeModule.Controller.UseSystemVisualStyle = chkUseSystemVisualStyle.Checked;
            UpdateRestartWarningVisibility();
        }

        private void Theme_Changed(bool colorsChanged, string themeName)
        {
            if (colorsChanged)
            {
                UpdateAppColors();
            }

            if (!string.IsNullOrEmpty(themeName))
            {
                chkUseSystemVisualStyle.Checked = false;
            }

            if (!_syncingTheme)
            {
                _syncingTheme = true;
                try
                {
                    UpdateComboBoxTheme(themeName);
                }
                finally
                {
                    _syncingTheme = false;
                }
            }

            UpdateRestartWarningVisibility();
        }

        private void Theme_ColorChanged() =>
            UpdateAppColors();

        private void UpdateRestartWarningVisibility()
        {
            lblRestartNeeded.Visible =
                !ThemeModule.Controller.IsThemeInitial() ||
                ThemeModule.Controller.IsThemeModified();
        }

        private void UpdateComboBoxTheme(string themeName)
        {
            _NO_TRANSLATE_cbSelectTheme.BeginUpdate();

            _NO_TRANSLATE_cbSelectTheme.Items.Clear();
            _NO_TRANSLATE_cbSelectTheme.Items.AddRange(
                ThemeModule.Controller.GetSavedThemeNames().Cast<object>().ToArray());
            _NO_TRANSLATE_cbSelectTheme.SelectedIndex = _NO_TRANSLATE_cbSelectTheme.Items.IndexOf(
                themeName ?? string.Empty);

            _NO_TRANSLATE_cbSelectTheme.EndUpdate();
        }
    }
}
