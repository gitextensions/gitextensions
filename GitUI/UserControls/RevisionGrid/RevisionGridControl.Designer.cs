using System.Windows.Forms;
using GitUI.Hotkey;
using GitUI.UserControls.RevisionGrid;

namespace GitUI
{
    partial class RevisionGridControl
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGridControl));
            this._gridView = new RevisionDataGridView();
            this.mainContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.markRevisionAsBadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markRevisionAsGoodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSkipRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopBisectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.copyToClipboardToolStripMenuItem = new GitUI.UserControls.RevisionGrid.CopyContextMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCurrentBranchToHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.createNewBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.compareToBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareWithCurrentBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareToBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareToWorkingDirectoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareSelectedCommitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAsBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.createTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.checkoutRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manipulateCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rewordCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixupCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.squashCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.navigateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBuildReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseInteractivelyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.rebaseWithAdvOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            this.mainContextMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // Graph
            //
            this._gridView.AllowUserToAddRows = false;
            this._gridView.AllowUserToDeleteRows = false;
            this._gridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this._gridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this._gridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._gridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this._gridView.ColumnHeadersVisible = false;
            this._gridView.ContextMenuStrip = this.mainContextMenu;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._gridView.DefaultCellStyle = dataGridViewCellStyle4;
            this._gridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridView.GridColor = System.Drawing.SystemColors.Window;
            this._gridView.Location = new System.Drawing.Point(0, 0);
            this._gridView.Name = "_gridView";
            this._gridView.ReadOnly = true;
            this._gridView.RowHeadersVisible = false;
            this._gridView.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this._gridView.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(185)))), ((int)(((byte)(235)))));
            this._gridView.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this._gridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._gridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._gridView.Size = new System.Drawing.Size(682, 235);
            this._gridView.StandardTab = true;
            this._gridView.TabIndex = 0;
            this._gridView.VirtualMode = true;
            // 
            // mainContextMenu
            // 
            this.mainContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.markRevisionAsBadToolStripMenuItem,
            this.markRevisionAsGoodToolStripMenuItem,
            this.bisectSkipRevisionToolStripMenuItem,
            this.stopBisectToolStripMenuItem,
            this.bisectSeparator,
            this.copyToClipboardToolStripMenuItem,
            this.toolStripSeparator8,
            this.checkoutBranchToolStripMenuItem,
            this.mergeBranchToolStripMenuItem,
            this.rebaseOnToolStripMenuItem,
            this.resetCurrentBranchToHereToolStripMenuItem,
            this.toolStripSeparator3,
            this.createNewBranchToolStripMenuItem,
            this.renameBranchToolStripMenuItem,
            this.deleteBranchToolStripMenuItem,
            this.toolStripSeparator4,
            this.createTagToolStripMenuItem,
            this.deleteTagToolStripMenuItem,
            this.toolStripSeparator2,
            this.checkoutRevisionToolStripMenuItem,
            this.revertCommitToolStripMenuItem,
            this.cherryPickCommitToolStripMenuItem,
            this.archiveRevisionToolStripMenuItem,
            this.manipulateCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.compareToolStripMenuItem,
            this.toolStripSeparator5,
            this.navigateToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.runScriptToolStripMenuItem,
            this.openBuildReportToolStripMenuItem});
            this.mainContextMenu.Name = "CreateTag";
            this.mainContextMenu.Size = new System.Drawing.Size(265, 620);
            this.mainContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuOpening);
            // 
            // markRevisionAsBadToolStripMenuItem
            // 
            this.markRevisionAsBadToolStripMenuItem.Image = GitUI.Properties.Images.BisectBad;
            this.markRevisionAsBadToolStripMenuItem.Name = "markRevisionAsBadToolStripMenuItem";
            this.markRevisionAsBadToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.markRevisionAsBadToolStripMenuItem.Text = "Mark revision as bad";
            this.markRevisionAsBadToolStripMenuItem.Click += new System.EventHandler(this.MarkRevisionAsBadToolStripMenuItemClick);
            // 
            // markRevisionAsGoodToolStripMenuItem
            // 
            this.markRevisionAsGoodToolStripMenuItem.Image = GitUI.Properties.Images.BisectGood;
            this.markRevisionAsGoodToolStripMenuItem.Name = "markRevisionAsGoodToolStripMenuItem";
            this.markRevisionAsGoodToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.markRevisionAsGoodToolStripMenuItem.Text = "Mark revision as good";
            this.markRevisionAsGoodToolStripMenuItem.Click += new System.EventHandler(this.MarkRevisionAsGoodToolStripMenuItemClick);
            // 
            // bisectSkipRevisionToolStripMenuItem
            // 
            this.bisectSkipRevisionToolStripMenuItem.Image = GitUI.Properties.Images.BisectSkip;
            this.bisectSkipRevisionToolStripMenuItem.Name = "bisectSkipRevisionToolStripMenuItem";
            this.bisectSkipRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.bisectSkipRevisionToolStripMenuItem.Text = "Skip revision";
            this.bisectSkipRevisionToolStripMenuItem.Click += new System.EventHandler(this.BisectSkipRevisionToolStripMenuItemClick);
            // 
            // stopBisectToolStripMenuItem
            // 
            this.stopBisectToolStripMenuItem.Image = GitUI.Properties.Images.BisectStop;
            this.stopBisectToolStripMenuItem.Name = "stopBisectToolStripMenuItem";
            this.stopBisectToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.stopBisectToolStripMenuItem.Text = "Stop bisect";
            this.stopBisectToolStripMenuItem.Click += new System.EventHandler(this.StopBisectToolStripMenuItemClick);
            // 
            // bisectSeparator
            // 
            this.bisectSeparator.Name = "bisectSeparator";
            this.bisectSeparator.Size = new System.Drawing.Size(261, 6);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(261, 6);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchCheckout;
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch...";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.Merge;
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.mergeBranchToolStripMenuItem.Text = "Merge into current branch...";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // rebaseOnToolStripMenuItem
            // 
            this.rebaseOnToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rebaseToolStripMenuItem,
            this.rebaseInteractivelyToolStripMenuItem,
            this.toolStripSeparator10,
            this.rebaseWithAdvOptionsToolStripMenuItem});
            this.rebaseOnToolStripMenuItem.Image = global::GitUI.Properties.Images.Rebase;
            this.rebaseOnToolStripMenuItem.Name = "rebaseOnToolStripMenuItem";
            this.rebaseOnToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.rebaseOnToolStripMenuItem.Text = "Rebase current branch on";
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            this.resetCurrentBranchToHereToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetCurrentBranchToHere;
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.resetCurrentBranchToHereToolStripMenuItem.Text = "Reset current branch to here...";
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.ResetCurrentBranchToHereToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(261, 6);
            // 
            // createNewBranchToolStripMenuItem
            // 
            this.createNewBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchCreate;
            this.createNewBranchToolStripMenuItem.Name = "createNewBranchToolStripMenuItem";
            this.createNewBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.createNewBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.createNewBranchToolStripMenuItem.Text = "Create new branch here...";
            this.createNewBranchToolStripMenuItem.Click += new System.EventHandler(this.CreateNewBranchToolStripMenuItemClick);
            // 
            // renameBranchToolStripMenuItem
            // 
            this.renameBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.Renamed;
            this.renameBranchToolStripMenuItem.Name = "renameBranchToolStripMenuItem";
            this.renameBranchToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.renameBranchToolStripMenuItem.Text = "Rename branch...";
            this.renameBranchToolStripMenuItem.Click += new System.EventHandler(this.renameBranchToolStripMenuItem_Click);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Image = global::GitUI.Properties.Images.BranchDelete;
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch...";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(220, 6);
            // 
            // compareToBranchToolStripMenuItem
            // 
            this.compareToBranchToolStripMenuItem.Name = "compareToBranchToolStripMenuItem";
            this.compareToBranchToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareToBranchToolStripMenuItem.Text = "Compare to branch...";
            this.compareToBranchToolStripMenuItem.Click += new System.EventHandler(this.CompareToBranchToolStripMenuItem_Click);
            // 
            // compareWithCurrentBranchToolStripMenuItem
            // 
            this.compareWithCurrentBranchToolStripMenuItem.Name = "compareWithCurrentBranchToolStripMenuItem";
            this.compareWithCurrentBranchToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareWithCurrentBranchToolStripMenuItem.Text = "Compare with current branch";
            this.compareWithCurrentBranchToolStripMenuItem.Click += new System.EventHandler(this.CompareWithCurrentBranchToolStripMenuItem_Click);
            // 
            // selectAsBaseToolStripMenuItem
            // 
            this.selectAsBaseToolStripMenuItem.Name = "selectAsBaseToolStripMenuItem";
            this.selectAsBaseToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.selectAsBaseToolStripMenuItem.Text = "Select as BASE to compare";
            this.selectAsBaseToolStripMenuItem.Click += new System.EventHandler(this.selectAsBaseToolStripMenuItem_Click);
            // 
            // compareToBaseToolStripMenuItem
            // 
            this.compareToBaseToolStripMenuItem.Name = "compareToBaseToolStripMenuItem";
            this.compareToBaseToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareToBaseToolStripMenuItem.Text = "Compare to BASE";
            this.compareToBaseToolStripMenuItem.Enabled = false;
            this.compareToBaseToolStripMenuItem.Click += new System.EventHandler(this.compareToBaseToolStripMenuItem_Click);
            // 
            // compareToWorkingDirectoryMenuItem
            // 
            this.compareToWorkingDirectoryMenuItem.Name = "compareToWorkingDirectoryMenuItem";
            this.compareToWorkingDirectoryMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareToWorkingDirectoryMenuItem.Text = "Compare to working directory";
            this.compareToWorkingDirectoryMenuItem.Click += new System.EventHandler(this.compareToWorkingDirectoryMenuItem_Click);
            // 
            // compareSelectedCommitsMenuItem
            // 
            this.compareSelectedCommitsMenuItem.Name = "compareSelectedCommitsMenuItem";
            this.compareSelectedCommitsMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareSelectedCommitsMenuItem.Text = "Compare selected commits";
            this.compareSelectedCommitsMenuItem.Click += new System.EventHandler(this.compareSelectedCommitsMenuItem_Click);
            // 
            // compareToolStripMenuItem
            // 
            this.compareToolStripMenuItem.Image = global::GitUI.Properties.Images.Diff;
            this.compareToolStripMenuItem.Name = "compareToolStripMenuItem";
            this.compareToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareToolStripMenuItem.Text = "Compare";
            this.compareToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                compareToBranchToolStripMenuItem,
                compareWithCurrentBranchToolStripMenuItem,
                selectAsBaseToolStripMenuItem,
                compareToBaseToolStripMenuItem,
                compareToWorkingDirectoryMenuItem,
                compareSelectedCommitsMenuItem,
            });
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(261, 6);
            // 
            // createTagToolStripMenuItem
            // 
            this.createTagToolStripMenuItem.Image = global::GitUI.Properties.Images.TagCreate;
            this.createTagToolStripMenuItem.Name = "createTagToolStripMenuItem";
            this.createTagToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.createTagToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.createTagToolStripMenuItem.Text = "Create new tag here...";
            this.createTagToolStripMenuItem.Click += new System.EventHandler(this.CreateTagToolStripMenuItemClick);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Image = global::GitUI.Properties.Images.TagDelete;
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.deleteTagToolStripMenuItem.Text = "Delete tag...";
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(261, 6);
            // 
            // checkoutRevisionToolStripMenuItem
            // 
            this.checkoutRevisionToolStripMenuItem.Image = global::GitUI.Properties.Images.Checkout;
            this.checkoutRevisionToolStripMenuItem.Name = "checkoutRevisionToolStripMenuItem";
            this.checkoutRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.checkoutRevisionToolStripMenuItem.Text = "Checkout this commit...";
            this.checkoutRevisionToolStripMenuItem.Click += new System.EventHandler(this.CheckoutRevisionToolStripMenuItemClick);
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.RevertCommit;
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.revertCommitToolStripMenuItem.Text = "Revert this commit...";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.RevertCommitToolStripMenuItemClick);
            // 
            // cherryPickCommitToolStripMenuItem
            // 
            this.cherryPickCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.CherryPick;
            this.cherryPickCommitToolStripMenuItem.Name = "cherryPickCommitToolStripMenuItem";
            this.cherryPickCommitToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.cherryPickCommitToolStripMenuItem.Text = "Cherry pick this commit...";
            this.cherryPickCommitToolStripMenuItem.Click += new System.EventHandler(this.CherryPickCommitToolStripMenuItemClick);
            // 
            // archiveRevisionToolStripMenuItem
            // 
            this.archiveRevisionToolStripMenuItem.Image = global::GitUI.Properties.Images.ArchiveRevision;
            this.archiveRevisionToolStripMenuItem.Name = "archiveRevisionToolStripMenuItem";
            this.archiveRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.archiveRevisionToolStripMenuItem.Text = "Archive this commit...";
            this.archiveRevisionToolStripMenuItem.Click += new System.EventHandler(this.ArchiveRevisionToolStripMenuItemClick);
            // 
            // manipulateCommitToolStripMenuItem
            // 
            this.manipulateCommitToolStripMenuItem.Image = global::GitUI.Properties.Images.Advanced;
            this.manipulateCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editCommitToolStripMenuItem,
            this.rewordCommitToolStripMenuItem,
            this.fixupCommitToolStripMenuItem,
            this.squashCommitToolStripMenuItem,
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem});
            this.manipulateCommitToolStripMenuItem.Name = "manipulateCommitToolStripMenuItem";
            this.manipulateCommitToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.manipulateCommitToolStripMenuItem.Text = "Advanced";
            //
            // fixupCommitToolStripMenuItem
            // 
            this.fixupCommitToolStripMenuItem.Name = "fixupCommitToolStripMenuItem";
            this.fixupCommitToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.fixupCommitToolStripMenuItem.Text = "Create a fixup commit...";
            this.fixupCommitToolStripMenuItem.Click += new System.EventHandler(this.FixupCommitToolStripMenuItemClick);
            // 
            // squashCommitToolStripMenuItem
            // 
            this.squashCommitToolStripMenuItem.Name = "squashCommitToolStripMenuItem";
            this.squashCommitToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.squashCommitToolStripMenuItem.Text = "Create a squash commit...";
            this.squashCommitToolStripMenuItem.Click += new System.EventHandler(this.SquashCommitToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(220, 6);
            // 
            // navigateToolStripMenuItem
            // 
            this.navigateToolStripMenuItem.Image = global::GitUI.Properties.Images.GotoCommit;
            this.navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            this.navigateToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.navigateToolStripMenuItem.Text = "Navigate";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Image = global::GitUI.Properties.Images.AdvancedSettings;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // runScriptToolStripMenuItem
            // 
            this.runScriptToolStripMenuItem.Image = global::GitUI.Properties.Images.Console;
            this.runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            this.runScriptToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.runScriptToolStripMenuItem.Text = "Run script";
            // 
            // openBuildReportToolStripMenuItem
            // 
            this.openBuildReportToolStripMenuItem.Image = global::GitUI.Properties.Images.Integration;
            this.openBuildReportToolStripMenuItem.Name = "openBuildReportToolStripMenuItem";
            this.openBuildReportToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.openBuildReportToolStripMenuItem.Text = "Open build report in the browser";
            this.openBuildReportToolStripMenuItem.Click += new System.EventHandler(this.openBuildReportToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(149, 6);
            // 
            // getHelpOnHowToUseTheseFeaturesToolStripMenuItem
            // 
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Name = "getHelpOnHowToUseTheseFeaturesToolStripMenuItem";
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Size = new System.Drawing.Size(333, 26);
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Text = "Get help on how to use these features";
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Click += new System.EventHandler(this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click);
            // 
            // editCommitToolStripMenuItem
            // 
            this.editCommitToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editCommitToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.editCommitToolStripMenuItem.Text = "Edit commit";
            this.editCommitToolStripMenuItem.Click += new System.EventHandler(this.editCommitToolStripMenuItem_Click);
            // 
            // rewordCommitToolStripMenuItem
            //
            this.rewordCommitToolStripMenuItem.Name = "rewordCommitToolStripMenuItem";
            this.rewordCommitToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.rewordCommitToolStripMenuItem.Text = "Reword commit";
            this.rewordCommitToolStripMenuItem.Click += new System.EventHandler(this.rewordCommitToolStripMenuItem_Click);
            // rebaseToolStripMenuItem
            // 
            this.rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            this.rebaseToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.rebaseToolStripMenuItem.Text = "Selected commit";
            this.rebaseToolStripMenuItem.Click += new System.EventHandler(this.ToolStripItemClickRebaseBranch);
            // 
            // rebaseInteractivelyToolStripMenuItem
            // 
            this.rebaseInteractivelyToolStripMenuItem.Name = "rebaseInteractivelyToolStripMenuItem";
            this.rebaseInteractivelyToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.rebaseInteractivelyToolStripMenuItem.Text = "Selected commit interactively...";
            this.rebaseInteractivelyToolStripMenuItem.Click += new System.EventHandler(this.OnRebaseInteractivelyClicked);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(195, 6);
            // 
            // rebaseWithAdvOptionsToolStripMenuItem
            // 
            this.rebaseWithAdvOptionsToolStripMenuItem.Name = "rebaseWithAdvOptionsToolStripMenuItem";
            this.rebaseWithAdvOptionsToolStripMenuItem.Size = new System.Drawing.Size(307, 22);
            this.rebaseWithAdvOptionsToolStripMenuItem.Text = "Selected commit with advanced options...";
            this.rebaseWithAdvOptionsToolStripMenuItem.Click += OnRebaseWithAdvOptionsClicked;
            // 
            // RevisionGrid
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this._gridView);
            this.Name = "RevisionGridControl";
            this.Size = new System.Drawing.Size(682, 235);
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.mainContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.SetShortcutKeys();
        }

        #endregion

        private RevisionDataGridView _gridView;

        private System.Windows.Forms.ContextMenuStrip mainContextMenu;
        private System.Windows.Forms.ToolStripMenuItem navigateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseInteractivelyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markRevisionAsBadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markRevisionAsGoodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopBisectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manipulateCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rewordCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixupCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem squashCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bisectSkipRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareWithCurrentBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToBaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToWorkingDirectoryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareSelectedCommitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAsBaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getHelpOnHowToUseTheseFeaturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openBuildReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseWithAdvOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createNewBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCurrentBranchToHereToolStripMenuItem;
        private GitUI.UserControls.RevisionGrid.CopyContextMenuItem copyToClipboardToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator bisectSeparator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
    }
}
