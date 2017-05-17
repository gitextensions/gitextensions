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
            this.chkSecondAbortConfirmation = new System.Windows.Forms.CheckBox();
            this.chkCommitAfterConflictsResolved = new System.Windows.Forms.CheckBox();
            this.chkResolveConflicts = new System.Windows.Forms.CheckBox();
            this.chkAmend = new System.Windows.Forms.CheckBox();
            this.chkCommitIfNoBranch = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterPull = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterCheckout = new System.Windows.Forms.CheckBox();
            this.chkAddTrackingRef = new System.Windows.Forms.CheckBox();
            this.chkPushNewBranch = new System.Windows.Forms.CheckBox();
            this.chkUpdateModules = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.CheckoutGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(459, 275);
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
            this.CheckoutGB.Size = new System.Drawing.Size(453, 269);
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
            this.tableLayoutPanel3.Controls.Add(this.chkSecondAbortConfirmation, 0, 9);
            this.tableLayoutPanel3.Controls.Add(this.chkCommitAfterConflictsResolved, 0, 8);
            this.tableLayoutPanel3.Controls.Add(this.chkResolveConflicts, 0, 7);
            this.tableLayoutPanel3.Controls.Add(this.chkAmend, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.chkCommitIfNoBranch, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.chkAutoPopStashAfterPull, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.chkAutoPopStashAfterCheckout, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.chkAddTrackingRef, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.chkPushNewBranch, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.chkUpdateModules, 0, 6);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 19);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 10;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(442, 230);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkSecondAbortConfirmation
            // 
            this.chkSecondAbortConfirmation.AutoSize = true;
            this.chkSecondAbortConfirmation.Location = new System.Drawing.Point(3, 210);
            this.chkSecondAbortConfirmation.Name = "chkSecondAbortConfirmation";
            this.chkSecondAbortConfirmation.Size = new System.Drawing.Size(243, 17);
            this.chkSecondAbortConfirmation.TabIndex = 10;
            this.chkSecondAbortConfirmation.Text = "Confirm for the second time to abort a merge";
            this.chkSecondAbortConfirmation.ThreeState = true;
            this.chkSecondAbortConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkCommitAfterConflictsResolved
            // 
            this.chkCommitAfterConflictsResolved.AutoSize = true;
            this.chkCommitAfterConflictsResolved.Location = new System.Drawing.Point(3, 187);
            this.chkCommitAfterConflictsResolved.Name = "chkCommitAfterConflictsResolved";
            this.chkCommitAfterConflictsResolved.Size = new System.Drawing.Size(271, 17);
            this.chkCommitAfterConflictsResolved.TabIndex = 9;
            this.chkCommitAfterConflictsResolved.Text = "Commit changes after conflicts have been resolved";
            this.chkCommitAfterConflictsResolved.ThreeState = true;
            this.chkCommitAfterConflictsResolved.UseVisualStyleBackColor = true;
            // 
            // chkResolveConflicts
            // 
            this.chkResolveConflicts.AutoSize = true;
            this.chkResolveConflicts.Location = new System.Drawing.Point(3, 164);
            this.chkResolveConflicts.Name = "chkResolveConflicts";
            this.chkResolveConflicts.Size = new System.Drawing.Size(106, 17);
            this.chkResolveConflicts.TabIndex = 8;
            this.chkResolveConflicts.Text = "Resolve conflicts";
            this.chkResolveConflicts.ThreeState = true;
            this.chkResolveConflicts.UseVisualStyleBackColor = true;
            // 
            // chkAmend
            // 
            this.chkAmend.AutoSize = true;
            this.chkAmend.Location = new System.Drawing.Point(3, 3);
            this.chkAmend.Name = "chkAmend";
            this.chkAmend.Size = new System.Drawing.Size(115, 17);
            this.chkAmend.TabIndex = 1;
            this.chkAmend.Text = "Amend last commit";
            this.chkAmend.UseVisualStyleBackColor = true;
            // 
            // chkCommitIfNoBranch
            // 
            this.chkCommitIfNoBranch.AutoSize = true;
            this.chkCommitIfNoBranch.Location = new System.Drawing.Point(3, 26);
            this.chkCommitIfNoBranch.Name = "chkCommitIfNoBranch";
            this.chkCommitIfNoBranch.Size = new System.Drawing.Size(339, 17);
            this.chkCommitIfNoBranch.TabIndex = 2;
            this.chkCommitIfNoBranch.Text = "Commit when no branch is currently checked out (headless state)";
            this.chkCommitIfNoBranch.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterPull
            // 
            this.chkAutoPopStashAfterPull.AutoSize = true;
            this.chkAutoPopStashAfterPull.Location = new System.Drawing.Point(3, 49);
            this.chkAutoPopStashAfterPull.Name = "chkAutoPopStashAfterPull";
            this.chkAutoPopStashAfterPull.Size = new System.Drawing.Size(409, 17);
            this.chkAutoPopStashAfterPull.TabIndex = 3;
            this.chkAutoPopStashAfterPull.Text = "Apply stashed changes after successful pull (stash will be popped automatically)";
            this.chkAutoPopStashAfterPull.ThreeState = true;
            this.chkAutoPopStashAfterPull.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterCheckout
            // 
            this.chkAutoPopStashAfterCheckout.AutoSize = true;
            this.chkAutoPopStashAfterCheckout.Location = new System.Drawing.Point(3, 72);
            this.chkAutoPopStashAfterCheckout.Name = "chkAutoPopStashAfterCheckout";
            this.chkAutoPopStashAfterCheckout.Size = new System.Drawing.Size(436, 17);
            this.chkAutoPopStashAfterCheckout.TabIndex = 4;
            this.chkAutoPopStashAfterCheckout.Text = "Apply stashed changes after successful checkout (stash will be popped automatical" +
    "ly)";
            this.chkAutoPopStashAfterCheckout.ThreeState = true;
            this.chkAutoPopStashAfterCheckout.UseVisualStyleBackColor = true;
            // 
            // chkAddTrackingRef
            // 
            this.chkAddTrackingRef.AutoSize = true;
            this.chkAddTrackingRef.Location = new System.Drawing.Point(3, 95);
            this.chkAddTrackingRef.Name = "chkAddTrackingRef";
            this.chkAddTrackingRef.Size = new System.Drawing.Size(267, 17);
            this.chkAddTrackingRef.TabIndex = 5;
            this.chkAddTrackingRef.Text = "Add a tracking reference for newly pushed branch";
            this.chkAddTrackingRef.UseVisualStyleBackColor = true;
            // 
            // chkPushNewBranch
            // 
            this.chkPushNewBranch.AutoSize = true;
            this.chkPushNewBranch.Location = new System.Drawing.Point(3, 118);
            this.chkPushNewBranch.Name = "chkPushNewBranch";
            this.chkPushNewBranch.Size = new System.Drawing.Size(190, 17);
            this.chkPushNewBranch.TabIndex = 6;
            this.chkPushNewBranch.Text = "Push a new branch for the remote";
            this.chkPushNewBranch.UseVisualStyleBackColor = true;
            // 
            // chkUpdateModules
            // 
            this.chkUpdateModules.AutoSize = true;
            this.chkUpdateModules.Location = new System.Drawing.Point(3, 141);
            this.chkUpdateModules.Name = "chkUpdateModules";
            this.chkUpdateModules.Size = new System.Drawing.Size(181, 17);
            this.chkUpdateModules.TabIndex = 7;
            this.chkUpdateModules.Text = "Update submodules on checkout";
            this.chkUpdateModules.ThreeState = true;
            this.chkUpdateModules.UseVisualStyleBackColor = true;
            // 
            // ConfirmationsSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "ConfirmationsSettingsPage";
            this.Size = new System.Drawing.Size(1109, 461);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.CheckoutGB.ResumeLayout(false);
            this.CheckoutGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
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
        private System.Windows.Forms.CheckBox chkCommitIfNoBranch;
        private System.Windows.Forms.CheckBox chkCommitAfterConflictsResolved;
        private System.Windows.Forms.CheckBox chkResolveConflicts;
        private System.Windows.Forms.CheckBox chkSecondAbortConfirmation;
    }
}
