namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ColorsSettingsPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbAppIcon = new System.Windows.Forms.GroupBox();
            this.label55 = new System.Windows.Forms.Label();
            this.IconPreviewSmall = new System.Windows.Forms.PictureBox();
            this.IconPreview = new System.Windows.Forms.PictureBox();
            this.IconStyle = new System.Windows.Forms.ComboBox();
            this.label54 = new System.Windows.Forms.Label();
            this.LightblueIcon = new System.Windows.Forms.RadioButton();
            this.RandomIcon = new System.Windows.Forms.RadioButton();
            this.YellowIcon = new System.Windows.Forms.RadioButton();
            this.RedIcon = new System.Windows.Forms.RadioButton();
            this.GreenIcon = new System.Windows.Forms.RadioButton();
            this.PurpleIcon = new System.Windows.Forms.RadioButton();
            this.BlueIcon = new System.Windows.Forms.RadioButton();
            this.DefaultIcon = new System.Windows.Forms.RadioButton();
            this.gbRevisionGraph = new System.Windows.Forms.GroupBox();
            this.chkDrawAlternateBackColor = new System.Windows.Forms.CheckBox();
            this.HighlightAuthoredRevisions = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesTextGray = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesGray = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_ColorGraphLabel = new System.Windows.Forms.Label();
            this.StripedBanchChange = new System.Windows.Forms.CheckBox();
            this.BranchBorders = new System.Windows.Forms.CheckBox();
            this.MulticolorBranches = new System.Windows.Forms.CheckBox();
            this.label33 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemoteBranchLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAuthoredRevisions = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorOtherLabel = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorTagLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorBranchLabel = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.gbDiffView = new System.Windows.Forms.GroupBox();
            this.label43 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineDiffLabel = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorSectionLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLine = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineLabel = new System.Windows.Forms.Label();
            this.gbAppIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreviewSmall)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).BeginInit();
            this.gbRevisionGraph.SuspendLayout();
            this.gbDiffView.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAppIcon
            // 
            this.gbAppIcon.Controls.Add(this.label55);
            this.gbAppIcon.Controls.Add(this.IconPreviewSmall);
            this.gbAppIcon.Controls.Add(this.IconPreview);
            this.gbAppIcon.Controls.Add(this.IconStyle);
            this.gbAppIcon.Controls.Add(this.label54);
            this.gbAppIcon.Controls.Add(this.LightblueIcon);
            this.gbAppIcon.Controls.Add(this.RandomIcon);
            this.gbAppIcon.Controls.Add(this.YellowIcon);
            this.gbAppIcon.Controls.Add(this.RedIcon);
            this.gbAppIcon.Controls.Add(this.GreenIcon);
            this.gbAppIcon.Controls.Add(this.PurpleIcon);
            this.gbAppIcon.Controls.Add(this.BlueIcon);
            this.gbAppIcon.Controls.Add(this.DefaultIcon);
            this.gbAppIcon.Location = new System.Drawing.Point(402, 3);
            this.gbAppIcon.Name = "gbAppIcon";
            this.gbAppIcon.Size = new System.Drawing.Size(321, 279);
            this.gbAppIcon.TabIndex = 1;
            this.gbAppIcon.TabStop = false;
            this.gbAppIcon.Text = "Application Icon";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(13, 55);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(54, 13);
            this.label55.TabIndex = 2;
            this.label55.Text = "Icon color";
            // 
            // IconPreviewSmall
            // 
            this.IconPreviewSmall.Location = new System.Drawing.Point(227, 66);
            this.IconPreviewSmall.Name = "IconPreviewSmall";
            this.IconPreviewSmall.Size = new System.Drawing.Size(16, 16);
            this.IconPreviewSmall.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.IconPreviewSmall.TabIndex = 13;
            this.IconPreviewSmall.TabStop = false;
            // 
            // IconPreview
            // 
            this.IconPreview.Location = new System.Drawing.Point(265, 50);
            this.IconPreview.Name = "IconPreview";
            this.IconPreview.Size = new System.Drawing.Size(32, 32);
            this.IconPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.IconPreview.TabIndex = 12;
            this.IconPreview.TabStop = false;
            // 
            // IconStyle
            // 
            this.IconStyle.FormattingEnabled = true;
            this.IconStyle.Items.AddRange(new object[] {
            "Default",
            "Large",
            "Small",
            "Cow"});
            this.IconStyle.Location = new System.Drawing.Point(111, 23);
            this.IconStyle.Name = "IconStyle";
            this.IconStyle.Size = new System.Drawing.Size(121, 21);
            this.IconStyle.TabIndex = 1;
            this.IconStyle.SelectedIndexChanged += new System.EventHandler(this.IconStyle_SelectedIndexChanged);
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(13, 26);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(54, 13);
            this.label54.TabIndex = 0;
            this.label54.Text = "Icon style";
            // 
            // LightblueIcon
            // 
            this.LightblueIcon.AutoSize = true;
            this.LightblueIcon.Location = new System.Drawing.Point(111, 81);
            this.LightblueIcon.Name = "LightblueIcon";
            this.LightblueIcon.Size = new System.Drawing.Size(71, 17);
            this.LightblueIcon.TabIndex = 4;
            this.LightblueIcon.TabStop = true;
            this.LightblueIcon.Text = "Light blue";
            this.LightblueIcon.UseVisualStyleBackColor = true;
            // 
            // RandomIcon
            // 
            this.RandomIcon.AutoSize = true;
            this.RandomIcon.Location = new System.Drawing.Point(111, 250);
            this.RandomIcon.Name = "RandomIcon";
            this.RandomIcon.Size = new System.Drawing.Size(64, 17);
            this.RandomIcon.TabIndex = 10;
            this.RandomIcon.TabStop = true;
            this.RandomIcon.Text = "Random";
            this.RandomIcon.UseVisualStyleBackColor = true;
            // 
            // YellowIcon
            // 
            this.YellowIcon.AutoSize = true;
            this.YellowIcon.Location = new System.Drawing.Point(111, 222);
            this.YellowIcon.Name = "YellowIcon";
            this.YellowIcon.Size = new System.Drawing.Size(55, 17);
            this.YellowIcon.TabIndex = 9;
            this.YellowIcon.TabStop = true;
            this.YellowIcon.Text = "Yellow";
            this.YellowIcon.UseVisualStyleBackColor = true;
            this.YellowIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // RedIcon
            // 
            this.RedIcon.AutoSize = true;
            this.RedIcon.Location = new System.Drawing.Point(111, 194);
            this.RedIcon.Name = "RedIcon";
            this.RedIcon.Size = new System.Drawing.Size(44, 17);
            this.RedIcon.TabIndex = 8;
            this.RedIcon.TabStop = true;
            this.RedIcon.Text = "Red";
            this.RedIcon.UseVisualStyleBackColor = true;
            this.RedIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // GreenIcon
            // 
            this.GreenIcon.AutoSize = true;
            this.GreenIcon.Location = new System.Drawing.Point(111, 165);
            this.GreenIcon.Name = "GreenIcon";
            this.GreenIcon.Size = new System.Drawing.Size(54, 17);
            this.GreenIcon.TabIndex = 7;
            this.GreenIcon.TabStop = true;
            this.GreenIcon.Text = "Green";
            this.GreenIcon.UseVisualStyleBackColor = true;
            this.GreenIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // PurpleIcon
            // 
            this.PurpleIcon.AutoSize = true;
            this.PurpleIcon.Location = new System.Drawing.Point(111, 137);
            this.PurpleIcon.Name = "PurpleIcon";
            this.PurpleIcon.Size = new System.Drawing.Size(55, 17);
            this.PurpleIcon.TabIndex = 6;
            this.PurpleIcon.TabStop = true;
            this.PurpleIcon.Text = "Purple";
            this.PurpleIcon.UseVisualStyleBackColor = true;
            this.PurpleIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // BlueIcon
            // 
            this.BlueIcon.AutoSize = true;
            this.BlueIcon.Location = new System.Drawing.Point(111, 109);
            this.BlueIcon.Name = "BlueIcon";
            this.BlueIcon.Size = new System.Drawing.Size(45, 17);
            this.BlueIcon.TabIndex = 5;
            this.BlueIcon.TabStop = true;
            this.BlueIcon.Text = "Blue";
            this.BlueIcon.UseVisualStyleBackColor = true;
            this.BlueIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // DefaultIcon
            // 
            this.DefaultIcon.AutoSize = true;
            this.DefaultIcon.Location = new System.Drawing.Point(111, 53);
            this.DefaultIcon.Name = "DefaultIcon";
            this.DefaultIcon.Size = new System.Drawing.Size(60, 17);
            this.DefaultIcon.TabIndex = 3;
            this.DefaultIcon.TabStop = true;
            this.DefaultIcon.Text = "Default";
            this.DefaultIcon.UseVisualStyleBackColor = true;
            this.DefaultIcon.CheckedChanged += new System.EventHandler(this.IconColor_CheckedChanged);
            // 
            // gbRevisionGraph
            // 
            this.gbRevisionGraph.Controls.Add(this.chkDrawAlternateBackColor);
            this.gbRevisionGraph.Controls.Add(this.HighlightAuthoredRevisions);
            this.gbRevisionGraph.Controls.Add(this.DrawNonRelativesTextGray);
            this.gbRevisionGraph.Controls.Add(this.DrawNonRelativesGray);
            this.gbRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorGraphLabel);
            this.gbRevisionGraph.Controls.Add(this.StripedBanchChange);
            this.gbRevisionGraph.Controls.Add(this.BranchBorders);
            this.gbRevisionGraph.Controls.Add(this.MulticolorBranches);
            this.gbRevisionGraph.Controls.Add(this.label33);
            this.gbRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorRemoteBranchLabel);
            this.gbRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorAuthoredRevisions);
            this.gbRevisionGraph.Controls.Add(this.label1);
            this.gbRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorOtherLabel);
            this.gbRevisionGraph.Controls.Add(this.label36);
            this.gbRevisionGraph.Controls.Add(this.label25);
            this.gbRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorTagLabel);
            this.gbRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorBranchLabel);
            this.gbRevisionGraph.Controls.Add(this.label32);
            this.gbRevisionGraph.Location = new System.Drawing.Point(3, 3);
            this.gbRevisionGraph.Name = "gbRevisionGraph";
            this.gbRevisionGraph.Size = new System.Drawing.Size(387, 340);
            this.gbRevisionGraph.TabIndex = 0;
            this.gbRevisionGraph.TabStop = false;
            this.gbRevisionGraph.Text = "Revision graph";
            // 
            // chkDrawAlternateBackColor
            // 
            this.chkDrawAlternateBackColor.AutoSize = true;
            this.chkDrawAlternateBackColor.Location = new System.Drawing.Point(9, 68);
            this.chkDrawAlternateBackColor.Name = "chkDrawAlternateBackColor";
            this.chkDrawAlternateBackColor.Size = new System.Drawing.Size(157, 17);
            this.chkDrawAlternateBackColor.TabIndex = 3;
            this.chkDrawAlternateBackColor.Text = "Draw alternate background";
            this.chkDrawAlternateBackColor.UseVisualStyleBackColor = true;
            // 
            // HighlightAuthoredRevisions
            // 
            this.HighlightAuthoredRevisions.AutoSize = true;
            this.HighlightAuthoredRevisions.Location = new System.Drawing.Point(9, 165);
            this.HighlightAuthoredRevisions.Name = "HighlightAuthoredRevisions";
            this.HighlightAuthoredRevisions.Size = new System.Drawing.Size(159, 17);
            this.HighlightAuthoredRevisions.TabIndex = 7;
            this.HighlightAuthoredRevisions.Text = "Highlight authored revisions";
            this.HighlightAuthoredRevisions.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesTextGray
            // 
            this.DrawNonRelativesTextGray.AutoSize = true;
            this.DrawNonRelativesTextGray.Location = new System.Drawing.Point(9, 140);
            this.DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(164, 17);
            this.DrawNonRelativesTextGray.TabIndex = 6;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(9, 116);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(172, 17);
            this.DrawNonRelativesGray.TabIndex = 5;
            this.DrawNonRelativesGray.Text = "Draw non relatives graph gray";
            this.DrawNonRelativesGray.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_ColorGraphLabel
            // 
            this._NO_TRANSLATE_ColorGraphLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorGraphLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorGraphLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorGraphLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorGraphLabel.Location = new System.Drawing.Point(287, 21);
            this._NO_TRANSLATE_ColorGraphLabel.Name = "_NO_TRANSLATE_ColorGraphLabel";
            this._NO_TRANSLATE_ColorGraphLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorGraphLabel.TabIndex = 1;
            this._NO_TRANSLATE_ColorGraphLabel.Text = "Red";
            this._NO_TRANSLATE_ColorGraphLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // StripedBanchChange
            // 
            this.StripedBanchChange.AutoSize = true;
            this.StripedBanchChange.Location = new System.Drawing.Point(9, 45);
            this.StripedBanchChange.Name = "StripedBanchChange";
            this.StripedBanchChange.Size = new System.Drawing.Size(134, 17);
            this.StripedBanchChange.TabIndex = 2;
            this.StripedBanchChange.Text = "Striped branch change";
            this.StripedBanchChange.UseVisualStyleBackColor = true;
            // 
            // BranchBorders
            // 
            this.BranchBorders.AutoSize = true;
            this.BranchBorders.Location = new System.Drawing.Point(9, 91);
            this.BranchBorders.Name = "BranchBorders";
            this.BranchBorders.Size = new System.Drawing.Size(127, 17);
            this.BranchBorders.TabIndex = 4;
            this.BranchBorders.Text = "Draw branch borders";
            this.BranchBorders.UseVisualStyleBackColor = true;
            // 
            // MulticolorBranches
            // 
            this.MulticolorBranches.AutoSize = true;
            this.MulticolorBranches.Location = new System.Drawing.Point(9, 20);
            this.MulticolorBranches.Name = "MulticolorBranches";
            this.MulticolorBranches.Size = new System.Drawing.Size(118, 17);
            this.MulticolorBranches.TabIndex = 0;
            this.MulticolorBranches.Text = "Multicolor branches";
            this.MulticolorBranches.UseVisualStyleBackColor = true;
            this.MulticolorBranches.CheckedChanged += new System.EventHandler(this.MulticolorBranches_CheckedChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 251);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(105, 13);
            this.label33.TabIndex = 12;
            this.label33.Text = "Color remote branch";
            // 
            // _NO_TRANSLATE_ColorRemoteBranchLabel
            // 
            this._NO_TRANSLATE_ColorRemoteBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Location = new System.Drawing.Point(287, 251);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Name = "_NO_TRANSLATE_ColorRemoteBranchLabel";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.TabIndex = 13;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // _NO_TRANSLATE_ColorAuthoredRevisions
            // 
            this._NO_TRANSLATE_ColorAuthoredRevisions.AutoSize = true;
            this._NO_TRANSLATE_ColorAuthoredRevisions.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAuthoredRevisions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAuthoredRevisions.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAuthoredRevisions.Location = new System.Drawing.Point(287, 308);
            this._NO_TRANSLATE_ColorAuthoredRevisions.Name = "_NO_TRANSLATE_ColorAuthoredRevisions";
            this._NO_TRANSLATE_ColorAuthoredRevisions.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorAuthoredRevisions.TabIndex = 17;
            this._NO_TRANSLATE_ColorAuthoredRevisions.Text = "Red";
            this._NO_TRANSLATE_ColorAuthoredRevisions.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 308);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Color authored revisions";
            // 
            // _NO_TRANSLATE_ColorOtherLabel
            // 
            this._NO_TRANSLATE_ColorOtherLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorOtherLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorOtherLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorOtherLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorOtherLabel.Location = new System.Drawing.Point(287, 279);
            this._NO_TRANSLATE_ColorOtherLabel.Name = "_NO_TRANSLATE_ColorOtherLabel";
            this._NO_TRANSLATE_ColorOtherLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorOtherLabel.TabIndex = 15;
            this._NO_TRANSLATE_ColorOtherLabel.Text = "Red";
            this._NO_TRANSLATE_ColorOtherLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 279);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(86, 13);
            this.label36.TabIndex = 14;
            this.label36.Text = "Color other label";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 194);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(51, 13);
            this.label25.TabIndex = 8;
            this.label25.Text = "Color tag";
            // 
            // _NO_TRANSLATE_ColorTagLabel
            // 
            this._NO_TRANSLATE_ColorTagLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorTagLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorTagLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorTagLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorTagLabel.Location = new System.Drawing.Point(287, 194);
            this._NO_TRANSLATE_ColorTagLabel.Name = "_NO_TRANSLATE_ColorTagLabel";
            this._NO_TRANSLATE_ColorTagLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorTagLabel.TabIndex = 9;
            this._NO_TRANSLATE_ColorTagLabel.Text = "Red";
            this._NO_TRANSLATE_ColorTagLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // _NO_TRANSLATE_ColorBranchLabel
            // 
            this._NO_TRANSLATE_ColorBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorBranchLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorBranchLabel.Location = new System.Drawing.Point(287, 222);
            this._NO_TRANSLATE_ColorBranchLabel.Name = "_NO_TRANSLATE_ColorBranchLabel";
            this._NO_TRANSLATE_ColorBranchLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorBranchLabel.TabIndex = 11;
            this._NO_TRANSLATE_ColorBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorBranchLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 222);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(68, 13);
            this.label32.TabIndex = 10;
            this.label32.Text = "Color branch";
            // 
            // gbDiffView
            // 
            this.gbDiffView.Controls.Add(this.label43);
            this.gbDiffView.Controls.Add(this._NO_TRANSLATE_ColorRemovedLineDiffLabel);
            this.gbDiffView.Controls.Add(this.label45);
            this.gbDiffView.Controls.Add(this._NO_TRANSLATE_ColorAddedLineDiffLabel);
            this.gbDiffView.Controls.Add(this.label27);
            this.gbDiffView.Controls.Add(this._NO_TRANSLATE_ColorSectionLabel);
            this.gbDiffView.Controls.Add(this._NO_TRANSLATE_ColorRemovedLine);
            this.gbDiffView.Controls.Add(this.label31);
            this.gbDiffView.Controls.Add(this.label29);
            this.gbDiffView.Controls.Add(this._NO_TRANSLATE_ColorAddedLineLabel);
            this.gbDiffView.Location = new System.Drawing.Point(3, 349);
            this.gbDiffView.Name = "gbDiffView";
            this.gbDiffView.Size = new System.Drawing.Size(387, 173);
            this.gbDiffView.TabIndex = 2;
            this.gbDiffView.TabStop = false;
            this.gbDiffView.Text = "Difference view";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(6, 79);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(153, 13);
            this.label43.TabIndex = 4;
            this.label43.Text = "Color removed line highlighting";
            // 
            // _NO_TRANSLATE_ColorRemovedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Location = new System.Drawing.Point(284, 79);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Name = "_NO_TRANSLATE_ColorRemovedLineDiffLabel";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.TabIndex = 5;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(6, 109);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(141, 13);
            this.label45.TabIndex = 6;
            this.label45.Text = "Color added line highlighting";
            // 
            // _NO_TRANSLATE_ColorAddedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Location = new System.Drawing.Point(284, 109);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Name = "_NO_TRANSLATE_ColorAddedLineDiffLabel";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.TabIndex = 7;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 18);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(96, 13);
            this.label27.TabIndex = 0;
            this.label27.Text = "Color removed line";
            // 
            // _NO_TRANSLATE_ColorSectionLabel
            // 
            this._NO_TRANSLATE_ColorSectionLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorSectionLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorSectionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorSectionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorSectionLabel.Location = new System.Drawing.Point(284, 138);
            this._NO_TRANSLATE_ColorSectionLabel.Name = "_NO_TRANSLATE_ColorSectionLabel";
            this._NO_TRANSLATE_ColorSectionLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorSectionLabel.TabIndex = 9;
            this._NO_TRANSLATE_ColorSectionLabel.Text = "Red";
            this._NO_TRANSLATE_ColorSectionLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // _NO_TRANSLATE_ColorRemovedLine
            // 
            this._NO_TRANSLATE_ColorRemovedLine.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLine.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorRemovedLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLine.Location = new System.Drawing.Point(284, 18);
            this._NO_TRANSLATE_ColorRemovedLine.Name = "_NO_TRANSLATE_ColorRemovedLine";
            this._NO_TRANSLATE_ColorRemovedLine.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorRemovedLine.TabIndex = 1;
            this._NO_TRANSLATE_ColorRemovedLine.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLine.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(6, 139);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(69, 13);
            this.label31.TabIndex = 8;
            this.label31.Text = "Color section";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 48);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(84, 13);
            this.label29.TabIndex = 2;
            this.label29.Text = "Color added line";
            // 
            // _NO_TRANSLATE_ColorAddedLineLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._NO_TRANSLATE_ColorAddedLineLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineLabel.Location = new System.Drawing.Point(284, 48);
            this._NO_TRANSLATE_ColorAddedLineLabel.Name = "_NO_TRANSLATE_ColorAddedLineLabel";
            this._NO_TRANSLATE_ColorAddedLineLabel.Size = new System.Drawing.Size(28, 15);
            this._NO_TRANSLATE_ColorAddedLineLabel.TabIndex = 3;
            this._NO_TRANSLATE_ColorAddedLineLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // ColorsSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.gbAppIcon);
            this.Controls.Add(this.gbRevisionGraph);
            this.Controls.Add(this.gbDiffView);
            this.Name = "ColorsSettingsPage";
            this.Size = new System.Drawing.Size(1360, 856);
            this.gbAppIcon.ResumeLayout(false);
            this.gbAppIcon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreviewSmall)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).EndInit();
            this.gbRevisionGraph.ResumeLayout(false);
            this.gbRevisionGraph.PerformLayout();
            this.gbDiffView.ResumeLayout(false);
            this.gbDiffView.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbAppIcon;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.PictureBox IconPreviewSmall;
        private System.Windows.Forms.PictureBox IconPreview;
        private System.Windows.Forms.ComboBox IconStyle;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.RadioButton LightblueIcon;
        private System.Windows.Forms.RadioButton RandomIcon;
        private System.Windows.Forms.RadioButton YellowIcon;
        private System.Windows.Forms.RadioButton RedIcon;
        private System.Windows.Forms.RadioButton GreenIcon;
        private System.Windows.Forms.RadioButton PurpleIcon;
        private System.Windows.Forms.RadioButton BlueIcon;
        private System.Windows.Forms.RadioButton DefaultIcon;
        private System.Windows.Forms.GroupBox gbRevisionGraph;
        private System.Windows.Forms.CheckBox DrawNonRelativesTextGray;
        private System.Windows.Forms.CheckBox DrawNonRelativesGray;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorGraphLabel;
        private System.Windows.Forms.CheckBox StripedBanchChange;
        private System.Windows.Forms.CheckBox BranchBorders;
        private System.Windows.Forms.CheckBox MulticolorBranches;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemoteBranchLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorOtherLabel;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorTagLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorBranchLabel;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.GroupBox gbDiffView;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemovedLineDiffLabel;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAddedLineDiffLabel;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorSectionLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemovedLine;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAddedLineLabel;
        private System.Windows.Forms.CheckBox HighlightAuthoredRevisions;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAuthoredRevisions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkDrawAlternateBackColor;
    }
}
