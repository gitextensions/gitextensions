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
            this.Revisions = new System.Windows.Forms.DataGridView();
            this.Graph = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelecctionTimer = new System.Windows.Forms.Timer(this.components);
            this.ScrollTimer = new System.Windows.Forms.Timer(this.components);
            this.CreateTag = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCurrentBranchToHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Error = new System.Windows.Forms.PictureBox();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.Revisions)).BeginInit();
            this.CreateTag.SuspendLayout();
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
            // SelecctionTimer
            // 
            this.SelecctionTimer.Interval = 200;
            this.SelecctionTimer.Tick += new System.EventHandler(this.SelecctionTimer_Tick);
            // 
            // ScrollTimer
            // 
            this.ScrollTimer.Tick += new System.EventHandler(this.ScrollTimer_Tick);
            // 
            // CreateTag
            // 
            this.CreateTag.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createTagToolStripMenuItem,
            this.createNewBranchToolStripMenuItem,
            this.resetCurrentBranchToHereToolStripMenuItem});
            this.CreateTag.Name = "CreateTag";
            this.CreateTag.Size = new System.Drawing.Size(224, 70);
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
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            this.resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            this.resetCurrentBranchToHereToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.resetCurrentBranchToHereToolStripMenuItem.Text = "Reset current branch to here";
            this.resetCurrentBranchToHereToolStripMenuItem.Click += new System.EventHandler(this.resetCurrentBranchToHereToolStripMenuItem_Click);
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
            // RevisionGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Error);
            this.Controls.Add(this.Loading);
            this.Controls.Add(this.Revisions);
            this.Name = "RevisionGrid";
            this.Size = new System.Drawing.Size(585, 204);
            this.Load += new System.EventHandler(this.RevisionGrid_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Revisions)).EndInit();
            this.CreateTag.ResumeLayout(false);
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
    }
}
