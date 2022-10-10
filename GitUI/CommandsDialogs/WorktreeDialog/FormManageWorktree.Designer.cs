
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
            if (disposing && (components is not null))
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.Worktrees = new System.Windows.Forms.DataGridView();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Branch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sha1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonPruneWorktrees = new System.Windows.Forms.Button();
            this.buttonDeleteSelectedWorktree = new System.Windows.Forms.Button();
            this.buttonOpenSelectedWorktree = new System.Windows.Forms.Button();
            this.buttonCreateNewWorktree = new System.Windows.Forms.Button();
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
            this.panel1.Size = new System.Drawing.Size(696, 214);
            this.panel1.TabIndex = 0;
            // 
            // Worktrees
            // 
            this.Worktrees.AllowUserToAddRows = false;
            this.Worktrees.AllowUserToDeleteRows = false;
            this.Worktrees.AllowUserToResizeRows = false;
            this.Worktrees.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Worktrees.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Worktrees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Worktrees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Path,
            this.Type,
            this.Branch,
            this.Sha1});
            this.Worktrees.Location = new System.Drawing.Point(10, 10);
            this.Worktrees.Margin = new System.Windows.Forms.Padding(10);
            this.Worktrees.MultiSelect = false;
            this.Worktrees.Name = "Worktrees";
            this.Worktrees.ReadOnly = true;
            this.Worktrees.RowHeadersVisible = false;
            this.Worktrees.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Worktrees.Size = new System.Drawing.Size(674, 204);
            this.Worktrees.TabIndex = 2;
            this.Worktrees.SelectionChanged += new System.EventHandler(this.Worktrees_SelectionChanged);
            // 
            // Path
            // 
            this.Path.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Path.HeaderText = "Path";
            this.Path.Name = "Path";
            this.Path.ReadOnly = true;
            this.Path.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Path.Width = 37;
            // 
            // Type
            // 
            this.Type.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Type.Width = 37;
            // 
            // Branch
            // 
            this.Branch.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Branch.HeaderText = "Branch";
            this.Branch.Name = "Branch";
            this.Branch.ReadOnly = true;
            this.Branch.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Branch.Width = 50;
            // 
            // Sha1
            // 
            this.Sha1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Sha1.HeaderText = "SHA-1";
            this.Sha1.MinimumWidth = 90;
            this.Sha1.Name = "Sha1";
            this.Sha1.ReadOnly = true;
            this.Sha1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // buttonOpenSelectedWorktree
            // 
            this.buttonOpenSelectedWorktree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenSelectedWorktree.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.buttonOpenSelectedWorktree.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonOpenSelectedWorktree.Location = new System.Drawing.Point(8, 226);
            this.buttonOpenSelectedWorktree.Name = "buttonOpenSelectedWorktree";
            this.buttonOpenSelectedWorktree.Size = new System.Drawing.Size(151, 23);
            this.buttonOpenSelectedWorktree.TabIndex = 1;
            this.buttonOpenSelectedWorktree.Text = "&Open selected";
            this.buttonOpenSelectedWorktree.UseVisualStyleBackColor = true;
            this.buttonOpenSelectedWorktree.Click += new System.EventHandler(this.buttonOpenSelectedWorktree_Click);
            // 
            // buttonDeleteSelectedWorktree
            // 
            this.buttonDeleteSelectedWorktree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDeleteSelectedWorktree.Image = global::GitUI.Properties.Images.DeleteFile;
            this.buttonDeleteSelectedWorktree.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonDeleteSelectedWorktree.Location = new System.Drawing.Point(165, 226);
            this.buttonDeleteSelectedWorktree.Name = "buttonDeleteSelectedWorktree";
            this.buttonDeleteSelectedWorktree.Size = new System.Drawing.Size(151, 23);
            this.buttonDeleteSelectedWorktree.TabIndex = 1;
            this.buttonDeleteSelectedWorktree.Text = "&Delete selected";
            this.buttonDeleteSelectedWorktree.UseVisualStyleBackColor = true;
            this.buttonDeleteSelectedWorktree.Click += new System.EventHandler(this.buttonDeleteSelectedWorktree_Click);
            // 
            // buttonCreateNewWorktree
            // 
            this.buttonCreateNewWorktree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCreateNewWorktree.Image = global::GitUI.Properties.Images.FileStatusAdded;
            this.buttonCreateNewWorktree.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCreateNewWorktree.Location = new System.Drawing.Point(322, 226);
            this.buttonCreateNewWorktree.Name = "buttonCreateNewWorktree";
            this.buttonCreateNewWorktree.Size = new System.Drawing.Size(151, 23);
            this.buttonCreateNewWorktree.TabIndex = 1;
            this.buttonCreateNewWorktree.Text = "&Create...";
            this.buttonCreateNewWorktree.UseVisualStyleBackColor = true;
            this.buttonCreateNewWorktree.Click += new System.EventHandler(this.buttonCreateNewWorktree_Click);
            // 
            // buttonPruneWorktrees
            // 
            this.buttonPruneWorktrees.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPruneWorktrees.Location = new System.Drawing.Point(493, 226);
            this.buttonPruneWorktrees.Name = "buttonPruneWorktrees";
            this.buttonPruneWorktrees.Size = new System.Drawing.Size(189, 23);
            this.buttonPruneWorktrees.TabIndex = 1;
            this.buttonPruneWorktrees.Text = "&Prune deleted worktrees";
            this.buttonPruneWorktrees.UseVisualStyleBackColor = true;
            this.buttonPruneWorktrees.Click += new System.EventHandler(this.buttonPruneWorktrees_Click);
            // 
            // FormManageWorktree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(694, 261);
            this.Controls.Add(this.buttonOpenSelectedWorktree);
            this.Controls.Add(this.buttonDeleteSelectedWorktree);
            this.Controls.Add(this.buttonCreateNewWorktree);
            this.Controls.Add(this.buttonPruneWorktrees);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(710, 200);
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
        private System.Windows.Forms.DataGridView Worktrees;
        private System.Windows.Forms.Button buttonPruneWorktrees;
        private System.Windows.Forms.Button buttonDeleteSelectedWorktree;
        private System.Windows.Forms.Button buttonOpenSelectedWorktree;
        private System.Windows.Forms.DataGridViewTextBoxColumn Path;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Branch;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sha1;
        private System.Windows.Forms.Button buttonCreateNewWorktree;
    }
}
