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
            this.tableLayoutPanelForDiffViewer = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowDiffForAllParents = new System.Windows.Forms.CheckBox();
            this.chkOpenSubmoduleDiffInSeparateWindow = new System.Windows.Forms.CheckBox();
            this.chkRememberIgnoreWhiteSpacePreference = new System.Windows.Forms.CheckBox();
            this.chkRememberShowNonPrintingCharsPreference = new System.Windows.Forms.CheckBox();
            this.chkRememberShowEntireFilePreference = new System.Windows.Forms.CheckBox();
            this.chkRememberNumberOfContextLines = new System.Windows.Forms.CheckBox();
            this.chkOmitUninterestingDiff = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.VerticalRulerPosition = new System.Windows.Forms.NumericUpDown();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelForDiffViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VerticalRulerPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelForDiffViewer
            // 
            this.tableLayoutPanelForDiffViewer.AutoSize = true;
            this.tableLayoutPanelForDiffViewer.ColumnCount = 2;
            this.tableLayoutPanelForDiffViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForDiffViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkShowDiffForAllParents, 0, 6);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkOpenSubmoduleDiffInSeparateWindow, 0, 5);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkRememberIgnoreWhiteSpacePreference, 0, 0);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkRememberShowNonPrintingCharsPreference, 0, 1);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkRememberShowEntireFilePreference, 0, 2);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkRememberNumberOfContextLines, 0, 3);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkOmitUninterestingDiff, 0, 4);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.label1, 0, 7);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.VerticalRulerPosition, 1, 7);
            this.tableLayoutPanelForDiffViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelForDiffViewer.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelForDiffViewer.Name = "tableLayoutPanelForDiffViewer";
            this.tableLayoutPanelForDiffViewer.RowCount = 9;
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.Size = new System.Drawing.Size(1132, 821);
            this.tableLayoutPanelForDiffViewer.TabIndex = 1;
            // 
            // chkShowDiffForAllParents
            // 
            this.chkShowDiffForAllParents.AutoSize = true;
            this.chkShowDiffForAllParents.Location = new System.Drawing.Point(3, 141);
            this.chkShowDiffForAllParents.Name = "chkShowDiffForAllParents";
            this.chkShowDiffForAllParents.Size = new System.Drawing.Size(280, 17);
            this.chkShowDiffForAllParents.TabIndex = 10;
            this.chkShowDiffForAllParents.Text = "Show file differences for all parents in browse dialog";
            this.chkShowDiffForAllParents.UseVisualStyleBackColor = true;
            // 
            // chkOpenSubmoduleDiffInSeparateWindow
            // 
            this.chkOpenSubmoduleDiffInSeparateWindow.AutoSize = true;
            this.chkOpenSubmoduleDiffInSeparateWindow.Location = new System.Drawing.Point(3, 118);
            this.chkOpenSubmoduleDiffInSeparateWindow.Name = "chkOpenSubmoduleDiffInSeparateWindow";
            this.chkOpenSubmoduleDiffInSeparateWindow.Size = new System.Drawing.Size(223, 17);
            this.chkOpenSubmoduleDiffInSeparateWindow.TabIndex = 9;
            this.chkOpenSubmoduleDiffInSeparateWindow.Text = "Open Submodule Diff in separate window";
            this.chkOpenSubmoduleDiffInSeparateWindow.UseVisualStyleBackColor = true;
            // 
            // chkRememberIgnoreWhiteSpacePreference
            // 
            this.chkRememberIgnoreWhiteSpacePreference.AutoSize = true;
            this.chkRememberIgnoreWhiteSpacePreference.Location = new System.Drawing.Point(3, 3);
            this.chkRememberIgnoreWhiteSpacePreference.Name = "chkRememberIgnoreWhiteSpacePreference";
            this.chkRememberIgnoreWhiteSpacePreference.Size = new System.Drawing.Size(253, 17);
            this.chkRememberIgnoreWhiteSpacePreference.TabIndex = 4;
            this.chkRememberIgnoreWhiteSpacePreference.Text = "Remember the \'Ignore whitespaces\' preference";
            this.chkRememberIgnoreWhiteSpacePreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowNonPrintingCharsPreference
            // 
            this.chkRememberShowNonPrintingCharsPreference.AutoSize = true;
            this.chkRememberShowNonPrintingCharsPreference.Location = new System.Drawing.Point(3, 26);
            this.chkRememberShowNonPrintingCharsPreference.Name = "chkRememberShowNonPrintingCharsPreference";
            this.chkRememberShowNonPrintingCharsPreference.Size = new System.Drawing.Size(296, 17);
            this.chkRememberShowNonPrintingCharsPreference.TabIndex = 5;
            this.chkRememberShowNonPrintingCharsPreference.Text = "Remember the \'Show nonprinting characters\' preference";
            this.chkRememberShowNonPrintingCharsPreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberShowEntireFilePreference
            // 
            this.chkRememberShowEntireFilePreference.AutoSize = true;
            this.chkRememberShowEntireFilePreference.Location = new System.Drawing.Point(3, 49);
            this.chkRememberShowEntireFilePreference.Name = "chkRememberShowEntireFilePreference";
            this.chkRememberShowEntireFilePreference.Size = new System.Drawing.Size(233, 17);
            this.chkRememberShowEntireFilePreference.TabIndex = 6;
            this.chkRememberShowEntireFilePreference.Text = "Remember the \'Show entire file\' preference";
            this.chkRememberShowEntireFilePreference.UseVisualStyleBackColor = true;
            // 
            // chkRememberNumberOfContextLines
            // 
            this.chkRememberNumberOfContextLines.AutoSize = true;
            this.chkRememberNumberOfContextLines.Location = new System.Drawing.Point(3, 72);
            this.chkRememberNumberOfContextLines.Name = "chkRememberNumberOfContextLines";
            this.chkRememberNumberOfContextLines.Size = new System.Drawing.Size(273, 17);
            this.chkRememberNumberOfContextLines.TabIndex = 7;
            this.chkRememberNumberOfContextLines.Text = "Remember the \'Number of context lines\' preference";
            this.chkRememberNumberOfContextLines.UseVisualStyleBackColor = true;
            // 
            // chkOmitUninterestingDiff
            // 
            this.chkOmitUninterestingDiff.AutoSize = true;
            this.chkOmitUninterestingDiff.Location = new System.Drawing.Point(3, 95);
            this.chkOmitUninterestingDiff.Name = "chkOmitUninterestingDiff";
            this.chkOmitUninterestingDiff.Size = new System.Drawing.Size(249, 17);
            this.chkOmitUninterestingDiff.TabIndex = 8;
            this.chkOmitUninterestingDiff.Text = "Omit uninteresting changes from combined diff";
            this.chkOmitUninterestingDiff.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 168);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Vertical ruler position [chars]";
            // 
            // VerticalRulerPosition
            // 
            this.VerticalRulerPosition.Location = new System.Drawing.Point(305, 164);
            this.VerticalRulerPosition.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.VerticalRulerPosition.Name = "VerticalRulerPosition";
            this.VerticalRulerPosition.Size = new System.Drawing.Size(120, 21);
            this.VerticalRulerPosition.TabIndex = 11;
            this.VerticalRulerPosition.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // tooltip
            // 
            this.tooltip.AutoPopDelay = 30000;
            this.tooltip.InitialDelay = 500;
            this.tooltip.ReshowDelay = 100;
            // 
            // DiffViewerSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanelForDiffViewer);
            this.Name = "DiffViewerSettingsPage";
            this.Size = new System.Drawing.Size(1132, 821);
            this.tableLayoutPanelForDiffViewer.ResumeLayout(false);
            this.tableLayoutPanelForDiffViewer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VerticalRulerPosition)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForDiffViewer;
        private System.Windows.Forms.CheckBox chkRememberIgnoreWhiteSpacePreference;
        private System.Windows.Forms.CheckBox chkRememberShowNonPrintingCharsPreference;
        private System.Windows.Forms.CheckBox chkRememberShowEntireFilePreference;
        private System.Windows.Forms.CheckBox chkRememberNumberOfContextLines;
        private System.Windows.Forms.CheckBox chkOmitUninterestingDiff;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.CheckBox chkOpenSubmoduleDiffInSeparateWindow;
        private System.Windows.Forms.CheckBox chkShowDiffForAllParents;
        private System.Windows.Forms.NumericUpDown VerticalRulerPosition;
        private System.Windows.Forms.Label label1;
    }
}
