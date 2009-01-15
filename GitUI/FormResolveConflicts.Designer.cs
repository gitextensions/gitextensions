namespace GitUI
{
    partial class FormResolveConflicts
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormResolveConflicts));
            this.ConflictedFiles = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Rescan = new System.Windows.Forms.Button();
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.Namex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // ConflictedFiles
            // 
            this.ConflictedFiles.AllowUserToAddRows = false;
            this.ConflictedFiles.AllowUserToDeleteRows = false;
            this.ConflictedFiles.AutoGenerateColumns = false;
            this.ConflictedFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ConflictedFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConflictedFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Namex});
            this.ConflictedFiles.DataSource = this.gitItemBindingSource;
            this.ConflictedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConflictedFiles.Location = new System.Drawing.Point(0, 0);
            this.ConflictedFiles.Name = "ConflictedFiles";
            this.ConflictedFiles.ReadOnly = true;
            this.ConflictedFiles.RowHeadersVisible = false;
            this.ConflictedFiles.Size = new System.Drawing.Size(441, 226);
            this.ConflictedFiles.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Rescan);
            this.splitContainer1.Panel2.Controls.Add(this.Mergetool);
            this.splitContainer1.Size = new System.Drawing.Size(576, 255);
            this.splitContainer1.SplitterDistance = 441;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.ConflictedFiles);
            this.splitContainer2.Size = new System.Drawing.Size(441, 255);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Unresolved merge conflicts";
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(3, 4);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(125, 23);
            this.Mergetool.TabIndex = 0;
            this.Mergetool.Text = "Run mergetool";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Rescan
            // 
            this.Rescan.Location = new System.Drawing.Point(3, 29);
            this.Rescan.Name = "Rescan";
            this.Rescan.Size = new System.Drawing.Size(125, 23);
            this.Rescan.TabIndex = 1;
            this.Rescan.Text = "Rescan mergeconflicts";
            this.Rescan.UseVisualStyleBackColor = true;
            this.Rescan.Click += new System.EventHandler(this.Rescan_Click);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // Namex
            // 
            this.Namex.DataPropertyName = "FileName";
            this.Namex.HeaderText = "Filename";
            this.Namex.Name = "Namex";
            this.Namex.ReadOnly = true;
            // 
            // FormResolveConflicts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 255);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormResolveConflicts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resolve merge conflicts";
            this.Load += new System.EventHandler(this.FormResolveConflicts_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ConflictedFiles)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView ConflictedFiles;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.Button Rescan;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn Namex;
    }
}