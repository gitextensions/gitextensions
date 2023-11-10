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
            lblGroupBranches = new Label();
            lblGroupStashes = new Label();
            lblGroupConflictResolution = new Label();
            lblGroupSubmodules = new Label();
            lblGroupWorktrees = new Label();
            lblGroupCommits = new Label();
            tlpnlMain = new TableLayoutPanel();
            gbConfirmations = new GroupBox();
            tlpnlConfirmations = new TableLayoutPanel();
            chkAmend = new CheckBox();
            chkUndoLastCommitConfirmation = new CheckBox();
            chkCommitIfNoBranch = new CheckBox();
            chkRebaseOnTopOfSelectedCommit = new CheckBox();
            chkFetchAndPruneAllConfirmation = new CheckBox();
            chkPushNewBranch = new CheckBox();
            chkBranchDeleteUnmerged = new CheckBox();
            chkAddTrackingRef = new CheckBox();
            chkAutoPopStashAfterCheckout = new CheckBox();
            chkAutoPopStashAfterPull = new CheckBox();
            chkConfirmStashDrop = new CheckBox();
            chkResolveConflicts = new CheckBox();
            chkCommitAfterConflictsResolved = new CheckBox();
            chkSecondAbortConfirmation = new CheckBox();
            chkUpdateModules = new CheckBox();
            chkSwitchWorktree = new CheckBox();
            chkBranchCheckoutConfirmation = new CheckBox();
            tlpnlMain.SuspendLayout();
            gbConfirmations.SuspendLayout();
            tlpnlConfirmations.SuspendLayout();
            SuspendLayout();
            // 
            // lblGroupBranches
            // 
            lblGroupBranches.AutoSize = true;
            lblGroupBranches.Dock = DockStyle.Fill;
            lblGroupBranches.Location = new Point(3, 127);
            lblGroupBranches.Name = "lblGroupBranches";
            lblGroupBranches.Size = new Size(1385, 15);
            lblGroupBranches.TabIndex = 0;
            lblGroupBranches.Text = "Branches:";
            // 
            // lblGroupStashes
            // 
            lblGroupStashes.AutoSize = true;
            lblGroupStashes.Dock = DockStyle.Fill;
            lblGroupStashes.Location = new Point(3, 279);
            lblGroupStashes.Name = "lblGroupStashes";
            lblGroupStashes.Size = new Size(1385, 15);
            lblGroupStashes.TabIndex = 0;
            lblGroupStashes.Text = "Stash:";
            // 
            // lblGroupConflictResolution
            // 
            lblGroupConflictResolution.AutoSize = true;
            lblGroupConflictResolution.Dock = DockStyle.Fill;
            lblGroupConflictResolution.Location = new Point(3, 381);
            lblGroupConflictResolution.Name = "lblGroupConflictResolution";
            lblGroupConflictResolution.Size = new Size(1385, 15);
            lblGroupConflictResolution.TabIndex = 0;
            lblGroupConflictResolution.Text = "Rebase / conflict resolution:";
            // 
            // lblGroupSubmodules
            // 
            lblGroupSubmodules.AutoSize = true;
            lblGroupSubmodules.Dock = DockStyle.Fill;
            lblGroupSubmodules.Location = new Point(3, 483);
            lblGroupSubmodules.Name = "lblGroupSubmodules";
            lblGroupSubmodules.Size = new Size(1385, 15);
            lblGroupSubmodules.TabIndex = 0;
            lblGroupSubmodules.Text = "Submodules:";
            // 
            // lblGroupWorktrees
            // 
            lblGroupWorktrees.AutoSize = true;
            lblGroupWorktrees.Dock = DockStyle.Fill;
            lblGroupWorktrees.Location = new Point(3, 535);
            lblGroupWorktrees.Name = "lblGroupWorktrees";
            lblGroupWorktrees.Size = new Size(1385, 15);
            lblGroupWorktrees.TabIndex = 0;
            lblGroupWorktrees.Text = "Worktrees:";
            // 
            // lblGroupCommits
            // 
            lblGroupCommits.AutoSize = true;
            lblGroupCommits.Dock = DockStyle.Fill;
            lblGroupCommits.Location = new Point(3, 0);
            lblGroupCommits.Name = "lblGroupCommits";
            lblGroupCommits.Size = new Size(1385, 15);
            lblGroupCommits.TabIndex = 0;
            lblGroupCommits.Text = "Commits:";
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlMain.Controls.Add(gbConfirmations, 0, 0);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 2;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.Size = new Size(1413, 742);
            tlpnlMain.TabIndex = 0;
            // 
            // gbConfirmations
            // 
            gbConfirmations.AutoSize = true;
            gbConfirmations.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbConfirmations.Controls.Add(tlpnlConfirmations);
            gbConfirmations.Dock = DockStyle.Fill;
            gbConfirmations.Location = new Point(3, 3);
            gbConfirmations.Name = "gbConfirmations";
            gbConfirmations.Padding = new Padding(8);
            gbConfirmations.Size = new Size(1407, 607);
            gbConfirmations.TabIndex = 0;
            gbConfirmations.TabStop = false;
            gbConfirmations.Text = "Confirm actions";
            // 
            // tlpnlConfirmations
            // 
            tlpnlConfirmations.AutoSize = true;
            tlpnlConfirmations.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlConfirmations.ColumnCount = 1;
            tlpnlConfirmations.ColumnStyles.Add(new ColumnStyle());
            tlpnlConfirmations.Controls.Add(lblGroupCommits, 0, 0);
            tlpnlConfirmations.Controls.Add(chkAmend, 0, 1);
            tlpnlConfirmations.Controls.Add(chkUndoLastCommitConfirmation, 0, 2);
            tlpnlConfirmations.Controls.Add(chkCommitIfNoBranch, 0, 3);
            tlpnlConfirmations.Controls.Add(chkRebaseOnTopOfSelectedCommit, 0, 4);
            tlpnlConfirmations.Controls.Add(lblGroupBranches, 0, 6);
            tlpnlConfirmations.Controls.Add(chkFetchAndPruneAllConfirmation, 0, 7);
            tlpnlConfirmations.Controls.Add(chkPushNewBranch, 0, 8);
            tlpnlConfirmations.Controls.Add(chkBranchDeleteUnmerged, 0, 10);
            tlpnlConfirmations.Controls.Add(chkAddTrackingRef, 0, 9);
            tlpnlConfirmations.Controls.Add(lblGroupStashes, 0, 13);
            tlpnlConfirmations.Controls.Add(chkAutoPopStashAfterCheckout, 0, 14);
            tlpnlConfirmations.Controls.Add(chkAutoPopStashAfterPull, 0, 15);
            tlpnlConfirmations.Controls.Add(chkConfirmStashDrop, 0, 16);
            tlpnlConfirmations.Controls.Add(lblGroupConflictResolution, 0, 18);
            tlpnlConfirmations.Controls.Add(chkResolveConflicts, 0, 19);
            tlpnlConfirmations.Controls.Add(chkCommitAfterConflictsResolved, 0, 20);
            tlpnlConfirmations.Controls.Add(chkSecondAbortConfirmation, 0, 21);
            tlpnlConfirmations.Controls.Add(lblGroupSubmodules, 0, 23);
            tlpnlConfirmations.Controls.Add(chkUpdateModules, 0, 24);
            tlpnlConfirmations.Controls.Add(lblGroupWorktrees, 0, 26);
            tlpnlConfirmations.Controls.Add(chkSwitchWorktree, 0, 27);
            tlpnlConfirmations.Controls.Add(chkBranchCheckoutConfirmation, 0, 11);
            tlpnlConfirmations.Dock = DockStyle.Fill;
            tlpnlConfirmations.Location = new Point(8, 24);
            tlpnlConfirmations.Name = "tlpnlConfirmations";
            tlpnlConfirmations.RowCount = 28;
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.RowStyles.Add(new RowStyle());
            tlpnlConfirmations.Size = new Size(1391, 575);
            tlpnlConfirmations.TabIndex = 0;
            // 
            // chkAmend
            // 
            chkAmend.AutoSize = true;
            chkAmend.Location = new Point(3, 18);
            chkAmend.Name = "chkAmend";
            chkAmend.Size = new Size(131, 19);
            chkAmend.TabIndex = 1;
            chkAmend.Text = "Amend last commit";
            chkAmend.UseVisualStyleBackColor = true;
            // 
            // chkUndoLastCommitConfirmation
            // 
            chkUndoLastCommitConfirmation.AutoSize = true;
            chkUndoLastCommitConfirmation.Location = new Point(3, 43);
            chkUndoLastCommitConfirmation.Name = "chkUndoLastCommitConfirmation";
            chkUndoLastCommitConfirmation.Size = new Size(121, 19);
            chkUndoLastCommitConfirmation.TabIndex = 2;
            chkUndoLastCommitConfirmation.Text = "Undo last commit";
            chkUndoLastCommitConfirmation.ThreeState = true;
            chkUndoLastCommitConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkCommitIfNoBranch
            // 
            chkCommitIfNoBranch.AutoSize = true;
            chkCommitIfNoBranch.Location = new Point(3, 68);
            chkCommitIfNoBranch.Name = "chkCommitIfNoBranch";
            chkCommitIfNoBranch.Size = new Size(372, 19);
            chkCommitIfNoBranch.TabIndex = 3;
            chkCommitIfNoBranch.Text = "Commit when no branch is currently checked out (headless state)";
            chkCommitIfNoBranch.UseVisualStyleBackColor = true;
            // 
            // chkRebaseOnTopOfSelectedCommit
            // 
            chkRebaseOnTopOfSelectedCommit.AutoSize = true;
            chkRebaseOnTopOfSelectedCommit.Location = new Point(3, 93);
            chkRebaseOnTopOfSelectedCommit.Name = "chkRebaseOnTopOfSelectedCommit";
            chkRebaseOnTopOfSelectedCommit.Size = new Size(206, 19);
            chkRebaseOnTopOfSelectedCommit.TabIndex = 4;
            chkRebaseOnTopOfSelectedCommit.Text = "Rebase on top of selected commit";
            chkRebaseOnTopOfSelectedCommit.ThreeState = true;
            chkRebaseOnTopOfSelectedCommit.UseVisualStyleBackColor = true;
            // 
            // chkFetchAndPruneAllConfirmation
            // 
            chkFetchAndPruneAllConfirmation.AutoSize = true;
            chkFetchAndPruneAllConfirmation.Location = new Point(3, 145);
            chkFetchAndPruneAllConfirmation.Name = "chkFetchAndPruneAllConfirmation";
            chkFetchAndPruneAllConfirmation.Size = new Size(163, 19);
            chkFetchAndPruneAllConfirmation.TabIndex = 5;
            chkFetchAndPruneAllConfirmation.Text = "Fetch and prune branches";
            chkFetchAndPruneAllConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkPushNewBranch
            // 
            chkPushNewBranch.AutoSize = true;
            chkPushNewBranch.Location = new Point(3, 170);
            chkPushNewBranch.Name = "chkPushNewBranch";
            chkPushNewBranch.Size = new Size(205, 19);
            chkPushNewBranch.TabIndex = 6;
            chkPushNewBranch.Text = "Push a new branch for the remote";
            chkPushNewBranch.UseVisualStyleBackColor = true;
            // 
            // chkBranchDeleteUnmerged
            // 
            chkBranchDeleteUnmerged.AutoSize = true;
            chkBranchDeleteUnmerged.Location = new Point(3, 220);
            chkBranchDeleteUnmerged.Name = "chkBranchDeleteUnmerged";
            chkBranchDeleteUnmerged.Size = new Size(168, 19);
            chkBranchDeleteUnmerged.TabIndex = 8;
            chkBranchDeleteUnmerged.Text = "Delete unmerged branches";
            chkBranchDeleteUnmerged.UseVisualStyleBackColor = true;
            // 
            // chkAddTrackingRef
            // 
            chkAddTrackingRef.AutoSize = true;
            chkAddTrackingRef.Location = new Point(3, 195);
            chkAddTrackingRef.Name = "chkAddTrackingRef";
            chkAddTrackingRef.Size = new Size(289, 19);
            chkAddTrackingRef.TabIndex = 7;
            chkAddTrackingRef.Text = "Add a tracking reference for newly pushed branch";
            chkAddTrackingRef.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterCheckout
            // 
            chkAutoPopStashAfterCheckout.AutoSize = true;
            chkAutoPopStashAfterCheckout.Location = new Point(3, 297);
            chkAutoPopStashAfterCheckout.Name = "chkAutoPopStashAfterCheckout";
            chkAutoPopStashAfterCheckout.Size = new Size(500, 19);
            chkAutoPopStashAfterCheckout.TabIndex = 10;
            chkAutoPopStashAfterCheckout.Text = "Apply stashed changes after successful checkout (else stash will be popped automatically)";
            chkAutoPopStashAfterCheckout.ThreeState = true;
            chkAutoPopStashAfterCheckout.UseVisualStyleBackColor = true;
            // 
            // chkAutoPopStashAfterPull
            // 
            chkAutoPopStashAfterPull.AutoSize = true;
            chkAutoPopStashAfterPull.Location = new Point(3, 322);
            chkAutoPopStashAfterPull.Name = "chkAutoPopStashAfterPull";
            chkAutoPopStashAfterPull.Size = new Size(471, 19);
            chkAutoPopStashAfterPull.TabIndex = 11;
            chkAutoPopStashAfterPull.Text = "Apply stashed changes after successful pull (else stash will be popped automatically)";
            chkAutoPopStashAfterPull.ThreeState = true;
            chkAutoPopStashAfterPull.UseVisualStyleBackColor = true;
            // 
            // chkConfirmStashDrop
            // 
            chkConfirmStashDrop.AutoSize = true;
            chkConfirmStashDrop.Location = new Point(3, 347);
            chkConfirmStashDrop.Name = "chkConfirmStashDrop";
            chkConfirmStashDrop.Size = new Size(82, 19);
            chkConfirmStashDrop.TabIndex = 12;
            chkConfirmStashDrop.Text = "Drop stash";
            chkConfirmStashDrop.UseVisualStyleBackColor = true;
            // 
            // chkResolveConflicts
            // 
            chkResolveConflicts.AutoSize = true;
            chkResolveConflicts.Location = new Point(3, 399);
            chkResolveConflicts.Name = "chkResolveConflicts";
            chkResolveConflicts.Size = new Size(114, 19);
            chkResolveConflicts.TabIndex = 13;
            chkResolveConflicts.Text = "Resolve conflicts";
            chkResolveConflicts.ThreeState = true;
            chkResolveConflicts.UseVisualStyleBackColor = true;
            // 
            // chkCommitAfterConflictsResolved
            // 
            chkCommitAfterConflictsResolved.AutoSize = true;
            chkCommitAfterConflictsResolved.Location = new Point(3, 424);
            chkCommitAfterConflictsResolved.Name = "chkCommitAfterConflictsResolved";
            chkCommitAfterConflictsResolved.Size = new Size(296, 19);
            chkCommitAfterConflictsResolved.TabIndex = 14;
            chkCommitAfterConflictsResolved.Text = "Commit changes after conflicts have been resolved";
            chkCommitAfterConflictsResolved.ThreeState = true;
            chkCommitAfterConflictsResolved.UseVisualStyleBackColor = true;
            // 
            // chkSecondAbortConfirmation
            // 
            chkSecondAbortConfirmation.AutoSize = true;
            chkSecondAbortConfirmation.Location = new Point(3, 449);
            chkSecondAbortConfirmation.Name = "chkSecondAbortConfirmation";
            chkSecondAbortConfirmation.Size = new Size(267, 19);
            chkSecondAbortConfirmation.TabIndex = 15;
            chkSecondAbortConfirmation.Text = "Confirm for the second time to abort a merge";
            chkSecondAbortConfirmation.ThreeState = true;
            chkSecondAbortConfirmation.UseVisualStyleBackColor = true;
            // 
            // chkUpdateModules
            // 
            chkUpdateModules.AutoSize = true;
            chkUpdateModules.Location = new Point(3, 501);
            chkUpdateModules.Name = "chkUpdateModules";
            chkUpdateModules.Size = new Size(201, 19);
            chkUpdateModules.TabIndex = 16;
            chkUpdateModules.Text = "Update submodules on checkout";
            chkUpdateModules.ThreeState = true;
            chkUpdateModules.UseVisualStyleBackColor = true;
            // 
            // chkSwitchWorktree
            // 
            chkSwitchWorktree.AutoSize = true;
            chkSwitchWorktree.Location = new Point(3, 553);
            chkSwitchWorktree.Name = "chkSwitchWorktree";
            chkSwitchWorktree.Size = new Size(110, 19);
            chkSwitchWorktree.TabIndex = 17;
            chkSwitchWorktree.Text = "Switch worktree";
            chkSwitchWorktree.UseVisualStyleBackColor = true;
            // 
            // chkBranchCheckoutConfirmation
            // 
            chkBranchCheckoutConfirmation.AutoSize = true;
            chkBranchCheckoutConfirmation.Location = new Point(3, 245);
            chkBranchCheckoutConfirmation.Name = "chkBranchCheckoutConfirmation";
            chkBranchCheckoutConfirmation.Size = new Size(201, 19);
            chkBranchCheckoutConfirmation.TabIndex = 9;
            chkBranchCheckoutConfirmation.Text = "Checkout branch using left panel";
            chkBranchCheckoutConfirmation.UseVisualStyleBackColor = true;
            // 
            // ConfirmationsSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tlpnlMain);
            Name = "ConfirmationsSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1429, 758);
            Text = "Confirmations";
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbConfirmations.ResumeLayout(false);
            gbConfirmations.PerformLayout();
            tlpnlConfirmations.ResumeLayout(false);
            tlpnlConfirmations.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpnlMain;
        private GroupBox gbConfirmations;
        private TableLayoutPanel tlpnlConfirmations;
        private CheckBox chkAmend;
        private CheckBox chkAutoPopStashAfterPull;
        private CheckBox chkPushNewBranch;
        private CheckBox chkAddTrackingRef;
        private CheckBox chkAutoPopStashAfterCheckout;
        private CheckBox chkConfirmStashDrop;
        private CheckBox chkUpdateModules;
        private CheckBox chkCommitIfNoBranch;
        private CheckBox chkCommitAfterConflictsResolved;
        private CheckBox chkResolveConflicts;
        private CheckBox chkSecondAbortConfirmation;
        private CheckBox chkRebaseOnTopOfSelectedCommit;
        private CheckBox chkUndoLastCommitConfirmation;
        private CheckBox chkFetchAndPruneAllConfirmation;
        private CheckBox chkSwitchWorktree;
        private Label lblGroupCommits;
        private Label lblGroupBranches;
        private Label lblGroupStashes;
        private Label lblGroupConflictResolution;
        private Label lblGroupSubmodules;
        private Label lblGroupWorktrees;
        private CheckBox chkBranchDeleteUnmerged;
        private CheckBox chkBranchCheckoutConfirmation;
    }
}
