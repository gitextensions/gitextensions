﻿namespace GitUI.CommandsDialogs.SettingsDialog.Pages
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
            if (disposing && (components is not null))
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TableLayoutPanel tlpnlMain;
            this.gbRevisionGraph = new System.Windows.Forms.GroupBox();
            this.tlpnlRevisionGraph = new System.Windows.Forms.TableLayoutPanel();
            this.chkHighlightAuthored = new System.Windows.Forms.CheckBox();
            this.MulticolorBranches = new System.Windows.Forms.CheckBox();
            this.chkDrawAlternateBackColor = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesTextGray = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesGray = new System.Windows.Forms.CheckBox();
            this.gbTheme = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkUseSystemVisualStyle = new System.Windows.Forms.CheckBox();
            this.chkColorblind = new System.Windows.Forms.CheckBox();
            this.sbOpenThemeFolder = new GitUI.Script.SplitButton();
            this.cmsOpenThemeFolders = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiApplicationFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_cbSelectTheme = new System.Windows.Forms.ComboBox();
            this.lblRestartNeeded = new System.Windows.Forms.Label();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.gbRevisionGraph.SuspendLayout();
            this.tlpnlRevisionGraph.SuspendLayout();
            this.gbTheme.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.cmsOpenThemeFolders.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlMain.Controls.Add(this.gbRevisionGraph, 0, 0);
            tlpnlMain.Controls.Add(this.gbTheme, 0, 2);
            tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlMain.Location = new System.Drawing.Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlMain.Size = new System.Drawing.Size(1421, 428);
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
            this.gbRevisionGraph.Size = new System.Drawing.Size(1415, 144);
            this.gbRevisionGraph.TabIndex = 0;
            this.gbRevisionGraph.TabStop = false;
            this.gbRevisionGraph.Text = "Revision graph";
            // 
            // tlpnlRevisionGraph
            // 
            this.tlpnlRevisionGraph.AutoSize = true;
            this.tlpnlRevisionGraph.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlRevisionGraph.ColumnCount = 2;
            this.tlpnlRevisionGraph.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlRevisionGraph.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlRevisionGraph.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlRevisionGraph.Controls.Add(this.chkHighlightAuthored, 0, 4);
            this.tlpnlRevisionGraph.Controls.Add(this.MulticolorBranches, 0, 0);
            this.tlpnlRevisionGraph.Controls.Add(this.chkDrawAlternateBackColor, 0, 1);
            this.tlpnlRevisionGraph.Controls.Add(this.DrawNonRelativesTextGray, 0, 3);
            this.tlpnlRevisionGraph.Controls.Add(this.DrawNonRelativesGray, 0, 2);
            this.tlpnlRevisionGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlRevisionGraph.Location = new System.Drawing.Point(8, 21);
            this.tlpnlRevisionGraph.Name = "tlpnlRevisionGraph";
            this.tlpnlRevisionGraph.RowCount = 5;
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.Size = new System.Drawing.Size(1399, 115);
            this.tlpnlRevisionGraph.TabIndex = 0;
            // 
            // chkHighlightAuthored
            // 
            this.chkHighlightAuthored.AutoSize = true;
            this.chkHighlightAuthored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkHighlightAuthored.Location = new System.Drawing.Point(3, 95);
            this.chkHighlightAuthored.Name = "chkHighlightAuthored";
            this.chkHighlightAuthored.Size = new System.Drawing.Size(167, 17);
            this.chkHighlightAuthored.TabIndex = 5;
            this.chkHighlightAuthored.Text = "Highlight authored revisions";
            this.chkHighlightAuthored.UseVisualStyleBackColor = true;
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
            // 
            // chkDrawAlternateBackColor
            // 
            this.chkDrawAlternateBackColor.AutoSize = true;
            this.chkDrawAlternateBackColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDrawAlternateBackColor.Location = new System.Drawing.Point(3, 26);
            this.chkDrawAlternateBackColor.Name = "chkDrawAlternateBackColor";
            this.chkDrawAlternateBackColor.Size = new System.Drawing.Size(167, 17);
            this.chkDrawAlternateBackColor.TabIndex = 2;
            this.chkDrawAlternateBackColor.Text = "Draw alternate background";
            this.chkDrawAlternateBackColor.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesTextGray
            // 
            this.DrawNonRelativesTextGray.AutoSize = true;
            this.DrawNonRelativesTextGray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawNonRelativesTextGray.Location = new System.Drawing.Point(3, 72);
            this.DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(167, 17);
            this.DrawNonRelativesTextGray.TabIndex = 4;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(3, 49);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(167, 17);
            this.DrawNonRelativesGray.TabIndex = 3;
            this.DrawNonRelativesGray.Text = "Draw non relatives graph gray";
            this.DrawNonRelativesGray.UseVisualStyleBackColor = true;
            // 
            // gbTheme
            // 
            this.gbTheme.AutoSize = true;
            this.gbTheme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbTheme.Controls.Add(this.tableLayoutPanel1);
            this.gbTheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTheme.Location = new System.Drawing.Point(3, 153);
            this.gbTheme.Name = "gbTheme";
            this.gbTheme.Padding = new System.Windows.Forms.Padding(8);
            this.gbTheme.Size = new System.Drawing.Size(1415, 120);
            this.gbTheme.TabIndex = 3;
            this.gbTheme.TabStop = false;
            this.gbTheme.Text = "Theme";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.chkUseSystemVisualStyle, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.chkColorblind, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.sbOpenThemeFolder, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_cbSelectTheme, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblRestartNeeded, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 21);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1399, 91);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // chkUseSystemVisualStyle
            // 
            this.chkUseSystemVisualStyle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseSystemVisualStyle.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkUseSystemVisualStyle, 2);
            this.chkUseSystemVisualStyle.Location = new System.Drawing.Point(3, 71);
            this.chkUseSystemVisualStyle.Name = "chkUseSystemVisualStyle";
            this.chkUseSystemVisualStyle.Size = new System.Drawing.Size(304, 17);
            this.chkUseSystemVisualStyle.TabIndex = 4;
            this.chkUseSystemVisualStyle.Text = "Use system-defined visual style (looks bad with dark colors)";
            this.chkUseSystemVisualStyle.UseVisualStyleBackColor = true;
            // 
            // chkColorblind
            // 
            this.chkColorblind.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkColorblind.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkColorblind, 2);
            this.chkColorblind.Location = new System.Drawing.Point(3, 48);
            this.chkColorblind.Name = "chkColorblind";
            this.chkColorblind.Size = new System.Drawing.Size(72, 17);
            this.chkColorblind.TabIndex = 6;
            this.chkColorblind.Text = "Colorblind";
            this.chkColorblind.UseVisualStyleBackColor = true;
            // 
            // sbOpenThemeFolder
            // 
            this.sbOpenThemeFolder.ContextMenuStrip = this.cmsOpenThemeFolders;
            this.sbOpenThemeFolder.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.sbOpenThemeFolder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.sbOpenThemeFolder.Location = new System.Drawing.Point(222, 19);
            this.sbOpenThemeFolder.Name = "sbOpenThemeFolder";
            this.sbOpenThemeFolder.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.sbOpenThemeFolder.Size = new System.Drawing.Size(162, 23);
            this.sbOpenThemeFolder.SplitMenuStrip = this.cmsOpenThemeFolders;
            this.sbOpenThemeFolder.TabIndex = 7;
            this.sbOpenThemeFolder.Text = "Open theme folder";
            this.sbOpenThemeFolder.UseVisualStyleBackColor = true;
            this.sbOpenThemeFolder.WholeButtonDropdown = true;
            // 
            // cmsOpenThemeFolders
            // 
            this.cmsOpenThemeFolders.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiApplicationFolder,
            this.tsmiUserFolder});
            this.cmsOpenThemeFolders.Name = "cmsOpenThemeFolders";
            this.cmsOpenThemeFolders.Size = new System.Drawing.Size(170, 48);
            // 
            // tsmiApplicationFolder
            // 
            this.tsmiApplicationFolder.Name = "tsmiApplicationFolder";
            this.tsmiApplicationFolder.Size = new System.Drawing.Size(169, 22);
            this.tsmiApplicationFolder.Text = "Application folder";
            this.tsmiApplicationFolder.Click += new System.EventHandler(this.tsmiApplicationFolder_Click);
            // 
            // tsmiUserFolder
            // 
            this.tsmiUserFolder.Name = "tsmiUserFolder";
            this.tsmiUserFolder.Size = new System.Drawing.Size(169, 22);
            this.tsmiUserFolder.Text = "User folder";
            this.tsmiUserFolder.Click += new System.EventHandler(this.tsmiUserFolder_Click);
            // 
            // _NO_TRANSLATE_cbSelectTheme
            // 
            this._NO_TRANSLATE_cbSelectTheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_cbSelectTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_cbSelectTheme.FormattingEnabled = true;
            this._NO_TRANSLATE_cbSelectTheme.Location = new System.Drawing.Point(3, 20);
            this._NO_TRANSLATE_cbSelectTheme.Name = "_NO_TRANSLATE_cbSelectTheme";
            this._NO_TRANSLATE_cbSelectTheme.Size = new System.Drawing.Size(213, 21);
            this._NO_TRANSLATE_cbSelectTheme.TabIndex = 0;
            // 
            // lblRestartNeeded
            // 
            this.lblRestartNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.lblRestartNeeded, 2);
            this.lblRestartNeeded.Image = global::GitUI.Properties.Images.Warning;
            this.lblRestartNeeded.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRestartNeeded.Location = new System.Drawing.Point(3, 0);
            this.lblRestartNeeded.Name = "lblRestartNeeded";
            this.lblRestartNeeded.Size = new System.Drawing.Size(200, 16);
            this.lblRestartNeeded.TabIndex = 5;
            this.lblRestartNeeded.Text = "Restart required to apply changes";
            this.lblRestartNeeded.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ColorsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tlpnlMain);
            this.Name = "ColorsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1437, 444);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.gbRevisionGraph.ResumeLayout(false);
            this.gbRevisionGraph.PerformLayout();
            this.tlpnlRevisionGraph.ResumeLayout(false);
            this.tlpnlRevisionGraph.PerformLayout();
            this.gbTheme.ResumeLayout(false);
            this.gbTheme.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.cmsOpenThemeFolders.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox gbRevisionGraph;
        private System.Windows.Forms.CheckBox DrawNonRelativesTextGray;
        private System.Windows.Forms.CheckBox DrawNonRelativesGray;
        private System.Windows.Forms.CheckBox MulticolorBranches;
        private System.Windows.Forms.CheckBox chkDrawAlternateBackColor;
        private System.Windows.Forms.TableLayoutPanel tlpnlRevisionGraph;
        private System.Windows.Forms.CheckBox chkHighlightAuthored;
        private System.Windows.Forms.GroupBox gbTheme;
        private System.Windows.Forms.ContextMenuStrip cmsOpenThemeFolders;
        private System.Windows.Forms.ToolStripMenuItem tsmiApplicationFolder;
        private System.Windows.Forms.ToolStripMenuItem tsmiUserFolder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkUseSystemVisualStyle;
        private System.Windows.Forms.CheckBox chkColorblind;
        private Script.SplitButton sbOpenThemeFolder;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cbSelectTheme;
        private System.Windows.Forms.Label lblRestartNeeded;
    }
}
