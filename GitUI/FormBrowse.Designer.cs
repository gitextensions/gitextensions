﻿using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;

namespace GitUI
{
    partial class FormBrowse
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBrowse));
            this.toolPanel = new System.Windows.Forms.SplitContainer();
            this.UserMenuToolStrip = new System.Windows.Forms.ToolStrip();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this._NO_TRANSLATE_Workingdir = new System.Windows.Forms.ToolStripSplitButton();
            this.branchSelect = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitStash = new System.Windows.Forms.ToolStripSplitButton();
            this.stashChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashPopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.viewStashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPull = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPush = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.GitBash = new System.Windows.Forms.ToolStripButton();
            this.EditSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripBranches = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.localToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toggleSplitViewLayout = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.commitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.committerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffContainsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.CommitInfoTabControl = new System.Windows.Forms.TabControl();
            this.CommitInfo = new System.Windows.Forms.TabPage();
            this.RevisionInfo = new GitUI.CommitInfo();
            this.Tree = new System.Windows.Forms.TabPage();
            this.FileTreeSplitContainer = new System.Windows.Forms.SplitContainer();
            this.GitTree = new System.Windows.Forms.TreeView();
            this.FileTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editCheckedOutFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFilenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileText = new GitUI.Editor.FileViewer();
            this.Diff = new System.Windows.Forms.TabPage();
            this.DiffSplitContainer = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFilenameToClipboardToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fileHistoryDiffToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.TreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.fileExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitBashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitGUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kGitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.branchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runMergetoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageRemoteRepositoriesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.PuTTYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAuthenticationAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateOrImportKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._repositoryHostsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._forkCloneRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._viewPullRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._createPullRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.updateAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synchronizeAllSubmodulesRecursiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.updateAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncronizeAllSubmodulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.openSubmoduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.gitMaintenanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verifyGitDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteIndexlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.editgitignoreToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editgitattributesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editmailmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commitcountPerUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitcommandLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changelogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.translateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolPanel.Panel1.SuspendLayout();
            this.toolPanel.Panel2.SuspendLayout();
            this.toolPanel.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.CommitInfoTabControl.SuspendLayout();
            this.CommitInfo.SuspendLayout();
            this.Tree.SuspendLayout();
            this.FileTreeSplitContainer.Panel1.SuspendLayout();
            this.FileTreeSplitContainer.Panel2.SuspendLayout();
            this.FileTreeSplitContainer.SuspendLayout();
            this.FileTreeContextMenu.SuspendLayout();
            this.Diff.SuspendLayout();
            this.DiffSplitContainer.Panel1.SuspendLayout();
            this.DiffSplitContainer.Panel2.SuspendLayout();
            this.DiffSplitContainer.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.TreeContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolPanel
            // 
            this.toolPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.toolPanel.IsSplitterFixed = true;
            this.toolPanel.Location = new System.Drawing.Point(0, 24);
            this.toolPanel.Name = "toolPanel";
            this.toolPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // toolPanel.Panel1
            // 
            this.toolPanel.Panel1.Controls.Add(this.UserMenuToolStrip);
            this.toolPanel.Panel1.Controls.Add(this.ToolStrip);
            // 
            // toolPanel.Panel2
            // 
            this.toolPanel.Panel2.Controls.Add(this.MainSplitContainer);
            this.toolPanel.Size = new System.Drawing.Size(814, 527);
            this.toolPanel.SplitterDistance = 25;
            this.toolPanel.SplitterWidth = 1;
            this.toolPanel.TabIndex = 4;
            // 
            // UserMenuToolStrip
            // 
            this.UserMenuToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.UserMenuToolStrip.Location = new System.Drawing.Point(836, 24);
            this.UserMenuToolStrip.Name = "UserMenuToolStrip";
            this.UserMenuToolStrip.Size = new System.Drawing.Size(111, 25);
            this.UserMenuToolStrip.TabIndex = 5;
            this.UserMenuToolStrip.Visible = false;
            // 
            // ToolStrip
            // 
            this.ToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshButton,
            this.toolStripSeparator17,
            this._NO_TRANSLATE_Workingdir,
            this.branchSelect,
            this.toolStripSeparator1,
            this.toolStripSplitStash,
            this.toolStripButton1,
            this.toolStripButtonPull,
            this.toolStripButtonPush,
            this.toolStripSeparator2,
            this.GitBash,
            this.EditSettings,
            this.toolStripSeparator5,
            this.toolStripLabel1,
            this.toolStripBranches,
            this.toolStripDropDownButton2,
            this.toolStripSeparator19,
            this.toolStripLabel2,
            this.toggleSplitViewLayout,
            this.toolStripTextBoxFilter,
            this.toolStripDropDownButton1});
            this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStrip.Size = new System.Drawing.Size(814, 25);
            this.ToolStrip.TabIndex = 4;
            this.ToolStrip.Text = "toolStrip1";
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = ((System.Drawing.Image)(resources.GetObject("RefreshButton.Image")));
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.White;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.ToolTipText = "Refresh";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButtonClick);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(6, 25);
            // 
            // _NO_TRANSLATE_Workingdir
            // 
            this._NO_TRANSLATE_Workingdir.Image = global::GitUI.Properties.Resources._40;
            this._NO_TRANSLATE_Workingdir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._NO_TRANSLATE_Workingdir.Name = "_NO_TRANSLATE_Workingdir";
            this._NO_TRANSLATE_Workingdir.Size = new System.Drawing.Size(99, 22);
            this._NO_TRANSLATE_Workingdir.Text = "WorkingDir";
            this._NO_TRANSLATE_Workingdir.ToolTipText = "Change working directory";
            this._NO_TRANSLATE_Workingdir.ButtonClick += new System.EventHandler(this.WorkingdirClick);
            this._NO_TRANSLATE_Workingdir.DropDownOpening += new System.EventHandler(this.WorkingdirDropDownOpening);
            // 
            // branchSelect
            // 
            this.branchSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.branchSelect.Name = "branchSelect";
            this.branchSelect.Size = new System.Drawing.Size(60, 22);
            this.branchSelect.Text = "Branch";
            this.branchSelect.ToolTipText = "Change current branch";
            this.branchSelect.ButtonClick += new System.EventHandler(this.CurrentBranchClick);
            this.branchSelect.DropDownOpening += new System.EventHandler(this.CurrentBranchDropDownOpening);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSplitStash
            // 
            this.toolStripSplitStash.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stashChangesToolStripMenuItem,
            this.stashPopToolStripMenuItem,
            this.toolStripSeparator9,
            this.viewStashToolStripMenuItem});
            this.toolStripSplitStash.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitStash.Image")));
            this.toolStripSplitStash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitStash.Name = "toolStripSplitStash";
            this.toolStripSplitStash.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitStash.ToolTipText = "Stash changes";
            this.toolStripSplitStash.ButtonClick += new System.EventHandler(this.ToolStripSplitStashButtonClick);
            // 
            // stashChangesToolStripMenuItem
            // 
            this.stashChangesToolStripMenuItem.Name = "stashChangesToolStripMenuItem";
            this.stashChangesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.stashChangesToolStripMenuItem.Text = "Stash";
            this.stashChangesToolStripMenuItem.ToolTipText = "Stash changes";
            this.stashChangesToolStripMenuItem.Click += new System.EventHandler(this.StashChangesToolStripMenuItemClick);
            // 
            // stashPopToolStripMenuItem
            // 
            this.stashPopToolStripMenuItem.Name = "stashPopToolStripMenuItem";
            this.stashPopToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.stashPopToolStripMenuItem.Text = "Stash pop";
            this.stashPopToolStripMenuItem.ToolTipText = "Apply and drop single stash";
            this.stashPopToolStripMenuItem.Click += new System.EventHandler(this.StashPopToolStripMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(126, 6);
            // 
            // viewStashToolStripMenuItem
            // 
            this.viewStashToolStripMenuItem.Name = "viewStashToolStripMenuItem";
            this.viewStashToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.viewStashToolStripMenuItem.Text = "View stash";
            this.viewStashToolStripMenuItem.ToolTipText = "View stash";
            this.viewStashToolStripMenuItem.Click += new System.EventHandler(this.ViewStashToolStripMenuItemClick);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(71, 22);
            this.toolStripButton1.Text = "Commit";
            this.toolStripButton1.Click += new System.EventHandler(this.ToolStripButton1Click);
            // 
            // toolStripButtonPull
            // 
            this.toolStripButtonPull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPull.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPull.Image")));
            this.toolStripButtonPull.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPull.Name = "toolStripButtonPull";
            this.toolStripButtonPull.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPull.Text = "Pull";
            this.toolStripButtonPull.Click += new System.EventHandler(this.ToolStripButtonPullClick);
            // 
            // toolStripButtonPush
            // 
            this.toolStripButtonPush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPush.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPush.Image")));
            this.toolStripButtonPush.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPush.Name = "toolStripButtonPush";
            this.toolStripButtonPush.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPush.Text = "Push";
            this.toolStripButtonPush.Click += new System.EventHandler(this.ToolStripButtonPushClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // GitBash
            // 
            this.GitBash.Image = ((System.Drawing.Image)(resources.GetObject("GitBash.Image")));
            this.GitBash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GitBash.Name = "GitBash";
            this.GitBash.Size = new System.Drawing.Size(23, 22);
            this.GitBash.ToolTipText = "Git bash";
            this.GitBash.Click += new System.EventHandler(this.GitBashClick);
            // 
            // EditSettings
            // 
            this.EditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.EditSettings.Image = ((System.Drawing.Image)(resources.GetObject("EditSettings.Image")));
            this.EditSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.EditSettings.Name = "EditSettings";
            this.EditSettings.Size = new System.Drawing.Size(23, 22);
            this.EditSettings.ToolTipText = "Settings";
            this.EditSettings.Click += new System.EventHandler(this.SettingsClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel1.Text = "Branches:";
            // 
            // toolStripBranches
            // 
            this.toolStripBranches.Name = "toolStripBranches";
            this.toolStripBranches.Size = new System.Drawing.Size(150, 25);
            this.toolStripBranches.DropDown += new System.EventHandler(this.toolStripBranches_DropDown);
            this.toolStripBranches.TextUpdate += new System.EventHandler(this.toolStripBranches_TextUpdate);
            this.toolStripBranches.Leave += new System.EventHandler(this.toolStripBranches_Leave);
            this.toolStripBranches.KeyUp += new System.Windows.Forms.KeyEventHandler(this.toolStripBranches_KeyUp);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localToolStripMenuItem,
            this.remoteToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(29, 22);
            // 
            // localToolStripMenuItem
            // 
            this.localToolStripMenuItem.Checked = true;
            this.localToolStripMenuItem.CheckOnClick = true;
            this.localToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.localToolStripMenuItem.Name = "localToolStripMenuItem";
            this.localToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.localToolStripMenuItem.Text = "Local";
            // 
            // remoteToolStripMenuItem
            // 
            this.remoteToolStripMenuItem.CheckOnClick = true;
            this.remoteToolStripMenuItem.Name = "remoteToolStripMenuItem";
            this.remoteToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.remoteToolStripMenuItem.Text = "Remote";
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel2.Text = "Filter:";
            this.toolStripLabel2.Click += new System.EventHandler(this.ToolStripLabel2Click);
            // 
            // toggleSplitViewLayout
            // 
            this.toggleSplitViewLayout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toggleSplitViewLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleSplitViewLayout.Image = global::GitUI.Properties.Resources.SplitViewLayout;
            this.toggleSplitViewLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleSplitViewLayout.Name = "toggleSplitViewLayout";
            this.toggleSplitViewLayout.Size = new System.Drawing.Size(23, 22);
            this.toggleSplitViewLayout.ToolTipText = "Toggle split view layout";
            this.toggleSplitViewLayout.Click += new System.EventHandler(this.toggleSplitViewLayout_Click);
            // 
            // toolStripTextBoxFilter
            // 
            this.toolStripTextBoxFilter.ForeColor = System.Drawing.Color.Black;
            this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            this.toolStripTextBoxFilter.Size = new System.Drawing.Size(120, 23);
            this.toolStripTextBoxFilter.Leave += new System.EventHandler(this.ToolStripTextBoxFilterLeave);
            this.toolStripTextBoxFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ToolStripTextBoxFilterKeyPress);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitToolStripMenuItem1,
            this.committerToolStripMenuItem,
            this.authorToolStripMenuItem,
            this.diffContainsToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 20);
            // 
            // commitToolStripMenuItem1
            // 
            this.commitToolStripMenuItem1.Checked = true;
            this.commitToolStripMenuItem1.CheckOnClick = true;
            this.commitToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.commitToolStripMenuItem1.Name = "commitToolStripMenuItem1";
            this.commitToolStripMenuItem1.Size = new System.Drawing.Size(184, 22);
            this.commitToolStripMenuItem1.Text = "Commit";
            // 
            // committerToolStripMenuItem
            // 
            this.committerToolStripMenuItem.CheckOnClick = true;
            this.committerToolStripMenuItem.Name = "committerToolStripMenuItem";
            this.committerToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.committerToolStripMenuItem.Text = "Committer";
            // 
            // authorToolStripMenuItem
            // 
            this.authorToolStripMenuItem.CheckOnClick = true;
            this.authorToolStripMenuItem.Name = "authorToolStripMenuItem";
            this.authorToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.authorToolStripMenuItem.Text = "Author";
            // 
            // diffContainsToolStripMenuItem
            // 
            this.diffContainsToolStripMenuItem.CheckOnClick = true;
            this.diffContainsToolStripMenuItem.Name = "diffContainsToolStripMenuItem";
            this.diffContainsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.diffContainsToolStripMenuItem.Text = "Diff contains (SLOW)";
            this.diffContainsToolStripMenuItem.Click += new System.EventHandler(this.diffContainsToolStripMenuItem_Click);
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.RevisionGrid);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.CommitInfoTabControl);
            this.MainSplitContainer.Panel2MinSize = 0;
            this.MainSplitContainer.Size = new System.Drawing.Size(814, 501);
            this.MainSplitContainer.SplitterDistance = 215;
            this.MainSplitContainer.TabIndex = 1;
            this.MainSplitContainer.TabStop = false;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.AllowGraphWithFilter = false;
            this.RevisionGrid.BranchFilter = "";
            this.RevisionGrid.CurrentCheckout = "";
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.RevisionGrid.InMemAuthorFilter = "";
            this.RevisionGrid.InMemCommitterFilter = "";
            this.RevisionGrid.InMemFilterIgnoreCase = false;
            this.RevisionGrid.InMemMessageFilter = "";
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.RevisionGrid.Size = new System.Drawing.Size(814, 215);
            this.RevisionGrid.TabIndex = 0;
            this.RevisionGrid.DoubleClick += new System.EventHandler(this.RevisionGridDoubleClick);
            // 
            // CommitInfoTabControl
            // 
            this.CommitInfoTabControl.Controls.Add(this.CommitInfo);
            this.CommitInfoTabControl.Controls.Add(this.Tree);
            this.CommitInfoTabControl.Controls.Add(this.Diff);
            this.CommitInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.CommitInfoTabControl.Name = "CommitInfoTabControl";
            this.CommitInfoTabControl.SelectedIndex = 0;
            this.CommitInfoTabControl.Size = new System.Drawing.Size(814, 282);
            this.CommitInfoTabControl.TabIndex = 0;
            this.CommitInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // CommitInfo
            // 
            this.CommitInfo.Controls.Add(this.RevisionInfo);
            this.CommitInfo.Location = new System.Drawing.Point(4, 24);
            this.CommitInfo.Margin = new System.Windows.Forms.Padding(15);
            this.CommitInfo.Name = "CommitInfo";
            this.CommitInfo.Size = new System.Drawing.Size(806, 254);
            this.CommitInfo.TabIndex = 2;
            this.CommitInfo.Text = "Commit";
            this.CommitInfo.UseVisualStyleBackColor = true;
            // 
            // RevisionInfo
            // 
            this.RevisionInfo.BackColor = System.Drawing.SystemColors.Window;
            this.RevisionInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RevisionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionInfo.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.RevisionInfo.Location = new System.Drawing.Point(0, 0);
            this.RevisionInfo.Margin = new System.Windows.Forms.Padding(10);
            this.RevisionInfo.Name = "RevisionInfo";
            this.RevisionInfo.Size = new System.Drawing.Size(806, 254);
            this.RevisionInfo.TabIndex = 1;
            // 
            // Tree
            // 
            this.Tree.Controls.Add(this.FileTreeSplitContainer);
            this.Tree.Location = new System.Drawing.Point(4, 24);
            this.Tree.Name = "Tree";
            this.Tree.Padding = new System.Windows.Forms.Padding(3);
            this.Tree.Size = new System.Drawing.Size(806, 254);
            this.Tree.TabIndex = 0;
            this.Tree.Text = "File tree";
            this.Tree.UseVisualStyleBackColor = true;
            // 
            // FileTreeSplitContainer
            // 
            this.FileTreeSplitContainer.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::GitUI.Properties.Settings.Default, "FormBrowse_FileTreeSplitContainer_SplitterDistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FileTreeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileTreeSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.FileTreeSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.FileTreeSplitContainer.Name = "FileTreeSplitContainer";
            // 
            // FileTreeSplitContainer.Panel1
            // 
            this.FileTreeSplitContainer.Panel1.Controls.Add(this.GitTree);
            // 
            // FileTreeSplitContainer.Panel2
            // 
            this.FileTreeSplitContainer.Panel2.Controls.Add(this.FileText);
            this.FileTreeSplitContainer.Size = new System.Drawing.Size(800, 248);
            this.FileTreeSplitContainer.SplitterDistance = global::GitUI.Properties.Settings.Default.FormBrowse_FileTreeSplitContainer_SplitterDistance;
            this.FileTreeSplitContainer.TabIndex = 1;
            // 
            // GitTree
            // 
            this.GitTree.ContextMenuStrip = this.FileTreeContextMenu;
            this.GitTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitTree.HideSelection = false;
            this.GitTree.Location = new System.Drawing.Point(0, 0);
            this.GitTree.Name = "GitTree";
            this.GitTree.Size = new System.Drawing.Size(215, 248);
            this.GitTree.TabIndex = 0;
            this.GitTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.GitTreeBeforeExpand);
            this.GitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1AfterSelect);
            this.GitTree.DoubleClick += new System.EventHandler(this.GitTreeDoubleClick);
            this.GitTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GitTreeMouseDown);
            // 
            // FileTreeContextMenu
            // 
            this.FileTreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.openFileToolStripMenuItem,
            this.openFileWithToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.editCheckedOutFileToolStripMenuItem,
            this.toolStripSeparator20,
            this.copyFilenameToClipboardToolStripMenuItem,
            this.fileHistoryToolStripMenuItem,
            this.toolStripSeparator18,
            this.findToolStripMenuItem});
            this.FileTreeContextMenu.Name = "FileTreeContextMenu";
            this.FileTreeContextMenu.Size = new System.Drawing.Size(269, 192);
            this.FileTreeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.FileTreeContextMenu_Opening);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsOnClick);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.openFileToolStripMenuItem.Text = "Open this revision (temp file)";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.OpenOnClick);
            // 
            // openFileWithToolStripMenuItem
            // 
            this.openFileWithToolStripMenuItem.Name = "openFileWithToolStripMenuItem";
            this.openFileWithToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.openFileWithToolStripMenuItem.Text = "Open this revision with... (temp file)";
            this.openFileWithToolStripMenuItem.Click += new System.EventHandler(this.OpenWithOnClick);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.openWithToolStripMenuItem.Text = "Open checked out file with...";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // editCheckedOutFileToolStripMenuItem
            // 
            this.editCheckedOutFileToolStripMenuItem.Name = "editCheckedOutFileToolStripMenuItem";
            this.editCheckedOutFileToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.editCheckedOutFileToolStripMenuItem.Text = "Edit checked out file";
            this.editCheckedOutFileToolStripMenuItem.Click += new System.EventHandler(this.editCheckedOutFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(265, 6);
            // 
            // copyFilenameToClipboardToolStripMenuItem
            // 
            this.copyFilenameToClipboardToolStripMenuItem.Name = "copyFilenameToClipboardToolStripMenuItem";
            this.copyFilenameToClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.copyFilenameToClipboardToolStripMenuItem.Text = "Copy filename to clipboard";
            this.copyFilenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem_Click);
            // 
            // fileHistoryToolStripMenuItem
            // 
            this.fileHistoryToolStripMenuItem.Name = "fileHistoryToolStripMenuItem";
            this.fileHistoryToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.fileHistoryToolStripMenuItem.Text = "File history";
            this.fileHistoryToolStripMenuItem.Click += new System.EventHandler(this.FileHistoryOnClick);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(265, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.FindFileOnClick);
            // 
            // FileText
            // 
            this.FileText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileText.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.FileText.IgnoreWhitespaceChanges = false;
            this.FileText.IsReadOnly = true;
            this.FileText.Location = new System.Drawing.Point(0, 0);
            this.FileText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FileText.Name = "FileText";
            this.FileText.NumberOfVisibleLines = 3;
            this.FileText.ScrollPos = 0;
            this.FileText.ShowEntireFile = false;
            this.FileText.ShowLineNumbers = true;
            this.FileText.Size = new System.Drawing.Size(581, 248);
            this.FileText.TabIndex = 0;
            this.FileText.TreatAllFilesAsText = false;
            // 
            // Diff
            // 
            this.Diff.Controls.Add(this.DiffSplitContainer);
            this.Diff.Location = new System.Drawing.Point(4, 24);
            this.Diff.Name = "Diff";
            this.Diff.Size = new System.Drawing.Size(806, 254);
            this.Diff.TabIndex = 1;
            this.Diff.Text = "Diff";
            this.Diff.UseVisualStyleBackColor = true;
            // 
            // DiffSplitContainer
            // 
            this.DiffSplitContainer.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::GitUI.Properties.Settings.Default, "FormBrowse_DiffSplitContainer_SplitterDistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DiffSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.DiffSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.DiffSplitContainer.Name = "DiffSplitContainer";
            // 
            // DiffSplitContainer.Panel1
            // 
            this.DiffSplitContainer.Panel1.Controls.Add(this.DiffFiles);
            // 
            // DiffSplitContainer.Panel2
            // 
            this.DiffSplitContainer.Panel2.Controls.Add(this.DiffText);
            this.DiffSplitContainer.Size = new System.Drawing.Size(806, 254);
            this.DiffSplitContainer.SplitterDistance = global::GitUI.Properties.Settings.Default.FormBrowse_DiffSplitContainer_SplitterDistance;
            this.DiffSplitContainer.TabIndex = 0;
            // 
            // DiffFiles
            // 
            this.DiffFiles.ContextMenuStrip = this.DiffContextMenu;
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.DiffFiles.GitItemStatuses = null;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Revision = null;
            this.DiffFiles.SelectedIndex = -1;
            this.DiffFiles.SelectedItem = null;
            this.DiffFiles.Size = new System.Drawing.Size(215, 254);
            this.DiffFiles.TabIndex = 1;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFilesSelectedIndexChanged);
            this.DiffFiles.DoubleClick += new System.EventHandler(this.DiffFilesDoubleClick);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.copyFilenameToClipboardToolStripMenuItem1,
            this.saveAsToolStripMenuItem1,
            this.fileHistoryDiffToolstripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(261, 92);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // copyFilenameToClipboardToolStripMenuItem1
            // 
            this.copyFilenameToClipboardToolStripMenuItem1.Name = "copyFilenameToClipboardToolStripMenuItem1";
            this.copyFilenameToClipboardToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyFilenameToClipboardToolStripMenuItem1.Size = new System.Drawing.Size(260, 22);
            this.copyFilenameToClipboardToolStripMenuItem1.Text = "Copy filename to clipboard";
            this.copyFilenameToClipboardToolStripMenuItem1.Click += new System.EventHandler(this.copyFilenameToClipboardToolStripMenuItem1_Click);
            // 
            // saveAsToolStripMenuItem1
            // 
            this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            this.saveAsToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(260, 22);
            this.saveAsToolStripMenuItem1.Text = "Save as";
            this.saveAsToolStripMenuItem1.Click += new System.EventHandler(this.saveAsToolStripMenuItem1_Click);
            // 
            // fileHistoryDiffToolstripMenuItem
            // 
            this.fileHistoryDiffToolstripMenuItem.Name = "fileHistoryDiffToolstripMenuItem";
            this.fileHistoryDiffToolstripMenuItem.Size = new System.Drawing.Size(260, 22);
            this.fileHistoryDiffToolstripMenuItem.Text = "File history";
            this.fileHistoryDiffToolstripMenuItem.Click += new System.EventHandler(this.fileHistoryDiffToolstripMenuItem_Click);
            // 
            // DiffText
            // 
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.DiffText.IgnoreWhitespaceChanges = false;
            this.DiffText.IsReadOnly = true;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffText.Name = "DiffText";
            this.DiffText.NumberOfVisibleLines = 3;
            this.DiffText.ScrollPos = 0;
            this.DiffText.ShowEntireFile = false;
            this.DiffText.ShowLineNumbers = true;
            this.DiffText.Size = new System.Drawing.Size(587, 254);
            this.DiffText.TabIndex = 0;
            this.DiffText.TreatAllFilesAsText = false;
            // 
            // TreeContextMenu
            // 
            this.TreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.TreeContextMenu.Name = "TreeContextMenu";
            this.TreeContextMenu.Size = new System.Drawing.Size(99, 26);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 551);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.statusStrip.Size = new System.Drawing.Size(814, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(14, 17);
            this.toolStripStatusLabel1.Text = "X";
            this.toolStripStatusLabel1.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.toolStripSeparator12,
            this.fileExplorerToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.FileToolStripMenuItemDropDownOpening);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItemClick);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("refreshToolStripMenuItem.Image")));
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.recentToolStripMenuItem.Text = "Recent Repositories";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(83, 22);
            this.toolStripMenuItem2.Text = "...";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(174, 6);
            // 
            // fileExplorerToolStripMenuItem
            // 
            this.fileExplorerToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("fileExplorerToolStripMenuItem.Image")));
            this.fileExplorerToolStripMenuItem.Name = "fileExplorerToolStripMenuItem";
            this.fileExplorerToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.fileExplorerToolStripMenuItem.Text = "File Explorer";
            this.fileExplorerToolStripMenuItem.Click += new System.EventHandler(this.FileExplorerToolStripMenuItemClick);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(174, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // gitToolStripMenuItem
            // 
            this.gitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitBashToolStripMenuItem,
            this.gitGUIToolStripMenuItem,
            this.kGitToolStripMenuItem});
            this.gitToolStripMenuItem.Name = "gitToolStripMenuItem";
            this.gitToolStripMenuItem.Size = new System.Drawing.Size(34, 20);
            this.gitToolStripMenuItem.Text = "Git";
            // 
            // gitBashToolStripMenuItem
            // 
            this.gitBashToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("gitBashToolStripMenuItem.Image")));
            this.gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            this.gitBashToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.gitBashToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.gitBashToolStripMenuItem.Text = "Git bash";
            this.gitBashToolStripMenuItem.Click += new System.EventHandler(this.GitBashToolStripMenuItemClick1);
            // 
            // gitGUIToolStripMenuItem
            // 
            this.gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            this.gitGUIToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.gitGUIToolStripMenuItem.Text = "Git GUI";
            this.gitGUIToolStripMenuItem.Click += new System.EventHandler(this.GitGuiToolStripMenuItemClick);
            // 
            // kGitToolStripMenuItem
            // 
            this.kGitToolStripMenuItem.Name = "kGitToolStripMenuItem";
            this.kGitToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.kGitToolStripMenuItem.Text = "GitK";
            this.kGitToolStripMenuItem.Click += new System.EventHandler(this.KGitToolStripMenuItemClick);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyPatchToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.bisectToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.checkoutToolStripMenuItem,
            this.cherryPickToolStripMenuItem,
            this.cleanupToolStripMenuItem,
            this.cloneToolStripMenuItem,
            this.commitToolStripMenuItem,
            this.branchToolStripMenuItem,
            this.tagToolStripMenuItem,
            this.deleteBranchToolStripMenuItem,
            this.deleteTagToolStripMenuItem,
            this.formatPatchToolStripMenuItem,
            this.goToToolStripMenuItem,
            this.initNewRepositoryToolStripMenuItem,
            this.mergeBranchToolStripMenuItem,
            this.pullToolStripMenuItem,
            this.pushToolStripMenuItem,
            this.rebaseToolStripMenuItem,
            this.runMergetoolToolStripMenuItem,
            this.stashToolStripMenuItem,
            this.viewDiffToolStripMenuItem,
            this.patchToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // applyPatchToolStripMenuItem
            // 
            this.applyPatchToolStripMenuItem.Name = "applyPatchToolStripMenuItem";
            this.applyPatchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.applyPatchToolStripMenuItem.Text = "Apply patch";
            this.applyPatchToolStripMenuItem.Click += new System.EventHandler(this.ApplyPatchToolStripMenuItemClick);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.archiveToolStripMenuItem.Text = "Archive";
            this.archiveToolStripMenuItem.Click += new System.EventHandler(this.ArchiveToolStripMenuItemClick);
            // 
            // bisectToolStripMenuItem
            // 
            this.bisectToolStripMenuItem.Name = "bisectToolStripMenuItem";
            this.bisectToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.bisectToolStripMenuItem.Text = "Bisect";
            this.bisectToolStripMenuItem.Click += new System.EventHandler(this.bisectToolStripMenuItem_Click);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("checkoutBranchToolStripMenuItem.Image")));
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+.";
            this.checkoutBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemPeriod)));
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.CheckoutBranchToolStripMenuItemClick);
            // 
            // checkoutToolStripMenuItem
            // 
            this.checkoutToolStripMenuItem.Name = "checkoutToolStripMenuItem";
            this.checkoutToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.checkoutToolStripMenuItem.Text = "Checkout revision";
            this.checkoutToolStripMenuItem.Click += new System.EventHandler(this.CheckoutToolStripMenuItemClick);
            // 
            // cherryPickToolStripMenuItem
            // 
            this.cherryPickToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cherryPickToolStripMenuItem.Image")));
            this.cherryPickToolStripMenuItem.Name = "cherryPickToolStripMenuItem";
            this.cherryPickToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.cherryPickToolStripMenuItem.Text = "Cherry pick";
            this.cherryPickToolStripMenuItem.Click += new System.EventHandler(this.CherryPickToolStripMenuItemClick);
            // 
            // cleanupToolStripMenuItem
            // 
            this.cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
            this.cleanupToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.cleanupToolStripMenuItem.Text = "Cleanup";
            this.cleanupToolStripMenuItem.Click += new System.EventHandler(this.CleanupToolStripMenuItemClick);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cloneToolStripMenuItem.Image")));
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.cloneToolStripMenuItem.Text = "Clone repository";
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.CloneToolStripMenuItemClick);
            // 
            // commitToolStripMenuItem
            // 
            this.commitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("commitToolStripMenuItem.Image")));
            this.commitToolStripMenuItem.Name = "commitToolStripMenuItem";
            this.commitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space)));
            this.commitToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.commitToolStripMenuItem.Text = "Commit";
            this.commitToolStripMenuItem.Click += new System.EventHandler(this.CommitToolStripMenuItemClick);
            // 
            // branchToolStripMenuItem
            // 
            this.branchToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("branchToolStripMenuItem.Image")));
            this.branchToolStripMenuItem.Name = "branchToolStripMenuItem";
            this.branchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.branchToolStripMenuItem.Text = "Create branch";
            this.branchToolStripMenuItem.Click += new System.EventHandler(this.BranchToolStripMenuItemClick);
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("tagToolStripMenuItem.Image")));
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.tagToolStripMenuItem.Text = "Create tag";
            this.tagToolStripMenuItem.Click += new System.EventHandler(this.TagToolStripMenuItemClick);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.DeleteBranchToolStripMenuItemClick);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.deleteTagToolStripMenuItem.Text = "Delete tag";
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.DeleteTagToolStripMenuItemClick);
            // 
            // formatPatchToolStripMenuItem
            // 
            this.formatPatchToolStripMenuItem.Name = "formatPatchToolStripMenuItem";
            this.formatPatchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.formatPatchToolStripMenuItem.Text = "Format patch";
            this.formatPatchToolStripMenuItem.Click += new System.EventHandler(this.FormatPatchToolStripMenuItemClick);
            // 
            // goToToolStripMenuItem
            // 
            this.goToToolStripMenuItem.Image = global::GitUI.Properties.Resources._75;
            this.goToToolStripMenuItem.Name = "goToToolStripMenuItem";
            this.goToToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.G)));
            this.goToToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.goToToolStripMenuItem.Text = "Go to commit";
            this.goToToolStripMenuItem.Click += new System.EventHandler(this.goToToolStripMenuItem_Click);
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("initNewRepositoryToolStripMenuItem.Image")));
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "Init new repository";
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.InitNewRepositoryToolStripMenuItemClick);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.mergeBranchToolStripMenuItem.Text = "Merge branches";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.MergeBranchToolStripMenuItemClick);
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pullToolStripMenuItem.Image")));
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.pullToolStripMenuItem.Text = "Pull";
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.PullToolStripMenuItemClick);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pushToolStripMenuItem.Image")));
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.pushToolStripMenuItem.Text = "Push";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.PushToolStripMenuItemClick);
            // 
            // rebaseToolStripMenuItem
            // 
            this.rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            this.rebaseToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.rebaseToolStripMenuItem.Text = "Rebase";
            this.rebaseToolStripMenuItem.Click += new System.EventHandler(this.RebaseToolStripMenuItemClick);
            // 
            // runMergetoolToolStripMenuItem
            // 
            this.runMergetoolToolStripMenuItem.Name = "runMergetoolToolStripMenuItem";
            this.runMergetoolToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.runMergetoolToolStripMenuItem.Text = "Solve mergeconflicts";
            this.runMergetoolToolStripMenuItem.Click += new System.EventHandler(this.RunMergetoolToolStripMenuItemClick);
            // 
            // stashToolStripMenuItem
            // 
            this.stashToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("stashToolStripMenuItem.Image")));
            this.stashToolStripMenuItem.Name = "stashToolStripMenuItem";
            this.stashToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.stashToolStripMenuItem.Text = "Stash changes";
            this.stashToolStripMenuItem.Click += new System.EventHandler(this.StashToolStripMenuItemClick);
            // 
            // viewDiffToolStripMenuItem
            // 
            this.viewDiffToolStripMenuItem.Name = "viewDiffToolStripMenuItem";
            this.viewDiffToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.viewDiffToolStripMenuItem.Text = "View diff";
            this.viewDiffToolStripMenuItem.Click += new System.EventHandler(this.ViewDiffToolStripMenuItemClick);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            this.patchToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.patchToolStripMenuItem.Text = "View patch file";
            this.patchToolStripMenuItem.Click += new System.EventHandler(this.PatchToolStripMenuItemClick);
            // 
            // remotesToolStripMenuItem
            // 
            this.remotesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageRemoteRepositoriesToolStripMenuItem1,
            this.toolStripSeparator6,
            this.PuTTYToolStripMenuItem});
            this.remotesToolStripMenuItem.Name = "remotesToolStripMenuItem";
            this.remotesToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.remotesToolStripMenuItem.Text = "Remotes";
            // 
            // manageRemoteRepositoriesToolStripMenuItem1
            // 
            this.manageRemoteRepositoriesToolStripMenuItem1.Name = "manageRemoteRepositoriesToolStripMenuItem1";
            this.manageRemoteRepositoriesToolStripMenuItem1.Size = new System.Drawing.Size(222, 22);
            this.manageRemoteRepositoriesToolStripMenuItem1.Text = "Manage remote repositories";
            this.manageRemoteRepositoriesToolStripMenuItem1.Click += new System.EventHandler(this.ManageRemoteRepositoriesToolStripMenuItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(219, 6);
            // 
            // PuTTYToolStripMenuItem
            // 
            this.PuTTYToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAuthenticationAgentToolStripMenuItem,
            this.generateOrImportKeyToolStripMenuItem});
            this.PuTTYToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("PuTTYToolStripMenuItem.Image")));
            this.PuTTYToolStripMenuItem.Name = "PuTTYToolStripMenuItem";
            this.PuTTYToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.PuTTYToolStripMenuItem.Text = "PuTTY";
            // 
            // startAuthenticationAgentToolStripMenuItem
            // 
            this.startAuthenticationAgentToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("startAuthenticationAgentToolStripMenuItem.Image")));
            this.startAuthenticationAgentToolStripMenuItem.Name = "startAuthenticationAgentToolStripMenuItem";
            this.startAuthenticationAgentToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.startAuthenticationAgentToolStripMenuItem.Text = "Start authentication agent";
            this.startAuthenticationAgentToolStripMenuItem.Click += new System.EventHandler(this.StartAuthenticationAgentToolStripMenuItemClick);
            // 
            // generateOrImportKeyToolStripMenuItem
            // 
            this.generateOrImportKeyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("generateOrImportKeyToolStripMenuItem.Image")));
            this.generateOrImportKeyToolStripMenuItem.Name = "generateOrImportKeyToolStripMenuItem";
            this.generateOrImportKeyToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.generateOrImportKeyToolStripMenuItem.Text = "Generate or import key";
            this.generateOrImportKeyToolStripMenuItem.Click += new System.EventHandler(this.GenerateOrImportKeyToolStripMenuItemClick);
            // 
            // _repositoryHostsToolStripMenuItem
            // 
            this._repositoryHostsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._forkCloneRepositoryToolStripMenuItem,
            this._viewPullRequestsToolStripMenuItem,
            this._createPullRequestsToolStripMenuItem});
            this._repositoryHostsToolStripMenuItem.Name = "_repositoryHostsToolStripMenuItem";
            this._repositoryHostsToolStripMenuItem.Size = new System.Drawing.Size(106, 20);
            this._repositoryHostsToolStripMenuItem.Text = "Repository hosts";
            // 
            // _forkCloneRepositoryToolStripMenuItem
            // 
            this._forkCloneRepositoryToolStripMenuItem.Name = "_forkCloneRepositoryToolStripMenuItem";
            this._forkCloneRepositoryToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this._forkCloneRepositoryToolStripMenuItem.Text = "Fork/Clone repository";
            this._forkCloneRepositoryToolStripMenuItem.Click += new System.EventHandler(this._forkCloneMenuItem_Click);
            // 
            // _viewPullRequestsToolStripMenuItem
            // 
            this._viewPullRequestsToolStripMenuItem.Name = "_viewPullRequestsToolStripMenuItem";
            this._viewPullRequestsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this._viewPullRequestsToolStripMenuItem.Text = "View pull requests";
            this._viewPullRequestsToolStripMenuItem.Click += new System.EventHandler(this._viewPullRequestsToolStripMenuItem_Click);
            // 
            // _createPullRequestsToolStripMenuItem
            // 
            this._createPullRequestsToolStripMenuItem.Name = "_createPullRequestsToolStripMenuItem";
            this._createPullRequestsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this._createPullRequestsToolStripMenuItem.Text = "Create pull requests";
            this._createPullRequestsToolStripMenuItem.Click += new System.EventHandler(this._createPullRequestToolStripMenuItem_Click);
            // 
            // submodulesToolStripMenuItem
            // 
            this.submodulesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageSubmodulesToolStripMenuItem,
            this.toolStripSeparator8,
            this.updateAllSubmodulesRecursiveToolStripMenuItem,
            this.initializeAllSubmodulesRecursiveToolStripMenuItem,
            this.synchronizeAllSubmodulesRecursiveToolStripMenuItem,
            this.toolStripSeparator14,
            this.updateAllSubmodulesToolStripMenuItem,
            this.initializeAllSubmodulesToolStripMenuItem,
            this.syncronizeAllSubmodulesToolStripMenuItem,
            this.toolStripSeparator10,
            this.openSubmoduleToolStripMenuItem});
            this.submodulesToolStripMenuItem.Name = "submodulesToolStripMenuItem";
            this.submodulesToolStripMenuItem.Size = new System.Drawing.Size(85, 20);
            this.submodulesToolStripMenuItem.Text = "Submodules";
            // 
            // manageSubmodulesToolStripMenuItem
            // 
            this.manageSubmodulesToolStripMenuItem.Name = "manageSubmodulesToolStripMenuItem";
            this.manageSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.manageSubmodulesToolStripMenuItem.Text = "Manage submodules";
            this.manageSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.ManageSubmodulesToolStripMenuItemClick);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(268, 6);
            // 
            // updateAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Name = "updateAllSubmodulesRecursiveToolStripMenuItem";
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Text = "Update all submodules recursive";
            this.updateAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.UpdateAllSubmodulesRecursiveToolStripMenuItemClick);
            // 
            // initializeAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Name = "initializeAllSubmodulesRecursiveToolStripMenuItem";
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Text = "Initialize all submodules recursive";
            this.initializeAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.InitializeAllSubmodulesRecursiveToolStripMenuItemClick);
            // 
            // synchronizeAllSubmodulesRecursiveToolStripMenuItem
            // 
            this.synchronizeAllSubmodulesRecursiveToolStripMenuItem.Name = "synchronizeAllSubmodulesRecursiveToolStripMenuItem";
            this.synchronizeAllSubmodulesRecursiveToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.synchronizeAllSubmodulesRecursiveToolStripMenuItem.Text = "Synchronize all submodules recursive";
            this.synchronizeAllSubmodulesRecursiveToolStripMenuItem.Click += new System.EventHandler(this.SynchronizeAllSubmodulesRecursiveToolStripMenuItemClick);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(268, 6);
            // 
            // updateAllSubmodulesToolStripMenuItem
            // 
            this.updateAllSubmodulesToolStripMenuItem.Name = "updateAllSubmodulesToolStripMenuItem";
            this.updateAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.updateAllSubmodulesToolStripMenuItem.Text = "Update all submodules";
            this.updateAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.UpdateAllSubmodulesToolStripMenuItemClick);
            // 
            // initializeAllSubmodulesToolStripMenuItem
            // 
            this.initializeAllSubmodulesToolStripMenuItem.Name = "initializeAllSubmodulesToolStripMenuItem";
            this.initializeAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.initializeAllSubmodulesToolStripMenuItem.Text = "Initialize all submodules";
            this.initializeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.InitializeAllSubmodulesToolStripMenuItemClick);
            // 
            // syncronizeAllSubmodulesToolStripMenuItem
            // 
            this.syncronizeAllSubmodulesToolStripMenuItem.Name = "syncronizeAllSubmodulesToolStripMenuItem";
            this.syncronizeAllSubmodulesToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.syncronizeAllSubmodulesToolStripMenuItem.Text = "Synchronize all submodules";
            this.syncronizeAllSubmodulesToolStripMenuItem.Click += new System.EventHandler(this.SyncronizeAllSubmodulesToolStripMenuItemClick);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(268, 6);
            // 
            // openSubmoduleToolStripMenuItem
            // 
            this.openSubmoduleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator11});
            this.openSubmoduleToolStripMenuItem.Name = "openSubmoduleToolStripMenuItem";
            this.openSubmoduleToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.openSubmoduleToolStripMenuItem.Text = "Browse submodule";
            this.openSubmoduleToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OpenSubmoduleToolStripMenuItemDropDownOpening);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(57, 6);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.toolStripSeparator15});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            this.pluginsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.pluginsToolStripMenuItem_DropDownOpening);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItemClick);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(113, 6);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitMaintenanceToolStripMenuItem,
            this.toolStripSeparator4,
            this.editgitignoreToolStripMenuItem1,
            this.editgitattributesToolStripMenuItem,
            this.editmailmapToolStripMenuItem,
            this.toolStripSeparator13,
            this.settingsToolStripMenuItem2});
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem1.Text = "Settings";
            // 
            // gitMaintenanceToolStripMenuItem
            // 
            this.gitMaintenanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressGitDatabaseToolStripMenuItem,
            this.verifyGitDatabaseToolStripMenuItem,
            this.deleteIndexlockToolStripMenuItem});
            this.gitMaintenanceToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("gitMaintenanceToolStripMenuItem.Image")));
            this.gitMaintenanceToolStripMenuItem.Name = "gitMaintenanceToolStripMenuItem";
            this.gitMaintenanceToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.gitMaintenanceToolStripMenuItem.Text = "Git maintenance";
            // 
            // compressGitDatabaseToolStripMenuItem
            // 
            this.compressGitDatabaseToolStripMenuItem.Name = "compressGitDatabaseToolStripMenuItem";
            this.compressGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.compressGitDatabaseToolStripMenuItem.Text = "Compress git database";
            this.compressGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.CompressGitDatabaseToolStripMenuItemClick);
            // 
            // verifyGitDatabaseToolStripMenuItem
            // 
            this.verifyGitDatabaseToolStripMenuItem.Name = "verifyGitDatabaseToolStripMenuItem";
            this.verifyGitDatabaseToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.verifyGitDatabaseToolStripMenuItem.Text = "Recover lost objects";
            this.verifyGitDatabaseToolStripMenuItem.Click += new System.EventHandler(this.VerifyGitDatabaseToolStripMenuItemClick);
            // 
            // deleteIndexlockToolStripMenuItem
            // 
            this.deleteIndexlockToolStripMenuItem.Name = "deleteIndexlockToolStripMenuItem";
            this.deleteIndexlockToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.deleteIndexlockToolStripMenuItem.Text = "Delete index.lock";
            this.deleteIndexlockToolStripMenuItem.Click += new System.EventHandler(this.deleteIndexlockToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(161, 6);
            // 
            // editgitignoreToolStripMenuItem1
            // 
            this.editgitignoreToolStripMenuItem1.Name = "editgitignoreToolStripMenuItem1";
            this.editgitignoreToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
            this.editgitignoreToolStripMenuItem1.Text = "Edit .gitignore";
            this.editgitignoreToolStripMenuItem1.Click += new System.EventHandler(this.EditGitignoreToolStripMenuItem1Click);
            // 
            // editgitattributesToolStripMenuItem
            // 
            this.editgitattributesToolStripMenuItem.Name = "editgitattributesToolStripMenuItem";
            this.editgitattributesToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.editgitattributesToolStripMenuItem.Text = "Edit .gitattributes";
            this.editgitattributesToolStripMenuItem.Click += new System.EventHandler(this.editgitattributesToolStripMenuItem_Click);
            // 
            // editmailmapToolStripMenuItem
            // 
            this.editmailmapToolStripMenuItem.Name = "editmailmapToolStripMenuItem";
            this.editmailmapToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.editmailmapToolStripMenuItem.Text = "Edit .mailmap";
            this.editmailmapToolStripMenuItem.Click += new System.EventHandler(this.EditMailMapToolStripMenuItemClick);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(161, 6);
            // 
            // settingsToolStripMenuItem2
            // 
            this.settingsToolStripMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("settingsToolStripMenuItem2.Image")));
            this.settingsToolStripMenuItem2.Name = "settingsToolStripMenuItem2";
            this.settingsToolStripMenuItem2.Size = new System.Drawing.Size(164, 22);
            this.settingsToolStripMenuItem2.Text = "Settings";
            this.settingsToolStripMenuItem2.Click += new System.EventHandler(this.SettingsToolStripMenuItem2Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitcountPerUserToolStripMenuItem,
            this.gitcommandLogToolStripMenuItem,
            this.toolStripSeparator7,
            this.userManualToolStripMenuItem,
            this.changelogToolStripMenuItem,
            this.toolStripSeparator3,
            this.translateToolStripMenuItem,
            this.toolStripSeparator16,
            this.donateToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // commitcountPerUserToolStripMenuItem
            // 
            this.commitcountPerUserToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("commitcountPerUserToolStripMenuItem.Image")));
            this.commitcountPerUserToolStripMenuItem.Name = "commitcountPerUserToolStripMenuItem";
            this.commitcountPerUserToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.commitcountPerUserToolStripMenuItem.Text = "Commits per user";
            this.commitcountPerUserToolStripMenuItem.Click += new System.EventHandler(this.CommitcountPerUserToolStripMenuItemClick);
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("gitcommandLogToolStripMenuItem.Image")));
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.gitcommandLogToolStripMenuItem.Text = "Gitcommand log";
            this.gitcommandLogToolStripMenuItem.Click += new System.EventHandler(this.GitcommandLogToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(165, 6);
            // 
            // userManualToolStripMenuItem
            // 
            this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            this.userManualToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.userManualToolStripMenuItem.Text = "User Manual";
            this.userManualToolStripMenuItem.Click += new System.EventHandler(this.UserManualToolStripMenuItemClick);
            // 
            // changelogToolStripMenuItem
            // 
            this.changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            this.changelogToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.changelogToolStripMenuItem.Text = "Changelog";
            this.changelogToolStripMenuItem.Click += new System.EventHandler(this.ChangelogToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(165, 6);
            // 
            // translateToolStripMenuItem
            // 
            this.translateToolStripMenuItem.Name = "translateToolStripMenuItem";
            this.translateToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.translateToolStripMenuItem.Text = "Translate";
            this.translateToolStripMenuItem.Click += new System.EventHandler(this.TranslateToolStripMenuItemClick);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(165, 6);
            // 
            // donateToolStripMenuItem
            // 
            this.donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            this.donateToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.donateToolStripMenuItem.Text = "Donate";
            this.donateToolStripMenuItem.Click += new System.EventHandler(this.DonateToolStripMenuItemClick);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("aboutToolStripMenuItem.Image")));
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.gitToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this.remotesToolStripMenuItem,
            this._repositoryHostsToolStripMenuItem,
            this.submodulesToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(814, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FormBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 573);
            this.Controls.Add(this.toolPanel);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip);
            this.Name = "FormBrowse";
            this.Text = "Git Extensions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBrowseFormClosing);
            this.Load += new System.EventHandler(this.BrowseLoad);
            this.toolPanel.Panel1.ResumeLayout(false);
            this.toolPanel.Panel1.PerformLayout();
            this.toolPanel.Panel2.ResumeLayout(false);
            this.toolPanel.ResumeLayout(false);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            this.MainSplitContainer.ResumeLayout(false);
            this.CommitInfoTabControl.ResumeLayout(false);
            this.CommitInfo.ResumeLayout(false);
            this.Tree.ResumeLayout(false);
            this.FileTreeSplitContainer.Panel1.ResumeLayout(false);
            this.FileTreeSplitContainer.Panel2.ResumeLayout(false);
            this.FileTreeSplitContainer.ResumeLayout(false);
            this.FileTreeContextMenu.ResumeLayout(false);
            this.Diff.ResumeLayout(false);
            this.DiffSplitContainer.Panel1.ResumeLayout(false);
            this.DiffSplitContainer.Panel2.ResumeLayout(false);
            this.DiffSplitContainer.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.TreeContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView GitTree;
        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.TabControl CommitInfoTabControl;
        private System.Windows.Forms.TabPage Tree;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private GitUI.RevisionGrid RevisionGrid;
        private System.Windows.Forms.SplitContainer FileTreeSplitContainer;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSplitButton _NO_TRANSLATE_Workingdir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton GitBash;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton EditSettings;
        private System.Windows.Forms.ToolStripButton RefreshButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
        private System.Windows.Forms.TabPage Diff;
        private System.Windows.Forms.SplitContainer DiffSplitContainer;
        private FileStatusList DiffFiles;
        private System.Windows.Forms.ToolStripButton toolStripButtonPull;
        private System.Windows.Forms.ToolStripButton toolStripButtonPush;
        private FileViewer FileText;
        private FileViewer DiffText;
        private System.Windows.Forms.TabPage CommitInfo;
        private CommitInfo RevisionInfo;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitStash;
        private System.Windows.Forms.ToolStripMenuItem stashChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stashPopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem viewStashToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip TreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private ContextMenuStrip FileTreeContextMenu;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem openFileToolStripMenuItem;
        private ToolStripMenuItem openFileWithToolStripMenuItem;
        private ToolStripMenuItem fileHistoryToolStripMenuItem;
        private ToolStripMenuItem findToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem;
        private ToolStripMenuItem copyFilenameToClipboardToolStripMenuItem1;
        private ToolStripMenuItem saveAsToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox toolStripBranches;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripMenuItem localToolStripMenuItem;
        private ToolStripMenuItem remoteToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem commitToolStripMenuItem1;
        private ToolStripMenuItem committerToolStripMenuItem;
        private ToolStripMenuItem authorToolStripMenuItem;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem openWithToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripMenuItem fileHistoryDiffToolstripMenuItem;
        private ToolStripSplitButton branchSelect;
        private ToolStripMenuItem diffContainsToolStripMenuItem;
        private ToolStripButton toggleSplitViewLayout;
        private ToolStripMenuItem editCheckedOutFileToolStripMenuItem;
        private SplitContainer toolPanel;
        private ToolStrip UserMenuToolStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem refreshToolStripMenuItem;
        private ToolStripMenuItem recentToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem fileExplorerToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem gitToolStripMenuItem;
        private ToolStripMenuItem gitBashToolStripMenuItem;
        private ToolStripMenuItem gitGUIToolStripMenuItem;
        private ToolStripMenuItem kGitToolStripMenuItem;
        private ToolStripMenuItem commandsToolStripMenuItem;
        private ToolStripMenuItem applyPatchToolStripMenuItem;
        private ToolStripMenuItem archiveToolStripMenuItem;
        private ToolStripMenuItem bisectToolStripMenuItem;
        private ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private ToolStripMenuItem checkoutToolStripMenuItem;
        private ToolStripMenuItem cherryPickToolStripMenuItem;
        private ToolStripMenuItem cleanupToolStripMenuItem;
        private ToolStripMenuItem cloneToolStripMenuItem;
        private ToolStripMenuItem commitToolStripMenuItem;
        private ToolStripMenuItem branchToolStripMenuItem;
        private ToolStripMenuItem tagToolStripMenuItem;
        private ToolStripMenuItem deleteBranchToolStripMenuItem;
        private ToolStripMenuItem deleteTagToolStripMenuItem;
        private ToolStripMenuItem formatPatchToolStripMenuItem;
        private ToolStripMenuItem goToToolStripMenuItem;
        private ToolStripMenuItem initNewRepositoryToolStripMenuItem;
        private ToolStripMenuItem mergeBranchToolStripMenuItem;
        private ToolStripMenuItem pullToolStripMenuItem;
        private ToolStripMenuItem pushToolStripMenuItem;
        private ToolStripMenuItem rebaseToolStripMenuItem;
        private ToolStripMenuItem runMergetoolToolStripMenuItem;
        private ToolStripMenuItem stashToolStripMenuItem;
        private ToolStripMenuItem viewDiffToolStripMenuItem;
        private ToolStripMenuItem patchToolStripMenuItem;
        private ToolStripMenuItem remotesToolStripMenuItem;
        private ToolStripMenuItem manageRemoteRepositoriesToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem PuTTYToolStripMenuItem;
        private ToolStripMenuItem startAuthenticationAgentToolStripMenuItem;
        private ToolStripMenuItem generateOrImportKeyToolStripMenuItem;
        private ToolStripMenuItem _repositoryHostsToolStripMenuItem;
        private ToolStripMenuItem _forkCloneRepositoryToolStripMenuItem;
        private ToolStripMenuItem _viewPullRequestsToolStripMenuItem;
        private ToolStripMenuItem _createPullRequestsToolStripMenuItem;
        private ToolStripMenuItem submodulesToolStripMenuItem;
        private ToolStripMenuItem manageSubmodulesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem updateAllSubmodulesRecursiveToolStripMenuItem;
        private ToolStripMenuItem initializeAllSubmodulesRecursiveToolStripMenuItem;
        private ToolStripMenuItem synchronizeAllSubmodulesRecursiveToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripMenuItem updateAllSubmodulesToolStripMenuItem;
        private ToolStripMenuItem initializeAllSubmodulesToolStripMenuItem;
        private ToolStripMenuItem syncronizeAllSubmodulesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem openSubmoduleToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem pluginsToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem settingsToolStripMenuItem1;
        private ToolStripMenuItem gitMaintenanceToolStripMenuItem;
        private ToolStripMenuItem compressGitDatabaseToolStripMenuItem;
        private ToolStripMenuItem verifyGitDatabaseToolStripMenuItem;
        private ToolStripMenuItem deleteIndexlockToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem editgitignoreToolStripMenuItem1;
        private ToolStripMenuItem editgitattributesToolStripMenuItem;
        private ToolStripMenuItem editmailmapToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem settingsToolStripMenuItem2;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem commitcountPerUserToolStripMenuItem;
        private ToolStripMenuItem gitcommandLogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem userManualToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem translateToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem donateToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private MenuStrip menuStrip1;
    }
}
