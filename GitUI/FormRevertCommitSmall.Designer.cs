namespace GitUI
{
    partial class FormRevertCommitSmall
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
            this.Revert = new System.Windows.Forms.Button();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.ParentsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BranchInfo = new System.Windows.Forms.Label();
            this.Commit = new System.Windows.Forms.Label();
            this.Author = new System.Windows.Forms.Label();
            this.Message = new System.Windows.Forms.Label();
            this.Date = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Revert
            // 
            this.Revert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Revert.Location = new System.Drawing.Point(425, 224);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(137, 25);
            this.Revert.TabIndex = 10;
            this.Revert.Text = "Revert this commit";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // AutoCommit
            // 
            this.AutoCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(12, 227);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(214, 22);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically creates a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(12, 128);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(236, 18);
            this.ParentsLabel.TabIndex = 15;
            this.ParentsLabel.Text = "This commit is a merge, select parent:";
            // 
            // ParentsList
            // 
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(12, 148);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(550, 65);
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
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(12, 9);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(126, 18);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Revert this commit:";
            // 
            // Commit
            // 
            this.Commit.AutoSize = true;
            this.Commit.Location = new System.Drawing.Point(30, 30);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(85, 18);
            this.Commit.TabIndex = 6;
            this.Commit.Text = "Commit: {0}";
            // 
            // Author
            // 
            this.Author.AutoSize = true;
            this.Author.Location = new System.Drawing.Point(30, 53);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(77, 18);
            this.Author.TabIndex = 7;
            this.Author.Text = "Author: {0}";
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(30, 102);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(88, 18);
            this.Message.TabIndex = 8;
            this.Message.Text = "Message: {0}";
            // 
            // Date
            // 
            this.Date.AutoSize = true;
            this.Date.Location = new System.Drawing.Point(30, 78);
            this.Date.Name = "Date";
            this.Date.Size = new System.Drawing.Size(115, 18);
            this.Date.TabIndex = 9;
            this.Date.Text = "Commit date: {0}";
            // 
            // FormRevertCommitSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(574, 261);
            this.Controls.Add(this.ParentsLabel);
            this.Controls.Add(this.ParentsList);
            this.Controls.Add(this.AutoCommit);
            this.Controls.Add(this.Revert);
            this.Controls.Add(this.Date);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.Author);
            this.Controls.Add(this.Commit);
            this.Controls.Add(this.BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRevertCommitSmall";
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
        private System.Windows.Forms.Label Commit;
        private System.Windows.Forms.Label Author;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.Label Date;
    }
}