using GitUI.UserControls.Settings;

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
            tlpnlGeneral = new TableLayoutPanel();
            chkRememberIgnoreWhiteSpacePreference = new CheckBox();
            chkRememberShowNonPrintingCharsPreference = new CheckBox();
            chkRememberShowEntireFilePreference = new CheckBox();
            chkRememberDiffAppearancePreference = new SettingsCheckBox();
            chkRememberNumberOfContextLines = new CheckBox();
            chkRememberShowSyntaxHighlightingInDiff = new CheckBox();
            chkOmitUninterestingDiff = new CheckBox();
            chkContScrollToNextFileOnlyWithAlt = new CheckBox();
            chkOpenSubmoduleDiffInSeparateWindow = new CheckBox();
            chkShowDiffForAllParents = new SettingsCheckBox();
            chkShowAllCustomDiffTools = new SettingsCheckBox();
            label1 = new Label();
            VerticalRulerPosition = new NumericUpDown();
            tlpnlMain = new TableLayoutPanel();
            gbGeneral = new GroupBox();
            gbDiffColoring = new GroupBox();
            tlpnlDiffColoring = new TableLayoutPanel();
            chkUseGitColoring = new SettingsCheckBox();
            chkUseGEThemeGitColoring = new SettingsCheckBox();
            tlpnlGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(VerticalRulerPosition)).BeginInit();
            tlpnlMain.SuspendLayout();
            gbGeneral.SuspendLayout();
            gbDiffColoring.SuspendLayout();
            tlpnlDiffColoring.SuspendLayout();
            SuspendLayout();
            // 
            // tlpnlGeneral
            // 
            tlpnlGeneral.AutoSize = true;
            tlpnlGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlGeneral.ColumnCount = 3;
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlGeneral.Controls.Add(chkRememberIgnoreWhiteSpacePreference, 0, 0);
            tlpnlGeneral.Controls.Add(chkRememberShowNonPrintingCharsPreference, 0, 1);
            tlpnlGeneral.Controls.Add(chkRememberShowEntireFilePreference, 0, 2);
            tlpnlGeneral.Controls.Add(chkRememberDiffAppearancePreference, 0, 3);
            tlpnlGeneral.Controls.Add(chkRememberNumberOfContextLines, 0, 4);
            tlpnlGeneral.Controls.Add(chkRememberShowSyntaxHighlightingInDiff, 0, 5);
            tlpnlGeneral.Controls.Add(chkOmitUninterestingDiff, 0, 6);
            tlpnlGeneral.Controls.Add(chkContScrollToNextFileOnlyWithAlt, 0, 7);
            tlpnlGeneral.Controls.Add(chkOpenSubmoduleDiffInSeparateWindow, 0, 8);
            tlpnlGeneral.Controls.Add(chkShowDiffForAllParents, 0, 9);
            tlpnlGeneral.Controls.Add(chkShowAllCustomDiffTools, 0, 10);
            tlpnlGeneral.Controls.Add(label1, 0, 11);
            tlpnlGeneral.Controls.Add(VerticalRulerPosition, 1, 11);
            tlpnlGeneral.Dock = DockStyle.Fill;
            tlpnlGeneral.Location = new Point(8, 24);
            tlpnlGeneral.Name = "tlpnlGeneral";
            tlpnlGeneral.RowCount = 12;
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpnlGeneral.Size = new Size(1725, 279);
            // 
            // chkRememberIgnoreWhiteSpacePreference
            // 
            chkRememberIgnoreWhiteSpacePreference.AutoSize = true;
            chkRememberIgnoreWhiteSpacePreference.Dock = DockStyle.Fill;
            chkRememberIgnoreWhiteSpacePreference.Location = new Point(3, 3);
            chkRememberIgnoreWhiteSpacePreference.Name = "chkRememberIgnoreWhiteSpacePreference";
            chkRememberIgnoreWhiteSpacePreference.Size = new Size(325, 19);
            chkRememberIgnoreWhiteSpacePreference.Text = "Remember the \'Ignore whitespaces\' preference";
            chkRememberIgnoreWhiteSpacePreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowNonPrintingCharsPreference
            // 
            chkRememberShowNonPrintingCharsPreference.AutoSize = true;
            chkRememberShowNonPrintingCharsPreference.Dock = DockStyle.Fill;
            chkRememberShowNonPrintingCharsPreference.Location = new Point(3, 28);
            chkRememberShowNonPrintingCharsPreference.Name = "chkRememberShowNonPrintingCharsPreference";
            chkRememberShowNonPrintingCharsPreference.Size = new Size(325, 19);
            chkRememberShowNonPrintingCharsPreference.Text = "Remember the \'Show nonprinting characters\' preference";
            chkRememberShowNonPrintingCharsPreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowEntireFilePreference
            // 
            chkRememberShowEntireFilePreference.AutoSize = true;
            chkRememberShowEntireFilePreference.Dock = DockStyle.Fill;
            chkRememberShowEntireFilePreference.Location = new Point(3, 53);
            chkRememberShowEntireFilePreference.Name = "chkRememberShowEntireFilePreference";
            chkRememberShowEntireFilePreference.Size = new Size(325, 19);
            chkRememberShowEntireFilePreference.Text = "Remember the \'Show entire file\' preference";
            chkRememberShowEntireFilePreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberDiffAppearancePreference
            // 
            chkRememberDiffAppearancePreference.AutoSize = true;
            chkRememberDiffAppearancePreference.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chkRememberDiffAppearancePreference.Checked = false;
            chkRememberDiffAppearancePreference.Dock = DockStyle.Fill;
            chkRememberDiffAppearancePreference.Location = new Point(3, 53);
            chkRememberDiffAppearancePreference.ManualSectionAnchorName = "diff-appearance";
            chkRememberDiffAppearancePreference.Name = "chkRememberDiffAppearancePreference";
            chkRememberDiffAppearancePreference.Size = new Size(325, 19);
            chkRememberDiffAppearancePreference.Text = "Remember the 'Diff appearance' preference";
            chkRememberDiffAppearancePreference.ToolTipText = "Diff appearance: patch (default), Git word-diff or Difftastic.";
            // 
            // chkRememberNumberOfContextLines
            // 
            chkRememberNumberOfContextLines.AutoSize = true;
            chkRememberNumberOfContextLines.Dock = DockStyle.Fill;
            chkRememberNumberOfContextLines.Location = new Point(3, 78);
            chkRememberNumberOfContextLines.Name = "chkRememberNumberOfContextLines";
            chkRememberNumberOfContextLines.Size = new Size(325, 19);
            chkRememberNumberOfContextLines.Text = "Remember the \'Number of context lines\' preference";
            chkRememberNumberOfContextLines.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowSyntaxHighlightingInDiff
            // 
            chkRememberShowSyntaxHighlightingInDiff.AutoSize = true;
            chkRememberShowSyntaxHighlightingInDiff.Dock = DockStyle.Fill;
            chkRememberShowSyntaxHighlightingInDiff.Location = new Point(3, 103);
            chkRememberShowSyntaxHighlightingInDiff.Name = "chkRememberShowSyntaxHighlightingInDiff";
            chkRememberShowSyntaxHighlightingInDiff.Size = new Size(325, 19);
            chkRememberShowSyntaxHighlightingInDiff.Text = "Remember the \'Show syntax highlighting\' preference";
            chkRememberShowSyntaxHighlightingInDiff.UseVisualStyleBackColor = true;
            // 
            // chkOmitUninterestingDiff
            // 
            chkOmitUninterestingDiff.AutoSize = true;
            chkOmitUninterestingDiff.Dock = DockStyle.Fill;
            chkOmitUninterestingDiff.Location = new Point(3, 128);
            chkOmitUninterestingDiff.Name = "chkOmitUninterestingDiff";
            chkOmitUninterestingDiff.Size = new Size(325, 19);
            chkOmitUninterestingDiff.Text = "Omit uninteresting changes from combined diff";
            chkOmitUninterestingDiff.UseVisualStyleBackColor = true;
            // 
            // chkContScrollToNextFileOnlyWithAlt
            // 
            chkContScrollToNextFileOnlyWithAlt.AutoSize = true;
            chkContScrollToNextFileOnlyWithAlt.Dock = DockStyle.Fill;
            chkContScrollToNextFileOnlyWithAlt.Location = new Point(3, 153);
            chkContScrollToNextFileOnlyWithAlt.Name = "chkContScrollToNextFileOnlyWithAlt";
            chkContScrollToNextFileOnlyWithAlt.Size = new Size(325, 19);
            chkContScrollToNextFileOnlyWithAlt.Text = "Enable automatic continuous scroll (without ALT button)";
            chkContScrollToNextFileOnlyWithAlt.UseVisualStyleBackColor = true;
            // 
            // chkOpenSubmoduleDiffInSeparateWindow
            // 
            chkOpenSubmoduleDiffInSeparateWindow.AutoSize = true;
            chkOpenSubmoduleDiffInSeparateWindow.Dock = DockStyle.Fill;
            chkOpenSubmoduleDiffInSeparateWindow.Location = new Point(3, 178);
            chkOpenSubmoduleDiffInSeparateWindow.Name = "chkOpenSubmoduleDiffInSeparateWindow";
            chkOpenSubmoduleDiffInSeparateWindow.Size = new Size(325, 19);
            chkOpenSubmoduleDiffInSeparateWindow.Text = "Open Submodule Diff in separate window";
            chkOpenSubmoduleDiffInSeparateWindow.UseVisualStyleBackColor = true;
            // 
            // chkShowDiffForAllParents
            // 
            chkShowDiffForAllParents.AutoSize = true;
            chkShowDiffForAllParents.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chkShowDiffForAllParents.Checked = false;
            chkShowDiffForAllParents.Dock = DockStyle.Fill;
            chkShowDiffForAllParents.Location = new Point(3, 203);
            chkShowDiffForAllParents.ManualSectionAnchorName = "general-show-file-differences-for-all-parents-in-browse-dialog";
            chkShowDiffForAllParents.Name = "chkShowDiffForAllParents";
            chkShowDiffForAllParents.Size = new Size(325, 19);
            chkShowDiffForAllParents.Text = "Show file differences for all parents in browse dialog";
            chkShowDiffForAllParents.ToolTipText = null;
            // 
            // chkShowAllCustomDiffTools
            // 
            chkShowAllCustomDiffTools.AutoSize = true;
            chkShowAllCustomDiffTools.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chkShowAllCustomDiffTools.Checked = false;
            chkShowAllCustomDiffTools.Dock = DockStyle.Fill;
            chkShowAllCustomDiffTools.Location = new Point(3, 228);
            chkShowAllCustomDiffTools.ManualSectionAnchorName = "general-show-all-available-difftools";
            chkShowAllCustomDiffTools.Name = "chkShowAllCustomDiffTools";
            chkShowAllCustomDiffTools.Size = new Size(325, 19);
            chkShowAllCustomDiffTools.Text = "Show all available difftools";
            chkShowAllCustomDiffTools.ToolTipText = "Show all configured difftools in a dropdown.\nThe primary difftool can still be selected by clicking the main menu entry.";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 250);
            label1.Name = "label1";
            label1.Size = new Size(325, 29);
            label1.Text = "Vertical ruler position [chars]";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // VerticalRulerPosition
            // 
            VerticalRulerPosition.Location = new Point(334, 253);
            VerticalRulerPosition.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            VerticalRulerPosition.Name = "VerticalRulerPosition";
            VerticalRulerPosition.Size = new Size(120, 23);
            VerticalRulerPosition.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // chkUseGitColoring
            // 
            chkUseGitColoring.AutoSize = true;
            chkUseGitColoring.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chkUseGitColoring.Checked = false;
            chkUseGitColoring.Dock = DockStyle.Fill;
            chkUseGitColoring.Location = new Point(3, 3);
            chkUseGitColoring.ManualSectionAnchorName = "diff-coloring-git-coloring";
            chkUseGitColoring.Name = "chkUseGitColoring";
            chkUseGitColoring.Size = new Size(183, 19);
            chkUseGitColoring.TabIndex = 1;
            chkUseGitColoring.Text = "Git coloring";
            chkUseGitColoring.ToolTipText = "Use Git coloring engine to show moved code etc.\n";
            chkUseGitColoring.CheckedChanged += chkUseGitColoring_CheckedChanged;
            // 
            // chkUseGEThemeGitColoring
            // 
            chkUseGEThemeGitColoring.AutoSize = true;
            chkUseGEThemeGitColoring.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chkUseGEThemeGitColoring.Checked = false;
            chkUseGEThemeGitColoring.Dock = DockStyle.Fill;
            chkUseGEThemeGitColoring.Location = new Point(3, 28);
            chkUseGEThemeGitColoring.ManualSectionAnchorName = "diff-coloring-reverse-background-color";
            chkUseGEThemeGitColoring.Name = "chkUseGEThemeGitColoring";
            chkUseGEThemeGitColoring.Size = new Size(183, 19);
            chkUseGEThemeGitColoring.TabIndex = 2;
            chkUseGEThemeGitColoring.Text = "Reverse background color";
            chkUseGEThemeGitColoring.ToolTipText = "Color the background at changes (invert colors).";
            // 
            // tlpnlDiffColoring
            // 
            tlpnlDiffColoring.AutoSize = true;
            tlpnlDiffColoring.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlDiffColoring.ColumnCount = 2;
            tlpnlDiffColoring.ColumnStyles.Add(new ColumnStyle());
            tlpnlDiffColoring.ColumnStyles.Add(new ColumnStyle());
            tlpnlDiffColoring.Controls.Add(chkUseGitColoring, 0, 0);
            tlpnlDiffColoring.Controls.Add(chkUseGEThemeGitColoring, 0, 1);
            tlpnlDiffColoring.Dock = DockStyle.Fill;
            tlpnlDiffColoring.Location = new Point(8, 24);
            tlpnlDiffColoring.Name = "tlpnlDiffColoring";
            tlpnlDiffColoring.RowCount = 3;
            tlpnlDiffColoring.RowStyles.Add(new RowStyle());
            tlpnlDiffColoring.RowStyles.Add(new RowStyle());
            tlpnlDiffColoring.RowStyles.Add(new RowStyle());
            tlpnlDiffColoring.Size = new Size(1489, 150);
            tlpnlDiffColoring.TabIndex = 0;
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(gbGeneral, 0, 0);
            tlpnlMain.Controls.Add(gbDiffColoring, 0, 1);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 5;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpnlMain.Size = new Size(1747, 1309);
            // 
            // gbGeneral
            // 
            gbGeneral.AutoSize = true;
            gbGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbGeneral.Controls.Add(tlpnlGeneral);
            gbGeneral.Dock = DockStyle.Fill;
            gbGeneral.Location = new Point(3, 3);
            gbGeneral.Name = "gbGeneral";
            gbGeneral.Padding = new Padding(8);
            gbGeneral.Size = new Size(1741, 311);
            gbGeneral.TabStop = false;
            gbGeneral.Text = "General";
            // 
            // gbDiffColoring
            // 
            gbDiffColoring.AutoSize = true;
            gbDiffColoring.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbDiffColoring.Controls.Add(tlpnlDiffColoring);
            gbDiffColoring.Dock = DockStyle.Fill;
            gbDiffColoring.Location = new Point(3, 3);
            gbDiffColoring.Name = "gbDiffColoring";
            gbDiffColoring.Padding = new Padding(8);
            gbDiffColoring.Size = new Size(1505, 182);
            gbDiffColoring.TabIndex = 0;
            gbDiffColoring.TabStop = false;
            gbDiffColoring.Text = "Diff coloring";
            // 
            // DiffViewerSettingsPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(tlpnlMain);
            Name = "DiffViewerSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1763, 1325);
            Text = "Diff viewer";
            tlpnlGeneral.ResumeLayout(false);
            tlpnlGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(VerticalRulerPosition)).EndInit();
            tlpnlDiffColoring.ResumeLayout(false);
            tlpnlDiffColoring.PerformLayout();
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbGeneral.ResumeLayout(false);
            gbGeneral.PerformLayout();
            gbDiffColoring.ResumeLayout(false);
            gbDiffColoring.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private GroupBox gbGeneral;
        private TableLayoutPanel tlpnlGeneral;
        private CheckBox chkRememberIgnoreWhiteSpacePreference;
        private CheckBox chkRememberShowNonPrintingCharsPreference;
        private CheckBox chkRememberShowEntireFilePreference;
        private SettingsCheckBox chkRememberDiffAppearancePreference;
        private CheckBox chkRememberNumberOfContextLines;
        private CheckBox chkRememberShowSyntaxHighlightingInDiff;
        private CheckBox chkOmitUninterestingDiff;
        private CheckBox chkOpenSubmoduleDiffInSeparateWindow;
        private CheckBox chkContScrollToNextFileOnlyWithAlt;
        private SettingsCheckBox chkShowDiffForAllParents;
        private SettingsCheckBox chkShowAllCustomDiffTools;
        private NumericUpDown VerticalRulerPosition;
        private Label label1;
        private TableLayoutPanel tlpnlMain;
        private GroupBox gbDiffColoring;
        private TableLayoutPanel tlpnlDiffColoring;
        private SettingsCheckBox chkUseGitColoring;
        private SettingsCheckBox chkUseGEThemeGitColoring;
    }
}
