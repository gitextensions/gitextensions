
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
            System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            this.buttonOpenSelectedWorktree = new System.Windows.Forms.Button();
            this.buttonDeleteSelectedWorktree = new System.Windows.Forms.Button();
            this.Worktrees = new System.Windows.Forms.DataGridView();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Branch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sha1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonPruneWorktrees = new System.Windows.Forms.Button();
            this.buttonCreateNewWorktree = new System.Windows.Forms.Button();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.MainPanel.SuspendLayout();
            this.ControlsPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Worktrees)).BeginInit();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(tableLayoutPanel1);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(9);
            this.MainPanel.Size = new System.Drawing.Size(697, 320);
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Controls.Add(this.buttonCreateNewWorktree);
            this.ControlsPanel.Location = new System.Drawing.Point(0, 320);
            this.ControlsPanel.Size = new System.Drawing.Size(697, 41);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(this.buttonOpenSelectedWorktree);
            flowLayoutPanel1.Controls.Add(this.buttonDeleteSelectedWorktree);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 271);
            flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(680, 31);
            flowLayoutPanel1.TabIndex = 3;
            flowLayoutPanel1.WrapContents = false;
            // 
            // buttonOpenSelectedWorktree
            // 
            this.buttonOpenSelectedWorktree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenSelectedWorktree.AutoSize = true;
            this.buttonOpenSelectedWorktree.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.buttonOpenSelectedWorktree.Location = new System.Drawing.Point(569, 3);
            this.buttonOpenSelectedWorktree.Name = "buttonOpenSelectedWorktree";
            this.buttonOpenSelectedWorktree.Size = new System.Drawing.Size(108, 25);
            this.buttonOpenSelectedWorktree.TabIndex = 1;
            this.buttonOpenSelectedWorktree.Text = "&Open selected";
            this.buttonOpenSelectedWorktree.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonOpenSelectedWorktree.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonOpenSelectedWorktree.UseVisualStyleBackColor = true;
            this.buttonOpenSelectedWorktree.Click += new System.EventHandler(this.buttonOpenSelectedWorktree_Click);
            // 
            // buttonDeleteSelectedWorktree
            // 
            this.buttonDeleteSelectedWorktree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDeleteSelectedWorktree.AutoSize = true;
            this.buttonDeleteSelectedWorktree.Image = global::GitUI.Properties.Images.DeleteFile;
            this.buttonDeleteSelectedWorktree.Location = new System.Drawing.Point(451, 3);
            this.buttonDeleteSelectedWorktree.Name = "buttonDeleteSelectedWorktree";
            this.buttonDeleteSelectedWorktree.Size = new System.Drawing.Size(112, 25);
            this.buttonDeleteSelectedWorktree.TabIndex = 1;
            this.buttonDeleteSelectedWorktree.Text = "&Delete selected";
            this.buttonDeleteSelectedWorktree.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonDeleteSelectedWorktree.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonDeleteSelectedWorktree.UseVisualStyleBackColor = true;
            this.buttonDeleteSelectedWorktree.Click += new System.EventHandler(this.buttonDeleteSelectedWorktree_Click);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(this.Worktrees, 0, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(679, 302);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // Worktrees
            // 
            this.Worktrees.AllowUserToAddRows = false;
            this.Worktrees.AllowUserToDeleteRows = false;
            this.Worktrees.AllowUserToResizeRows = false;
            this.Worktrees.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Worktrees.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.Worktrees.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Worktrees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Worktrees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Path,
            this.Type,
            this.Branch,
            this.Sha1});
            this.Worktrees.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Worktrees.Location = new System.Drawing.Point(3, 3);
            this.Worktrees.MultiSelect = false;
            this.Worktrees.Name = "Worktrees";
            this.Worktrees.ReadOnly = true;
            this.Worktrees.RowHeadersVisible = false;
            this.Worktrees.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Worktrees.Size = new System.Drawing.Size(674, 265);
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
            // buttonPruneWorktrees
            // 
            this.buttonPruneWorktrees.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPruneWorktrees.AutoSize = true;
            this.buttonPruneWorktrees.Location = new System.Drawing.Point(12, 327);
            this.buttonPruneWorktrees.Name = "buttonPruneWorktrees";
            this.buttonPruneWorktrees.Size = new System.Drawing.Size(144, 25);
            this.buttonPruneWorktrees.TabIndex = 1;
            this.buttonPruneWorktrees.Text = "&Prune deleted worktrees";
            this.buttonPruneWorktrees.UseVisualStyleBackColor = true;
            this.buttonPruneWorktrees.Click += new System.EventHandler(this.buttonPruneWorktrees_Click);
            // 
            // buttonCreateNewWorktree
            // 
            this.buttonCreateNewWorktree.AutoSize = true;
            this.buttonCreateNewWorktree.Image = global::GitUI.Properties.Images.FileStatusAdded;
            this.buttonCreateNewWorktree.Location = new System.Drawing.Point(584, 8);
            this.buttonCreateNewWorktree.Name = "buttonCreateNewWorktree";
            this.buttonCreateNewWorktree.Size = new System.Drawing.Size(100, 25);
            this.buttonCreateNewWorktree.TabIndex = 1;
            this.buttonCreateNewWorktree.Text = "&Create...";
            this.buttonCreateNewWorktree.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonCreateNewWorktree.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonCreateNewWorktree.UseVisualStyleBackColor = true;
            this.buttonCreateNewWorktree.Click += new System.EventHandler(this.buttonCreateNewWorktree_Click);
            // 
            // FormManageWorktree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(697, 361);
            this.Controls.Add(this.buttonPruneWorktrees);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(710, 200);
            this.Name = "FormManageWorktree";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Existing worktrees";
            this.Controls.SetChildIndex(this.ControlsPanel, 0);
            this.Controls.SetChildIndex(this.MainPanel, 0);
            this.Controls.SetChildIndex(this.buttonPruneWorktrees, 0);
            this.MainPanel.ResumeLayout(false);
            this.ControlsPanel.ResumeLayout(false);
            this.ControlsPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Worktrees)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
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
