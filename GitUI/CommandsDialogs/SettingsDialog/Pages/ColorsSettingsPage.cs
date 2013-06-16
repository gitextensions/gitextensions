﻿using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
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

        protected override string GetCommaSeparatedKeywordList()
        {
            return "color,graph,diff,icon";
        }

        protected override void SettingsToPage()
        {
            MulticolorBranches.Checked = AppSettings.MulticolorBranches;
            MulticolorBranches_CheckedChanged(null, null);

            DrawNonRelativesGray.Checked = AppSettings.RevisionGraphDrawNonRelativesGray;
            DrawNonRelativesTextGray.Checked = AppSettings.RevisionGraphDrawNonRelativesTextGray;
            BranchBorders.Checked = AppSettings.BranchBorders;
            StripedBanchChange.Checked = AppSettings.StripedBranchChange;

            _NO_TRANSLATE_ColorGraphLabel.BackColor = AppSettings.GraphColor;
            _NO_TRANSLATE_ColorGraphLabel.Text = AppSettings.GraphColor.Name;
            _NO_TRANSLATE_ColorGraphLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorGraphLabel.BackColor);
            _NO_TRANSLATE_ColorTagLabel.BackColor = AppSettings.TagColor;
            _NO_TRANSLATE_ColorTagLabel.Text = AppSettings.TagColor.Name;
            _NO_TRANSLATE_ColorTagLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
            _NO_TRANSLATE_ColorBranchLabel.BackColor = AppSettings.BranchColor;
            _NO_TRANSLATE_ColorBranchLabel.Text = AppSettings.BranchColor.Name;
            _NO_TRANSLATE_ColorBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
            _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = AppSettings.RemoteBranchColor;
            _NO_TRANSLATE_ColorRemoteBranchLabel.Text = AppSettings.RemoteBranchColor.Name;
            _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
            _NO_TRANSLATE_ColorOtherLabel.BackColor = AppSettings.OtherTagColor;
            _NO_TRANSLATE_ColorOtherLabel.Text = AppSettings.OtherTagColor.Name;
            _NO_TRANSLATE_ColorOtherLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);

            _NO_TRANSLATE_ColorAddedLineLabel.BackColor = AppSettings.DiffAddedColor;
            _NO_TRANSLATE_ColorAddedLineLabel.Text = AppSettings.DiffAddedColor.Name;
            _NO_TRANSLATE_ColorAddedLineLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
            _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = AppSettings.DiffAddedExtraColor;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = AppSettings.DiffAddedExtraColor.Name;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);

            _NO_TRANSLATE_ColorRemovedLine.BackColor = AppSettings.DiffRemovedColor;
            _NO_TRANSLATE_ColorRemovedLine.Text = AppSettings.DiffRemovedColor.Name;
            _NO_TRANSLATE_ColorRemovedLine.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = AppSettings.DiffRemovedExtraColor;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = AppSettings.DiffRemovedExtraColor.Name;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
            _NO_TRANSLATE_ColorSectionLabel.BackColor = AppSettings.DiffSectionColor;
            _NO_TRANSLATE_ColorSectionLabel.Text = AppSettings.DiffSectionColor.Name;
            _NO_TRANSLATE_ColorSectionLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);

            string iconColor = AppSettings.IconColor.ToLower();
            DefaultIcon.Checked = iconColor == "default";
            BlueIcon.Checked = iconColor == "blue";
            GreenIcon.Checked = iconColor == "green";
            PurpleIcon.Checked = iconColor == "purple";
            RedIcon.Checked = iconColor == "red";
            YellowIcon.Checked = iconColor == "yellow";
            RandomIcon.Checked = iconColor == "random";

            IconStyle.Text = AppSettings.IconStyle;

            ShowIconPreview();            
        }

        protected override void PageToSettings()
        {
            AppSettings.MulticolorBranches = MulticolorBranches.Checked;
            AppSettings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            AppSettings.BranchBorders = BranchBorders.Checked;
            AppSettings.StripedBranchChange = StripedBanchChange.Checked;
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

            AppSettings.IconColor = GetSelectedApplicationIconColor();
            AppSettings.IconStyle = IconStyle.Text;            
        }

        private string GetSelectedApplicationIconColor()
        {
            if (BlueIcon.Checked)
                return "blue";
            if (LightblueIcon.Checked)
                return "lightblue";
            if (GreenIcon.Checked)
                return "green";
            if (PurpleIcon.Checked)
                return "purple";
            if (RedIcon.Checked)
                return "red";
            if (YellowIcon.Checked)
                return "yellow";
            if (RandomIcon.Checked)
                return "random";
            return "default";
        }

        private void IconStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
                return;

            ShowIconPreview();
        }

        private void IconColor_CheckedChanged(object sender, EventArgs e)
        {
            if (IsLoadingSettings)
                return;

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
            string color = IconStyle.Text.ToLowerInvariant();
            Icon icon = null;
            switch (color)
            {
                case "default":
                    IconPreview.Image = (new Icon(GitExtensionsForm.GetApplicationIcon("Large", GetSelectedApplicationIconColor()), 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(GitExtensionsForm.GetApplicationIcon("Small", GetSelectedApplicationIconColor()), 16, 16)).ToBitmap();
                    break;
                case "small":
                    icon = GitExtensionsForm.GetApplicationIcon("Small", GetSelectedApplicationIconColor());
                    IconPreview.Image = (new Icon(icon, 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(icon, 16, 16)).ToBitmap();
                    break;
                case "large":
                    icon = GitExtensionsForm.GetApplicationIcon("Large", GetSelectedApplicationIconColor());
                    IconPreview.Image = (new Icon(icon, 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(icon, 16, 16)).ToBitmap();
                    break;
                case "cow":
                    icon = GitExtensionsForm.GetApplicationIcon("Cow", GetSelectedApplicationIconColor());
                    IconPreview.Image = (new Icon(icon, 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(icon, 16, 16)).ToBitmap();
                    break;
            }
        }

        private void ColorAddedLineDiffLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog
            {
                Color = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor
            })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);
        }

        private void _ColorGraphLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorGraphLabel.BackColor })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorGraphLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorGraphLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorGraphLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);
        }

        private void colorAddedLineLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog
            {
                Color = _NO_TRANSLATE_ColorAddedLineLabel.BackColor
            })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorAddedLineLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorAddedLineLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorAddedLineLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
        }

        private void ColorRemovedLineDiffLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog
            {
                Color = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor
            })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
        }

        private void ColorRemovedLine_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorRemovedLine.BackColor })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorRemovedLine.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorRemovedLine.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorRemovedLine.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
        }

        private void ColorSectionLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorSectionLabel.BackColor })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorSectionLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorSectionLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorSectionLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);
        }

        private void ColorTagLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorTagLabel.BackColor })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorTagLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorTagLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorTagLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
        }

        private void ColorBranchLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorBranchLabel.BackColor })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorBranchLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorBranchLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
        }

        private void ColorRemoteBranchLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog
            {
                Color = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor
            })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorRemoteBranchLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
        }

        private void ColorOtherLabel_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorOtherLabel.BackColor })
            {
                colorDialog.ShowDialog(this);
                _NO_TRANSLATE_ColorOtherLabel.BackColor = colorDialog.Color;
                _NO_TRANSLATE_ColorOtherLabel.Text = colorDialog.Color.Name;
            }
            _NO_TRANSLATE_ColorOtherLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);
        }
    }
}
