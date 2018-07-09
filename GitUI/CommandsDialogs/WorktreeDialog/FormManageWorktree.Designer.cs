
namespace GitUI.CommandsDialogs.WorktreeDialog
{
    partial class FormManageWorktree
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Worktrees = new System.Windows.Forms.DataGridView();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Branch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sha1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsDeleted = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Open = new System.Windows.Forms.DataGridViewImageColumn();
            this.Delete = new System.Windows.Forms.DataGridViewImageColumn();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonPruneWorktrees = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Worktrees)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.Worktrees);
            this.panel1.Location = new System.Drawing.Point(-2, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(834, 250);
            this.panel1.TabIndex = 0;
            // 
            // Worktrees
            // 
            this.Worktrees.AllowUserToAddRows = false;
            this.Worktrees.AllowUserToDeleteRows = false;
            this.Worktrees.AllowUserToResizeRows = false;
            this.Worktrees.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Worktrees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Worktrees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Path,
            this.Type,
            this.Branch,
            this.Sha1,
            this.IsDeleted,
            this.Open,
            this.Delete});
            this.Worktrees.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Worktrees.Location = new System.Drawing.Point(0, 0);
            this.Worktrees.MultiSelect = false;
            this.Worktrees.Name = "Worktrees";
            this.Worktrees.ReadOnly = true;
            this.Worktrees.RowHeadersVisible = false;
            this.Worktrees.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Worktrees.Size = new System.Drawing.Size(834, 250);
            this.Worktrees.TabIndex = 2;
            this.Worktrees.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Worktrees_CellClick);
            // 
            // Path
            // 
            this.Path.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Path.HeaderText = "Path";
            this.Path.Name = "Path";
            this.Path.ReadOnly = true;
            this.Path.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Type
            // 
            this.Type.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Branch
            // 
            this.Branch.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Branch.HeaderText = "Branch";
            this.Branch.Name = "Branch";
            this.Branch.ReadOnly = true;
            this.Branch.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Sha1
            // 
            this.Sha1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Sha1.HeaderText = "SHA-1";
            this.Sha1.Name = "Sha1";
            this.Sha1.ReadOnly = true;
            this.Sha1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // IsDeleted
            // 
            this.IsDeleted.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.IsDeleted.HeaderText = "Deleted";
            this.IsDeleted.Name = "IsDeleted";
            this.IsDeleted.ReadOnly = true;
            this.IsDeleted.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Open
            // 
            this.Open.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Open.HeaderText = "Open";
            this.Open.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.Open.Name = "Open";
            this.Open.ReadOnly = true;
            this.Open.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Delete
            // 
            this.Delete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Delete.DefaultCellStyle = dataGridViewCellStyle1;
            this.Delete.HeaderText = "Delete";
            this.Delete.Image = global::GitUI.Properties.Images.Delete;
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            this.Delete.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonClose.Location = new System.Drawing.Point(450, 262);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(178, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonPruneWorktrees
            // 
            this.buttonPruneWorktrees.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonPruneWorktrees.Location = new System.Drawing.Point(213, 262);
            this.buttonPruneWorktrees.Name = "buttonPruneWorktrees";
            this.buttonPruneWorktrees.Size = new System.Drawing.Size(178, 23);
            this.buttonPruneWorktrees.TabIndex = 1;
            this.buttonPruneWorktrees.Text = "Prune the deleted worktrees";
            this.buttonPruneWorktrees.UseVisualStyleBackColor = true;
            this.buttonPruneWorktrees.Click += new System.EventHandler(this.buttonPruneWorktrees_Click);
            // 
            // FormManageWorktree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(832, 297);
            this.Controls.Add(this.buttonPruneWorktrees);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 333);
            this.Name = "FormManageWorktree";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Existing worktrees";
            this.Load += new System.EventHandler(this.FormManageWorktree_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Worktrees)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.DataGridView Worktrees;
        private System.Windows.Forms.Button buttonPruneWorktrees;
        private System.Windows.Forms.DataGridViewTextBoxColumn Path;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Branch;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sha1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsDeleted;
        private System.Windows.Forms.DataGridViewImageColumn Open;
        private System.Windows.Forms.DataGridViewImageColumn Delete;
    }
}