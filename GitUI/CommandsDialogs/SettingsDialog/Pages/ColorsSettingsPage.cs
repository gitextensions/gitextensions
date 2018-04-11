using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Editor;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ColorsSettingsPage : SettingsPageWithHeader
    {
        public ColorsSettingsPage()
        {
            InitializeComponent();
            Text = "Colors";
            Translate();
        }

        private static int GetIconStyleIndex(string text)
        {
            switch (text.ToLowerInvariant())
            {
                case "large":
                    return 1;
                case "small":
                    return 2;
                case "cow":
                    return 3;
                default:
                    return 0;
            }
        }

        private static string GetIconStyleString(int index)
        {
            switch (index)
            {
                case 1:
                    return "large";
                case 2:
                    return "small";
                case 3:
                    return "cow";
                default:
                    return "default";
            }
        }

        protected override void SettingsToPage()
        {
            MulticolorBranches.Checked = AppSettings.MulticolorBranches;
            MulticolorBranches_CheckedChanged(null, null);

            chkDrawAlternateBackColor.Checked = AppSettings.RevisionGraphDrawAlternateBackColor;
            DrawNonRelativesGray.Checked = AppSettings.RevisionGraphDrawNonRelativesGray;
            DrawNonRelativesTextGray.Checked = AppSettings.RevisionGraphDrawNonRelativesTextGray;
            BranchBorders.Checked = AppSettings.BranchBorders;
            StripedBanchChange.Checked = AppSettings.StripedBranchChange;
            HighlightAuthoredRevisions.Checked = AppSettings.HighlightAuthoredRevisions;

            _NO_TRANSLATE_ColorGraphLabel.BackColor = AppSettings.GraphColor;
            _NO_TRANSLATE_ColorGraphLabel.Text = AppSettings.GraphColor.Name;
            _NO_TRANSLATE_ColorGraphLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorGraphLabel.BackColor);
            _NO_TRANSLATE_ColorTagLabel.BackColor = AppSettings.TagColor;
            _NO_TRANSLATE_ColorTagLabel.Text = AppSettings.TagColor.Name;
            _NO_TRANSLATE_ColorTagLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
            _NO_TRANSLATE_ColorBranchLabel.BackColor = AppSettings.BranchColor;
            _NO_TRANSLATE_ColorBranchLabel.Text = AppSettings.BranchColor.Name;
            _NO_TRANSLATE_ColorBranchLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
            _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = AppSettings.RemoteBranchColor;
            _NO_TRANSLATE_ColorRemoteBranchLabel.Text = AppSettings.RemoteBranchColor.Name;
            _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
            _NO_TRANSLATE_ColorOtherLabel.BackColor = AppSettings.OtherTagColor;
            _NO_TRANSLATE_ColorOtherLabel.Text = AppSettings.OtherTagColor.Name;
            _NO_TRANSLATE_ColorOtherLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);

            _NO_TRANSLATE_ColorAddedLineLabel.BackColor = AppSettings.DiffAddedColor;
            _NO_TRANSLATE_ColorAddedLineLabel.Text = AppSettings.DiffAddedColor.Name;
            _NO_TRANSLATE_ColorAddedLineLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
            _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = AppSettings.DiffAddedExtraColor;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = AppSettings.DiffAddedExtraColor.Name;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);

            _NO_TRANSLATE_ColorRemovedLine.BackColor = AppSettings.DiffRemovedColor;
            _NO_TRANSLATE_ColorRemovedLine.Text = AppSettings.DiffRemovedColor.Name;
            _NO_TRANSLATE_ColorRemovedLine.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = AppSettings.DiffRemovedExtraColor;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = AppSettings.DiffRemovedExtraColor.Name;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
            _NO_TRANSLATE_ColorSectionLabel.BackColor = AppSettings.DiffSectionColor;
            _NO_TRANSLATE_ColorSectionLabel.Text = AppSettings.DiffSectionColor.Name;
            _NO_TRANSLATE_ColorSectionLabel.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);

            _NO_TRANSLATE_ColorAuthoredRevisions.BackColor = AppSettings.AuthoredRevisionsColor;
            _NO_TRANSLATE_ColorAuthoredRevisions.Text = AppSettings.AuthoredRevisionsColor.Name;
            _NO_TRANSLATE_ColorAuthoredRevisions.ForeColor = ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAuthoredRevisions.BackColor);

            string iconColor = AppSettings.IconColor.ToLower();
            DefaultIcon.Checked = iconColor == "default";
            BlueIcon.Checked = iconColor == "blue";
            GreenIcon.Checked = iconColor == "green";
            PurpleIcon.Checked = iconColor == "purple";
            RedIcon.Checked = iconColor == "red";
            YellowIcon.Checked = iconColor == "yellow";
            RandomIcon.Checked = iconColor == "random";

            IconStyle.SelectedIndex = GetIconStyleIndex(AppSettings.IconStyle);

            ShowIconPreview();
        }

        protected override void PageToSettings()
        {
            AppSettings.MulticolorBranches = MulticolorBranches.Checked;
            AppSettings.RevisionGraphDrawAlternateBackColor = chkDrawAlternateBackColor.Checked;
            AppSettings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            AppSettings.BranchBorders = BranchBorders.Checked;
            AppSettings.StripedBranchChange = StripedBanchChange.Checked;
            AppSettings.HighlightAuthoredRevisions = HighlightAuthoredRevisions.Checked;

            AppSettings.GraphColor = _NO_TRANSLATE_ColorGraphLabel.BackColor;
            AppSettings.TagColor = _NO_TRANSLATE_ColorTagLabel.BackColor;
            AppSettings.BranchColor = _NO_TRANSLATE_ColorBranchLabel.BackColor;
            AppSettings.RemoteBranchColor = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor;
            AppSettings.OtherTagColor = _NO_TRANSLATE_ColorOtherLabel.BackColor;
            AppSettings.AuthoredRevisionsColor = _NO_TRANSLATE_ColorAuthoredRevisions.BackColor;

            AppSettings.DiffAddedColor = _NO_TRANSLATE_ColorAddedLineLabel.BackColor;
            AppSettings.DiffRemovedColor = _NO_TRANSLATE_ColorRemovedLine.BackColor;
            AppSettings.DiffAddedExtraColor = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor;
            AppSettings.DiffRemovedExtraColor = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor;
            AppSettings.DiffSectionColor = _NO_TRANSLATE_ColorSectionLabel.BackColor;

            AppSettings.IconColor = GetSelectedApplicationIconColor();
            AppSettings.IconStyle = GetIconStyleString(IconStyle.SelectedIndex);
        }

        private string GetSelectedApplicationIconColor()
        {
            if (BlueIcon.Checked)
            {
                return "blue";
            }

            if (LightblueIcon.Checked)
            {
                return "lightblue";
            }

            if (GreenIcon.Checked)
            {
                return "green";
            }

            if (PurpleIcon.Checked)
            {
                return "purple";
            }

            if (RedIcon.Checked)
            {
                return "red";
            }

            if (YellowIcon.Checked)
            {
                return "yellow";
            }

            if (RandomIcon.Checked)
            {
                return "random";
            }

            return "default";
        }

        private void IconStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            ShowIconPreview();
        }

        private void IconColor_CheckedChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
            {
                return;
            }

            ShowIconPreview();
        }

        private void MulticolorBranches_CheckedChanged(object sender, EventArgs e)
        {
            if (MulticolorBranches.Checked)
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = false;
                StripedBanchChange.Enabled = true;
            }
            else
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = true;
                StripedBanchChange.Enabled = false;
            }
        }

        private void ShowIconPreview()
        {
            switch (IconStyle.SelectedIndex)
            {
                case 0:
                    IconPreview.Image = GetIcon("Large", 32);
                    IconPreviewSmall.Image = GetIcon("Small", 16);
                    break;
                case 1:
                    IconPreview.Image = GetIcon("Small", 32);
                    IconPreviewSmall.Image = GetIcon("Small", 16);
                    break;
                case 2:
                    IconPreview.Image = GetIcon("Large", 32);
                    IconPreviewSmall.Image = GetIcon("Large", 16);
                    break;
                case 3:
                    IconPreview.Image = GetIcon("Cow", 32);
                    IconPreviewSmall.Image = GetIcon("Cow", 16);
                    break;
            }

            Image GetIcon(string name, int size)
            {
                var icon = GitExtensionsForm.GetApplicationIcon(name, GetSelectedApplicationIconColor());

                var targetWidth = (int)(DpiUtil.ScaleX * size);
                var targetHeight = (int)(DpiUtil.ScaleY * size);

                if (icon.Width != targetWidth && icon.Height != targetHeight)
                {
                    icon = new Icon(icon, targetWidth, targetHeight);
                }

                return icon.ToBitmap();
            }
        }

        private void ColorLabel_Click(object sender, EventArgs e)
        {
            PickColor((Label)sender);
        }

        private void PickColor(Control targetColorControl)
        {
            using (var colorDialog = new ColorDialog { Color = targetColorControl.BackColor })
            {
                colorDialog.ShowDialog(this);
                targetColorControl.BackColor = colorDialog.Color;
                targetColorControl.Text = colorDialog.Color.Name;
            }

            targetColorControl.ForeColor =
                ColorHelper.GetForeColorForBackColor(targetColorControl.BackColor);
        }
    }
}
