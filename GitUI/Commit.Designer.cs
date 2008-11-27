namespace GitUI
{
    partial class FormCommit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCommit));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.Untracked = new System.Windows.Forms.DataGridView();
            this.Tracked = new System.Windows.Forms.DataGridView();
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.SelectedDiff = new System.Windows.Forms.RichTextBox();
            this.Commit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.OutPut = new System.Windows.Forms.RichTextBox();
            this.Message = new System.Windows.Forms.RichTextBox();
            this.Scan = new System.Windows.Forms.Button();
            this.AddFiles = new System.Windows.Forms.Button();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Untracked)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Tracked)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.Controls.Add(this.Ok);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(675, 543);
            this.splitContainer1.SplitterDistance = 266;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer2.Size = new System.Drawing.Size(266, 543);
            this.splitContainer2.SplitterDistance = 241;
            this.splitContainer2.TabIndex = 3;
            // 
            // Untracked
            // 
            this.Untracked.AllowUserToAddRows = false;
            this.Untracked.AllowUserToDeleteRows = false;
            this.Untracked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Untracked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Untracked.Location = new System.Drawing.Point(0, 0);
            this.Untracked.Name = "Untracked";
            this.Untracked.ReadOnly = true;
            this.Untracked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Untracked.Size = new System.Drawing.Size(266, 212);
            this.Untracked.TabIndex = 0;
            this.Untracked.SelectionChanged += new System.EventHandler(this.Untracked_SelectionChanged);
            // 
            // Tracked
            // 
            this.Tracked.AllowUserToAddRows = false;
            this.Tracked.AllowUserToDeleteRows = false;
            this.Tracked.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Tracked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tracked.Location = new System.Drawing.Point(0, 0);
            this.Tracked.Name = "Tracked";
            this.Tracked.ReadOnly = true;
            this.Tracked.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Tracked.Size = new System.Drawing.Size(266, 269);
            this.Tracked.TabIndex = 0;
            this.Tracked.SelectionChanged += new System.EventHandler(this.Tracked_SelectionChanged);
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(334, 10);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "Commit";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.SelectedDiff);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.AddFiles);
            this.splitContainer3.Panel2.Controls.Add(this.Scan);
            this.splitContainer3.Panel2.Controls.Add(this.Commit);
            this.splitContainer3.Panel2.Controls.Add(this.label1);
            this.splitContainer3.Panel2.Controls.Add(this.OutPut);
            this.splitContainer3.Panel2.Controls.Add(this.Message);
            this.splitContainer3.Size = new System.Drawing.Size(405, 543);
            this.splitContainer3.SplitterDistance = 365;
            this.splitContainer3.TabIndex = 0;
            // 
            // SelectedDiff
            // 
            this.SelectedDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedDiff.Location = new System.Drawing.Point(0, 0);
            this.SelectedDiff.Name = "SelectedDiff";
            this.SelectedDiff.Size = new System.Drawing.Size(405, 365);
            this.SelectedDiff.TabIndex = 0;
            this.SelectedDiff.Text = "";
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(12, 10);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(87, 23);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "Commit";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(178, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Commit message";
            // 
            // OutPut
            // 
            this.OutPut.Location = new System.Drawing.Point(178, 105);
            this.OutPut.Name = "OutPut";
            this.OutPut.ReadOnly = true;
            this.OutPut.Size = new System.Drawing.Size(227, 69);
            this.OutPut.TabIndex = 0;
            this.OutPut.Text = "";
            // 
            // Message
            // 
            this.Message.Location = new System.Drawing.Point(178, 29);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(227, 70);
            this.Message.TabIndex = 0;
            this.Message.Text = "";
            // 
            // Scan
            // 
            this.Scan.Location = new System.Drawing.Point(12, 39);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(87, 23);
            this.Scan.TabIndex = 3;
            this.Scan.Text = "Scan changes";
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(12, 68);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(87, 23);
            this.AddFiles.TabIndex = 4;
            this.AddFiles.Text = "Add files to git";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.Stage_Click);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.Untracked);
            this.splitContainer4.Size = new System.Drawing.Size(266, 241);
            this.splitContainer4.SplitterDistance = 25;
            this.splitContainer4.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Untracked by Git";
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.label3);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.Tracked);
            this.splitContainer5.Size = new System.Drawing.Size(266, 298);
            this.splitContainer5.SplitterDistance = 25;
            this.splitContainer5.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Tracked by Git";
            // 
            // FormCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 543);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCommit";
            this.Text = "Commit";
            this.Load += new System.EventHandler(this.FormCommit_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Untracked)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Tracked)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.RichTextBox OutPut;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.RichTextBox Message;
        private System.Windows.Forms.DataGridView Untracked;
        private System.Windows.Forms.DataGridView Tracked;
        private System.Windows.Forms.RichTextBox SelectedDiff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button Scan;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.Label label3;
    }
}