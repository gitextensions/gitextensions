namespace GitUI
{
    partial class FormRemotes
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRemotes));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Remotes = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RemoteName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Url = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.New = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.RemoteBranches = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UpdateBranch = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.gitHeadBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.BranchName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RemoteCombo = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.mergeWithDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitHeadBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Remotes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.Delete);
            this.splitContainer1.Panel2.Controls.Add(this.New);
            this.splitContainer1.Panel2.Controls.Add(this.Save);
            this.splitContainer1.Size = new System.Drawing.Size(592, 208);
            this.splitContainer1.SplitterDistance = 164;
            this.splitContainer1.TabIndex = 0;
            // 
            // Remotes
            // 
            this.Remotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(0, 0);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(164, 199);
            this.Remotes.TabIndex = 0;
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.Remotes_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RemoteName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Url);
            this.groupBox1.Controls.Add(this.Browse);
            this.groupBox1.Location = new System.Drawing.Point(2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(412, 168);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            // 
            // RemoteName
            // 
            this.RemoteName.Location = new System.Drawing.Point(101, 19);
            this.RemoteName.Name = "RemoteName";
            this.RemoteName.Size = new System.Drawing.Size(221, 20);
            this.RemoteName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Url";
            // 
            // Url
            // 
            this.Url.FormattingEnabled = true;
            this.Url.Location = new System.Drawing.Point(101, 48);
            this.Url.Name = "Url";
            this.Url.Size = new System.Drawing.Size(221, 21);
            this.Url.TabIndex = 3;
            this.Url.DropDown += new System.EventHandler(this.Url_DropDown);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(328, 46);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 4;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(177, 177);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 7;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // New
            // 
            this.New.Location = new System.Drawing.Point(258, 177);
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(75, 23);
            this.New.TabIndex = 6;
            this.New.Text = "New";
            this.New.UseVisualStyleBackColor = true;
            this.New.Click += new System.EventHandler(this.New_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(339, 177);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 5;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(606, 240);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(598, 214);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Remote repositories";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(598, 214);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Default pull behaviour (fetch & merge)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // RemoteBranches
            // 
            this.RemoteBranches.AllowUserToAddRows = false;
            this.RemoteBranches.AllowUserToDeleteRows = false;
            this.RemoteBranches.AutoGenerateColumns = false;
            this.RemoteBranches.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.RemoteBranches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.RemoteBranches.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BranchName,
            this.RemoteCombo,
            this.mergeWithDataGridViewTextBoxColumn});
            this.RemoteBranches.DataSource = this.gitHeadBindingSource;
            this.RemoteBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemoteBranches.Location = new System.Drawing.Point(0, 0);
            this.RemoteBranches.Name = "RemoteBranches";
            this.RemoteBranches.Size = new System.Drawing.Size(592, 174);
            this.RemoteBranches.TabIndex = 0;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Branch";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // BName
            // 
            this.BName.DataPropertyName = "Name";
            this.BName.HeaderText = "Name";
            this.BName.Name = "BName";
            this.BName.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // UpdateBranch
            // 
            this.UpdateBranch.Location = new System.Drawing.Point(425, 3);
            this.UpdateBranch.Name = "UpdateBranch";
            this.UpdateBranch.Size = new System.Drawing.Size(167, 23);
            this.UpdateBranch.TabIndex = 10;
            this.UpdateBranch.Text = "Update all remote branch info";
            this.UpdateBranch.UseVisualStyleBackColor = true;
            this.UpdateBranch.Click += new System.EventHandler(this.UpdateBranch_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.RemoteBranches);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.UpdateBranch);
            this.splitContainer2.Size = new System.Drawing.Size(592, 208);
            this.splitContainer2.SplitterDistance = 174;
            this.splitContainer2.TabIndex = 11;
            // 
            // gitHeadBindingSource
            // 
            this.gitHeadBindingSource.DataSource = typeof(GitCommands.GitHead);
            // 
            // BranchName
            // 
            this.BranchName.DataPropertyName = "Name";
            this.BranchName.HeaderText = "Local branch name";
            this.BranchName.Name = "BranchName";
            this.BranchName.ReadOnly = true;
            // 
            // RemoteCombo
            // 
            this.RemoteCombo.DataPropertyName = "Remote";
            this.RemoteCombo.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.RemoteCombo.DisplayStyleForCurrentCellOnly = true;
            this.RemoteCombo.HeaderText = "Remote repository";
            this.RemoteCombo.Name = "RemoteCombo";
            this.RemoteCombo.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.RemoteCombo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // mergeWithDataGridViewTextBoxColumn
            // 
            this.mergeWithDataGridViewTextBoxColumn.DataPropertyName = "MergeWith";
            this.mergeWithDataGridViewTextBoxColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.mergeWithDataGridViewTextBoxColumn.DisplayStyleForCurrentCellOnly = true;
            this.mergeWithDataGridViewTextBoxColumn.HeaderText = "Default merge with";
            this.mergeWithDataGridViewTextBoxColumn.Name = "mergeWithDataGridViewTextBoxColumn";
            this.mergeWithDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.mergeWithDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // FormRemotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 240);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormRemotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repositories";
            this.Load += new System.EventHandler(this.FormRemotes_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RemoteBranches)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitHeadBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox Remotes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.ComboBox Url;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox RemoteName;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button New;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView RemoteBranches;
        private System.Windows.Forms.BindingSource gitHeadBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button UpdateBranch;
        private System.Windows.Forms.DataGridViewTextBoxColumn BranchName;
        private System.Windows.Forms.DataGridViewComboBoxColumn RemoteCombo;
        private System.Windows.Forms.DataGridViewComboBoxColumn mergeWithDataGridViewTextBoxColumn;
    }
}