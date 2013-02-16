namespace GitUI.CommandsDialogs
{
    partial class FormRevertCommit
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
            this.commitSummaryUserControl1 = new GitUI.CommitSummaryUserControl();
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.ParentsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.Revert = new System.Windows.Forms.Button();
            this.BranchInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(28, 32);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(440, 160);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(584, 160);
            this.commitSummaryUserControl1.TabIndex = 16;
            // 
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(12, 206);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(207, 15);
            this.ParentsLabel.TabIndex = 15;
            this.ParentsLabel.Text = "This commit is a merge, select parent:";
            // 
            // ParentsList
            // 
            this.ParentsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(12, 226);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(599, 86);
            this.ParentsList.TabIndex = 14;
            this.ParentsList.UseCompatibleStateImageBehavior = false;
            this.ParentsList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No.";
            this.columnHeader1.Width = 38;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Message";
            this.columnHeader2.Width = 291;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Author";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Date";
            this.columnHeader4.Width = 80;
            // 
            // AutoCommit
            // 
            this.AutoCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(12, 341);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(189, 19);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically create a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // Revert
            // 
            this.Revert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Revert.Location = new System.Drawing.Point(475, 335);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(137, 25);
            this.Revert.TabIndex = 10;
            this.Revert.Text = "Revert this commit";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(12, 9);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(110, 15);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Revert this commit:";
            // 
            // FormRevertCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(624, 372);
            this.Controls.Add(this.commitSummaryUserControl1);
            this.Controls.Add(this.ParentsLabel);
            this.Controls.Add(this.ParentsList);
            this.Controls.Add(this.AutoCommit);
            this.Controls.Add(this.Revert);
            this.Controls.Add(this.BranchInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 410);
            this.Name = "FormRevertCommit";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Revert commit";
            this.Load += new System.EventHandler(this.FormRevertCommitSmall_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Revert;
        private System.Windows.Forms.CheckBox AutoCommit;
        private System.Windows.Forms.Label ParentsLabel;
        private System.Windows.Forms.ListView ParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label BranchInfo;
        private CommitSummaryUserControl commitSummaryUserControl1;
    }
}