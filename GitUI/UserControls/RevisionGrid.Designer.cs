
using GitUI.RevisionGridClasses;

namespace GitUI
{
    partial class RevisionGrid
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
            if (disposing)
            {
                DisposeRevisionGraphCommand();
                if (_indexWatcher != null)
                {
                    _indexWatcher.Dispose();
                    _indexWatcher = null;
                }

                _selectedItemBrush = null;

                if (_filledItemBrush != null)
                {
                    _filledItemBrush.Dispose();
                    _filledItemBrush = null;
                }
            }

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGrid));
            this.Revisions = new GitUI.RevisionGridClasses.DvcsGraph();
            this.Graph = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mainContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.markRevisionAsBadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markRevisionAsGoodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSkipRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopBisectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.messageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.branchNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCurrentBranchToHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.createNewBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.createTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.checkoutRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manipulateCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixupCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.squashCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCurrentBranchOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFilteredBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowRemoteBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemView = new System.Windows.Forms.ToolStripMenuItem();
            this.showRevisionGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawNonrelativesGrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orderRevisionsByDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAuthorDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRelativeDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGitNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.runScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelectionTimer = new System.Windows.Forms.Timer(this.components);
            this.NoCommits = new System.Windows.Forms.Panel();
            this.NoGit = new System.Windows.Forms.Panel();
            this.InitRepository = new System.Windows.Forms.Button();
            this.CloneRepository = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.GitIgnore = new System.Windows.Forms.Button();
            this.Commit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Error = new System.Windows.Forms.PictureBox();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.quickSearchTimer = new System.Windows.Forms.Timer(this.components);
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)(this.Revisions)).BeginInit();
            this.mainContextMenu.SuspendLayout();
            this.NoCommits.SuspendLayout();
            this.NoGit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.SuspendLayout();
            // 
            // Revisions
            // 
            this.Revisions.AllowUserToAddRows = false;
            this.Revisions.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Revisions.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.Revisions.BackgroundColor = System.Drawing.SystemColors.Window;
            this.Revisions.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Revisions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.Revisions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Revisions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Graph,
            this.Message,
            this.Author,
            this.Date});
            this.Revisions.ContextMenuStrip = this.mainContextMenu;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.DefaultCellStyle = dataGridViewCellStyle4;
            this.Revisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Revisions.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Revisions.GridColor = System.Drawing.SystemColors.Window;
            this.Revisions.Location = new System.Drawing.Point(0, 0);
            this.Revisions.Name = "Revisions";
            this.Revisions.ReadOnly = true;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Revisions.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.Revisions.RowHeadersVisible = false;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Revisions.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.Revisions.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(185)))), ((int)(((byte)(235)))));
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.Revisions.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Revisions.Size = new System.Drawing.Size(682, 235);
            this.Revisions.StandardTab = true;
            this.Revisions.TabIndex = 0;
            this.Revisions.VirtualMode = true;
            this.Revisions.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.RevisionsCellMouseDown);
            this.Revisions.DoubleClick += new System.EventHandler(this.RevisionsDoubleClick);
            this.Revisions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RevisionsMouseClick);
            // 
            // Graph
            // 
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Graph.DefaultCellStyle = dataGridViewCellStyle3;
            this.Graph.Frozen = true;
            this.Graph.HeaderText = "";
            this.Graph.Name = "Graph";
            this.Graph.ReadOnly = true;
            this.Graph.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Graph.Width = 70;
            // 
            // Message
            // 
            this.Message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Author
            // 
            this.Author.HeaderText = "Author";
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Author.Width = 150;
            // 
            // Date
            // 
            this.Date.HeaderText = "Date";
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Date.Width = 180;
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
            this.toolStripSeparator5,
            this.createTagToolStripMenuItem,
            this.deleteTagToolStripMenuItem,
            this.toolStripSeparator2,
            this.checkoutRevisionToolStripMenuItem,
            this.revertCommitToolStripMenuItem,
            this.cherryPickCommitToolStripMenuItem,
            this.archiveRevisionToolStripMenuItem,
            this.manipulateCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.showBranchesToolStripMenuItem,
            this.toolStripMenuItemView,
            this.filterToolStripMenuItem,
            this.toolStripSeparator7,
            this.runScriptToolStripMenuItem});
            this.mainContextMenu.Name = "CreateTag";
            this.mainContextMenu.Size = new System.Drawing.Size(265, 620);
            this.mainContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.CreateTagOpening);
            // 
            // markRevisionAsBadToolStripMenuItem
            // 
            this.markRevisionAsBadToolStripMenuItem.Name = "markRevisionAsBadToolStripMenuItem";
            this.markRevisionAsBadToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.markRevisionAsBadToolStripMenuItem.Text = "Mark revision as bad";
            this.markRevisionAsBadToolStripMenuItem.Click += new System.EventHandler(this.markRevisionAsBadToolStripMenuItem_Click);
            // 
            // markRevisionAsGoodToolStripMenuItem
            // 
            this.markRevisionAsGoodToolStripMenuItem.Name = "markRevisionAsGoodToolStripMenuItem";
            this.markRevisionAsGoodToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.markRevisionAsGoodToolStripMenuItem.Text = "Mark revision as good";
            this.markRevisionAsGoodToolStripMenuItem.Click += new System.EventHandler(this.markRevisionAsGoodToolStripMenuItem_Click);
            // 
            // bisectSkipRevisionToolStripMenuItem
            // 
            this.bisectSkipRevisionToolStripMenuItem.Name = "bisectSkipRevisionToolStripMenuItem";
            this.bisectSkipRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.bisectSkipRevisionToolStripMenuItem.Text = "Skip revision";
            this.bisectSkipRevisionToolStripMenuItem.Click += new System.EventHandler(this.bisectSkipRevisionToolStripMenuItem_Click);
            // 
            // stopBisectToolStripMenuItem
            // 
            this.stopBisectToolStripMenuItem.Name = "stopBisectToolStripMenuItem";
            this.stopBisectToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.stopBisectToolStripMenuItem.Text = "Stop bisect";
            this.stopBisectToolStripMenuItem.Click += new System.EventHandler(this.stopBisectToolStripMenuItem_Click);
            // 
            // bisectSeparator
            // 
            this.bisectSeparator.Name = "bisectSeparator";
            this.bisectSeparator.Size = new System.Drawing.Size(261, 6);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hashToolStripMenuItem,
            this.messageToolStripMenuItem,
            this.authorToolStripMenuItem,
            this.dateToolStripMenuItem,
            this.toolStripSeparator6,
            this.branchNameToolStripMenuItem,
            this.tagToolStripMenuItem});
            this.copyToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            this.copyToClipboardToolStripMenuItem.DropDownOpened += new System.EventHandler(this.copyToClipboardToolStripMenuItem_DropDownOpened);
            // 
            // messageToolStripMenuItem
            // 
            this.messageToolStripMenuItem.Name = "messageToolStripMenuItem";
            this.messageToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
            this.messageToolStripMenuItem.Text = "Message";
            this.messageToolStripMenuItem.Click += new System.EventHandler(this.messageToolStripMenuItem_Click);
            // 
            // authorToolStripMenuItem
            // 
            this.authorToolStripMenuItem.Name = "authorToolStripMenuItem";
            this.authorToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
            this.authorToolStripMenuItem.Text = "Author";
            this.authorToolStripMenuItem.Click += new System.EventHandler(this.authorToolStripMenuItem_Click);
            // 
            // dateToolStripMenuItem
            // 
            this.dateToolStripMenuItem.Name = "dateToolStripMenuItem";
            this.dateToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
            this.dateToolStripMenuItem.Text = "Date";
            this.dateToolStripMenuItem.Click += new System.EventHandler(this.dateToolStripMenuItem_Click);
            // 
            // hashToolStripMenuItem
            // 
            this.hashToolStripMenuItem.Name = "hashToolStripMenuItem";
            this.hashToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
            this.hashToolStripMenuItem.Text = "Commit hash";
            this.hashToolStripMenuItem.Click += new System.EventHandler(this.hashToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(162, 6);
            // 
            // branchNameToolStripMenuItem
            // 
            this.branchNameToolStripMenuItem.Name = "branchNameToolStripMenuItem";
            this.branchNameToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
            this.branchNameToolStripMenuItem.Text = "Branch name";
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
            this.tagToolStripMenuItem.Text = "Tag";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(261, 6);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchCheckout;
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconMerge;
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.mergeBranchToolStripMenuItem.Text = "Merge into current branch";
            this.mergeBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            this.resetCurrentBranchToHereToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetCurrentBranchToHere;
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.resetCurrentBranchToHereToolStripMenuItem.Text = "Reset current branch to here";
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.ResetCurrentBranchToHereToolStripMenuItemClick);
            // 
            // rebaseOnToolStripMenuItem
            // 
            this.rebaseOnToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRebase;
            this.rebaseOnToolStripMenuItem.Name = "rebaseOnToolStripMenuItem";
            this.rebaseOnToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.rebaseOnToolStripMenuItem.Text = "Rebase current branch on";
            this.rebaseOnToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(261, 6);
            // 
            // createNewBranchToolStripMenuItem
            // 
            this.createNewBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchCreate;
            this.createNewBranchToolStripMenuItem.Name = "createNewBranchToolStripMenuItem";
            this.createNewBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.createNewBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.createNewBranchToolStripMenuItem.Text = "Create new branch";
            this.createNewBranchToolStripMenuItem.Click += new System.EventHandler(this.CreateNewBranchToolStripMenuItemClick);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchDelete;
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch";
            this.deleteBranchToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // renameBranchToolStripMenuItem
            // 
            this.renameBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.Renamed;
            this.renameBranchToolStripMenuItem.Name = "renameBranchToolStripMenuItem";
            this.renameBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.renameBranchToolStripMenuItem.Text = "Rename branch";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(261, 6);
            // 
            // createTagToolStripMenuItem
            // 
            this.createTagToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconTagCreate;
            this.createTagToolStripMenuItem.Name = "createTagToolStripMenuItem";
            this.createTagToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.createTagToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.createTagToolStripMenuItem.Text = "Create new tag";
            this.createTagToolStripMenuItem.Click += new System.EventHandler(this.CreateTagToolStripMenuItemClick);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconTagDelete;
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.deleteTagToolStripMenuItem.Text = "Delete tag";
            this.deleteTagToolStripMenuItem.Click += new System.EventHandler(this.deleteBranchTagToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(261, 6);
            // 
            // checkoutRevisionToolStripMenuItem
            // 
            this.checkoutRevisionToolStripMenuItem.Image = global::GitUI.Properties.Resources.RevisionCheckout;
            this.checkoutRevisionToolStripMenuItem.Name = "checkoutRevisionToolStripMenuItem";
            this.checkoutRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.checkoutRevisionToolStripMenuItem.Text = "Checkout revision";
            this.checkoutRevisionToolStripMenuItem.Click += new System.EventHandler(this.CheckoutRevisionToolStripMenuItemClick);
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRevertCommit;
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.RevertCommitToolStripMenuItemClick);
            // 
            // cherryPickCommitToolStripMenuItem
            // 
            this.cherryPickCommitToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCherryPick;
            this.cherryPickCommitToolStripMenuItem.Name = "cherryPickCommitToolStripMenuItem";
            this.cherryPickCommitToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.cherryPickCommitToolStripMenuItem.Text = "Cherry pick commit";
            this.cherryPickCommitToolStripMenuItem.Click += new System.EventHandler(this.CherryPickCommitToolStripMenuItemClick);
            // 
            // archiveRevisionToolStripMenuItem
            // 
            this.archiveRevisionToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconArchiveRevision;
            this.archiveRevisionToolStripMenuItem.Name = "archiveRevisionToolStripMenuItem";
            this.archiveRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.archiveRevisionToolStripMenuItem.Text = "Archive revision";
            this.archiveRevisionToolStripMenuItem.Click += new System.EventHandler(this.ArchiveRevisionToolStripMenuItemClick);
            // 
            // manipulateCommitToolStripMenuItem
            // 
            this.manipulateCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fixupCommitToolStripMenuItem,
            this.squashCommitToolStripMenuItem});
            this.manipulateCommitToolStripMenuItem.Name = "manipulateCommitToolStripMenuItem";
            this.manipulateCommitToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.manipulateCommitToolStripMenuItem.Text = "Advanced";
            // 
            // fixupCommitToolStripMenuItem
            // 
            this.fixupCommitToolStripMenuItem.Name = "fixupCommitToolStripMenuItem";
            this.fixupCommitToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.fixupCommitToolStripMenuItem.Text = "Fixup commit";
            this.fixupCommitToolStripMenuItem.Click += new System.EventHandler(this.FixupCommitToolStripMenuItemClick);
            // 
            // squashCommitToolStripMenuItem
            // 
            this.squashCommitToolStripMenuItem.Name = "squashCommitToolStripMenuItem";
            this.squashCommitToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.squashCommitToolStripMenuItem.Text = "Squash commit";
            this.squashCommitToolStripMenuItem.Click += new System.EventHandler(this.SquashCommitToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(261, 6);
            // 
            // showBranchesToolStripMenuItem
            // 
            this.showBranchesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAllBranchesToolStripMenuItem,
            this.showCurrentBranchOnlyToolStripMenuItem,
            this.showFilteredBranchesToolStripMenuItem,
            this.toolStripSeparator4,
            this.ShowRemoteBranches});
            this.showBranchesToolStripMenuItem.Name = "showBranchesToolStripMenuItem";
            this.showBranchesToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.showBranchesToolStripMenuItem.Text = "Show branches";
            // 
            // showAllBranchesToolStripMenuItem
            // 
            this.showAllBranchesToolStripMenuItem.Name = "showAllBranchesToolStripMenuItem";
            this.showAllBranchesToolStripMenuItem.Size = new System.Drawing.Size(245, 24);
            this.showAllBranchesToolStripMenuItem.Text = "Show all branches";
            this.showAllBranchesToolStripMenuItem.Click += new System.EventHandler(this.ShowAllBranchesToolStripMenuItemClick);
            // 
            // showCurrentBranchOnlyToolStripMenuItem
            // 
            this.showCurrentBranchOnlyToolStripMenuItem.Name = "showCurrentBranchOnlyToolStripMenuItem";
            this.showCurrentBranchOnlyToolStripMenuItem.Size = new System.Drawing.Size(245, 24);
            this.showCurrentBranchOnlyToolStripMenuItem.Text = "Show current branch only";
            this.showCurrentBranchOnlyToolStripMenuItem.Click += new System.EventHandler(this.ShowCurrentBranchOnlyToolStripMenuItemClick);
            // 
            // showFilteredBranchesToolStripMenuItem
            // 
            this.showFilteredBranchesToolStripMenuItem.Name = "showFilteredBranchesToolStripMenuItem";
            this.showFilteredBranchesToolStripMenuItem.Size = new System.Drawing.Size(245, 24);
            this.showFilteredBranchesToolStripMenuItem.Text = "Show filtered branches";
            this.showFilteredBranchesToolStripMenuItem.Click += new System.EventHandler(this.ShowFilteredBranchesToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(242, 6);
            // 
            // ShowRemoteBranches
            // 
            this.ShowRemoteBranches.Checked = true;
            this.ShowRemoteBranches.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowRemoteBranches.Name = "ShowRemoteBranches";
            this.ShowRemoteBranches.Size = new System.Drawing.Size(245, 24);
            this.ShowRemoteBranches.Text = "Show remote branches";
            this.ShowRemoteBranches.Click += new System.EventHandler(this.ShowRemoteBranchesClick);
            // 
            // toolStripMenuItemView
            // 
            this.toolStripMenuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showRevisionGraphToolStripMenuItem,
            this.drawNonrelativesGrayToolStripMenuItem,
            this.orderRevisionsByDateToolStripMenuItem,
            this.showAuthorDateToolStripMenuItem,
            this.showRelativeDateToolStripMenuItem,
            this.showGitNotesToolStripMenuItem,
            this.showTagsToolStripMenuItem});
            this.toolStripMenuItemView.Name = "toolStripMenuItemView";
            this.toolStripMenuItemView.Size = new System.Drawing.Size(264, 24);
            this.toolStripMenuItemView.Text = "View";
            // 
            // showRevisionGraphToolStripMenuItem
            // 
            this.showRevisionGraphToolStripMenuItem.Name = "showRevisionGraphToolStripMenuItem";
            this.showRevisionGraphToolStripMenuItem.Size = new System.Drawing.Size(234, 24);
            this.showRevisionGraphToolStripMenuItem.Text = "Show revision graph";
            this.showRevisionGraphToolStripMenuItem.Click += new System.EventHandler(this.ShowRevisionGraphToolStripMenuItemClick);
            // 
            // drawNonrelativesGrayToolStripMenuItem
            // 
            this.drawNonrelativesGrayToolStripMenuItem.Name = "drawNonrelativesGrayToolStripMenuItem";
            this.drawNonrelativesGrayToolStripMenuItem.Size = new System.Drawing.Size(234, 24);
            this.drawNonrelativesGrayToolStripMenuItem.Text = "Draw non relatives gray";
            this.drawNonrelativesGrayToolStripMenuItem.Click += new System.EventHandler(this.drawNonrelativesGrayToolStripMenuItem_Click);
            // 
            // orderRevisionsByDateToolStripMenuItem
            // 
            this.orderRevisionsByDateToolStripMenuItem.Name = "orderRevisionsByDateToolStripMenuItem";
            this.orderRevisionsByDateToolStripMenuItem.Size = new System.Drawing.Size(234, 24);
            this.orderRevisionsByDateToolStripMenuItem.Text = "Order revisions by date";
            this.orderRevisionsByDateToolStripMenuItem.Click += new System.EventHandler(this.OrderRevisionsByDateToolStripMenuItemClick);
            // 
            // showAuthorDateToolStripMenuItem
            // 
            this.showAuthorDateToolStripMenuItem.Name = "showAuthorDateToolStripMenuItem";
            this.showAuthorDateToolStripMenuItem.Size = new System.Drawing.Size(234, 24);
            this.showAuthorDateToolStripMenuItem.Text = "Show author date";
            this.showAuthorDateToolStripMenuItem.Click += new System.EventHandler(this.ShowAuthorDateToolStripMenuItemClick);
            // 
            // showRelativeDateToolStripMenuItem
            // 
            this.showRelativeDateToolStripMenuItem.Name = "showRelativeDateToolStripMenuItem";
            this.showRelativeDateToolStripMenuItem.Size = new System.Drawing.Size(234, 24);
            this.showRelativeDateToolStripMenuItem.Text = "Show relative date";
            this.showRelativeDateToolStripMenuItem.Click += new System.EventHandler(this.ShowRelativeDateToolStripMenuItemClick);
            // 
            // showGitNotesToolStripMenuItem
            // 
            this.showGitNotesToolStripMenuItem.Name = "showGitNotesToolStripMenuItem";
            this.showGitNotesToolStripMenuItem.Size = new System.Drawing.Size(234, 24);
            this.showGitNotesToolStripMenuItem.Text = "Show git notes";
            this.showGitNotesToolStripMenuItem.Click += new System.EventHandler(this.ShowGitNotesToolStripMenuItem_Click);
            // 
            // showTagsToolStripMenuItem
            // 
            this.showTagsToolStripMenuItem.Name = "showTagsToolStripMenuItem";
            this.showTagsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.showTagsToolStripMenuItem.Text = "Show tags";
            this.showTagsToolStripMenuItem.Click += new System.EventHandler(this.showTagsToolStripMenuItem_Click);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconFilter;
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.filterToolStripMenuItem.Text = "Set advanced filter";
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.FilterToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(261, 6);
            // 
            // runScriptToolStripMenuItem
            // 
            this.runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            this.runScriptToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.runScriptToolStripMenuItem.Text = "Run script";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.Frozen = true;
            this.dataGridViewTextBoxColumn3.HeaderText = "";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.Width = 70;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.Frozen = true;
            this.dataGridViewTextBoxColumn2.HeaderText = "";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn2.Width = 70;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.Frozen = true;
            this.dataGridViewTextBoxColumn1.HeaderText = "";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 70;
            // 
            // SelectionTimer
            // 
            this.SelectionTimer.Interval = 200;
            this.SelectionTimer.Tick += new System.EventHandler(this.SelectionTimerTick);
            // 
            // NoCommits
            // 
            this.NoCommits.Controls.Add(this.NoGit);
            this.NoCommits.Controls.Add(this.GitIgnore);
            this.NoCommits.Controls.Add(this.Commit);
            this.NoCommits.Controls.Add(this.label1);
            this.NoCommits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoCommits.Location = new System.Drawing.Point(0, 0);
            this.NoCommits.Name = "NoCommits";
            this.NoCommits.Size = new System.Drawing.Size(682, 235);
            this.NoCommits.TabIndex = 3;
            // 
            // NoGit
            // 
            this.NoGit.Controls.Add(this.InitRepository);
            this.NoGit.Controls.Add(this.CloneRepository);
            this.NoGit.Controls.Add(this.label2);
            this.NoGit.Location = new System.Drawing.Point(0, 0);
            this.NoGit.Name = "NoGit";
            this.NoGit.Size = new System.Drawing.Size(682, 235);
            this.NoGit.TabIndex = 4;
            // 
            // InitRepository
            // 
            this.InitRepository.Location = new System.Drawing.Point(22, 48);
            this.InitRepository.Name = "InitRepository";
            this.InitRepository.Size = new System.Drawing.Size(195, 31);
            this.InitRepository.TabIndex = 2;
            this.InitRepository.Text = "Initialize repository";
            this.InitRepository.UseVisualStyleBackColor = true;
            this.InitRepository.Click += new System.EventHandler(this.InitRepository_Click);
            // 
            // CloneRepository
            // 
            this.CloneRepository.Location = new System.Drawing.Point(224, 48);
            this.CloneRepository.Name = "CloneRepository";
            this.CloneRepository.Size = new System.Drawing.Size(195, 31);
            this.CloneRepository.TabIndex = 3;
            this.CloneRepository.Text = "Clone repository";
            this.CloneRepository.UseVisualStyleBackColor = true;
            this.CloneRepository.Visible = false;
            this.CloneRepository.Click += new System.EventHandler(this.CloneRepository_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(359, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "The current working dir is not a git repository.";
            // 
            // GitIgnore
            // 
            this.GitIgnore.Location = new System.Drawing.Point(468, 10);
            this.GitIgnore.Name = "GitIgnore";
            this.GitIgnore.Size = new System.Drawing.Size(182, 27);
            this.GitIgnore.TabIndex = 3;
            this.GitIgnore.Text = "Edit .gitignore";
            this.GitIgnore.UseVisualStyleBackColor = true;
            this.GitIgnore.Click += new System.EventHandler(this.GitIgnoreClick);
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(468, 42);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(182, 27);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.CommitClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(521, 184);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // Error
            // 
            this.Error.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Error.Image = global::GitUI.Properties.Resources.error;
            this.Error.Location = new System.Drawing.Point(0, 0);
            this.Error.Name = "Error";
            this.Error.Size = new System.Drawing.Size(682, 235);
            this.Error.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Error.TabIndex = 2;
            this.Error.TabStop = false;
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(682, 235);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 1;
            this.Loading.TabStop = false;
            this.Loading.Visible = false;
            // 
            // quickSearchTimer
            // 
            this.quickSearchTimer.Interval = 500;
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(149, 6);
            // 
            // RevisionGrid
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.NoCommits);
            this.Controls.Add(this.Error);
            this.Controls.Add(this.Loading);
            this.Controls.Add(this.Revisions);
            this.Name = "RevisionGrid";
            this.Size = new System.Drawing.Size(682, 235);
            ((System.ComponentModel.ISupportInitialize)(this.Revisions)).EndInit();
            this.mainContextMenu.ResumeLayout(false);
            this.NoCommits.ResumeLayout(false);
            this.NoCommits.PerformLayout();
            this.NoGit.ResumeLayout(false);
            this.NoGit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DvcsGraph Revisions;
        private System.Windows.Forms.PictureBox Loading;
        private System.Windows.Forms.Timer SelectionTimer;
        public System.Windows.Forms.PictureBox Error;
        private System.Windows.Forms.ContextMenuStrip mainContextMenu;
        private System.Windows.Forms.ToolStripMenuItem createTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createNewBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCurrentBranchToHereToolStripMenuItem;
        private System.Windows.Forms.Panel NoCommits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button GitIgnore;
        private System.Windows.Forms.ToolStripMenuItem ShowRemoteBranches;
        private System.Windows.Forms.ToolStripMenuItem showBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem revertCommitToolStripMenuItem;
        private System.Windows.Forms.Panel NoGit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickCommitToolStripMenuItem;
        private System.Windows.Forms.Timer quickSearchTimer;
        private System.Windows.Forms.ToolStripMenuItem rebaseOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAllBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCurrentBranchOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFilteredBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.ToolStripMenuItem branchNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem messageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem authorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dateToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem markRevisionAsBadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markRevisionAsGoodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopBisectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator bisectSeparator;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem runScriptToolStripMenuItem;
        private System.Windows.Forms.Button InitRepository;
        private System.Windows.Forms.Button CloneRepository;
        private System.Windows.Forms.ToolStripMenuItem showRevisionGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawNonrelativesGrayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orderRevisionsByDateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAuthorDateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showRelativeDateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showGitNotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manipulateCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixupCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem squashCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bisectSkipRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.DataGridViewTextBoxColumn Graph;
        private System.Windows.Forms.ToolStripMenuItem showTagsToolStripMenuItem;
    }
}
