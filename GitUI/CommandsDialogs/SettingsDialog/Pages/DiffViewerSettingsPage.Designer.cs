namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class DiffViewerSettingsPage
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
            this.tlpnlGeneral = new System.Windows.Forms.TableLayoutPanel();
            this.chkRememberIgnoreWhiteSpacePreference = new System.Windows.Forms.CheckBox();
            this.chkRememberShowNonPrintingCharsPreference = new System.Windows.Forms.CheckBox();
            this.chkRememberShowEntireFilePreference = new System.Windows.Forms.CheckBox();
            this.chkRememberNumberOfContextLines = new System.Windows.Forms.CheckBox();
            this.chkRememberShowSyntaxHighlightingInDiff = new System.Windows.Forms.CheckBox();
            this.chkOmitUninterestingDiff = new System.Windows.Forms.CheckBox();
            this.chkContScrollToNextFileOnlyWithAlt = new System.Windows.Forms.CheckBox();
            this.chkOpenSubmoduleDiffInSeparateWindow = new System.Windows.Forms.CheckBox();
            this.chkShowDiffForAllParents = new GitUI.UserControls.Settings.SettingsCheckBox();
            this.chkShowAllCustomDiffTools = new GitUI.UserControls.Settings.SettingsCheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.VerticalRulerPosition = new System.Windows.Forms.NumericUpDown();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.tlpnlGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VerticalRulerPosition)).BeginInit();
            this.tlpnlMain.SuspendLayout();
            this.gbGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpnlGeneral
            // 
            this.tlpnlGeneral.AutoSize = true;
            this.tlpnlGeneral.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlGeneral.ColumnCount = 3;
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlGeneral.Controls.Add(this.chkRememberIgnoreWhiteSpacePreference, 0, 0);
            this.tlpnlGeneral.Controls.Add(this.chkRememberShowNonPrintingCharsPreference, 0, 1);
            this.tlpnlGeneral.Controls.Add(this.chkRememberShowEntireFilePreference, 0, 2);
            this.tlpnlGeneral.Controls.Add(this.chkRememberNumberOfContextLines, 0, 3);
            this.tlpnlGeneral.Controls.Add(this.chkRememberShowSyntaxHighlightingInDiff, 0, 4);
            this.tlpnlGeneral.Controls.Add(this.chkOmitUninterestingDiff, 0, 5);
            this.tlpnlGeneral.Controls.Add(this.chkContScrollToNextFileOnlyWithAlt, 0, 6);
            this.tlpnlGeneral.Controls.Add(this.chkOpenSubmoduleDiffInSeparateWindow, 0, 7);
            this.tlpnlGeneral.Controls.Add(this.chkShowDiffForAllParents, 0, 8);
            this.tlpnlGeneral.Controls.Add(this.chkShowAllCustomDiffTools, 0, 9);
            this.tlpnlGeneral.Controls.Add(this.label1, 0, 10);
            this.tlpnlGeneral.Controls.Add(this.VerticalRulerPosition, 1, 10);
            this.tlpnlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlGeneral.Location = new System.Drawing.Point(8, 24);
            this.tlpnlGeneral.Name = "tlpnlGeneral";
            this.tlpnlGeneral.RowCount = 11;
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlGeneral.Size = new System.Drawing.Size(1725, 279);
            // 
            // chkRememberIgnoreWhiteSpacePreference
            // 
            this.chkRememberIgnoreWhiteSpacePreference.AutoSize = true;
            this.chkRememberIgnoreWhiteSpacePreference.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRememberIgnoreWhiteSpacePreference.Location = new System.Drawing.Point(3, 3);
            this.chkRememberIgnoreWhiteSpacePreference.Name = "chkRememberIgnoreWhiteSpacePreference";
            this.chkRememberIgnoreWhiteSpacePreference.Size = new System.Drawing.Size(325, 19);            this.chkRememberIgnoreWhiteSpacePreference.Text = "Remember the \'Ignore whitespaces\' preference";
            this.chkRememberIgnoreWhiteSpacePreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowNonPrintingCharsPreference
            // 
            this.chkRememberShowNonPrintingCharsPreference.AutoSize = true;
            this.chkRememberShowNonPrintingCharsPreference.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRememberShowNonPrintingCharsPreference.Location = new System.Drawing.Point(3, 28);
            this.chkRememberShowNonPrintingCharsPreference.Name = "chkRememberShowNonPrintingCharsPreference";
            this.chkRememberShowNonPrintingCharsPreference.Size = new System.Drawing.Size(325, 19);
            this.chkRememberShowNonPrintingCharsPreference.Text = "Remember the \'Show nonprinting characters\' preference";
            this.chkRememberShowNonPrintingCharsPreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowEntireFilePreference
            // 
            this.chkRememberShowEntireFilePreference.AutoSize = true;
            this.chkRememberShowEntireFilePreference.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRememberShowEntireFilePreference.Location = new System.Drawing.Point(3, 53);
            this.chkRememberShowEntireFilePreference.Name = "chkRememberShowEntireFilePreference";
            this.chkRememberShowEntireFilePreference.Size = new System.Drawing.Size(325, 19);
            this.chkRememberShowEntireFilePreference.Text = "Remember the \'Show entire file\' preference";
            this.chkRememberShowEntireFilePreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberNumberOfContextLines
            // 
            this.chkRememberNumberOfContextLines.AutoSize = true;
            this.chkRememberNumberOfContextLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRememberNumberOfContextLines.Location = new System.Drawing.Point(3, 78);
            this.chkRememberNumberOfContextLines.Name = "chkRememberNumberOfContextLines";
            this.chkRememberNumberOfContextLines.Size = new System.Drawing.Size(325, 19);
            this.chkRememberNumberOfContextLines.Text = "Remember the \'Number of context lines\' preference";
            this.chkRememberNumberOfContextLines.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowSyntaxHighlightingInDiff
            // 
            this.chkRememberShowSyntaxHighlightingInDiff.AutoSize = true;
            this.chkRememberShowSyntaxHighlightingInDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRememberShowSyntaxHighlightingInDiff.Location = new System.Drawing.Point(3, 103);
            this.chkRememberShowSyntaxHighlightingInDiff.Name = "chkRememberShowSyntaxHighlightingInDiff";
            this.chkRememberShowSyntaxHighlightingInDiff.Size = new System.Drawing.Size(325, 19);
            this.chkRememberShowSyntaxHighlightingInDiff.Text = "Remember the \'Show syntax highlighting\' preference";
            this.chkRememberShowSyntaxHighlightingInDiff.UseVisualStyleBackColor = true;
            // 
            // chkOmitUninterestingDiff
            // 
            this.chkOmitUninterestingDiff.AutoSize = true;
            this.chkOmitUninterestingDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkOmitUninterestingDiff.Location = new System.Drawing.Point(3, 128);
            this.chkOmitUninterestingDiff.Name = "chkOmitUninterestingDiff";
            this.chkOmitUninterestingDiff.Size = new System.Drawing.Size(325, 19);
            this.chkOmitUninterestingDiff.Text = "Omit uninteresting changes from combined diff";
            this.chkOmitUninterestingDiff.UseVisualStyleBackColor = true;
            // 
            // chkContScrollToNextFileOnlyWithAlt
            // 
            this.chkContScrollToNextFileOnlyWithAlt.AutoSize = true;
            this.chkContScrollToNextFileOnlyWithAlt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkContScrollToNextFileOnlyWithAlt.Location = new System.Drawing.Point(3, 153);
            this.chkContScrollToNextFileOnlyWithAlt.Name = "chkContScrollToNextFileOnlyWithAlt";
            this.chkContScrollToNextFileOnlyWithAlt.Size = new System.Drawing.Size(325, 19);
            this.chkContScrollToNextFileOnlyWithAlt.Text = "Enable automatic continuous scroll (without ALT button)";
            this.chkContScrollToNextFileOnlyWithAlt.UseVisualStyleBackColor = true;
            // 
            // chkOpenSubmoduleDiffInSeparateWindow
            // 
            this.chkOpenSubmoduleDiffInSeparateWindow.AutoSize = true;
            this.chkOpenSubmoduleDiffInSeparateWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkOpenSubmoduleDiffInSeparateWindow.Location = new System.Drawing.Point(3, 178);
            this.chkOpenSubmoduleDiffInSeparateWindow.Name = "chkOpenSubmoduleDiffInSeparateWindow";
            this.chkOpenSubmoduleDiffInSeparateWindow.Size = new System.Drawing.Size(325, 19);
            this.chkOpenSubmoduleDiffInSeparateWindow.Text = "Open Submodule Diff in separate window";
            this.chkOpenSubmoduleDiffInSeparateWindow.UseVisualStyleBackColor = true;
            // 
            // chkShowDiffForAllParents
            // 
            this.chkShowDiffForAllParents.AutoSize = true;
            this.chkShowDiffForAllParents.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.chkShowDiffForAllParents.Checked = false;
            this.chkShowDiffForAllParents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowDiffForAllParents.Location = new System.Drawing.Point(3, 203);
            this.chkShowDiffForAllParents.Name = "chkShowDiffForAllParents";
            this.chkShowDiffForAllParents.Size = new System.Drawing.Size(325, 19);
            this.chkShowDiffForAllParents.Text = "Show file differences for all parents in browse dialog";
            this.chkShowDiffForAllParents.ToolTipText = null;
            // 
            // chkShowAllCustomDiffTools
            // 
            this.chkShowAllCustomDiffTools.AutoSize = true;
            this.chkShowAllCustomDiffTools.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.chkShowAllCustomDiffTools.Checked = false;
            this.chkShowAllCustomDiffTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowAllCustomDiffTools.Location = new System.Drawing.Point(3, 228);
            this.chkShowAllCustomDiffTools.Name = "chkShowAllCustomDiffTools";
            this.chkShowAllCustomDiffTools.Size = new System.Drawing.Size(325, 19);
            this.chkShowAllCustomDiffTools.Text = "Show all available difftools";
            this.chkShowAllCustomDiffTools.ToolTipText = "Show all configured difftools in a dropdown.\nThe primary difftool can still be selected by clicking the main menu entry.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 250);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(325, 29);
            this.label1.Text = "Vertical ruler position [chars]";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VerticalRulerPosition
            // 
            this.VerticalRulerPosition.Location = new System.Drawing.Point(334, 253);
            this.VerticalRulerPosition.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.VerticalRulerPosition.Name = "VerticalRulerPosition";
            this.VerticalRulerPosition.Size = new System.Drawing.Size(120, 23);
            this.VerticalRulerPosition.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlMain.Controls.Add(this.gbGeneral, 0, 0);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Location = new System.Drawing.Point(8, 8);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 2;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlMain.Size = new System.Drawing.Size(1747, 1309);
            // 
            // gbGeneral
            // 
            this.gbGeneral.AutoSize = true;
            this.gbGeneral.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbGeneral.Controls.Add(this.tlpnlGeneral);
            this.gbGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbGeneral.Location = new System.Drawing.Point(3, 3);
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.Padding = new System.Windows.Forms.Padding(8);
            this.gbGeneral.Size = new System.Drawing.Size(1741, 311);
            this.gbGeneral.TabStop = false;
            this.gbGeneral.Text = "General";
            // 
            // DiffViewerSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tlpnlMain);
            this.Name = "DiffViewerSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1763, 1325);
            this.tlpnlGeneral.ResumeLayout(false);
            this.tlpnlGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VerticalRulerPosition)).EndInit();
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.TableLayoutPanel tlpnlGeneral;
        private System.Windows.Forms.CheckBox chkRememberIgnoreWhiteSpacePreference;
        private System.Windows.Forms.CheckBox chkRememberShowNonPrintingCharsPreference;
        private System.Windows.Forms.CheckBox chkRememberShowEntireFilePreference;
        private System.Windows.Forms.CheckBox chkRememberNumberOfContextLines;
        private System.Windows.Forms.CheckBox chkRememberShowSyntaxHighlightingInDiff;
        private System.Windows.Forms.CheckBox chkOmitUninterestingDiff;
        private System.Windows.Forms.CheckBox chkOpenSubmoduleDiffInSeparateWindow;
        private System.Windows.Forms.CheckBox chkContScrollToNextFileOnlyWithAlt;
        private UserControls.Settings.SettingsCheckBox chkShowDiffForAllParents;
        private UserControls.Settings.SettingsCheckBox chkShowAllCustomDiffTools;
        private System.Windows.Forms.NumericUpDown VerticalRulerPosition;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
    }
}
