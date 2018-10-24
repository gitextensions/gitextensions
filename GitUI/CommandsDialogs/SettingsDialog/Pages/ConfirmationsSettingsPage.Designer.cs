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
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbConfirmations = new System.Windows.Forms.GroupBox();
            this.tlpnlConfirmations = new System.Windows.Forms.TableLayoutPanel();
            this.chkFetchAndPruneAllConfirmation = new System.Windows.Forms.CheckBox();
            this.chkUndoLastCommitConfirmation = new System.Windows.Forms.CheckBox();
            this.chkRebaseOnTopOfSelectedCommit = new System.Windows.Forms.CheckBox();
            this.chkSecondAbortConfirmation = new System.Windows.Forms.CheckBox();
            this.chkCommitAfterConflictsResolved = new System.Windows.Forms.CheckBox();
            this.chkResolveConflicts = new System.Windows.Forms.CheckBox();
            this.chkAmend = new System.Windows.Forms.CheckBox();
            this.chkCommitIfNoBranch = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterPull = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterCheckout = new System.Windows.Forms.CheckBox();
            this.chkConfirmStashDrop = new System.Windows.Forms.CheckBox();
            this.chkAddTrackingRef = new System.Windows.Forms.CheckBox();
            this.chkPushNewBranch = new System.Windows.Forms.CheckBox();
            this.chkUpdateModules = new System.Windows.Forms.CheckBox();
            this.tlpnlMain.SuspendLayout();
            this.gbConfirmations.SuspendLayout();
            this.tlpnlConfirmations.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.Location = new System.Drawing.Point(3, 3);
            this.tlpnlMain.Controls.Add(this.gbConfirmations, 0, 0);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 2;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.Size = new System.Drawing.Size(500, 372);
            this.tlpnlMain.TabIndex = 2;
            // 
            // gbConfirmations
            // 
            this.gbConfirmations.AutoSize = true;
            this.gbConfirmations.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbConfirmations.Controls.Add(this.tlpnlConfirmations);
            this.gbConfirmations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbConfirmations.Location = new System.Drawing.Point(3, 3);
            this.gbConfirmations.Name = "gbConfirmations";
            this.gbConfirmations.Padding = new System.Windows.Forms.Padding(8);
            this.gbConfirmations.Size = new System.Drawing.Size(494, 366);
            this.gbConfirmations.TabIndex = 0;
            this.gbConfirmations.TabStop = false;
            this.gbConfirmations.Text = "Don\'t ask to confirm to (use with caution)";
            // 
            // tlpnlConfirmations
            // 
            this.tlpnlConfirmations.AutoSize = true;
            this.tlpnlConfirmations.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlConfirmations.ColumnCount = 1;
            this.tlpnlConfirmations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlConfirmations.Controls.Add(this.chkAmend, 0, 0);
            this.tlpnlConfirmations.Controls.Add(this.chkCommitIfNoBranch, 0, 1);
            this.tlpnlConfirmations.Controls.Add(this.chkAutoPopStashAfterPull, 0, 2);
            this.tlpnlConfirmations.Controls.Add(this.chkAutoPopStashAfterCheckout, 0, 3);
            this.tlpnlConfirmations.Controls.Add(this.chkConfirmStashDrop, 0, 4);
            this.tlpnlConfirmations.Controls.Add(this.chkAddTrackingRef, 0, 5);
            this.tlpnlConfirmations.Controls.Add(this.chkPushNewBranch, 0, 6);
            this.tlpnlConfirmations.Controls.Add(this.chkUpdateModules, 0, 7);
            this.tlpnlConfirmations.Controls.Add(this.chkResolveConflicts, 0, 8);
            this.tlpnlConfirmations.Controls.Add(this.chkCommitAfterConflictsResolved, 0, 9);
            this.tlpnlConfirmations.Controls.Add(this.chkSecondAbortConfirmation, 0, 10);
            this.tlpnlConfirmations.Controls.Add(this.chkRebaseOnTopOfSelectedCommit, 0, 11);
            this.tlpnlConfirmations.Controls.Add(this.chkUndoLastCommitConfirmation, 0, 12);
            this.tlpnlConfirmations.Controls.Add(this.chkFetchAndPruneAllConfirmation, 0, 13);
            this.tlpnlConfirmations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlConfirmations.Location = new System.Drawing.Point(5, 19);
            this.tlpnlConfirmations.Name = "tlpnlConfirmations";
            this.tlpnlConfirmations.RowCount = 14;
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.Size = new System.Drawing.Size(483, 325);
            this.tlpnlConfirmations.TabIndex = 1;
            // 
            // chkFetchAndPruneAllConfirmation
            // 
            this.chkFetchAndPruneAllConfirmation.AutoSize = true;
            this.chkFetchAndPruneAllConfirmation.Location = new System.Drawing.Point(3, 303);
            this.chkFetchAndPruneAllConfirmation.Name = "chkFetchAndPruneAllConfirmation";
            this.chkFetchAndPruneAllConfirmation.Size = new System.Drawing.Size(127, 19);
            this.chkFetchAndPruneAllConfirmation.TabIndex = 14;
            this.chkFetchAndPruneAllConfirmation.Text = "Fetch and prune all";
            this.chkFetchAndPruneAllConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkUndoLastCommitConfirmation
            // 
            this.chkUndoLastCommitConfirmation.AutoSize = true;
            this.chkUndoLastCommitConfirmation.Location = new System.Drawing.Point(3, 278);
            this.chkUndoLastCommitConfirmation.Name = "chkUndoLastCommitConfirmation";
            this.chkUndoLastCommitConfirmation.Size = new System.Drawing.Size(121, 19);
            this.chkUndoLastCommitConfirmation.TabIndex = 13;
            this.chkUndoLastCommitConfirmation.Text = "Undo last commit";
            this.chkUndoLastCommitConfirmation.ThreeState = true;
            this.chkUndoLastCommitConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkRebaseOnTopOfSelectedCommit
            // 
            this.chkRebaseOnTopOfSelectedCommit.AutoSize = true;
            this.chkRebaseOnTopOfSelectedCommit.Location = new System.Drawing.Point(3, 253);
            this.chkRebaseOnTopOfSelectedCommit.Name = "chkRebaseOnTopOfSelectedCommit";
            this.chkRebaseOnTopOfSelectedCommit.Size = new System.Drawing.Size(206, 19);
            this.chkRebaseOnTopOfSelectedCommit.TabIndex = 12;
            this.chkRebaseOnTopOfSelectedCommit.Text = "Rebase on top of selected commit";
            this.chkRebaseOnTopOfSelectedCommit.ThreeState = true;
            this.chkRebaseOnTopOfSelectedCommit.UseVisualStyleBackColor = true;
            // 
            // chkSecondAbortConfirmation
            // 
            this.chkSecondAbortConfirmation.AutoSize = true;
            this.chkSecondAbortConfirmation.Location = new System.Drawing.Point(3, 228);
            this.chkSecondAbortConfirmation.Name = "chkSecondAbortConfirmation";
            this.chkSecondAbortConfirmation.Size = new System.Drawing.Size(267, 19);
            this.chkSecondAbortConfirmation.TabIndex = 11;
            this.chkSecondAbortConfirmation.Text = "Confirm for the second time to abort a merge";
            this.chkSecondAbortConfirmation.ThreeState = true;
            this.chkSecondAbortConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkCommitAfterConflictsResolved
            // 
            this.chkCommitAfterConflictsResolved.AutoSize = true;
            this.chkCommitAfterConflictsResolved.Location = new System.Drawing.Point(3, 203);
            this.chkCommitAfterConflictsResolved.Name = "chkCommitAfterConflictsResolved";
            this.chkCommitAfterConflictsResolved.Size = new System.Drawing.Size(296, 19);
            this.chkCommitAfterConflictsResolved.TabIndex = 10;
            this.chkCommitAfterConflictsResolved.Text = "Commit changes after conflicts have been resolved";
            this.chkCommitAfterConflictsResolved.ThreeState = true;
            this.chkCommitAfterConflictsResolved.UseVisualStyleBackColor = true;
            // 
            // chkResolveConflicts
            // 
            this.chkResolveConflicts.AutoSize = true;
            this.chkResolveConflicts.Location = new System.Drawing.Point(3, 178);
            this.chkResolveConflicts.Name = "chkResolveConflicts";
            this.chkResolveConflicts.Size = new System.Drawing.Size(114, 19);
            this.chkResolveConflicts.TabIndex = 9;
            this.chkResolveConflicts.Text = "Resolve conflicts";
            this.chkResolveConflicts.ThreeState = true;
            this.chkResolveConflicts.UseVisualStyleBackColor = true;
            // 
            // chkAmend
            // 
            this.chkAmend.AutoSize = true;
            this.chkAmend.Location = new System.Drawing.Point(3, 3);
            this.chkAmend.Name = "chkAmend";
            this.chkAmend.Size = new System.Drawing.Size(131, 19);
            this.chkAmend.TabIndex = 1;
            this.chkAmend.Text = "Amend last commit";
            this.chkAmend.UseVisualStyleBackColor = true;
            // 
            // chkCommitIfNoBranch
            // 
            this.chkCommitIfNoBranch.AutoSize = true;
            this.chkCommitIfNoBranch.Location = new System.Drawing.Point(3, 28);
            this.chkCommitIfNoBranch.Name = "chkCommitIfNoBranch";
            this.chkCommitIfNoBranch.Size = new System.Drawing.Size(372, 19);
            this.chkCommitIfNoBranch.TabIndex = 2;
            this.chkCommitIfNoBranch.Text = "Commit when no branch is currently checked out (headless state)";
            this.chkCommitIfNoBranch.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterPull
            // 
            this.chkAutoPopStashAfterPull.AutoSize = true;
            this.chkAutoPopStashAfterPull.Location = new System.Drawing.Point(3, 53);
            this.chkAutoPopStashAfterPull.Name = "chkAutoPopStashAfterPull";
            this.chkAutoPopStashAfterPull.Size = new System.Drawing.Size(448, 19);
            this.chkAutoPopStashAfterPull.TabIndex = 3;
            this.chkAutoPopStashAfterPull.Text = "Apply stashed changes after successful pull (stash will be popped automatically)";
            this.chkAutoPopStashAfterPull.ThreeState = true;
            this.chkAutoPopStashAfterPull.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterCheckout
            // 
            this.chkAutoPopStashAfterCheckout.AutoSize = true;
            this.chkAutoPopStashAfterCheckout.Location = new System.Drawing.Point(3, 78);
            this.chkAutoPopStashAfterCheckout.Name = "chkAutoPopStashAfterCheckout";
            this.chkAutoPopStashAfterCheckout.Size = new System.Drawing.Size(477, 19);
            this.chkAutoPopStashAfterCheckout.TabIndex = 4;
            this.chkAutoPopStashAfterCheckout.Text = "Apply stashed changes after successful checkout (stash will be popped automatical" +
    "ly)";
            this.chkAutoPopStashAfterCheckout.ThreeState = true;
            this.chkAutoPopStashAfterCheckout.UseVisualStyleBackColor = true;
            // 
            // chkConfirmStashDrop
            // 
            this.chkConfirmStashDrop.AutoSize = true;
            this.chkConfirmStashDrop.Location = new System.Drawing.Point(3, 78);
            this.chkConfirmStashDrop.Name = "chkConfirmStashDrop";
            this.chkConfirmStashDrop.Size = new System.Drawing.Size(477, 19);
            this.chkConfirmStashDrop.TabIndex = 5;
            this.chkConfirmStashDrop.Text = "Drop stash";
            this.chkConfirmStashDrop.UseVisualStyleBackColor = true;
            // 
            // chkAddTrackingRef
            // 
            this.chkAddTrackingRef.AutoSize = true;
            this.chkAddTrackingRef.Location = new System.Drawing.Point(3, 103);
            this.chkAddTrackingRef.Name = "chkAddTrackingRef";
            this.chkAddTrackingRef.Size = new System.Drawing.Size(289, 19);
            this.chkAddTrackingRef.TabIndex = 6;
            this.chkAddTrackingRef.Text = "Add a tracking reference for newly pushed branch";
            this.chkAddTrackingRef.UseVisualStyleBackColor = true;
            // 
            // chkPushNewBranch
            // 
            this.chkPushNewBranch.AutoSize = true;
            this.chkPushNewBranch.Location = new System.Drawing.Point(3, 128);
            this.chkPushNewBranch.Name = "chkPushNewBranch";
            this.chkPushNewBranch.Size = new System.Drawing.Size(205, 19);
            this.chkPushNewBranch.TabIndex = 7;
            this.chkPushNewBranch.Text = "Push a new branch for the remote";
            this.chkPushNewBranch.UseVisualStyleBackColor = true;
            // 
            // chkUpdateModules
            // 
            this.chkUpdateModules.AutoSize = true;
            this.chkUpdateModules.Location = new System.Drawing.Point(3, 153);
            this.chkUpdateModules.Name = "chkUpdateModules";
            this.chkUpdateModules.Size = new System.Drawing.Size(201, 19);
            this.chkUpdateModules.TabIndex = 8;
            this.chkUpdateModules.Text = "Update submodules on checkout";
            this.chkUpdateModules.ThreeState = true;
            this.chkUpdateModules.UseVisualStyleBackColor = true;
            // 
            // ConfirmationsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpnlMain);
            this.Name = "ConfirmationsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1001, 614);
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.gbConfirmations.ResumeLayout(false);
            this.gbConfirmations.PerformLayout();
            this.tlpnlConfirmations.ResumeLayout(false);
            this.tlpnlConfirmations.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
        private System.Windows.Forms.GroupBox gbConfirmations;
        private System.Windows.Forms.TableLayoutPanel tlpnlConfirmations;
        private System.Windows.Forms.CheckBox chkAmend;
        private System.Windows.Forms.CheckBox chkAutoPopStashAfterPull;
        private System.Windows.Forms.CheckBox chkPushNewBranch;
        private System.Windows.Forms.CheckBox chkAddTrackingRef;
        private System.Windows.Forms.CheckBox chkAutoPopStashAfterCheckout;
        private System.Windows.Forms.CheckBox chkConfirmStashDrop;
        private System.Windows.Forms.CheckBox chkUpdateModules;
        private System.Windows.Forms.CheckBox chkCommitIfNoBranch;
        private System.Windows.Forms.CheckBox chkCommitAfterConflictsResolved;
        private System.Windows.Forms.CheckBox chkResolveConflicts;
        private System.Windows.Forms.CheckBox chkSecondAbortConfirmation;
        private System.Windows.Forms.CheckBox chkRebaseOnTopOfSelectedCommit;
        private System.Windows.Forms.CheckBox chkUndoLastCommitConfirmation;
        private System.Windows.Forms.CheckBox chkFetchAndPruneAllConfirmation;
    }
}
