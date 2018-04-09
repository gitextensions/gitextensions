
using System.Windows.Forms;
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

                if (BuildServerWatcher != null)
                {
                    BuildServerWatcher.Dispose();
                    BuildServerWatcher = null;
                }

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

                if (_authoredRevisionsBrush != null)
                {
                    _authoredRevisionsBrush.Dispose();
                    _authoredRevisionsBrush = null;
                }

                if (_fontOfSHAColumn != null)
                {
                    _fontOfSHAColumn.Dispose();
                    _fontOfSHAColumn = null;
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
            this.GraphDataGridViewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MessageDataGridViewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsMessageMultilineDataGridViewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AuthorDataGridViewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateDataGridViewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IdDataGridViewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mainContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.markRevisionAsBadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markRevisionAsGoodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSkipRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopBisectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.messageCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.branchNameCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagNameCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.showMergeCommitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBuildReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.getHelpOnHowToUseTheseFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseInteractivelyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseWithAdvOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.GraphDataGridViewColumn,
            this.MessageDataGridViewColumn,
            this.AuthorDataGridViewColumn,
            this.DateDataGridViewColumn,
            this.IdDataGridViewColumn,
            this.IsMessageMultilineDataGridViewColumn});
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
            this.Revisions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RevisionsDoubleClick);
            this.Revisions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RevisionsMouseClick);
            // 
            // Graph
            // 
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GraphDataGridViewColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.GraphDataGridViewColumn.Frozen = true;
            this.GraphDataGridViewColumn.HeaderText = "";
            this.GraphDataGridViewColumn.Name = "Graph";
            this.GraphDataGridViewColumn.ReadOnly = true;
            this.GraphDataGridViewColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Message
            // 
            this.MessageDataGridViewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.MessageDataGridViewColumn.HeaderText = "Message";
            this.MessageDataGridViewColumn.Name = "Message";
            this.MessageDataGridViewColumn.ReadOnly = true;
            this.MessageDataGridViewColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.MessageDataGridViewColumn.FillWeight = 80;
            //
            // IsMessageMultilineDataGridViewColumn
            //
            this.IsMessageMultilineDataGridViewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.IsMessageMultilineDataGridViewColumn.HeaderText = "";
            this.IsMessageMultilineDataGridViewColumn.Name = "IsMessageMultiline";
            this.IsMessageMultilineDataGridViewColumn.ReadOnly = true;
            this.IsMessageMultilineDataGridViewColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Author
            // 
            this.AuthorDataGridViewColumn.HeaderText = "Author";
            this.AuthorDataGridViewColumn.Name = "Author";
            this.AuthorDataGridViewColumn.ReadOnly = true;
            this.AuthorDataGridViewColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Date
            // 
            this.DateDataGridViewColumn.HeaderText = "Date";
            this.DateDataGridViewColumn.Name = "Date";
            this.DateDataGridViewColumn.ReadOnly = true;
            this.DateDataGridViewColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            //
            // Id
            //
            this.IdDataGridViewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.IdDataGridViewColumn.HeaderText = "SHA-1";
            this.IdDataGridViewColumn.Name = "ID";
            this.IdDataGridViewColumn.ReadOnly = true;
            this.IdDataGridViewColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.IdDataGridViewColumn.Visible = false;
            this.IdDataGridViewColumn.FillWeight = 20f;
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
            this.compareToolStripMenuItem,
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
            this.markRevisionAsBadToolStripMenuItem.Name = "markRevisionAsBadToolStripMenuItem";
            this.markRevisionAsBadToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.markRevisionAsBadToolStripMenuItem.Text = "Mark revision as bad";
            this.markRevisionAsBadToolStripMenuItem.Click += new System.EventHandler(this.MarkRevisionAsBadToolStripMenuItemClick);
            // 
            // markRevisionAsGoodToolStripMenuItem
            // 
            this.markRevisionAsGoodToolStripMenuItem.Name = "markRevisionAsGoodToolStripMenuItem";
            this.markRevisionAsGoodToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.markRevisionAsGoodToolStripMenuItem.Text = "Mark revision as good";
            this.markRevisionAsGoodToolStripMenuItem.Click += new System.EventHandler(this.MarkRevisionAsGoodToolStripMenuItemClick);
            // 
            // bisectSkipRevisionToolStripMenuItem
            // 
            this.bisectSkipRevisionToolStripMenuItem.Name = "bisectSkipRevisionToolStripMenuItem";
            this.bisectSkipRevisionToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.bisectSkipRevisionToolStripMenuItem.Text = "Skip revision";
            this.bisectSkipRevisionToolStripMenuItem.Click += new System.EventHandler(this.BisectSkipRevisionToolStripMenuItemClick);
            // 
            // stopBisectToolStripMenuItem
            // 
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
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.branchNameCopyToolStripMenuItem,
            this.tagNameCopyToolStripMenuItem,
            this.toolStripSeparator6,
            this.hashCopyToolStripMenuItem,
            this.messageCopyToolStripMenuItem,
            this.authorCopyToolStripMenuItem,
            this.dateCopyToolStripMenuItem});
            this.copyToClipboardToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCopyToClipboard;
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            this.copyToClipboardToolStripMenuItem.DropDownOpened += new System.EventHandler(this.copyToClipboardToolStripMenuItem_DropDownOpened);
            // 
            // branchNameCopyToolStripMenuItem
            // 
            this.branchNameCopyToolStripMenuItem.Enabled = false;
            this.branchNameCopyToolStripMenuItem.Name = "branchNameCopyToolStripMenuItem";
            this.branchNameCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.branchNameCopyToolStripMenuItem.Text = "Branch name:";
            // 
            // tagNameCopyToolStripMenuItem
            // 
            this.tagNameCopyToolStripMenuItem.Enabled = false;
            this.tagNameCopyToolStripMenuItem.Name = "tagNameCopyToolStripMenuItem";
            this.tagNameCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.tagNameCopyToolStripMenuItem.Text = "Tag name:";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(185, 6);
            // 
            // hashCopyToolStripMenuItem
            // 
            this.hashCopyToolStripMenuItem.Name = "hashCopyToolStripMenuItem";
            this.hashCopyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.hashCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.hashCopyToolStripMenuItem.Text = "Commit hash";
            this.hashCopyToolStripMenuItem.Click += new System.EventHandler(this.HashToolStripMenuItemClick);
            // 
            // messageCopyToolStripMenuItem
            // 
            this.messageCopyToolStripMenuItem.Name = "messageCopyToolStripMenuItem";
            this.messageCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.messageCopyToolStripMenuItem.Text = "Message";
            this.messageCopyToolStripMenuItem.Click += new System.EventHandler(this.MessageToolStripMenuItemClick);
            // 
            // authorCopyToolStripMenuItem
            // 
            this.authorCopyToolStripMenuItem.Name = "authorCopyToolStripMenuItem";
            this.authorCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.authorCopyToolStripMenuItem.Text = "Author";
            this.authorCopyToolStripMenuItem.Click += new System.EventHandler(this.AuthorToolStripMenuItemClick);
            // 
            // dateCopyToolStripMenuItem
            // 
            this.dateCopyToolStripMenuItem.Name = "dateCopyToolStripMenuItem";
            this.dateCopyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.dateCopyToolStripMenuItem.Text = "Date";
            this.dateCopyToolStripMenuItem.Click += new System.EventHandler(this.DateToolStripMenuItemClick);
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
            // rebaseOnToolStripMenuItem
            // 
            this.rebaseOnToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rebaseToolStripMenuItem,
            this.rebaseInteractivelyToolStripMenuItem,
            this.rebaseWithAdvOptionsToolStripMenuItem});
            this.rebaseOnToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconRebase;
            this.rebaseOnToolStripMenuItem.Name = "rebaseOnToolStripMenuItem";
            this.rebaseOnToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.rebaseOnToolStripMenuItem.Text = "Rebase current branch on";
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            this.resetCurrentBranchToHereToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconResetCurrentBranchToHere;
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.resetCurrentBranchToHereToolStripMenuItem.Text = "Reset current branch to here";
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.ResetCurrentBranchToHereToolStripMenuItemClick);
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
            // renameBranchToolStripMenuItem
            // 
            this.renameBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.Renamed;
            this.renameBranchToolStripMenuItem.Name = "renameBranchToolStripMenuItem";
            this.renameBranchToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.renameBranchToolStripMenuItem.Text = "Rename branch";
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconBranchDelete;
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(264, 24);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch";
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
            this.selectAsBaseToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.L;
            this.selectAsBaseToolStripMenuItem.Click += new System.EventHandler(this.selectAsBaseToolStripMenuItem_Click);
            // 
            // selectAsBaseToolStripMenuItem
            // 
            this.compareToBaseToolStripMenuItem.Name = "compareToBaseToolStripMenuItem";
            this.compareToBaseToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareToBaseToolStripMenuItem.Text = "Compare to BASE";
            this.compareToBaseToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            this.compareToBaseToolStripMenuItem.Click += new System.EventHandler(this.compareToBaseToolStripMenuItem_Click);
            // 
            // compareToolStripMenuItem
            // 
            this.compareToolStripMenuItem.Name = "compareToolStripMenuItem";
            this.compareToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.compareToolStripMenuItem.Text = "Compare";
            this.compareToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                compareToBranchToolStripMenuItem,
                compareWithCurrentBranchToolStripMenuItem,
                selectAsBaseToolStripMenuItem,
                compareToBaseToolStripMenuItem,
            });
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
            this.fixupCommitToolStripMenuItem.Text = "Create a fixup commit";
            this.fixupCommitToolStripMenuItem.Click += new System.EventHandler(this.FixupCommitToolStripMenuItemClick);
            // 
            // squashCommitToolStripMenuItem
            // 
            this.squashCommitToolStripMenuItem.Name = "squashCommitToolStripMenuItem";
            this.squashCommitToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.squashCommitToolStripMenuItem.Text = "Create a squash commit";
            this.squashCommitToolStripMenuItem.Click += new System.EventHandler(this.SquashCommitToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(220, 6);
            // 
            // navigateToolStripMenuItem
            // 
            this.navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            this.navigateToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.navigateToolStripMenuItem.Text = "Navigate";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // runScriptToolStripMenuItem
            // 
            this.runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            this.runScriptToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.runScriptToolStripMenuItem.Text = "Run script";
            // 
            // openBuildReportToolStripMenuItem
            // 
            this.openBuildReportToolStripMenuItem.Name = "openBuildReportToolStripMenuItem";
            this.openBuildReportToolStripMenuItem.Size = new System.Drawing.Size(301, 26);
            this.openBuildReportToolStripMenuItem.Text = "Open build report in the browser";
            this.openBuildReportToolStripMenuItem.Click += new System.EventHandler(this.openBuildReportToolStripMenuItem_Click);
            // 
            // showMergeCommitsToolStripMenuItem
            // 
            this.showMergeCommitsToolStripMenuItem.Name = "showMergeCommitsToolStripMenuItem";
            this.showMergeCommitsToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
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
            this.label2.Size = new System.Drawing.Size(281, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "The current working directory is not a git repository.";
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
            this.label1.Size = new System.Drawing.Size(393, 120);
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
            this.rebaseInteractivelyToolStripMenuItem.Text = "Selected commit interactively";
            this.rebaseInteractivelyToolStripMenuItem.Click += new System.EventHandler(this.OnRebaseInteractivelyClicked);
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
        private System.Windows.Forms.ToolStripMenuItem navigateToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem revertCommitToolStripMenuItem;
        private System.Windows.Forms.Panel NoGit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem deleteTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickCommitToolStripMenuItem;
        private System.Windows.Forms.Timer quickSearchTimer;
        private System.Windows.Forms.ToolStripMenuItem rebaseOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebaseInteractivelyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem branchNameCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagNameCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem messageCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem authorCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dateCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem markRevisionAsBadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markRevisionAsGoodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopBisectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator bisectSeparator;
        private System.Windows.Forms.ToolStripMenuItem runScriptToolStripMenuItem;
        private System.Windows.Forms.Button InitRepository;
        private System.Windows.Forms.Button CloneRepository;
        private System.Windows.Forms.ToolStripMenuItem showMergeCommitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manipulateCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rewordCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixupCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem squashCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hashCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bisectSkipRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.DataGridViewTextBoxColumn MessageDataGridViewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn AuthorDataGridViewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateDataGridViewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn IdDataGridViewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn GraphDataGridViewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsMessageMultilineDataGridViewColumn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem compareToBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareWithCurrentBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToBaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAsBaseToolStripMenuItem;
        private ToolStripMenuItem getHelpOnHowToUseTheseFeaturesToolStripMenuItem;
        private ToolStripMenuItem openBuildReportToolStripMenuItem;
        private ToolStripMenuItem editCommitToolStripMenuItem;
        private ToolStripMenuItem rebaseWithAdvOptionsToolStripMenuItem;
    }
}
