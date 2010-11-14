
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
            if (disposing && _revisionGraphCommand != null)
            {
                _revisionGraphCommand.Dispose();
                _revisionGraphCommand = null;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGrid));
            this.Revisions = new GitUI.DvcsGraph();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreateTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.markRevisionAsBadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markRevisionAsGoodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.messageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.branchNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.createTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebaseOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCurrentBranchToHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCurrentBranchOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFilteredBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowRemoteBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.showRevisionGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawNonrelativesGrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAuthorDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRelativeDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orderRevisionsByDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelecctionTimer = new System.Windows.Forms.Timer(this.components);
            this.NoCommits = new System.Windows.Forms.Panel();
            this.NoGit = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.GitIgnore = new System.Windows.Forms.Button();
            this.Commit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Error = new System.Windows.Forms.PictureBox();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.quickSearchTimer = new System.Windows.Forms.Timer(this.components);
            this.stopBisectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bisectSeparator = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)(this.Revisions)).BeginInit();
            this.CreateTag.SuspendLayout();
            this.NoCommits.SuspendLayout();
            this.NoGit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // Revisions
            // 
            this.Revisions.AllowUserToAddRows = false;
            this.Revisions.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.Message,
            this.Author,
            this.Date});
            this.Revisions.ContextMenuStrip = this.CreateTag;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.DefaultCellStyle = dataGridViewCellStyle4;
            this.Revisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Revisions.FilterMode = GitUI.DvcsGraph.FilterType.None;
            this.Revisions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Revisions.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.Revisions.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(185)))), ((int)(((byte)(235)))));
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.Revisions.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.SelectedIds = null;
            this.Revisions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Revisions.Size = new System.Drawing.Size(585, 204);
            this.Revisions.StandardTab = true;
            this.Revisions.TabIndex = 0;
            this.Revisions.VirtualMode = true;
            this.Revisions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RevisionsMouseClick);
            this.Revisions.DoubleClick += new System.EventHandler(this.RevisionsDoubleClick);
            this.Revisions.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.RevisionsCellMouseDown);
            this.Revisions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RevisionsKeyUp);
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
            // CreateTag
            // 
            this.CreateTag.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.markRevisionAsBadToolStripMenuItem,
            this.markRevisionAsGoodToolStripMenuItem,
            this.stopBisectToolStripMenuItem,
            this.bisectSeparator,
            this.copyToClipboardToolStripMenuItem,
            this.toolStripSeparator5,
            this.createTagToolStripMenuItem,
            this.deleteTagToolStripMenuItem,
            this.createNewBranchToolStripMenuItem,
            this.deleteBranchToolStripMenuItem,
            this.toolStripSeparator2,
            this.mergeBranchToolStripMenuItem,
            this.rebaseOnToolStripMenuItem,
            this.resetCurrentBranchToHereToolStripMenuItem,
            this.checkoutBranchToolStripMenuItem,
            this.checkoutRevisionToolStripMenuItem,
            this.revertCommitToolStripMenuItem,
            this.cherryPickCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.showBranchesToolStripMenuItem,
            this.showRevisionGraphToolStripMenuItem,
            this.drawNonrelativesGrayToolStripMenuItem,
            this.showAuthorDateToolStripMenuItem,
            this.showRelativeDateToolStripMenuItem,
            this.orderRevisionsByDateToolStripMenuItem,
            this.toolStripSeparator3,
            this.filterToolStripMenuItem});
            this.CreateTag.Name = "CreateTag";
            this.CreateTag.Size = new System.Drawing.Size(226, 540);
            this.CreateTag.Opening += new System.ComponentModel.CancelEventHandler(this.CreateTagOpening);
            // 
            // markRevisionAsBadToolStripMenuItem
            // 
            this.markRevisionAsBadToolStripMenuItem.Name = "markRevisionAsBadToolStripMenuItem";
            this.markRevisionAsBadToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.markRevisionAsBadToolStripMenuItem.Text = "Mark revision as bad";
            this.markRevisionAsBadToolStripMenuItem.Click += new System.EventHandler(this.markRevisionAsBadToolStripMenuItem_Click);
            // 
            // markRevisionAsGoodToolStripMenuItem
            // 
            this.markRevisionAsGoodToolStripMenuItem.Name = "markRevisionAsGoodToolStripMenuItem";
            this.markRevisionAsGoodToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.markRevisionAsGoodToolStripMenuItem.Text = "Mark revision as good";
            this.markRevisionAsGoodToolStripMenuItem.Click += new System.EventHandler(this.markRevisionAsGoodToolStripMenuItem_Click);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.messageToolStripMenuItem,
            this.authorToolStripMenuItem,
            this.dateToolStripMenuItem,
            this.toolStripSeparator6,
            this.branchNameToolStripMenuItem,
            this.tagToolStripMenuItem});
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            // 
            // messageToolStripMenuItem
            // 
            this.messageToolStripMenuItem.Name = "messageToolStripMenuItem";
            this.messageToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.messageToolStripMenuItem.Text = "Message";
            this.messageToolStripMenuItem.Click += new System.EventHandler(this.messageToolStripMenuItem_Click);
            // 
            // authorToolStripMenuItem
            // 
            this.authorToolStripMenuItem.Name = "authorToolStripMenuItem";
            this.authorToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.authorToolStripMenuItem.Text = "Author";
            this.authorToolStripMenuItem.Click += new System.EventHandler(this.authorToolStripMenuItem_Click);
            // 
            // dateToolStripMenuItem
            // 
            this.dateToolStripMenuItem.Name = "dateToolStripMenuItem";
            this.dateToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.dateToolStripMenuItem.Text = "Date";
            this.dateToolStripMenuItem.Click += new System.EventHandler(this.dateToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(144, 6);
            // 
            // branchNameToolStripMenuItem
            // 
            this.branchNameToolStripMenuItem.Name = "branchNameToolStripMenuItem";
            this.branchNameToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.branchNameToolStripMenuItem.Text = "Branch name";
            // 
            // tagToolStripMenuItem
            // 
            this.tagToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.tagToolStripMenuItem.Text = "Tag";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(222, 6);
            // 
            // createTagToolStripMenuItem
            // 
            this.createTagToolStripMenuItem.Image = global::GitUI.Properties.Resources._33;
            this.createTagToolStripMenuItem.Name = "createTagToolStripMenuItem";
            this.createTagToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.createTagToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.createTagToolStripMenuItem.Text = "Create new tag";
            this.createTagToolStripMenuItem.Click += new System.EventHandler(this.CreateTagToolStripMenuItemClick);
            // 
            // deleteTagToolStripMenuItem
            // 
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.deleteTagToolStripMenuItem.Text = "Delete tag";
            // 
            // createNewBranchToolStripMenuItem
            // 
            this.createNewBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources._35;
            this.createNewBranchToolStripMenuItem.Name = "createNewBranchToolStripMenuItem";
            this.createNewBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.createNewBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.createNewBranchToolStripMenuItem.Text = "Create new branch";
            this.createNewBranchToolStripMenuItem.Click += new System.EventHandler(this.CreateNewBranchToolStripMenuItemClick);
            // 
            // deleteBranchToolStripMenuItem
            // 
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.deleteBranchToolStripMenuItem.Text = "Delete branch";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(222, 6);
            // 
            // mergeBranchToolStripMenuItem
            // 
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.mergeBranchToolStripMenuItem.Text = "Merge with branch";
            // 
            // rebaseOnToolStripMenuItem
            // 
            this.rebaseOnToolStripMenuItem.Name = "rebaseOnToolStripMenuItem";
            this.rebaseOnToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.rebaseOnToolStripMenuItem.Text = "Rebase on";
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.resetCurrentBranchToHereToolStripMenuItem.Text = "Reset current branch to here";
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.ResetCurrentBranchToHereToolStripMenuItemClick);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.checkoutBranchToolStripMenuItem.Text = "Checkout branch";
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.CheckoutBranchToolStripMenuItemClick);
            // 
            // checkoutRevisionToolStripMenuItem
            // 
            this.checkoutRevisionToolStripMenuItem.Name = "checkoutRevisionToolStripMenuItem";
            this.checkoutRevisionToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.checkoutRevisionToolStripMenuItem.Text = "Checkout revision";
            this.checkoutRevisionToolStripMenuItem.Click += new System.EventHandler(this.CheckoutRevisionToolStripMenuItemClick);
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.RevertCommitToolStripMenuItemClick);
            // 
            // cherryPickCommitToolStripMenuItem
            // 
            this.cherryPickCommitToolStripMenuItem.Name = "cherryPickCommitToolStripMenuItem";
            this.cherryPickCommitToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.cherryPickCommitToolStripMenuItem.Text = "Cherry pick commit";
            this.cherryPickCommitToolStripMenuItem.Click += new System.EventHandler(this.CherryPickCommitToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(222, 6);
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
            this.showBranchesToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showBranchesToolStripMenuItem.Text = "Show branches";
            // 
            // showAllBranchesToolStripMenuItem
            // 
            this.showAllBranchesToolStripMenuItem.Name = "showAllBranchesToolStripMenuItem";
            this.showAllBranchesToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.showAllBranchesToolStripMenuItem.Text = "Show all branches";
            this.showAllBranchesToolStripMenuItem.Click += new System.EventHandler(this.ShowAllBranchesToolStripMenuItemClick);
            // 
            // showCurrentBranchOnlyToolStripMenuItem
            // 
            this.showCurrentBranchOnlyToolStripMenuItem.Name = "showCurrentBranchOnlyToolStripMenuItem";
            this.showCurrentBranchOnlyToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.showCurrentBranchOnlyToolStripMenuItem.Text = "Show current branch only";
            this.showCurrentBranchOnlyToolStripMenuItem.Click += new System.EventHandler(this.ShowCurrentBranchOnlyToolStripMenuItemClick);
            // 
            // showFilteredBranchesToolStripMenuItem
            // 
            this.showFilteredBranchesToolStripMenuItem.Name = "showFilteredBranchesToolStripMenuItem";
            this.showFilteredBranchesToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.showFilteredBranchesToolStripMenuItem.Text = "Show filtered branches";
            this.showFilteredBranchesToolStripMenuItem.Click += new System.EventHandler(this.ShowFilteredBranchesToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(205, 6);
            // 
            // ShowRemoteBranches
            // 
            this.ShowRemoteBranches.Checked = true;
            this.ShowRemoteBranches.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowRemoteBranches.Name = "ShowRemoteBranches";
            this.ShowRemoteBranches.Size = new System.Drawing.Size(208, 22);
            this.ShowRemoteBranches.Text = "Show remote branches";
            this.ShowRemoteBranches.Click += new System.EventHandler(this.ShowRemoteBranchesClick);
            // 
            // showRevisionGraphToolStripMenuItem
            // 
            this.showRevisionGraphToolStripMenuItem.Name = "showRevisionGraphToolStripMenuItem";
            this.showRevisionGraphToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showRevisionGraphToolStripMenuItem.Text = "Show revision graph";
            this.showRevisionGraphToolStripMenuItem.Click += new System.EventHandler(this.ShowRevisionGraphToolStripMenuItemClick);
            // 
            // drawNonrelativesGrayToolStripMenuItem
            // 
            this.drawNonrelativesGrayToolStripMenuItem.Name = "drawNonrelativesGrayToolStripMenuItem";
            this.drawNonrelativesGrayToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.drawNonrelativesGrayToolStripMenuItem.Text = "Draw non relatives gray";
            this.drawNonrelativesGrayToolStripMenuItem.Click += new System.EventHandler(this.drawNonrelativesGrayToolStripMenuItem_Click);
            // 
            // showAuthorDateToolStripMenuItem
            // 
            this.showAuthorDateToolStripMenuItem.Name = "showAuthorDateToolStripMenuItem";
            this.showAuthorDateToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showAuthorDateToolStripMenuItem.Text = "Show author date";
            this.showAuthorDateToolStripMenuItem.Click += new System.EventHandler(this.ShowAuthorDateToolStripMenuItemClick);
            // 
            // showRelativeDateToolStripMenuItem
            // 
            this.showRelativeDateToolStripMenuItem.Name = "showRelativeDateToolStripMenuItem";
            this.showRelativeDateToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showRelativeDateToolStripMenuItem.Text = "Show relative date";
            this.showRelativeDateToolStripMenuItem.Click += new System.EventHandler(this.ShowRelativeDateToolStripMenuItemClick);
            // 
            // orderRevisionsByDateToolStripMenuItem
            // 
            this.orderRevisionsByDateToolStripMenuItem.Name = "orderRevisionsByDateToolStripMenuItem";
            this.orderRevisionsByDateToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.orderRevisionsByDateToolStripMenuItem.Text = "Order revisions by date";
            this.orderRevisionsByDateToolStripMenuItem.Click += new System.EventHandler(this.OrderRevisionsByDateToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(222, 6);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.filterToolStripMenuItem.Text = "Set advanced filter";
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.FilterToolStripMenuItemClick);
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewTextBoxColumn2.Frozen = true;
            this.dataGridViewTextBoxColumn2.HeaderText = "";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn2.Width = 70;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridViewTextBoxColumn1.Frozen = true;
            this.dataGridViewTextBoxColumn1.HeaderText = "";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 70;
            // 
            // SelecctionTimer
            // 
            this.SelecctionTimer.Interval = 200;
            this.SelecctionTimer.Tick += new System.EventHandler(this.SelecctionTimerTick);
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
            this.NoCommits.Size = new System.Drawing.Size(585, 204);
            this.NoCommits.TabIndex = 3;
            // 
            // NoGit
            // 
            this.NoGit.Controls.Add(this.label2);
            this.NoGit.Location = new System.Drawing.Point(0, 0);
            this.NoGit.Name = "NoGit";
            this.NoGit.Size = new System.Drawing.Size(585, 204);
            this.NoGit.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(227, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "The current working dir is not a git repository.";
            // 
            // GitIgnore
            // 
            this.GitIgnore.Location = new System.Drawing.Point(401, 9);
            this.GitIgnore.Name = "GitIgnore";
            this.GitIgnore.Size = new System.Drawing.Size(156, 23);
            this.GitIgnore.TabIndex = 3;
            this.GitIgnore.Text = "Edit .gitignore";
            this.GitIgnore.UseVisualStyleBackColor = true;
            this.GitIgnore.Click += new System.EventHandler(this.GitIgnoreClick);
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(401, 36);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(156, 23);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.CommitClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(334, 104);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // Error
            // 
            this.Error.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Error.Image = global::GitUI.Properties.Resources.error;
            this.Error.Location = new System.Drawing.Point(0, 0);
            this.Error.Name = "Error";
            this.Error.Size = new System.Drawing.Size(585, 204);
            this.Error.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Error.TabIndex = 2;
            this.Error.TabStop = false;
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(585, 204);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 1;
            this.Loading.TabStop = false;
            this.Loading.Visible = false;
            // 
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // quickSearchTimer
            // 
            this.quickSearchTimer.Interval = 500;
            // 
            // stopBisectToolStripMenuItem
            // 
            this.stopBisectToolStripMenuItem.Name = "stopBisectToolStripMenuItem";
            this.stopBisectToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.stopBisectToolStripMenuItem.Text = "Stop bisect";
            this.stopBisectToolStripMenuItem.Click += new System.EventHandler(this.stopBisectToolStripMenuItem_Click);
            // 
            // bisectSeparator
            // 
            this.bisectSeparator.Name = "bisectSeparator";
            this.bisectSeparator.Size = new System.Drawing.Size(222, 6);
            // 
            // RevisionGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.NoCommits);
            this.Controls.Add(this.Error);
            this.Controls.Add(this.Loading);
            this.Controls.Add(this.Revisions);
            this.Name = "RevisionGrid";
            this.Size = new System.Drawing.Size(585, 204);
            ((System.ComponentModel.ISupportInitialize)(this.Revisions)).EndInit();
            this.CreateTag.ResumeLayout(false);
            this.NoCommits.ResumeLayout(false);
            this.NoCommits.PerformLayout();
            this.NoGit.ResumeLayout(false);
            this.NoGit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Error)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DvcsGraph Revisions;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.PictureBox Loading;
        private System.Windows.Forms.Timer SelecctionTimer;
        public System.Windows.Forms.PictureBox Error;
        private System.Windows.Forms.ContextMenuStrip CreateTag;
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
        private System.Windows.Forms.ToolStripMenuItem showRevisionGraphToolStripMenuItem;
        private System.Windows.Forms.Panel NoGit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutRevisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orderRevisionsByDateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAuthorDateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showRelativeDateToolStripMenuItem;
        private System.Windows.Forms.Timer quickSearchTimer;
        private System.Windows.Forms.ToolStripMenuItem rebaseOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAllBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCurrentBranchOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFilteredBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem drawNonrelativesGrayToolStripMenuItem;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.ToolStripMenuItem stopBisectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator bisectSeparator;
    }
}
