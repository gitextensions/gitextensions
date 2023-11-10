
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
            FlowLayoutPanel flowLayoutPanel1;
            TableLayoutPanel tableLayoutPanel1;
            buttonOpenSelectedWorktree = new Button();
            buttonDeleteSelectedWorktree = new Button();
            Worktrees = new DataGridView();
            Path = new DataGridViewTextBoxColumn();
            Type = new DataGridViewTextBoxColumn();
            Branch = new DataGridViewTextBoxColumn();
            Sha1 = new DataGridViewTextBoxColumn();
            buttonPruneWorktrees = new Button();
            buttonCreateNewWorktree = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(Worktrees)).BeginInit();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tableLayoutPanel1);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(697, 320);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(buttonCreateNewWorktree);
            ControlsPanel.Location = new Point(0, 320);
            ControlsPanel.Size = new Size(697, 41);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(buttonOpenSelectedWorktree);
            flowLayoutPanel1.Controls.Add(buttonDeleteSelectedWorktree);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(0, 271);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(680, 31);
            flowLayoutPanel1.TabIndex = 3;
            flowLayoutPanel1.WrapContents = false;
            // 
            // buttonOpenSelectedWorktree
            // 
            buttonOpenSelectedWorktree.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOpenSelectedWorktree.AutoSize = true;
            buttonOpenSelectedWorktree.Image = Properties.Images.BrowseFileExplorer;
            buttonOpenSelectedWorktree.Location = new Point(569, 3);
            buttonOpenSelectedWorktree.Name = "buttonOpenSelectedWorktree";
            buttonOpenSelectedWorktree.Size = new Size(108, 25);
            buttonOpenSelectedWorktree.TabIndex = 1;
            buttonOpenSelectedWorktree.Text = "&Open selected";
            buttonOpenSelectedWorktree.TextAlign = ContentAlignment.MiddleRight;
            buttonOpenSelectedWorktree.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonOpenSelectedWorktree.UseVisualStyleBackColor = true;
            buttonOpenSelectedWorktree.Click += buttonOpenSelectedWorktree_Click;
            // 
            // buttonDeleteSelectedWorktree
            // 
            buttonDeleteSelectedWorktree.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonDeleteSelectedWorktree.AutoSize = true;
            buttonDeleteSelectedWorktree.Image = Properties.Images.DeleteFile;
            buttonDeleteSelectedWorktree.Location = new Point(451, 3);
            buttonDeleteSelectedWorktree.Name = "buttonDeleteSelectedWorktree";
            buttonDeleteSelectedWorktree.Size = new Size(112, 25);
            buttonDeleteSelectedWorktree.TabIndex = 1;
            buttonDeleteSelectedWorktree.Text = "&Delete selected";
            buttonDeleteSelectedWorktree.TextAlign = ContentAlignment.MiddleRight;
            buttonDeleteSelectedWorktree.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonDeleteSelectedWorktree.UseVisualStyleBackColor = true;
            buttonDeleteSelectedWorktree.Click += buttonDeleteSelectedWorktree_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(Worktrees, 0, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(9, 9);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(679, 302);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // Worktrees
            // 
            Worktrees.AllowUserToAddRows = false;
            Worktrees.AllowUserToDeleteRows = false;
            Worktrees.AllowUserToResizeRows = false;
            Worktrees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Worktrees.BackgroundColor = SystemColors.ControlLight;
            Worktrees.BorderStyle = BorderStyle.Fixed3D;
            Worktrees.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Worktrees.Columns.AddRange(new DataGridViewColumn[] {
            Path,
            Type,
            Branch,
            Sha1});
            Worktrees.Dock = DockStyle.Fill;
            Worktrees.Location = new Point(3, 3);
            Worktrees.MultiSelect = false;
            Worktrees.Name = "Worktrees";
            Worktrees.ReadOnly = true;
            Worktrees.RowHeadersVisible = false;
            Worktrees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Worktrees.Size = new Size(674, 265);
            Worktrees.TabIndex = 2;
            Worktrees.SelectionChanged += Worktrees_SelectionChanged;
            // 
            // Path
            // 
            Path.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Path.HeaderText = "Path";
            Path.Name = "Path";
            Path.ReadOnly = true;
            Path.SortMode = DataGridViewColumnSortMode.NotSortable;
            Path.Width = 37;
            // 
            // Type
            // 
            Type.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Type.HeaderText = "Type";
            Type.Name = "Type";
            Type.ReadOnly = true;
            Type.SortMode = DataGridViewColumnSortMode.NotSortable;
            Type.Width = 37;
            // 
            // Branch
            // 
            Branch.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Branch.HeaderText = "Branch";
            Branch.Name = "Branch";
            Branch.ReadOnly = true;
            Branch.SortMode = DataGridViewColumnSortMode.NotSortable;
            Branch.Width = 50;
            // 
            // Sha1
            // 
            Sha1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Sha1.HeaderText = "SHA-1";
            Sha1.MinimumWidth = 90;
            Sha1.Name = "Sha1";
            Sha1.ReadOnly = true;
            Sha1.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // buttonPruneWorktrees
            // 
            buttonPruneWorktrees.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonPruneWorktrees.AutoSize = true;
            buttonPruneWorktrees.Location = new Point(12, 327);
            buttonPruneWorktrees.Name = "buttonPruneWorktrees";
            buttonPruneWorktrees.Size = new Size(144, 25);
            buttonPruneWorktrees.TabIndex = 1;
            buttonPruneWorktrees.Text = "&Prune deleted worktrees";
            buttonPruneWorktrees.UseVisualStyleBackColor = true;
            buttonPruneWorktrees.Click += buttonPruneWorktrees_Click;
            // 
            // buttonCreateNewWorktree
            // 
            buttonCreateNewWorktree.AutoSize = true;
            buttonCreateNewWorktree.Image = Properties.Images.FileStatusAdded;
            buttonCreateNewWorktree.Location = new Point(584, 8);
            buttonCreateNewWorktree.Name = "buttonCreateNewWorktree";
            buttonCreateNewWorktree.Size = new Size(100, 25);
            buttonCreateNewWorktree.TabIndex = 1;
            buttonCreateNewWorktree.Text = "&Create...";
            buttonCreateNewWorktree.TextAlign = ContentAlignment.MiddleRight;
            buttonCreateNewWorktree.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonCreateNewWorktree.UseVisualStyleBackColor = true;
            buttonCreateNewWorktree.Click += buttonCreateNewWorktree_Click;
            // 
            // FormManageWorktree
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(697, 361);
            Controls.Add(buttonPruneWorktrees);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(710, 200);
            Name = "FormManageWorktree";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Existing worktrees";
            Controls.SetChildIndex(ControlsPanel, 0);
            Controls.SetChildIndex(MainPanel, 0);
            Controls.SetChildIndex(buttonPruneWorktrees, 0);
            MainPanel.ResumeLayout(false);
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(Worktrees)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private DataGridView Worktrees;
        private Button buttonPruneWorktrees;
        private Button buttonDeleteSelectedWorktree;
        private Button buttonOpenSelectedWorktree;
        private DataGridViewTextBoxColumn Path;
        private DataGridViewTextBoxColumn Type;
        private DataGridViewTextBoxColumn Branch;
        private DataGridViewTextBoxColumn Sha1;
        private Button buttonCreateNewWorktree;
    }
}
