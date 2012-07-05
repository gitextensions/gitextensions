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
            this.Date = new System.Windows.Forms.Label();
            this.Message = new System.Windows.Forms.Label();
            this.Author = new System.Windows.Forms.Label();
            this.Commit = new System.Windows.Forms.Label();
            this.BranchInfo = new System.Windows.Forms.Label();
            this.Revert = new System.Windows.Forms.Button();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Date
            // 
            this.Date.AutoSize = true;
            this.Date.Location = new System.Drawing.Point(30, 78);
            this.Date.Name = "Date";
            this.Date.Size = new System.Drawing.Size(78, 12);
            this.Date.TabIndex = 9;
            this.Date.Text = "Commit date: {0}";
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(30, 102);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(59, 12);
            this.Message.TabIndex = 8;
            this.Message.Text = "Message: {0}";
            // 
            // Author
            // 
            this.Author.AutoSize = true;
            this.Author.Location = new System.Drawing.Point(30, 53);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(51, 12);
            this.Author.TabIndex = 7;
            this.Author.Text = "Author: {0}";
            // 
            // Commit
            // 
            this.Commit.AutoSize = true;
            this.Commit.Location = new System.Drawing.Point(30, 30);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(56, 12);
            this.Commit.TabIndex = 6;
            this.Commit.Text = "Commit: {0}";
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(12, 9);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(89, 12);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Revert this commit:";
            // 
            // Revert
            // 
            this.Revert.Location = new System.Drawing.Point(425, 135);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(137, 25);
            this.Revert.TabIndex = 10;
            this.Revert.Text = "Revert this commit";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // AutoCommit
            // 
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(12, 138);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(161, 16);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically creates a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // FormRevertCommitSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 170);
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

        private System.Windows.Forms.Label Date;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.Label Author;
        private System.Windows.Forms.Label Commit;
        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Revert;
        private System.Windows.Forms.CheckBox AutoCommit;
    }
}