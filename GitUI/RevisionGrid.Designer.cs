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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGrid));
            this.Revisions = new System.Windows.Forms.DataGridView();
            this.Graph = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreateTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.resetCurrentBranchToHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowRemoteBranches = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllBranchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SelecctionTimer = new System.Windows.Forms.Timer(this.components);
            this.ScrollTimer = new System.Windows.Forms.Timer(this.components);
            this.NoCommits = new System.Windows.Forms.Panel();
            this.NoGit = new System.Windows.Forms.Panel();
            this.Clone = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.Init = new System.Windows.Forms.Button();
            this.GitIgnore = new System.Windows.Forms.Button();
            this.Commit = new System.Windows.Forms.Button();
            this.AddFiles = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Error = new System.Windows.Forms.PictureBox();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.showRevisionGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.Revisions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Revisions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Graph,
            this.Message,
            this.Author,
            this.Date});
            this.Revisions.ContextMenuStrip = this.CreateTag;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.DefaultCellStyle = dataGridViewCellStyle1;
            this.Revisions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Revisions.Location = new System.Drawing.Point(0, 0);
            this.Revisions.Name = "Revisions";
            this.Revisions.ReadOnly = true;
            this.Revisions.RowHeadersVisible = false;
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.Revisions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Revisions.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Revisions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Revisions.Size = new System.Drawing.Size(585, 204);
            this.Revisions.StandardTab = true;
            this.Revisions.TabIndex = 0;
            this.Revisions.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Revisions_Scroll);
            this.Revisions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Revisions_MouseClick);
            this.Revisions.DoubleClick += new System.EventHandler(this.Revisions_DoubleClick);
            this.Revisions.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Revisions_CellMouseDown);
            this.Revisions.CellContextMenuStripChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.Revisions_CellContextMenuStripChanged);
            this.Revisions.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Revisions_CellContentClick);
            // 
            // Graph
            // 
            this.Graph.HeaderText = "Graph";
            this.Graph.Name = "Graph";
            this.Graph.ReadOnly = true;
            this.Graph.Width = 200;
            // 
            // Message
            // 
            this.Message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            // 
            // Author
            // 
            this.Author.HeaderText = "Author";
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.Width = 150;
            // 
            // Date
            // 
            this.Date.HeaderText = "Date";
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.Width = 180;
            // 
            // CreateTag
            // 
            this.CreateTag.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createTagToolStripMenuItem,
            this.createNewBranchToolStripMenuItem,
            this.toolStripSeparator2,
            this.resetCurrentBranchToHereToolStripMenuItem,
            this.revertCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.ShowRemoteBranches,
            this.showAllBranchesToolStripMenuItem,
            this.showRevisionGraphToolStripMenuItem});
            this.CreateTag.Name = "CreateTag";
            this.CreateTag.Size = new System.Drawing.Size(224, 192);
            // 
            // createTagToolStripMenuItem
            // 
            this.createTagToolStripMenuItem.Name = "createTagToolStripMenuItem";
            this.createTagToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.createTagToolStripMenuItem.Text = "Create new tag";
            this.createTagToolStripMenuItem.Click += new System.EventHandler(this.createTagToolStripMenuItem_Click);
            // 
            // createNewBranchToolStripMenuItem
            // 
            this.createNewBranchToolStripMenuItem.Name = "createNewBranchToolStripMenuItem";
            this.createNewBranchToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.createNewBranchToolStripMenuItem.Text = "Create new branch";
            this.createNewBranchToolStripMenuItem.Click += new System.EventHandler(this.createNewBranchToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(220, 6);
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.resetCurrentBranchToHereToolStripMenuItem.Text = "Reset current branch to here";
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.resetCurrentBranchToHereToolStripMenuItem_Click);
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.revertCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(220, 6);
            // 
            // ShowRemoteBranches
            // 
            this.ShowRemoteBranches.Checked = true;
            this.ShowRemoteBranches.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowRemoteBranches.Name = "ShowRemoteBranches";
            this.ShowRemoteBranches.Size = new System.Drawing.Size(223, 22);
            this.ShowRemoteBranches.Text = "Show remote branches";
            this.ShowRemoteBranches.Click += new System.EventHandler(this.ShowRemoteBranches_Click);
            // 
            // showAllBranchesToolStripMenuItem
            // 
            this.showAllBranchesToolStripMenuItem.Checked = true;
            this.showAllBranchesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showAllBranchesToolStripMenuItem.Name = "showAllBranchesToolStripMenuItem";
            this.showAllBranchesToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.showAllBranchesToolStripMenuItem.Text = "Show all branches";
            this.showAllBranchesToolStripMenuItem.Click += new System.EventHandler(this.showAllBranchesToolStripMenuItem_Click);
            // 
            // SelecctionTimer
            // 
            this.SelecctionTimer.Interval = 200;
            this.SelecctionTimer.Tick += new System.EventHandler(this.SelecctionTimer_Tick);
            // 
            // ScrollTimer
            // 
            this.ScrollTimer.Tick += new System.EventHandler(this.ScrollTimer_Tick);
            // 
            // NoCommits
            // 
            this.NoCommits.Controls.Add(this.NoGit);
            this.NoCommits.Controls.Add(this.GitIgnore);
            this.NoCommits.Controls.Add(this.Commit);
            this.NoCommits.Controls.Add(this.AddFiles);
            this.NoCommits.Controls.Add(this.label1);
            this.NoCommits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoCommits.Location = new System.Drawing.Point(0, 0);
            this.NoCommits.Name = "NoCommits";
            this.NoCommits.Size = new System.Drawing.Size(585, 204);
            this.NoCommits.TabIndex = 3;
            // 
            // NoGit
            // 
            this.NoGit.Controls.Add(this.Clone);
            this.NoGit.Controls.Add(this.label2);
            this.NoGit.Controls.Add(this.Init);
            this.NoGit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoGit.Location = new System.Drawing.Point(0, 0);
            this.NoGit.Name = "NoGit";
            this.NoGit.Size = new System.Drawing.Size(585, 204);
            this.NoGit.TabIndex = 4;
            // 
            // Clone
            // 
            this.Clone.Location = new System.Drawing.Point(329, 35);
            this.Clone.Name = "Clone";
            this.Clone.Size = new System.Drawing.Size(143, 23);
            this.Clone.TabIndex = 2;
            this.Clone.Text = "Clone existing repository";
            this.Clone.UseVisualStyleBackColor = true;
            this.Clone.Click += new System.EventHandler(this.Clone_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "The current working dir is not a git repository.";
            // 
            // Init
            // 
            this.Init.Location = new System.Drawing.Point(329, 5);
            this.Init.Name = "Init";
            this.Init.Size = new System.Drawing.Size(143, 23);
            this.Init.TabIndex = 0;
            this.Init.Text = "Initialize new repository";
            this.Init.UseVisualStyleBackColor = true;
            this.Init.Click += new System.EventHandler(this.Init_Click);
            // 
            // GitIgnore
            // 
            this.GitIgnore.Location = new System.Drawing.Point(401, 10);
            this.GitIgnore.Name = "GitIgnore";
            this.GitIgnore.Size = new System.Drawing.Size(86, 23);
            this.GitIgnore.TabIndex = 3;
            this.GitIgnore.Text = "Edit .gitignore";
            this.GitIgnore.UseVisualStyleBackColor = true;
            this.GitIgnore.Click += new System.EventHandler(this.GitIgnore_Click);
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(400, 68);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(87, 23);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(400, 39);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(87, 23);
            this.AddFiles.TabIndex = 1;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFiles_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(381, 117);
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
            // showRevisionGraphToolStripMenuItem
            // 
            this.showRevisionGraphToolStripMenuItem.Name = "showRevisionGraphToolStripMenuItem";
            this.showRevisionGraphToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.showRevisionGraphToolStripMenuItem.Text = "Show revision graph";
            this.showRevisionGraphToolStripMenuItem.Click += new System.EventHandler(this.showRevisionGraphToolStripMenuItem_Click);
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
            this.Load += new System.EventHandler(this.RevisionGrid_Load);
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

        private System.Windows.Forms.DataGridView Revisions;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.PictureBox Loading;
        private System.Windows.Forms.Timer SelecctionTimer;
        private System.Windows.Forms.Timer ScrollTimer;
        public System.Windows.Forms.PictureBox Error;
        private System.Windows.Forms.DataGridViewTextBoxColumn Graph;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.ContextMenuStrip CreateTag;
        private System.Windows.Forms.ToolStripMenuItem createTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createNewBranchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCurrentBranchToHereToolStripMenuItem;
        private System.Windows.Forms.Panel NoCommits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button GitIgnore;
        private System.Windows.Forms.Panel NoGit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Init;
        private System.Windows.Forms.ToolStripMenuItem ShowRemoteBranches;
        private System.Windows.Forms.ToolStripMenuItem showAllBranchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem revertCommitToolStripMenuItem;
        private System.Windows.Forms.Button Clone;
        private System.Windows.Forms.ToolStripMenuItem showRevisionGraphToolStripMenuItem;
    }
}
