namespace GitUI.CommandsDialogs.SettingsDialog.Pages
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.DiffViewerGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelForDiffViewer = new System.Windows.Forms.TableLayoutPanel();
            this.chkOmitUninterestingDiff = new System.Windows.Forms.CheckBox();
            this.chkRememberIgnoreWhiteSpacePreference = new System.Windows.Forms.CheckBox();
            this.CheckoutGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkAlwaysShowCheckoutDlg = new System.Windows.Forms.CheckBox();
            this.chkUseLocalChangesAction = new System.Windows.Forms.CheckBox();
            this.GeneralGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkCheckForRCVersions = new System.Windows.Forms.CheckBox();
            this.chkAlwaysShowAdvOpt = new System.Windows.Forms.CheckBox();
            this.chkDontSHowHelpImages = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.DiffViewerGB.SuspendLayout();
            this.tableLayoutPanelForDiffViewer.SuspendLayout();
            this.CheckoutGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.GeneralGB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(443, 337);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // DiffViewerGB
            // 
            this.DiffViewerGB.AutoSize = true;
            this.DiffViewerGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DiffViewerGB.Controls.Add(this.tableLayoutPanelForDiffViewer);
            this.DiffViewerGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.DiffViewerGB.Location = new System.Drawing.Point(3, 241);
            this.DiffViewerGB.Name = "DiffViewerGB";
            this.DiffViewerGB.Size = new System.Drawing.Size(437, 93);
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
            this.tableLayoutPanelForDiffViewer.Location = new System.Drawing.Point(6, 21);
            this.tableLayoutPanelForDiffViewer.Name = "tableLayoutPanelForDiffViewer";
            this.tableLayoutPanelForDiffViewer.RowCount = 1;
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForDiffViewer.Size = new System.Drawing.Size(286, 50);
            this.tableLayoutPanelForDiffViewer.TabIndex = 1;
            // 
            // chkOmitUninterestingDiff
            // 
            this.chkOmitUninterestingDiff.AutoSize = true;
            this.chkOmitUninterestingDiff.Location = new System.Drawing.Point(3, 28);
            this.chkOmitUninterestingDiff.Name = "chkOmitUninterestingDiff";
            this.chkOmitUninterestingDiff.Size = new System.Drawing.Size(280, 19);
            this.chkOmitUninterestingDiff.TabIndex = 6;
            this.chkOmitUninterestingDiff.Text = "Omit uninteresting changes from combined diff";
            this.chkOmitUninterestingDiff.UseVisualStyleBackColor = true;
            // 
            // chkRememberIgnoreWhiteSpacePreference
            // 
            this.chkRememberIgnoreWhiteSpacePreference.AutoSize = true;
            this.chkRememberIgnoreWhiteSpacePreference.Location = new System.Drawing.Point(3, 3);
            this.chkRememberIgnoreWhiteSpacePreference.Name = "chkRememberIgnoreWhiteSpacePreference";
            this.chkRememberIgnoreWhiteSpacePreference.Size = new System.Drawing.Size(269, 19);
            this.chkRememberIgnoreWhiteSpacePreference.TabIndex = 5;
            this.chkRememberIgnoreWhiteSpacePreference.Text = "Remember the ignore-white-space preference";
            this.chkRememberIgnoreWhiteSpacePreference.UseVisualStyleBackColor = true;
            // 
            // CheckoutGB
            // 
            this.CheckoutGB.AutoSize = true;
            this.CheckoutGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CheckoutGB.Controls.Add(this.tableLayoutPanel3);
            this.CheckoutGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckoutGB.Location = new System.Drawing.Point(3, 3);
            this.CheckoutGB.Name = "CheckoutGB";
            this.CheckoutGB.Size = new System.Drawing.Size(437, 108);
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(422, 65);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkAlwaysShowCheckoutDlg
            // 
            this.chkAlwaysShowCheckoutDlg.AutoSize = true;
            this.chkAlwaysShowCheckoutDlg.Location = new System.Drawing.Point(3, 3);
            this.chkAlwaysShowCheckoutDlg.Name = "chkAlwaysShowCheckoutDlg";
            this.chkAlwaysShowCheckoutDlg.Size = new System.Drawing.Size(182, 19);
            this.chkAlwaysShowCheckoutDlg.TabIndex = 0;
            this.chkAlwaysShowCheckoutDlg.Text = "Always show checkout dialog";
            this.chkAlwaysShowCheckoutDlg.UseVisualStyleBackColor = true;
            // 
            // chkUseLocalChangesAction
            // 
            this.chkUseLocalChangesAction.AutoSize = true;
            this.chkUseLocalChangesAction.Location = new System.Drawing.Point(3, 28);
            this.chkUseLocalChangesAction.Name = "chkUseLocalChangesAction";
            this.chkUseLocalChangesAction.Size = new System.Drawing.Size(416, 34);
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
            this.GeneralGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.GeneralGB.Location = new System.Drawing.Point(3, 117);
            this.GeneralGB.Name = "GeneralGB";
            this.GeneralGB.Size = new System.Drawing.Size(437, 118);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 21);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(223, 75);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chkCheckForRCVersions
            // 
            this.chkCheckForRCVersions.AutoSize = true;
            this.chkCheckForRCVersions.Location = new System.Drawing.Point(3, 53);
            this.chkCheckForRCVersions.Name = "chkCheckForRCVersions";
            this.chkCheckForRCVersions.Size = new System.Drawing.Size(217, 19);
            this.chkCheckForRCVersions.TabIndex = 3;
            this.chkCheckForRCVersions.Text = "Check for release candidate versions";
            this.chkCheckForRCVersions.UseVisualStyleBackColor = true;
            // 
            // chkAlwaysShowAdvOpt
            // 
            this.chkAlwaysShowAdvOpt.AutoSize = true;
            this.chkAlwaysShowAdvOpt.Location = new System.Drawing.Point(3, 28);
            this.chkAlwaysShowAdvOpt.Name = "chkAlwaysShowAdvOpt";
            this.chkAlwaysShowAdvOpt.Size = new System.Drawing.Size(191, 19);
            this.chkAlwaysShowAdvOpt.TabIndex = 2;
            this.chkAlwaysShowAdvOpt.Text = "Always show advanced options";
            this.chkAlwaysShowAdvOpt.UseVisualStyleBackColor = true;
            // 
            // chkDontSHowHelpImages
            // 
            this.chkDontSHowHelpImages.AutoSize = true;
            this.chkDontSHowHelpImages.Location = new System.Drawing.Point(3, 3);
            this.chkDontSHowHelpImages.Name = "chkDontSHowHelpImages";
            this.chkDontSHowHelpImages.Size = new System.Drawing.Size(153, 19);
            this.chkDontSHowHelpImages.TabIndex = 1;
            this.chkDontSHowHelpImages.Text = "Don\'t show help images";
            this.chkDontSHowHelpImages.UseVisualStyleBackColor = true;
            // 
            // AdvancedSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "AdvancedSettingsPage";
            this.Size = new System.Drawing.Size(1371, 575);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.DiffViewerGB.ResumeLayout(false);
            this.DiffViewerGB.PerformLayout();
            this.tableLayoutPanelForDiffViewer.ResumeLayout(false);
            this.tableLayoutPanelForDiffViewer.PerformLayout();
            this.CheckoutGB.ResumeLayout(false);
            this.CheckoutGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.GeneralGB.ResumeLayout(false);
            this.GeneralGB.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
    }
}
