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
            System.Windows.Forms.TableLayoutPanel tlpnlMain;
            this.gbRevisionGraph = new System.Windows.Forms.GroupBox();
            this.tlpnlRevisionGraph = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel = new System.Windows.Forms.Label();
            this.lblColorHighlightAuthored = new System.Windows.Forms.Label();
            this.chkHighlightAuthored = new System.Windows.Forms.CheckBox();
            this.MulticolorBranches = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_ColorRemoteBranchLabel = new System.Windows.Forms.Label();
            this.lblColorBranchRemote = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorOtherLabel = new System.Windows.Forms.Label();
            this.lblColorLabel = new System.Windows.Forms.Label();
            this.chkDrawAlternateBackColor = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesTextGray = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_ColorGraphLabel = new System.Windows.Forms.Label();
            this.DrawNonRelativesGray = new System.Windows.Forms.CheckBox();
            this.lblColorBranchLocal = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorBranchLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorTagLabel = new System.Windows.Forms.Label();
            this.lblColorTag = new System.Windows.Forms.Label();
            this.gbDiffView = new System.Windows.Forms.GroupBox();
            this.tlpnlDiffView = new System.Windows.Forms.TableLayoutPanel();
            this.lblColorLineRemoved = new System.Windows.Forms.Label();
            this.lblColorSection = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorSectionLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineDiffLabel = new System.Windows.Forms.Label();
            this.lblColorHilghlightLineAdded = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel = new System.Windows.Forms.Label();
            this.lblColorHilghlightLineRemoved = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorRemovedLine = new System.Windows.Forms.Label();
            this.lblColorLineAdded = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ColorAddedLineLabel = new System.Windows.Forms.Label();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.gbRevisionGraph.SuspendLayout();
            this.tlpnlRevisionGraph.SuspendLayout();
            this.gbDiffView.SuspendLayout();
            this.tlpnlDiffView.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlMain.Controls.Add(this.gbRevisionGraph, 0, 0);
            tlpnlMain.Controls.Add(this.gbDiffView, 0, 1);
            tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlMain.Location = new System.Drawing.Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 3;
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlMain.Size = new System.Drawing.Size(1394, 836);
            tlpnlMain.TabIndex = 0;
            // 
            // gbRevisionGraph
            // 
            this.gbRevisionGraph.AutoSize = true;
            this.gbRevisionGraph.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbRevisionGraph.Controls.Add(this.tlpnlRevisionGraph);
            this.gbRevisionGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbRevisionGraph.Location = new System.Drawing.Point(3, 3);
            this.gbRevisionGraph.Name = "gbRevisionGraph";
            this.gbRevisionGraph.Padding = new System.Windows.Forms.Padding(8);
            this.gbRevisionGraph.Size = new System.Drawing.Size(1388, 262);
            this.gbRevisionGraph.TabIndex = 0;
            this.gbRevisionGraph.TabStop = false;
            this.gbRevisionGraph.Text = "Revision graph";
            // 
            // tlpnlRevisionGraph
            // 
            this.tlpnlRevisionGraph.AutoSize = true;
            this.tlpnlRevisionGraph.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlRevisionGraph.ColumnCount = 3;
            this.tlpnlRevisionGraph.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlRevisionGraph.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlRevisionGraph.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorHighlightAuthoredLabel, 1, 6);
            this.tlpnlRevisionGraph.Controls.Add(this.lblColorHighlightAuthored, 0, 6);
            this.tlpnlRevisionGraph.Controls.Add(this.chkHighlightAuthored, 0, 5);
            this.tlpnlRevisionGraph.Controls.Add(this.MulticolorBranches, 0, 0);
            this.tlpnlRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorRemoteBranchLabel, 1, 9);
            this.tlpnlRevisionGraph.Controls.Add(this.lblColorBranchRemote, 0, 9);
            this.tlpnlRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorOtherLabel, 1, 10);
            this.tlpnlRevisionGraph.Controls.Add(this.lblColorLabel, 0, 10);
            this.tlpnlRevisionGraph.Controls.Add(this.chkDrawAlternateBackColor, 0, 1);
            this.tlpnlRevisionGraph.Controls.Add(this.DrawNonRelativesTextGray, 0, 4);
            this.tlpnlRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorGraphLabel, 1, 0);
            this.tlpnlRevisionGraph.Controls.Add(this.DrawNonRelativesGray, 0, 3);
            this.tlpnlRevisionGraph.Controls.Add(this.lblColorBranchLocal, 0, 8);
            this.tlpnlRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorBranchLabel, 1, 8);
            this.tlpnlRevisionGraph.Controls.Add(this._NO_TRANSLATE_ColorTagLabel, 1, 7);
            this.tlpnlRevisionGraph.Controls.Add(this.lblColorTag, 0, 7);
            this.tlpnlRevisionGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlRevisionGraph.Location = new System.Drawing.Point(8, 21);
            this.tlpnlRevisionGraph.Name = "tlpnlRevisionGraph";
            this.tlpnlRevisionGraph.RowCount = 12;
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.Size = new System.Drawing.Size(1372, 233);
            this.tlpnlRevisionGraph.TabIndex = 0;
            // 
            // _NO_TRANSLATE_ColorHighlightAuthoredLabel
            // 
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Enabled = false;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Location = new System.Drawing.Point(176, 138);
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Name = "_NO_TRANSLATE_ColorHighlightAuthoredLabel";
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.TabIndex = 8;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Text = "Red";
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorHighlightAuthoredLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorHighlightAuthored
            // 
            this.lblColorHighlightAuthored.AutoSize = true;
            this.lblColorHighlightAuthored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorHighlightAuthored.Enabled = false;
            this.lblColorHighlightAuthored.Location = new System.Drawing.Point(3, 141);
            this.lblColorHighlightAuthored.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorHighlightAuthored.Name = "lblColorHighlightAuthored";
            this.lblColorHighlightAuthored.Size = new System.Drawing.Size(167, 13);
            this.lblColorHighlightAuthored.TabIndex = 7;
            this.lblColorHighlightAuthored.Text = "Color authored revisions";
            // 
            // chkHighlightAuthored
            // 
            this.chkHighlightAuthored.AutoSize = true;
            this.chkHighlightAuthored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkHighlightAuthored.Location = new System.Drawing.Point(3, 118);
            this.chkHighlightAuthored.Name = "chkHighlightAuthored";
            this.chkHighlightAuthored.Size = new System.Drawing.Size(167, 17);
            this.chkHighlightAuthored.TabIndex = 6;
            this.chkHighlightAuthored.Text = "Highlight authored revisions";
            this.chkHighlightAuthored.UseVisualStyleBackColor = true;
            this.chkHighlightAuthored.CheckedChanged += new System.EventHandler(this.chkHighlightAuthored_CheckedChanged);
            // 
            // MulticolorBranches
            // 
            this.MulticolorBranches.AutoSize = true;
            this.MulticolorBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MulticolorBranches.Location = new System.Drawing.Point(3, 3);
            this.MulticolorBranches.Name = "MulticolorBranches";
            this.MulticolorBranches.Size = new System.Drawing.Size(167, 17);
            this.MulticolorBranches.TabIndex = 0;
            this.MulticolorBranches.Text = "Multicolor branches";
            this.MulticolorBranches.UseVisualStyleBackColor = true;
            this.MulticolorBranches.CheckedChanged += new System.EventHandler(this.MulticolorBranches_CheckedChanged);
            // 
            // _NO_TRANSLATE_ColorRemoteBranchLabel
            // 
            this._NO_TRANSLATE_ColorRemoteBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Location = new System.Drawing.Point(176, 195);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Name = "_NO_TRANSLATE_ColorRemoteBranchLabel";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorRemoteBranchLabel.TabIndex = 14;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemoteBranchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorRemoteBranchLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorBranchRemote
            // 
            this.lblColorBranchRemote.AutoSize = true;
            this.lblColorBranchRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorBranchRemote.Location = new System.Drawing.Point(3, 198);
            this.lblColorBranchRemote.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorBranchRemote.Name = "lblColorBranchRemote";
            this.lblColorBranchRemote.Size = new System.Drawing.Size(167, 13);
            this.lblColorBranchRemote.TabIndex = 13;
            this.lblColorBranchRemote.Text = "Color remote branch";
            // 
            // _NO_TRANSLATE_ColorOtherLabel
            // 
            this._NO_TRANSLATE_ColorOtherLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorOtherLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorOtherLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorOtherLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorOtherLabel.Location = new System.Drawing.Point(176, 214);
            this._NO_TRANSLATE_ColorOtherLabel.Name = "_NO_TRANSLATE_ColorOtherLabel";
            this._NO_TRANSLATE_ColorOtherLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorOtherLabel.TabIndex = 16;
            this._NO_TRANSLATE_ColorOtherLabel.Text = "Red";
            this._NO_TRANSLATE_ColorOtherLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorOtherLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorLabel
            // 
            this.lblColorLabel.AutoSize = true;
            this.lblColorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorLabel.Location = new System.Drawing.Point(3, 217);
            this.lblColorLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorLabel.Name = "lblColorLabel";
            this.lblColorLabel.Size = new System.Drawing.Size(167, 13);
            this.lblColorLabel.TabIndex = 15;
            this.lblColorLabel.Text = "Color other label";
            // 
            // chkDrawAlternateBackColor
            // 
            this.chkDrawAlternateBackColor.AutoSize = true;
            this.chkDrawAlternateBackColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDrawAlternateBackColor.Location = new System.Drawing.Point(3, 49);
            this.chkDrawAlternateBackColor.Name = "chkDrawAlternateBackColor";
            this.chkDrawAlternateBackColor.Size = new System.Drawing.Size(167, 17);
            this.chkDrawAlternateBackColor.TabIndex = 3;
            this.chkDrawAlternateBackColor.Text = "Draw alternate background";
            this.chkDrawAlternateBackColor.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesTextGray
            // 
            this.DrawNonRelativesTextGray.AutoSize = true;
            this.DrawNonRelativesTextGray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawNonRelativesTextGray.Location = new System.Drawing.Point(3, 95);
            this.DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(167, 17);
            this.DrawNonRelativesTextGray.TabIndex = 5;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_ColorGraphLabel
            // 
            this._NO_TRANSLATE_ColorGraphLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorGraphLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorGraphLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorGraphLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorGraphLabel.Location = new System.Drawing.Point(176, 0);
            this._NO_TRANSLATE_ColorGraphLabel.Name = "_NO_TRANSLATE_ColorGraphLabel";
            this._NO_TRANSLATE_ColorGraphLabel.Size = new System.Drawing.Size(27, 23);
            this._NO_TRANSLATE_ColorGraphLabel.TabIndex = 1;
            this._NO_TRANSLATE_ColorGraphLabel.Text = "Red";
            this._NO_TRANSLATE_ColorGraphLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorGraphLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(3, 72);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(167, 17);
            this.DrawNonRelativesGray.TabIndex = 4;
            this.DrawNonRelativesGray.Text = "Draw non relatives graph gray";
            this.DrawNonRelativesGray.UseVisualStyleBackColor = true;
            // 
            // lblColorBranchLocal
            // 
            this.lblColorBranchLocal.AutoSize = true;
            this.lblColorBranchLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorBranchLocal.Location = new System.Drawing.Point(3, 179);
            this.lblColorBranchLocal.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorBranchLocal.Name = "lblColorBranchLocal";
            this.lblColorBranchLocal.Size = new System.Drawing.Size(167, 13);
            this.lblColorBranchLocal.TabIndex = 11;
            this.lblColorBranchLocal.Text = "Color branch";
            // 
            // _NO_TRANSLATE_ColorBranchLabel
            // 
            this._NO_TRANSLATE_ColorBranchLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorBranchLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorBranchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorBranchLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorBranchLabel.Location = new System.Drawing.Point(176, 176);
            this._NO_TRANSLATE_ColorBranchLabel.Name = "_NO_TRANSLATE_ColorBranchLabel";
            this._NO_TRANSLATE_ColorBranchLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorBranchLabel.TabIndex = 12;
            this._NO_TRANSLATE_ColorBranchLabel.Text = "Red";
            this._NO_TRANSLATE_ColorBranchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorBranchLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // _NO_TRANSLATE_ColorTagLabel
            // 
            this._NO_TRANSLATE_ColorTagLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorTagLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorTagLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorTagLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorTagLabel.Location = new System.Drawing.Point(176, 157);
            this._NO_TRANSLATE_ColorTagLabel.Name = "_NO_TRANSLATE_ColorTagLabel";
            this._NO_TRANSLATE_ColorTagLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorTagLabel.TabIndex = 10;
            this._NO_TRANSLATE_ColorTagLabel.Text = "Red";
            this._NO_TRANSLATE_ColorTagLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorTagLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorTag
            // 
            this.lblColorTag.AutoSize = true;
            this.lblColorTag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorTag.Location = new System.Drawing.Point(3, 160);
            this.lblColorTag.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorTag.Name = "lblColorTag";
            this.lblColorTag.Size = new System.Drawing.Size(167, 13);
            this.lblColorTag.TabIndex = 9;
            this.lblColorTag.Text = "Color tag";
            // 
            // gbDiffView
            // 
            this.gbDiffView.AutoSize = true;
            this.gbDiffView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbDiffView.Controls.Add(this.tlpnlDiffView);
            this.gbDiffView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDiffView.Location = new System.Drawing.Point(3, 271);
            this.gbDiffView.Name = "gbDiffView";
            this.gbDiffView.Padding = new System.Windows.Forms.Padding(8);
            this.gbDiffView.Size = new System.Drawing.Size(1388, 124);
            this.gbDiffView.TabIndex = 1;
            this.gbDiffView.TabStop = false;
            this.gbDiffView.Text = "Difference view";
            // 
            // tlpnlDiffView
            // 
            this.tlpnlDiffView.AutoSize = true;
            this.tlpnlDiffView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlDiffView.ColumnCount = 3;
            this.tlpnlDiffView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlDiffView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlDiffView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlDiffView.Controls.Add(this.lblColorLineRemoved, 0, 0);
            this.tlpnlDiffView.Controls.Add(this.lblColorSection, 0, 4);
            this.tlpnlDiffView.Controls.Add(this._NO_TRANSLATE_ColorSectionLabel, 1, 4);
            this.tlpnlDiffView.Controls.Add(this._NO_TRANSLATE_ColorAddedLineDiffLabel, 1, 3);
            this.tlpnlDiffView.Controls.Add(this.lblColorHilghlightLineAdded, 0, 3);
            this.tlpnlDiffView.Controls.Add(this._NO_TRANSLATE_ColorRemovedLineDiffLabel, 1, 2);
            this.tlpnlDiffView.Controls.Add(this.lblColorHilghlightLineRemoved, 0, 2);
            this.tlpnlDiffView.Controls.Add(this._NO_TRANSLATE_ColorRemovedLine, 1, 0);
            this.tlpnlDiffView.Controls.Add(this.lblColorLineAdded, 0, 1);
            this.tlpnlDiffView.Controls.Add(this._NO_TRANSLATE_ColorAddedLineLabel, 1, 1);
            this.tlpnlDiffView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlDiffView.Location = new System.Drawing.Point(8, 21);
            this.tlpnlDiffView.Name = "tlpnlDiffView";
            this.tlpnlDiffView.RowCount = 5;
            this.tlpnlDiffView.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlDiffView.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlDiffView.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlDiffView.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlDiffView.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlDiffView.Size = new System.Drawing.Size(1372, 95);
            this.tlpnlDiffView.TabIndex = 0;
            // 
            // lblColorLineRemoved
            // 
            this.lblColorLineRemoved.AutoSize = true;
            this.lblColorLineRemoved.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorLineRemoved.Location = new System.Drawing.Point(3, 3);
            this.lblColorLineRemoved.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorLineRemoved.Name = "lblColorLineRemoved";
            this.lblColorLineRemoved.Size = new System.Drawing.Size(150, 13);
            this.lblColorLineRemoved.TabIndex = 0;
            this.lblColorLineRemoved.Text = "Color removed line";
            // 
            // lblColorSection
            // 
            this.lblColorSection.AutoSize = true;
            this.lblColorSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorSection.Location = new System.Drawing.Point(3, 79);
            this.lblColorSection.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorSection.Name = "lblColorSection";
            this.lblColorSection.Size = new System.Drawing.Size(150, 13);
            this.lblColorSection.TabIndex = 8;
            this.lblColorSection.Text = "Color section";
            // 
            // _NO_TRANSLATE_ColorSectionLabel
            // 
            this._NO_TRANSLATE_ColorSectionLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorSectionLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorSectionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorSectionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorSectionLabel.Location = new System.Drawing.Point(159, 76);
            this._NO_TRANSLATE_ColorSectionLabel.Name = "_NO_TRANSLATE_ColorSectionLabel";
            this._NO_TRANSLATE_ColorSectionLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorSectionLabel.TabIndex = 9;
            this._NO_TRANSLATE_ColorSectionLabel.Text = "Red";
            this._NO_TRANSLATE_ColorSectionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorSectionLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // _NO_TRANSLATE_ColorAddedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Location = new System.Drawing.Point(159, 57);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Name = "_NO_TRANSLATE_ColorAddedLineDiffLabel";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.TabIndex = 7;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorAddedLineDiffLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorHilghlightLineAdded
            // 
            this.lblColorHilghlightLineAdded.AutoSize = true;
            this.lblColorHilghlightLineAdded.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorHilghlightLineAdded.Location = new System.Drawing.Point(3, 60);
            this.lblColorHilghlightLineAdded.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorHilghlightLineAdded.Name = "lblColorHilghlightLineAdded";
            this.lblColorHilghlightLineAdded.Size = new System.Drawing.Size(150, 13);
            this.lblColorHilghlightLineAdded.TabIndex = 6;
            this.lblColorHilghlightLineAdded.Text = "Color added line highlighting";
            // 
            // _NO_TRANSLATE_ColorRemovedLineDiffLabel
            // 
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Location = new System.Drawing.Point(159, 38);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Name = "_NO_TRANSLATE_ColorRemovedLineDiffLabel";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.TabIndex = 5;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorRemovedLineDiffLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorHilghlightLineRemoved
            // 
            this.lblColorHilghlightLineRemoved.AutoSize = true;
            this.lblColorHilghlightLineRemoved.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorHilghlightLineRemoved.Location = new System.Drawing.Point(3, 41);
            this.lblColorHilghlightLineRemoved.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorHilghlightLineRemoved.Name = "lblColorHilghlightLineRemoved";
            this.lblColorHilghlightLineRemoved.Size = new System.Drawing.Size(150, 13);
            this.lblColorHilghlightLineRemoved.TabIndex = 4;
            this.lblColorHilghlightLineRemoved.Text = "Color removed line highlighting";
            // 
            // _NO_TRANSLATE_ColorRemovedLine
            // 
            this._NO_TRANSLATE_ColorRemovedLine.AutoSize = true;
            this._NO_TRANSLATE_ColorRemovedLine.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorRemovedLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorRemovedLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorRemovedLine.Location = new System.Drawing.Point(159, 0);
            this._NO_TRANSLATE_ColorRemovedLine.Name = "_NO_TRANSLATE_ColorRemovedLine";
            this._NO_TRANSLATE_ColorRemovedLine.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorRemovedLine.TabIndex = 1;
            this._NO_TRANSLATE_ColorRemovedLine.Text = "Red";
            this._NO_TRANSLATE_ColorRemovedLine.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorRemovedLine.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // lblColorLineAdded
            // 
            this.lblColorLineAdded.AutoSize = true;
            this.lblColorLineAdded.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColorLineAdded.Location = new System.Drawing.Point(3, 22);
            this.lblColorLineAdded.Margin = new System.Windows.Forms.Padding(3);
            this.lblColorLineAdded.Name = "lblColorLineAdded";
            this.lblColorLineAdded.Size = new System.Drawing.Size(150, 13);
            this.lblColorLineAdded.TabIndex = 2;
            this.lblColorLineAdded.Text = "Color added line";
            // 
            // _NO_TRANSLATE_ColorAddedLineLabel
            // 
            this._NO_TRANSLATE_ColorAddedLineLabel.AutoSize = true;
            this._NO_TRANSLATE_ColorAddedLineLabel.BackColor = System.Drawing.Color.Red;
            this._NO_TRANSLATE_ColorAddedLineLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ColorAddedLineLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ColorAddedLineLabel.Location = new System.Drawing.Point(159, 19);
            this._NO_TRANSLATE_ColorAddedLineLabel.Name = "_NO_TRANSLATE_ColorAddedLineLabel";
            this._NO_TRANSLATE_ColorAddedLineLabel.Size = new System.Drawing.Size(27, 19);
            this._NO_TRANSLATE_ColorAddedLineLabel.TabIndex = 3;
            this._NO_TRANSLATE_ColorAddedLineLabel.Text = "Red";
            this._NO_TRANSLATE_ColorAddedLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._NO_TRANSLATE_ColorAddedLineLabel.Click += new System.EventHandler(this.ColorLabel_Click);
            // 
            // ColorsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tlpnlMain);
            this.Name = "ColorsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1410, 852);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.gbRevisionGraph.ResumeLayout(false);
            this.gbRevisionGraph.PerformLayout();
            this.tlpnlRevisionGraph.ResumeLayout(false);
            this.tlpnlRevisionGraph.PerformLayout();
            this.gbDiffView.ResumeLayout(false);
            this.gbDiffView.PerformLayout();
            this.tlpnlDiffView.ResumeLayout(false);
            this.tlpnlDiffView.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox gbRevisionGraph;
        private System.Windows.Forms.CheckBox DrawNonRelativesTextGray;
        private System.Windows.Forms.CheckBox DrawNonRelativesGray;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorGraphLabel;
        private System.Windows.Forms.CheckBox MulticolorBranches;
        private System.Windows.Forms.Label lblColorBranchRemote;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemoteBranchLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorOtherLabel;
        private System.Windows.Forms.Label lblColorLabel;
        private System.Windows.Forms.Label lblColorTag;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorTagLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorBranchLabel;
        private System.Windows.Forms.Label lblColorBranchLocal;
        private System.Windows.Forms.GroupBox gbDiffView;
        private System.Windows.Forms.Label lblColorHilghlightLineRemoved;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemovedLineDiffLabel;
        private System.Windows.Forms.Label lblColorHilghlightLineAdded;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAddedLineDiffLabel;
        private System.Windows.Forms.Label lblColorLineRemoved;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorSectionLabel;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorRemovedLine;
        private System.Windows.Forms.Label lblColorSection;
        private System.Windows.Forms.Label lblColorLineAdded;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorAddedLineLabel;
        private System.Windows.Forms.CheckBox chkDrawAlternateBackColor;
        private System.Windows.Forms.TableLayoutPanel tlpnlRevisionGraph;
        private System.Windows.Forms.TableLayoutPanel tlpnlDiffView;
        private System.Windows.Forms.Label lblColorHighlightAuthored;
        private System.Windows.Forms.CheckBox chkHighlightAuthored;
        private System.Windows.Forms.Label _NO_TRANSLATE_ColorHighlightAuthoredLabel;
    }
}
