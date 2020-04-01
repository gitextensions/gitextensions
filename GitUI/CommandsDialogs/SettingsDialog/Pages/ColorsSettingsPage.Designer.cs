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
            this.chkHighlightAuthored = new System.Windows.Forms.CheckBox();
            this.MulticolorBranches = new System.Windows.Forms.CheckBox();
            this.chkDrawAlternateBackColor = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesTextGray = new System.Windows.Forms.CheckBox();
            this.DrawNonRelativesGray = new System.Windows.Forms.CheckBox();
            this.gbTheme = new System.Windows.Forms.GroupBox();
            this.fpnlTheme = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRestartNeeded = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_cbSelectTheme = new System.Windows.Forms.ComboBox();
            this.chkUseSystemVisualStyle = new System.Windows.Forms.CheckBox();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.gbRevisionGraph.SuspendLayout();
            this.tlpnlRevisionGraph.SuspendLayout();
            this.gbTheme.SuspendLayout();
            this.fpnlTheme.SuspendLayout();
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
            tlpnlMain.Size = new System.Drawing.Size(1004, 612);
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
            this.gbRevisionGraph.Size = new System.Drawing.Size(998, 157);
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
            this.tlpnlRevisionGraph.Controls.Add(this.chkHighlightAuthored, 0, 4);
            this.tlpnlRevisionGraph.Controls.Add(this.MulticolorBranches, 0, 0);
            this.tlpnlRevisionGraph.Controls.Add(this.chkDrawAlternateBackColor, 0, 1);
            this.tlpnlRevisionGraph.Controls.Add(this.DrawNonRelativesTextGray, 0, 3);
            this.tlpnlRevisionGraph.Controls.Add(this.DrawNonRelativesGray, 0, 2);
            this.tlpnlRevisionGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlRevisionGraph.Location = new System.Drawing.Point(8, 24);
            this.tlpnlRevisionGraph.Name = "tlpnlRevisionGraph";
            this.tlpnlRevisionGraph.RowCount = 5;
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlRevisionGraph.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlRevisionGraph.Size = new System.Drawing.Size(982, 125);
            this.tlpnlRevisionGraph.TabIndex = 0;
            // 
            // chkHighlightAuthored
            // 
            this.chkHighlightAuthored.AutoSize = true;
            this.chkHighlightAuthored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkHighlightAuthored.Location = new System.Drawing.Point(3, 103);
            this.chkHighlightAuthored.Name = "chkHighlightAuthored";
            this.chkHighlightAuthored.Size = new System.Drawing.Size(183, 19);
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
            this.MulticolorBranches.Size = new System.Drawing.Size(183, 19);
            this.MulticolorBranches.TabIndex = 0;
            this.MulticolorBranches.Text = "Multicolor branches";
            this.MulticolorBranches.UseVisualStyleBackColor = true;
            // 
            // chkDrawAlternateBackColor
            // 
            this.chkDrawAlternateBackColor.AutoSize = true;
            this.chkDrawAlternateBackColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDrawAlternateBackColor.Location = new System.Drawing.Point(3, 28);
            this.chkDrawAlternateBackColor.Name = "chkDrawAlternateBackColor";
            this.chkDrawAlternateBackColor.Size = new System.Drawing.Size(183, 19);
            this.chkDrawAlternateBackColor.TabIndex = 2;
            this.chkDrawAlternateBackColor.Text = "Draw alternate background";
            this.chkDrawAlternateBackColor.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesTextGray
            // 
            this.DrawNonRelativesTextGray.AutoSize = true;
            this.DrawNonRelativesTextGray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawNonRelativesTextGray.Location = new System.Drawing.Point(3, 78);
            this.DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            this.DrawNonRelativesTextGray.Size = new System.Drawing.Size(183, 19);
            this.DrawNonRelativesTextGray.TabIndex = 4;
            this.DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            this.DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            this.DrawNonRelativesGray.AutoSize = true;
            this.DrawNonRelativesGray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawNonRelativesGray.Location = new System.Drawing.Point(3, 53);
            this.DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            this.DrawNonRelativesGray.Size = new System.Drawing.Size(183, 19);
            this.DrawNonRelativesGray.TabIndex = 3;
            this.DrawNonRelativesGray.Text = "Draw non relatives graph gray";
            this.DrawNonRelativesGray.UseVisualStyleBackColor = true;
            // 
            // gbTheme
            // 
            this.gbTheme.AutoSize = true;
            this.gbTheme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbTheme.Controls.Add(this.fpnlTheme);
            this.gbTheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTheme.Location = new System.Drawing.Point(3, 166);
            this.gbTheme.Name = "gbTheme";
            this.gbTheme.Size = new System.Drawing.Size(998, 76);
            this.gbTheme.TabIndex = 3;
            this.gbTheme.TabStop = false;
            this.gbTheme.Text = "Theme";
            // 
            // fpnlTheme
            // 
            this.fpnlTheme.AutoSize = true;
            this.fpnlTheme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.fpnlTheme.Controls.Add(this.lblRestartNeeded);
            this.fpnlTheme.Controls.Add(this._NO_TRANSLATE_cbSelectTheme);
            this.fpnlTheme.Controls.Add(this.chkUseSystemVisualStyle);
            this.fpnlTheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fpnlTheme.Location = new System.Drawing.Point(3, 19);
            this.fpnlTheme.Name = "fpnlTheme";
            this.fpnlTheme.Size = new System.Drawing.Size(992, 54);
            this.fpnlTheme.TabIndex = 0;
            // 
            // lblRestartNeeded
            // 
            this.lblRestartNeeded.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.fpnlTheme.SetFlowBreak(this.lblRestartNeeded, true);
            this.lblRestartNeeded.Image = global::GitUI.Properties.Images.Warning;
            this.lblRestartNeeded.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRestartNeeded.Location = new System.Drawing.Point(3, 5);
            this.lblRestartNeeded.Name = "lblRestartNeeded";
            this.lblRestartNeeded.Size = new System.Drawing.Size(221, 16);
            this.lblRestartNeeded.TabIndex = 5;
            this.lblRestartNeeded.Text = "Restart required to apply changes";
            this.lblRestartNeeded.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _NO_TRANSLATE_cbSelectTheme
            // 
            this._NO_TRANSLATE_cbSelectTheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_cbSelectTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_cbSelectTheme.FormattingEnabled = true;
            this._NO_TRANSLATE_cbSelectTheme.Location = new System.Drawing.Point(3, 30);
            this._NO_TRANSLATE_cbSelectTheme.Name = "_NO_TRANSLATE_cbSelectTheme";
            this._NO_TRANSLATE_cbSelectTheme.Size = new System.Drawing.Size(179, 23);
            this._NO_TRANSLATE_cbSelectTheme.TabIndex = 0;
            this._NO_TRANSLATE_cbSelectTheme.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTheme_SelectedIndexChanged);
            // 
            // chkUseSystemVisualStyle
            // 
            this.chkUseSystemVisualStyle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUseSystemVisualStyle.AutoSize = true;
            this.chkUseSystemVisualStyle.Location = new System.Drawing.Point(188, 31);
            this.chkUseSystemVisualStyle.Name = "chkUseSystemVisualStyle";
            this.chkUseSystemVisualStyle.Size = new System.Drawing.Size(339, 19);
            this.chkUseSystemVisualStyle.TabIndex = 4;
            this.chkUseSystemVisualStyle.Text = "Use system-defined visual style (looks bad with dark colors)";
            this.chkUseSystemVisualStyle.UseVisualStyleBackColor = true;
            this.chkUseSystemVisualStyle.CheckedChanged += new System.EventHandler(this.ChkUseSystemVisualStyle_CheckedChanged);
            // 
            // ColorsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tlpnlMain);
            this.Name = "ColorsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1020, 628);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.gbRevisionGraph.ResumeLayout(false);
            this.gbRevisionGraph.PerformLayout();
            this.tlpnlRevisionGraph.ResumeLayout(false);
            this.tlpnlRevisionGraph.PerformLayout();
            this.gbTheme.ResumeLayout(false);
            this.gbTheme.PerformLayout();
            this.fpnlTheme.ResumeLayout(false);
            this.fpnlTheme.PerformLayout();
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
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cbSelectTheme;
        private System.Windows.Forms.FlowLayoutPanel fpnlTheme;
        private System.Windows.Forms.CheckBox chkUseSystemVisualStyle;
        private System.Windows.Forms.Label lblRestartNeeded;
    }
}
