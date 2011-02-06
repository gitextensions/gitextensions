namespace GitUI
{
    partial class FormCherryPickCommitSmall
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
            this.Date = new System.Windows.Forms.Label();
            this.Message = new System.Windows.Forms.Label();
            this.Author = new System.Windows.Forms.Label();
            this.Commit = new System.Windows.Forms.Label();
            this.BranchInfo = new System.Windows.Forms.Label();
            this.Pick = new System.Windows.Forms.Button();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.ParentsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Date
            // 
            this.Date.AutoSize = true;
            this.Date.Location = new System.Drawing.Point(30, 78);
            this.Date.Name = "Date";
            this.Date.Size = new System.Drawing.Size(71, 13);
            this.Date.TabIndex = 9;
            this.Date.Text = "Commit date:";
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(30, 102);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(53, 13);
            this.Message.TabIndex = 8;
            this.Message.Text = "Message:";
            // 
            // Author
            // 
            this.Author.AutoSize = true;
            this.Author.Location = new System.Drawing.Point(30, 53);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(44, 13);
            this.Author.TabIndex = 7;
            this.Author.Text = "Author:";
            // 
            // Commit
            // 
            this.Commit.AutoSize = true;
            this.Commit.Location = new System.Drawing.Point(30, 30);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(46, 13);
            this.Commit.TabIndex = 6;
            this.Commit.Text = "Commit:";
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(12, 9);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(121, 13);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Cherry pick this commit:";
            // 
            // Pick
            // 
            this.Pick.Location = new System.Drawing.Point(453, 216);
            this.Pick.Name = "Pick";
            this.Pick.Size = new System.Drawing.Size(109, 25);
            this.Pick.TabIndex = 10;
            this.Pick.Text = "Pick this commit";
            this.Pick.UseVisualStyleBackColor = true;
            this.Pick.Click += new System.EventHandler(this.Revert_Click);
            // 
            // AutoCommit
            // 
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(12, 216);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(174, 17);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically creates a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // ParentsList
            // 
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.ParentsList.Font = new System.Drawing.Font("Tahoma", 9F);
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(12, 145);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(550, 65);
            this.ParentsList.TabIndex = 12;
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
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(12, 129);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(188, 13);
            this.ParentsLabel.TabIndex = 13;
            this.ParentsLabel.Text = "This commit is a merge, select parent:";
            // 
            // FormCherryPickCommitSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(574, 247);
            this.Controls.Add(this.ParentsLabel);
            this.Controls.Add(this.ParentsList);
            this.Controls.Add(this.AutoCommit);
            this.Controls.Add(this.Pick);
            this.Controls.Add(this.Date);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.Author);
            this.Controls.Add(this.Commit);
            this.Controls.Add(this.BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCherryPickCommitSmall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cherry pick commit";
            this.Load += new System.EventHandler(this.FormCherryPickCommitSmall_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Date;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.Label Author;
        private System.Windows.Forms.Label Commit;
        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Pick;
        private System.Windows.Forms.CheckBox AutoCommit;
        private System.Windows.Forms.ListView ParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label ParentsLabel;
    }
}