namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ConfirmationsSettingsPage
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
            this.CheckoutGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkUpdateModules = new System.Windows.Forms.CheckBox();
            this.chkAmend = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterPull = new System.Windows.Forms.CheckBox();
            this.chkAddTrackingRef = new System.Windows.Forms.CheckBox();
            this.chkPushNewBranch = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterCheckout = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanelForDiffViewer = new System.Windows.Forms.TableLayoutPanel();
            this.chkCheckForDiffViewerSelectedFilesLimitNumber = new System.Windows.Forms.CheckBox();
            this.upDownDiffViewerSelectedFilesLimitNumber = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel2.SuspendLayout();
            this.CheckoutGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanelForDiffViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownDiffViewerSelectedFilesLimitNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.CheckoutGB, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(457, 202);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // CheckoutGB
            // 
            this.CheckoutGB.AutoSize = true;
            this.CheckoutGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CheckoutGB.Controls.Add(this.tableLayoutPanel3);
            this.CheckoutGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.CheckoutGB.Location = new System.Drawing.Point(3, 3);
            this.CheckoutGB.Name = "CheckoutGB";
            this.CheckoutGB.Size = new System.Drawing.Size(451, 196);
            this.CheckoutGB.TabIndex = 0;
            this.CheckoutGB.TabStop = false;
            this.CheckoutGB.Text = "Don\'t ask to confirm to (use with caution)";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.chkUpdateModules, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.chkAmend, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.chkAutoPopStashAfterPull, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.chkAddTrackingRef, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.chkPushNewBranch, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.chkAutoPopStashAfterCheckout, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanelForDiffViewer, 0, 6);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 12);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 7;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(442, 164);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkUpdateModules
            // 
            this.chkUpdateModules.AutoSize = true;
            this.chkUpdateModules.Location = new System.Drawing.Point(3, 118);
            this.chkUpdateModules.Name = "chkUpdateModules";
            this.chkUpdateModules.Size = new System.Drawing.Size(181, 17);
            this.chkUpdateModules.TabIndex = 6;
            this.chkUpdateModules.Text = "Update submodules on checkout";
            this.chkUpdateModules.ThreeState = true;
            this.chkUpdateModules.UseVisualStyleBackColor = true;
            // 
            // chkAmend
            // 
            this.chkAmend.AutoSize = true;
            this.chkAmend.Location = new System.Drawing.Point(3, 3);
            this.chkAmend.Name = "chkAmend";
            this.chkAmend.Size = new System.Drawing.Size(115, 17);
            this.chkAmend.TabIndex = 0;
            this.chkAmend.Text = "Amend last commit";
            this.chkAmend.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterPull
            // 
            this.chkAutoPopStashAfterPull.AutoSize = true;
            this.chkAutoPopStashAfterPull.Location = new System.Drawing.Point(3, 26);
            this.chkAutoPopStashAfterPull.Name = "chkAutoPopStashAfterPull";
            this.chkAutoPopStashAfterPull.Size = new System.Drawing.Size(409, 17);
            this.chkAutoPopStashAfterPull.TabIndex = 1;
            this.chkAutoPopStashAfterPull.Text = "Apply stashed changes after successful pull (stash will be popped automatically)";
            this.chkAutoPopStashAfterPull.ThreeState = true;
            this.chkAutoPopStashAfterPull.UseVisualStyleBackColor = true;
            // 
            // chkAddTrackingRef
            // 
            this.chkAddTrackingRef.AutoSize = true;
            this.chkAddTrackingRef.Location = new System.Drawing.Point(3, 72);
            this.chkAddTrackingRef.Name = "chkAddTrackingRef";
            this.chkAddTrackingRef.Size = new System.Drawing.Size(267, 17);
            this.chkAddTrackingRef.TabIndex = 4;
            this.chkAddTrackingRef.Text = "Add a tracking reference for newly pushed branch";
            this.chkAddTrackingRef.UseVisualStyleBackColor = true;
            // 
            // chkPushNewBranch
            // 
            this.chkPushNewBranch.AutoSize = true;
            this.chkPushNewBranch.Location = new System.Drawing.Point(3, 95);
            this.chkPushNewBranch.Name = "chkPushNewBranch";
            this.chkPushNewBranch.Size = new System.Drawing.Size(190, 17);
            this.chkPushNewBranch.TabIndex = 3;
            this.chkPushNewBranch.Text = "Push a new branch for the remote";
            this.chkPushNewBranch.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterCheckout
            // 
            this.chkAutoPopStashAfterCheckout.AutoSize = true;
            this.chkAutoPopStashAfterCheckout.Location = new System.Drawing.Point(3, 49);
            this.chkAutoPopStashAfterCheckout.Name = "chkAutoPopStashAfterCheckout";
            this.chkAutoPopStashAfterCheckout.Size = new System.Drawing.Size(436, 17);
            this.chkAutoPopStashAfterCheckout.TabIndex = 5;
            this.chkAutoPopStashAfterCheckout.Text = "Apply stashed changes after successful checkout (stash will be popped automatical" +
    "ly)";
            this.chkAutoPopStashAfterCheckout.ThreeState = true;
            this.chkAutoPopStashAfterCheckout.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelForDiffViewer
            // 
            this.tableLayoutPanelForDiffViewer.ColumnCount = 2;
            this.tableLayoutPanelForDiffViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.18182F));
            this.tableLayoutPanelForDiffViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.81818F));
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.chkCheckForDiffViewerSelectedFilesLimitNumber, 0, 0);
            this.tableLayoutPanelForDiffViewer.Controls.Add(this.upDownDiffViewerSelectedFilesLimitNumber, 1, 0);
            this.tableLayoutPanelForDiffViewer.Location = new System.Drawing.Point(3, 141);
            this.tableLayoutPanelForDiffViewer.Name = "tableLayoutPanelForDiffViewer";
            this.tableLayoutPanelForDiffViewer.RowCount = 1;
            this.tableLayoutPanelForDiffViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanelForDiffViewer.Size = new System.Drawing.Size(275, 20);
            this.tableLayoutPanelForDiffViewer.TabIndex = 7;
            // 
            // chkCheckForDiffViewerSelectedFilesLimitNumber
            // 
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.AutoSize = true;
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Location = new System.Drawing.Point(0, 3);
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Name = "chkCheckForDiffViewerSelectedFilesLimitNumber";
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Size = new System.Drawing.Size(168, 17);
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.TabIndex = 0;
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.Text = "Warn if selected files exceed:";
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.UseVisualStyleBackColor = true;
            this.chkCheckForDiffViewerSelectedFilesLimitNumber.CheckedChanged += new System.EventHandler(this.chkCheckForDiffViewerSelectedFilesLimitNumber_CheckedChanged);
            // 
            // upDownDiffViewerSelectedFilesLimitNumber
            // 
            this.upDownDiffViewerSelectedFilesLimitNumber.Location = new System.Drawing.Point(174, 0);
            this.upDownDiffViewerSelectedFilesLimitNumber.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.upDownDiffViewerSelectedFilesLimitNumber.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.upDownDiffViewerSelectedFilesLimitNumber.Name = "upDownDiffViewerSelectedFilesLimitNumber";
            this.upDownDiffViewerSelectedFilesLimitNumber.Size = new System.Drawing.Size(40, 21);
            this.upDownDiffViewerSelectedFilesLimitNumber.TabIndex = 1;
            this.upDownDiffViewerSelectedFilesLimitNumber.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // ConfirmationsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "ConfirmationsSettingsPage";
            this.Size = new System.Drawing.Size(977, 394);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.CheckoutGB.ResumeLayout(false);
            this.CheckoutGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanelForDiffViewer.ResumeLayout(false);
            this.tableLayoutPanelForDiffViewer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownDiffViewerSelectedFilesLimitNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox CheckoutGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox chkAmend;
        private System.Windows.Forms.CheckBox chkAutoPopStashAfterPull;
        private System.Windows.Forms.CheckBox chkPushNewBranch;
        private System.Windows.Forms.CheckBox chkAddTrackingRef;
        private System.Windows.Forms.CheckBox chkAutoPopStashAfterCheckout;
        private System.Windows.Forms.CheckBox chkUpdateModules;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForDiffViewer;
        private System.Windows.Forms.CheckBox chkCheckForDiffViewerSelectedFilesLimitNumber;
        private System.Windows.Forms.NumericUpDown upDownDiffViewerSelectedFilesLimitNumber;

    }
}
