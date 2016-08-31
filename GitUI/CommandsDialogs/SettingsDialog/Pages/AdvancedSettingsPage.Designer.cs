﻿namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class AdvancedSettingsPage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettingsPage));
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.DiffViewerGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelForDiffViewer = new System.Windows.Forms.TableLayoutPanel();
            this.chkOmitUninterestingDiff = new System.Windows.Forms.CheckBox();
            this.chkRememberIgnoreWhiteSpacePreference = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanelForWarnDiffFiles = new System.Windows.Forms.TableLayoutPanel();
            this.chkCheckForDiffViewerSelectedFilesLimitNumber = new System.Windows.Forms.CheckBox();
            this.upDownDiffViewerSelectedFilesLimitNumber = new System.Windows.Forms.NumericUpDown();
            this.CheckoutGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAlwaysShowCheckoutDlg = new System.Windows.Forms.CheckBox();
            this.chkUseLocalChangesAction = new System.Windows.Forms.CheckBox();
            this.GeneralGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkCheckForRCVersions = new System.Windows.Forms.CheckBox();
            this.chkAlwaysShowAdvOpt = new System.Windows.Forms.CheckBox();
            this.chkDontSHowHelpImages = new System.Windows.Forms.CheckBox();
            this.chkConsoleEmulator = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAutoNormaliseBranchName = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cboAutoNormaliseSymbol = new System.Windows.Forms.ComboBox();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel2.SuspendLayout();
            this.DiffViewerGB.SuspendLayout();
            this.tableLayoutPanelForDiffViewer.SuspendLayout();
            this.tableLayoutPanelForWarnDiffFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownDiffViewerSelectedFilesLimitNumber)).BeginInit();
            this.CheckoutGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.GeneralGB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.DiffViewerGB, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.CheckoutGB, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.GeneralGB, 0, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 12);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(399, 404);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // DiffViewerGB
            // 
            this.DiffViewerGB.AutoSize = true;
            this.DiffViewerGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DiffViewerGB.Controls.Add(this.tableLayoutPanelForDiffViewer);
            this.DiffViewerGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.DiffViewerGB.Location = new System.Drawing.Point(3, 283);
            this.DiffViewerGB.Name = "DiffViewerGB";
            this.DiffViewerGB.Size = new System.Drawing.Size(393, 118);
            this.DiffViewerGB.TabIndex = 2;
            this.DiffViewerGB.TabStop = false;
            this.DiffViewerGB.Text = "Diff Viewer";
            // 
            // tableLayoutPanelForDiffViewer
            // 
            this.tableLayoutPanelForDiffViewer.AutoSize = true;
            this.tableLayoutPanelForDiffViewer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelForDiffViewer.ColumnCount = 1;
            this.tableLayoutPanelForDiffViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkOmitUninterestingDiff, 0, 0);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkRememberIgnoreWhiteSpacePreference, 0, 0);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.tableLayoutPanelForWarnDiffFiles, 0, 3);
            this.tableLayoutPanelForDiffViewer.Location = new System.Drawing.Point(6, 20);
            this.tableLayoutPanelForDiffViewer.Name = "tableLayoutPanelForDiffViewer";
            this.tableLayoutPanelForDiffViewer.RowCount = 1;
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.Size = new System.Drawing.Size(255, 78);
            this.tableLayoutPanelForDiffViewer.TabIndex = 1;
            // 
            // chkOmitUninterestingDiff
            // 
            this.chkOmitUninterestingDiff.AutoSize = true;
            this.chkOmitUninterestingDiff.Location = new System.Drawing.Point(3, 26);
            this.chkOmitUninterestingDiff.Name = "chkOmitUninterestingDiff";
            this.chkOmitUninterestingDiff.Size = new System.Drawing.Size(249, 17);
            this.chkOmitUninterestingDiff.TabIndex = 6;
            this.chkOmitUninterestingDiff.Text = "Omit uninteresting changes from combined diff";
            this.chkOmitUninterestingDiff.UseVisualStyleBackColor = true;
            // 
            // chkRememberIgnoreWhiteSpacePreference
            // 
            this.chkRememberIgnoreWhiteSpacePreference.AutoSize = true;
            this.chkRememberIgnoreWhiteSpacePreference.Location = new System.Drawing.Point(3, 3);
            this.chkRememberIgnoreWhiteSpacePreference.Name = "chkRememberIgnoreWhiteSpacePreference";
            this.chkRememberIgnoreWhiteSpacePreference.Size = new System.Drawing.Size(247, 17);
            this.chkRememberIgnoreWhiteSpacePreference.TabIndex = 4;
            this.chkRememberIgnoreWhiteSpacePreference.Text = "Remember the ignore-white-space preference";
            this.chkRememberIgnoreWhiteSpacePreference.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelForWarnDiffFiles
            // 
            this.tableLayoutPanelForWarnDiffFiles.ColumnCount = 2;
            this.tableLayoutPanelForWarnDiffFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForWarnDiffFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForWarnDiffFiles.Controls.Add(this.chkCheckForDiffViewerSelectedFilesLimitNumber, 0, 0);
            this.tableLayoutPanelForWarnDiffFiles.Controls.Add(this.upDownDiffViewerSelectedFilesLimitNumber, 1, 0);
            this.tableLayoutPanelForWarnDiffFiles.Location = new System.Drawing.Point(3, 49);
            this.tableLayoutPanelForWarnDiffFiles.Name = "tableLayoutPanelForWarnDiffFiles";
            this.tableLayoutPanelForWarnDiffFiles.RowCount = 1;
            this.tableLayoutPanelForWarnDiffFiles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForWarnDiffFiles.Size = new System.Drawing.Size(247, 26);
            this.tableLayoutPanelForWarnDiffFiles.TabIndex = 7;
            // 
            // chkCheckForDiffViewerSelectedFilesLimitNumber
            // 
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.AutoSize = true;
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Location = new System.Drawing.Point(0, 3);
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Name = "chkCheckForDiffViewerSelectedFilesLimitNumber";
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Size = new System.Drawing.Size(189, 17);
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.TabIndex = 0;
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Text = "Warn if selected files exceed:       ";
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.UseVisualStyleBackColor = true;
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.CheckedChanged += new System.EventHandler(this.chkCheckForDiffViewerSelectedFilesLimitNumber_CheckedChanged);
            // 
            // upDownDiffViewerSelectedFilesLimitNumber
            // 
            this.upDownDiffViewerSelectedFilesLimitNumber.Location = new System.Drawing.Point(195, 3);
            this.upDownDiffViewerSelectedFilesLimitNumber.Name = "upDownDiffViewerSelectedFilesLimitNumber";
            this.upDownDiffViewerSelectedFilesLimitNumber.Size = new System.Drawing.Size(48, 21);
            this.upDownDiffViewerSelectedFilesLimitNumber.TabIndex = 1;
            this.upDownDiffViewerSelectedFilesLimitNumber.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // CheckoutGB
            // 
            this.CheckoutGB.AutoSize = true;
            this.CheckoutGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CheckoutGB.Controls.Add(this.tableLayoutPanel3);
            this.CheckoutGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckoutGB.Location = new System.Drawing.Point(3, 3);
            this.CheckoutGB.Name = "CheckoutGB";
            this.CheckoutGB.Size = new System.Drawing.Size(393, 100);
            this.CheckoutGB.TabIndex = 0;
            this.CheckoutGB.TabStop = false;
            this.CheckoutGB.Text = "Checkout";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.chkAlwaysShowCheckoutDlg, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.chkUseLocalChangesAction, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(9, 21);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(378, 59);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkAlwaysShowCheckoutDlg
            // 
            this.chkAlwaysShowCheckoutDlg.AutoSize = true;
            this.chkAlwaysShowCheckoutDlg.Location = new System.Drawing.Point(3, 3);
            this.chkAlwaysShowCheckoutDlg.Name = "chkAlwaysShowCheckoutDlg";
            this.chkAlwaysShowCheckoutDlg.Size = new System.Drawing.Size(165, 17);
            this.chkAlwaysShowCheckoutDlg.TabIndex = 0;
            this.chkAlwaysShowCheckoutDlg.Text = "Always show checkout dialog";
            this.chkAlwaysShowCheckoutDlg.UseVisualStyleBackColor = true;
            // 
            // chkUseLocalChangesAction
            // 
            this.chkUseLocalChangesAction.AutoSize = true;
            this.chkUseLocalChangesAction.Location = new System.Drawing.Point(3, 26);
            this.chkUseLocalChangesAction.Name = "chkUseLocalChangesAction";
            this.chkUseLocalChangesAction.Size = new System.Drawing.Size(372, 30);
            this.chkUseLocalChangesAction.TabIndex = 1;
            this.chkUseLocalChangesAction.Text = "Use last chosen \"local changes\" action as default action.\r\nThis action will be pe" +
    "rformed without warning while checking out branch.";
            this.chkUseLocalChangesAction.UseVisualStyleBackColor = true;
            // 
            // GeneralGB
            // 
            this.GeneralGB.AutoSize = true;
            this.GeneralGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GeneralGB.Controls.Add(this.tableLayoutPanel1);
            this.GeneralGB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GeneralGB.Location = new System.Drawing.Point(3, 109);
            this.GeneralGB.Name = "GeneralGB";
            this.GeneralGB.Size = new System.Drawing.Size(393, 168);
            this.GeneralGB.TabIndex = 1;
            this.GeneralGB.TabStop = false;
            this.GeneralGB.Text = "General";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chkCheckForRCVersions, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkAlwaysShowAdvOpt, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkDontSHowHelpImages, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkConsoleEmulator, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(387, 148);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chkCheckForRCVersions
            // 
            this.chkCheckForRCVersions.AutoSize = true;
            this.chkCheckForRCVersions.Location = new System.Drawing.Point(3, 49);
            this.chkCheckForRCVersions.Name = "chkCheckForRCVersions";
            this.chkCheckForRCVersions.Size = new System.Drawing.Size(203, 17);
            this.chkCheckForRCVersions.TabIndex = 3;
            this.chkCheckForRCVersions.Text = "Check for release candidate versions";
            this.chkCheckForRCVersions.UseVisualStyleBackColor = true;
            // 
            // chkAlwaysShowAdvOpt
            // 
            this.chkAlwaysShowAdvOpt.AutoSize = true;
            this.chkAlwaysShowAdvOpt.Location = new System.Drawing.Point(3, 26);
            this.chkAlwaysShowAdvOpt.Name = "chkAlwaysShowAdvOpt";
            this.chkAlwaysShowAdvOpt.Size = new System.Drawing.Size(176, 17);
            this.chkAlwaysShowAdvOpt.TabIndex = 2;
            this.chkAlwaysShowAdvOpt.Text = "Always show advanced options";
            this.chkAlwaysShowAdvOpt.UseVisualStyleBackColor = true;
            // 
            // chkDontSHowHelpImages
            // 
            this.chkDontSHowHelpImages.AutoSize = true;
            this.chkDontSHowHelpImages.Location = new System.Drawing.Point(3, 3);
            this.chkDontSHowHelpImages.Name = "chkDontSHowHelpImages";
            this.chkDontSHowHelpImages.Size = new System.Drawing.Size(138, 17);
            this.chkDontSHowHelpImages.TabIndex = 1;
            this.chkDontSHowHelpImages.Text = "Don\'t show help images";
            this.chkDontSHowHelpImages.UseVisualStyleBackColor = true;
            // 
            // chkConsoleEmulator
            // 
            this.chkConsoleEmulator.AutoSize = true;
            this.chkConsoleEmulator.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkConsoleEmulator.Location = new System.Drawing.Point(3, 72);
            this.chkConsoleEmulator.Name = "chkConsoleEmulator";
            this.chkConsoleEmulator.Size = new System.Drawing.Size(381, 17);
            this.chkConsoleEmulator.TabIndex = 4;
            this.chkConsoleEmulator.Text = "Use Console Emulator for console output in command dialogs";
            this.tooltip.SetToolTip(this.chkConsoleEmulator, resources.GetString("chkConsoleEmulator.ToolTip"));
            this.chkConsoleEmulator.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.chkAutoNormaliseBranchName, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.cboAutoNormaliseSymbol, 1, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 95);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(381, 50);
            this.tableLayoutPanel4.TabIndex = 5;
            // 
            // chkAutoNormaliseBranchName
            // 
            this.chkAutoNormaliseBranchName.AutoSize = true;
            this.tableLayoutPanel4.SetColumnSpan(this.chkAutoNormaliseBranchName, 2);
            this.chkAutoNormaliseBranchName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkAutoNormaliseBranchName.Location = new System.Drawing.Point(3, 3);
            this.chkAutoNormaliseBranchName.Name = "chkAutoNormaliseBranchName";
            this.chkAutoNormaliseBranchName.Size = new System.Drawing.Size(375, 17);
            this.chkAutoNormaliseBranchName.TabIndex = 6;
            this.chkAutoNormaliseBranchName.Text = "Auto normalise branch name";
            this.tooltip.SetToolTip(this.chkAutoNormaliseBranchName, resources.GetString("chkAutoNormaliseBranchName.ToolTip"));
            this.chkAutoNormaliseBranchName.UseVisualStyleBackColor = true;
            this.chkAutoNormaliseBranchName.CheckedChanged += new System.EventHandler(this.chkAutoNormaliseBranchName_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 27);
            this.label1.TabIndex = 7;
            this.label1.Text = "Symbol to use:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboAutoNormaliseSymbol
            // 
            this.cboAutoNormaliseSymbol.DisplayMember = "Key";
            this.cboAutoNormaliseSymbol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAutoNormaliseSymbol.Enabled = false;
            this.cboAutoNormaliseSymbol.FormattingEnabled = true;
            this.cboAutoNormaliseSymbol.Location = new System.Drawing.Point(109, 26);
            this.cboAutoNormaliseSymbol.Name = "cboAutoNormaliseSymbol";
            this.cboAutoNormaliseSymbol.Size = new System.Drawing.Size(81, 21);
            this.cboAutoNormaliseSymbol.TabIndex = 8;
            this.cboAutoNormaliseSymbol.ValueMember = "Value";
            // 
            // tooltip
            // 
            this.tooltip.AutoPopDelay = 30000;
            this.tooltip.InitialDelay = 500;
            this.tooltip.ReshowDelay = 100;
            // 
            // AdvancedSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "AdvancedSettingsPage";
            this.Size = new System.Drawing.Size(1032, 484);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.DiffViewerGB.ResumeLayout(false);
            this.DiffViewerGB.PerformLayout();
            this.tableLayoutPanelForDiffViewer.ResumeLayout(false);
            this.tableLayoutPanelForDiffViewer.PerformLayout();
            this.tableLayoutPanelForWarnDiffFiles.ResumeLayout(false);
            this.tableLayoutPanelForWarnDiffFiles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownDiffViewerSelectedFilesLimitNumber)).EndInit();
            this.CheckoutGB.ResumeLayout(false);
            this.CheckoutGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.GeneralGB.ResumeLayout(false);
            this.GeneralGB.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox CheckoutGB;
        private System.Windows.Forms.GroupBox GeneralGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkDontSHowHelpImages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox chkAlwaysShowCheckoutDlg;
        private System.Windows.Forms.CheckBox chkUseLocalChangesAction;
        private System.Windows.Forms.CheckBox chkAlwaysShowAdvOpt;
        private System.Windows.Forms.CheckBox chkCheckForRCVersions;
        private System.Windows.Forms.GroupBox DiffViewerGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForDiffViewer;
        private System.Windows.Forms.CheckBox chkRememberIgnoreWhiteSpacePreference;
        private System.Windows.Forms.CheckBox chkOmitUninterestingDiff;
        private System.Windows.Forms.CheckBox chkConsoleEmulator;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.CheckBox chkAutoNormaliseBranchName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboAutoNormaliseSymbol;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForWarnDiffFiles;
        private System.Windows.Forms.CheckBox chkCheckForDiffViewerSelectedFilesLimitNumber;
        private System.Windows.Forms.NumericUpDown upDownDiffViewerSelectedFilesLimitNumber;
    }
}
