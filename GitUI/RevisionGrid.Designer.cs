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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGrid));
            this.Revisions = new DvcsGraph();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreateTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mergeBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCurrentBranchToHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkoutRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowRemoteBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRevisionGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAuthorDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRelativeDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orderRevisionsByDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.rebaseOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            resources.ApplyResources(this.Revisions, "Revisions");
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Revisions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.Revisions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Revisions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Message,
            this.Author,
            this.Date});
            this.Revisions.ContextMenuStrip = this.CreateTag;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.DefaultCellStyle = dataGridViewCellStyle2;
            this.Revisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Revisions.Location = new System.Drawing.Point(0, 0);
            this.Revisions.Name = "Revisions";
            this.Revisions.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Revisions.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.Revisions.RowHeadersVisible = false;
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Revisions.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Revisions.Size = new System.Drawing.Size(585, 204);
            this.Revisions.StandardTab = true;
            this.Revisions.TabIndex = 0;
            this.Revisions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Revisions_MouseClick);
            this.Revisions.DoubleClick += new System.EventHandler(this.Revisions_DoubleClick);
            this.Revisions.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Revisions_CellMouseDown);
            this.Revisions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Revisions_KeyUp);
            // 
            // Message
            // 
            this.Message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.Message, "Message");
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            // 
            // Author
            // 
            resources.ApplyResources(this.Author, "Author");
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.Width = 150;
            // 
            // Date
            // 
            resources.ApplyResources(this.Date, "Date");
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.Width = 180;
            // 
            // CreateTag
            // 
            resources.ApplyResources(this.CreateTag, "CreateTag");
            this.CreateTag.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
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
            this.ShowRemoteBranches,
            this.showAllBranchesToolStripMenuItem,
            this.showRevisionGraphToolStripMenuItem,
            this.showAuthorDateToolStripMenuItem,
            this.showRelativeDateToolStripMenuItem,
            this.orderRevisionsByDateToolStripMenuItem,
            this.toolStripSeparator3,
            this.filterToolStripMenuItem});
            this.CreateTag.Name = "CreateTag";
            this.CreateTag.Size = new System.Drawing.Size(226, 440);
            this.CreateTag.Opening += new System.ComponentModel.CancelEventHandler(this.CreateTag_Opening);
            // 
            // createTagToolStripMenuItem
            // 
            resources.ApplyResources(this.createTagToolStripMenuItem, "createTagToolStripMenuItem");
            this.createTagToolStripMenuItem.Image = global::GitUI.Properties.Resources._33;
            this.createTagToolStripMenuItem.Name = "createTagToolStripMenuItem";
            this.createTagToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.createTagToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.createTagToolStripMenuItem.Click += new System.EventHandler(this.createTagToolStripMenuItem_Click);
            // 
            // deleteTagToolStripMenuItem
            // 
            resources.ApplyResources(this.deleteTagToolStripMenuItem, "deleteTagToolStripMenuItem");
            this.deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            this.deleteTagToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            // 
            // createNewBranchToolStripMenuItem
            // 
            resources.ApplyResources(this.createNewBranchToolStripMenuItem, "createNewBranchToolStripMenuItem");
            this.createNewBranchToolStripMenuItem.Image = global::GitUI.Properties.Resources._35;
            this.createNewBranchToolStripMenuItem.Name = "createNewBranchToolStripMenuItem";
            this.createNewBranchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.createNewBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.createNewBranchToolStripMenuItem.Click += new System.EventHandler(this.createNewBranchToolStripMenuItem_Click);
            // 
            // deleteBranchToolStripMenuItem
            // 
            resources.ApplyResources(this.deleteBranchToolStripMenuItem, "deleteBranchToolStripMenuItem");
            this.deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            this.deleteBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(222, 6);
            // 
            // mergeBranchToolStripMenuItem
            // 
            resources.ApplyResources(this.mergeBranchToolStripMenuItem, "mergeBranchToolStripMenuItem");
            this.mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            this.mergeBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            resources.ApplyResources(this.resetCurrentBranchToHereToolStripMenuItem, "resetCurrentBranchToHereToolStripMenuItem");
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.resetCurrentBranchToHereToolStripMenuItem_Click);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            resources.ApplyResources(this.checkoutBranchToolStripMenuItem, "checkoutBranchToolStripMenuItem");
            this.checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            this.checkoutBranchToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.checkoutBranchToolStripMenuItem.Click += new System.EventHandler(this.checkoutBranchToolStripMenuItem_Click);
            // 
            // checkoutRevisionToolStripMenuItem
            // 
            resources.ApplyResources(this.checkoutRevisionToolStripMenuItem, "checkoutRevisionToolStripMenuItem");
            this.checkoutRevisionToolStripMenuItem.Name = "checkoutRevisionToolStripMenuItem";
            this.checkoutRevisionToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.checkoutRevisionToolStripMenuItem.Click += new System.EventHandler(this.checkoutRevisionToolStripMenuItem_Click);
            // 
            // revertCommitToolStripMenuItem
            // 
            resources.ApplyResources(this.revertCommitToolStripMenuItem, "revertCommitToolStripMenuItem");
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.revertCommitToolStripMenuItem_Click);
            // 
            // cherryPickCommitToolStripMenuItem
            // 
            resources.ApplyResources(this.cherryPickCommitToolStripMenuItem, "cherryPickCommitToolStripMenuItem");
            this.cherryPickCommitToolStripMenuItem.Name = "cherryPickCommitToolStripMenuItem";
            this.cherryPickCommitToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.cherryPickCommitToolStripMenuItem.Click += new System.EventHandler(this.cherryPickCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(222, 6);
            // 
            // ShowRemoteBranches
            // 
            resources.ApplyResources(this.ShowRemoteBranches, "ShowRemoteBranches");
            this.ShowRemoteBranches.Checked = true;
            this.ShowRemoteBranches.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowRemoteBranches.Name = "ShowRemoteBranches";
            this.ShowRemoteBranches.Size = new System.Drawing.Size(225, 22);
            this.ShowRemoteBranches.Click += new System.EventHandler(this.ShowRemoteBranches_Click);
            // 
            // showAllBranchesToolStripMenuItem
            // 
            resources.ApplyResources(this.showAllBranchesToolStripMenuItem, "showAllBranchesToolStripMenuItem");
            this.showAllBranchesToolStripMenuItem.Checked = true;
            this.showAllBranchesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showAllBranchesToolStripMenuItem.Name = "showAllBranchesToolStripMenuItem";
            this.showAllBranchesToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showAllBranchesToolStripMenuItem.Click += new System.EventHandler(this.showAllBranchesToolStripMenuItem_Click);
            // 
            // showRevisionGraphToolStripMenuItem
            // 
            resources.ApplyResources(this.showRevisionGraphToolStripMenuItem, "showRevisionGraphToolStripMenuItem");
            this.showRevisionGraphToolStripMenuItem.Name = "showRevisionGraphToolStripMenuItem";
            this.showRevisionGraphToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showRevisionGraphToolStripMenuItem.Click += new System.EventHandler(this.showRevisionGraphToolStripMenuItem_Click);
            // 
            // showAuthorDateToolStripMenuItem
            // 
            resources.ApplyResources(this.showAuthorDateToolStripMenuItem, "showAuthorDateToolStripMenuItem");
            this.showAuthorDateToolStripMenuItem.Name = "showAuthorDateToolStripMenuItem";
            this.showAuthorDateToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showAuthorDateToolStripMenuItem.Click += new System.EventHandler(this.showAuthorDateToolStripMenuItem_Click);
            // 
            // showRelativeDateToolStripMenuItem
            // 
            resources.ApplyResources(this.showRelativeDateToolStripMenuItem, "showRelativeDateToolStripMenuItem");
            this.showRelativeDateToolStripMenuItem.Name = "showRelativeDateToolStripMenuItem";
            this.showRelativeDateToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showRelativeDateToolStripMenuItem.Click += new System.EventHandler(this.showRelativeDateToolStripMenuItem_Click);
            // 
            // orderRevisionsByDateToolStripMenuItem
            // 
            resources.ApplyResources(this.orderRevisionsByDateToolStripMenuItem, "orderRevisionsByDateToolStripMenuItem");
            this.orderRevisionsByDateToolStripMenuItem.Name = "orderRevisionsByDateToolStripMenuItem";
            this.orderRevisionsByDateToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.orderRevisionsByDateToolStripMenuItem.Click += new System.EventHandler(this.orderRevisionsByDateToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(222, 6);
            // 
            // filterToolStripMenuItem
            // 
            resources.ApplyResources(this.filterToolStripMenuItem, "filterToolStripMenuItem");
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.filterToolStripMenuItem_Click);
            // 
            // SelecctionTimer
            // 
            this.SelecctionTimer.Interval = 200;
            this.SelecctionTimer.Tick += new System.EventHandler(this.SelecctionTimer_Tick);
            // 
            // NoCommits
            // 
            resources.ApplyResources(this.NoCommits, "NoCommits");
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
            resources.ApplyResources(this.NoGit, "NoGit");
            this.NoGit.Controls.Add(this.label2);
            this.NoGit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoGit.Location = new System.Drawing.Point(0, 0);
            this.NoGit.Name = "NoGit";
            this.NoGit.Size = new System.Drawing.Size(585, 204);
            this.NoGit.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 10);
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 13);
            this.label2.TabIndex = 1;
            // 
            // GitIgnore
            // 
            this.GitIgnore.Location = new System.Drawing.Point(401, 10);
            resources.ApplyResources(this.GitIgnore, "GitIgnore");
            this.GitIgnore.Name = "GitIgnore";
            this.GitIgnore.Size = new System.Drawing.Size(86, 23);
            this.GitIgnore.TabIndex = 3;
            this.GitIgnore.UseVisualStyleBackColor = true;
            this.GitIgnore.Click += new System.EventHandler(this.GitIgnore_Click);
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(401, 39);
            resources.ApplyResources(this.Commit, "Commit");
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(87, 23);
            this.Commit.TabIndex = 2;
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(315, 104);
            this.label1.TabIndex = 0;
            // 
            // Error
            // 
            this.Error.Dock = System.Windows.Forms.DockStyle.Fill;
            resources.ApplyResources(this.Error, "Error");
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
            resources.ApplyResources(this.Loading, "Loading");
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
            // rebaseOnToolStripMenuItem
            // 
            this.rebaseOnToolStripMenuItem.Name = "rebaseOnToolStripMenuItem";
            this.rebaseOnToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.rebaseOnToolStripMenuItem.Text = "Rebase on";
            // 
            // RevisionGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            resources.ApplyResources(this, "$this");
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
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.ContextMenuStrip CreateTag;
        private System.Windows.Forms.ToolStripMenuItem createTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createNewBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCurrentBranchToHereToolStripMenuItem;
        private System.Windows.Forms.Panel NoCommits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button GitIgnore;
        private System.Windows.Forms.ToolStripMenuItem ShowRemoteBranches;
        private System.Windows.Forms.ToolStripMenuItem showAllBranchesToolStripMenuItem;
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
    }
}
