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
            this.lblGroupBranches = new System.Windows.Forms.Label();
            this.lblGroupStashes = new System.Windows.Forms.Label();
            this.lblGroupConflictResolution = new System.Windows.Forms.Label();
            this.lblGroupSubmodules = new System.Windows.Forms.Label();
            this.lblGroupWorktrees = new System.Windows.Forms.Label();
            this.lblGroupCommits = new System.Windows.Forms.Label();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbConfirmations = new System.Windows.Forms.GroupBox();
            this.tlpnlConfirmations = new System.Windows.Forms.TableLayoutPanel();
            this.chkAmend = new System.Windows.Forms.CheckBox();
            this.chkUndoLastCommitConfirmation = new System.Windows.Forms.CheckBox();
            this.chkCommitIfNoBranch = new System.Windows.Forms.CheckBox();
            this.chkRebaseOnTopOfSelectedCommit = new System.Windows.Forms.CheckBox();
            this.chkFetchAndPruneAllConfirmation = new System.Windows.Forms.CheckBox();
            this.chkPushNewBranch = new System.Windows.Forms.CheckBox();
            this.chkAddTrackingRef = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterCheckout = new System.Windows.Forms.CheckBox();
            this.chkAutoPopStashAfterPull = new System.Windows.Forms.CheckBox();
            this.chkConfirmStashDrop = new System.Windows.Forms.CheckBox();
            this.chkResolveConflicts = new System.Windows.Forms.CheckBox();
            this.chkCommitAfterConflictsResolved = new System.Windows.Forms.CheckBox();
            this.chkSecondAbortConfirmation = new System.Windows.Forms.CheckBox();
            this.chkUpdateModules = new System.Windows.Forms.CheckBox();
            this.chkSwitchWorktree = new System.Windows.Forms.CheckBox();
            this.chkBranchDeleteUnmerged = new System.Windows.Forms.CheckBox();
            this.tlpnlMain.SuspendLayout();
            this.gbConfirmations.SuspendLayout();
            this.tlpnlConfirmations.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblGroupBranches
            // 
            this.lblGroupBranches.AutoSize = true;
            this.lblGroupBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupBranches.Location = new System.Drawing.Point(3, 127);
            this.lblGroupBranches.Name = "lblGroupBranches";
            this.lblGroupBranches.Size = new System.Drawing.Size(1589, 15);
            this.lblGroupBranches.Text = "Branches:";
            // 
            // lblGroupStashes
            // 
            this.lblGroupStashes.AutoSize = true;
            this.lblGroupStashes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupStashes.Location = new System.Drawing.Point(3, 254);
            this.lblGroupStashes.Name = "lblGroupStashes";
            this.lblGroupStashes.Size = new System.Drawing.Size(1589, 15);
            this.lblGroupStashes.Text = "Stash:";
            // 
            // lblGroupConflictResolution
            // 
            this.lblGroupConflictResolution.AutoSize = true;
            this.lblGroupConflictResolution.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupConflictResolution.Location = new System.Drawing.Point(3, 356);
            this.lblGroupConflictResolution.Name = "lblGroupConflictResolution";
            this.lblGroupConflictResolution.Size = new System.Drawing.Size(1589, 15);
            this.lblGroupConflictResolution.Text = "Rebase / conflict resolution:";
            // 
            // lblGroupSubmodules
            // 
            this.lblGroupSubmodules.AutoSize = true;
            this.lblGroupSubmodules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupSubmodules.Location = new System.Drawing.Point(3, 458);
            this.lblGroupSubmodules.Name = "lblGroupSubmodules";
            this.lblGroupSubmodules.Size = new System.Drawing.Size(1589, 15);
            this.lblGroupSubmodules.Text = "Submodules:";
            // 
            // lblGroupWorktrees
            // 
            this.lblGroupWorktrees.AutoSize = true;
            this.lblGroupWorktrees.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupWorktrees.Location = new System.Drawing.Point(3, 510);
            this.lblGroupWorktrees.Name = "lblGroupWorktrees";
            this.lblGroupWorktrees.Size = new System.Drawing.Size(1589, 15);
            this.lblGroupWorktrees.Text = "Worktrees:";
            // 
            // lblGroupCommits
            // 
            this.lblGroupCommits.AutoSize = true;
            this.lblGroupCommits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupCommits.Location = new System.Drawing.Point(3, 0);
            this.lblGroupCommits.Name = "lblGroupCommits";
            this.lblGroupCommits.Size = new System.Drawing.Size(1589, 15);
            this.lblGroupCommits.Text = "Commits:";
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.Controls.Add(this.gbConfirmations, 0, 0);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Location = new System.Drawing.Point(8, 8);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 2;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.Size = new System.Drawing.Size(1617, 1273);
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
            this.gbConfirmations.Size = new System.Drawing.Size(1611, 582);
            this.gbConfirmations.TabStop = false;
            this.gbConfirmations.Text = "Confirm actions";
            // 
            // tlpnlConfirmations
            // 
            this.tlpnlConfirmations.AutoSize = true;
            this.tlpnlConfirmations.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlConfirmations.ColumnCount = 1;
            this.tlpnlConfirmations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlConfirmations.Controls.Add(this.lblGroupCommits, 0, 0);
            this.tlpnlConfirmations.Controls.Add(this.chkAmend, 0, 1);
            this.tlpnlConfirmations.Controls.Add(this.chkUndoLastCommitConfirmation, 0, 2);
            this.tlpnlConfirmations.Controls.Add(this.chkCommitIfNoBranch, 0, 3);
            this.tlpnlConfirmations.Controls.Add(this.chkRebaseOnTopOfSelectedCommit, 0, 4);
            this.tlpnlConfirmations.Controls.Add(this.lblGroupBranches, 0, 6);
            this.tlpnlConfirmations.Controls.Add(this.chkFetchAndPruneAllConfirmation, 0, 7);
            this.tlpnlConfirmations.Controls.Add(this.chkPushNewBranch, 0, 8);
            this.tlpnlConfirmations.Controls.Add(this.chkBranchDeleteUnmerged, 0, 10);
            this.tlpnlConfirmations.Controls.Add(this.chkAddTrackingRef, 0, 9);
            this.tlpnlConfirmations.Controls.Add(this.lblGroupStashes, 0, 12);
            this.tlpnlConfirmations.Controls.Add(this.chkAutoPopStashAfterCheckout, 0, 13);
            this.tlpnlConfirmations.Controls.Add(this.chkAutoPopStashAfterPull, 0, 14);
            this.tlpnlConfirmations.Controls.Add(this.chkConfirmStashDrop, 0, 15);
            this.tlpnlConfirmations.Controls.Add(this.lblGroupConflictResolution, 0, 17);
            this.tlpnlConfirmations.Controls.Add(this.chkResolveConflicts, 0, 18);
            this.tlpnlConfirmations.Controls.Add(this.chkCommitAfterConflictsResolved, 0, 19);
            this.tlpnlConfirmations.Controls.Add(this.chkSecondAbortConfirmation, 0, 20);
            this.tlpnlConfirmations.Controls.Add(this.lblGroupSubmodules, 0, 22);
            this.tlpnlConfirmations.Controls.Add(this.chkUpdateModules, 0, 23);
            this.tlpnlConfirmations.Controls.Add(this.lblGroupWorktrees, 0, 25);
            this.tlpnlConfirmations.Controls.Add(this.chkSwitchWorktree, 0, 26);
            this.tlpnlConfirmations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlConfirmations.Location = new System.Drawing.Point(8, 24);
            this.tlpnlConfirmations.Name = "tlpnlConfirmations";
            this.tlpnlConfirmations.RowCount = 27;
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlConfirmations.Size = new System.Drawing.Size(1595, 550);
            // 
            // chkAmend
            // 
            this.chkAmend.AutoSize = true;
            this.chkAmend.Location = new System.Drawing.Point(3, 18);
            this.chkAmend.Name = "chkAmend";
            this.chkAmend.Size = new System.Drawing.Size(131, 19);
            this.chkAmend.Text = "Amend last commit";
            this.chkAmend.UseVisualStyleBackColor = true;
            // 
            // chkUndoLastCommitConfirmation
            // 
            this.chkUndoLastCommitConfirmation.AutoSize = true;
            this.chkUndoLastCommitConfirmation.Location = new System.Drawing.Point(3, 43);
            this.chkUndoLastCommitConfirmation.Name = "chkUndoLastCommitConfirmation";
            this.chkUndoLastCommitConfirmation.Size = new System.Drawing.Size(121, 19);
            this.chkUndoLastCommitConfirmation.Text = "Undo last commit";
            this.chkUndoLastCommitConfirmation.ThreeState = true;
            this.chkUndoLastCommitConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkCommitIfNoBranch
            // 
            this.chkCommitIfNoBranch.AutoSize = true;
            this.chkCommitIfNoBranch.Location = new System.Drawing.Point(3, 68);
            this.chkCommitIfNoBranch.Name = "chkCommitIfNoBranch";
            this.chkCommitIfNoBranch.Size = new System.Drawing.Size(372, 19);
            this.chkCommitIfNoBranch.Text = "Commit when no branch is currently checked out (headless state)";
            this.chkCommitIfNoBranch.UseVisualStyleBackColor = true;
            // 
            // chkRebaseOnTopOfSelectedCommit
            // 
            this.chkRebaseOnTopOfSelectedCommit.AutoSize = true;
            this.chkRebaseOnTopOfSelectedCommit.Location = new System.Drawing.Point(3, 93);
            this.chkRebaseOnTopOfSelectedCommit.Name = "chkRebaseOnTopOfSelectedCommit";
            this.chkRebaseOnTopOfSelectedCommit.Size = new System.Drawing.Size(206, 19);
            this.chkRebaseOnTopOfSelectedCommit.Text = "Rebase on top of selected commit";
            this.chkRebaseOnTopOfSelectedCommit.ThreeState = true;
            this.chkRebaseOnTopOfSelectedCommit.UseVisualStyleBackColor = true;
            // 
            // chkFetchAndPruneAllConfirmation
            // 
            this.chkFetchAndPruneAllConfirmation.AutoSize = true;
            this.chkFetchAndPruneAllConfirmation.Location = new System.Drawing.Point(3, 145);
            this.chkFetchAndPruneAllConfirmation.Name = "chkFetchAndPruneAllConfirmation";
            this.chkFetchAndPruneAllConfirmation.Size = new System.Drawing.Size(163, 19);
            this.chkFetchAndPruneAllConfirmation.Text = "Fetch and prune branches";
            this.chkFetchAndPruneAllConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkPushNewBranch
            // 
            this.chkPushNewBranch.AutoSize = true;
            this.chkPushNewBranch.Location = new System.Drawing.Point(3, 170);
            this.chkPushNewBranch.Name = "chkPushNewBranch";
            this.chkPushNewBranch.Size = new System.Drawing.Size(205, 19);
            this.chkPushNewBranch.Text = "Push a new branch for the remote";
            this.chkPushNewBranch.UseVisualStyleBackColor = true;
            // 
            // chkAddTrackingRef
            // 
            this.chkAddTrackingRef.AutoSize = true;
            this.chkAddTrackingRef.Location = new System.Drawing.Point(3, 195);
            this.chkAddTrackingRef.Name = "chkAddTrackingRef";
            this.chkAddTrackingRef.Size = new System.Drawing.Size(289, 19);
            this.chkAddTrackingRef.Text = "Add a tracking reference for newly pushed branch";
            this.chkAddTrackingRef.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterCheckout
            // 
            this.chkAutoPopStashAfterCheckout.AutoSize = true;
            this.chkAutoPopStashAfterCheckout.Location = new System.Drawing.Point(3, 272);
            this.chkAutoPopStashAfterCheckout.Name = "chkAutoPopStashAfterCheckout";
            this.chkAutoPopStashAfterCheckout.Size = new System.Drawing.Size(500, 19);
            this.chkAutoPopStashAfterCheckout.Text = "Apply stashed changes after successful checkout (else stash will be popped automatically)";
            this.chkAutoPopStashAfterCheckout.ThreeState = true;
            this.chkAutoPopStashAfterCheckout.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterPull
            // 
            this.chkAutoPopStashAfterPull.AutoSize = true;
            this.chkAutoPopStashAfterPull.Location = new System.Drawing.Point(3, 297);
            this.chkAutoPopStashAfterPull.Name = "chkAutoPopStashAfterPull";
            this.chkAutoPopStashAfterPull.Size = new System.Drawing.Size(471, 19);
            this.chkAutoPopStashAfterPull.Text = "Apply stashed changes after successful pull (else stash will be popped automatically)";
            this.chkAutoPopStashAfterPull.ThreeState = true;
            this.chkAutoPopStashAfterPull.UseVisualStyleBackColor = true;
            // 
            // chkConfirmStashDrop
            // 
            this.chkConfirmStashDrop.AutoSize = true;
            this.chkConfirmStashDrop.Location = new System.Drawing.Point(3, 322);
            this.chkConfirmStashDrop.Name = "chkConfirmStashDrop";
            this.chkConfirmStashDrop.Size = new System.Drawing.Size(82, 19);
            this.chkConfirmStashDrop.Text = "Drop stash";
            this.chkConfirmStashDrop.UseVisualStyleBackColor = true;
            // 
            // chkResolveConflicts
            // 
            this.chkResolveConflicts.AutoSize = true;
            this.chkResolveConflicts.Location = new System.Drawing.Point(3, 374);
            this.chkResolveConflicts.Name = "chkResolveConflicts";
            this.chkResolveConflicts.Size = new System.Drawing.Size(114, 19);
            this.chkResolveConflicts.Text = "Resolve conflicts";
            this.chkResolveConflicts.ThreeState = true;
            this.chkResolveConflicts.UseVisualStyleBackColor = true;
            // 
            // chkCommitAfterConflictsResolved
            // 
            this.chkCommitAfterConflictsResolved.AutoSize = true;
            this.chkCommitAfterConflictsResolved.Location = new System.Drawing.Point(3, 399);
            this.chkCommitAfterConflictsResolved.Name = "chkCommitAfterConflictsResolved";
            this.chkCommitAfterConflictsResolved.Size = new System.Drawing.Size(296, 19);
            this.chkCommitAfterConflictsResolved.Text = "Commit changes after conflicts have been resolved";
            this.chkCommitAfterConflictsResolved.ThreeState = true;
            this.chkCommitAfterConflictsResolved.UseVisualStyleBackColor = true;
            // 
            // chkSecondAbortConfirmation
            // 
            this.chkSecondAbortConfirmation.AutoSize = true;
            this.chkSecondAbortConfirmation.Location = new System.Drawing.Point(3, 424);
            this.chkSecondAbortConfirmation.Name = "chkSecondAbortConfirmation";
            this.chkSecondAbortConfirmation.Size = new System.Drawing.Size(267, 19);
            this.chkSecondAbortConfirmation.Text = "Confirm for the second time to abort a merge";
            this.chkSecondAbortConfirmation.ThreeState = true;
            this.chkSecondAbortConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkUpdateModules
            // 
            this.chkUpdateModules.AutoSize = true;
            this.chkUpdateModules.Location = new System.Drawing.Point(3, 476);
            this.chkUpdateModules.Name = "chkUpdateModules";
            this.chkUpdateModules.Size = new System.Drawing.Size(201, 19);
            this.chkUpdateModules.Text = "Update submodules on checkout";
            this.chkUpdateModules.ThreeState = true;
            this.chkUpdateModules.UseVisualStyleBackColor = true;
            // 
            // chkSwitchWorktree
            // 
            this.chkSwitchWorktree.AutoSize = true;
            this.chkSwitchWorktree.Location = new System.Drawing.Point(3, 528);
            this.chkSwitchWorktree.Name = "chkSwitchWorktree";
            this.chkSwitchWorktree.Size = new System.Drawing.Size(110, 19);
            this.chkSwitchWorktree.Text = "Switch worktree";
            this.chkSwitchWorktree.UseVisualStyleBackColor = true;
            // 
            // chkBranchDeleteUnmerged
            // 
            this.chkBranchDeleteUnmerged.AutoSize = true;
            this.chkBranchDeleteUnmerged.Location = new System.Drawing.Point(3, 220);
            this.chkBranchDeleteUnmerged.Name = "chkBranchDeleteUnmerged";
            this.chkBranchDeleteUnmerged.Size = new System.Drawing.Size(168, 19);
            this.chkBranchDeleteUnmerged.Text = "Delete unmerged branches";
            this.chkBranchDeleteUnmerged.UseVisualStyleBackColor = true;
            // 
            // ConfirmationsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpnlMain);
            this.Name = "ConfirmationsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1633, 1289);
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.gbConfirmations.ResumeLayout(false);
            this.gbConfirmations.PerformLayout();
            this.tlpnlConfirmations.ResumeLayout(false);
            this.tlpnlConfirmations.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.CheckBox chkSwitchWorktree;
        private System.Windows.Forms.Label lblGroupCommits;
        private System.Windows.Forms.Label lblGroupBranches;
        private System.Windows.Forms.Label lblGroupStashes;
        private System.Windows.Forms.Label lblGroupConflictResolution;
        private System.Windows.Forms.Label lblGroupSubmodules;
        private System.Windows.Forms.Label lblGroupWorktrees;
        private System.Windows.Forms.CheckBox chkBranchDeleteUnmerged;
    }
}
