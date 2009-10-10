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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRevertCommitSmall));
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
            this.Date.Size = new System.Drawing.Size(68, 13);
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
            this.Author.Size = new System.Drawing.Size(41, 13);
            this.Author.TabIndex = 7;
            this.Author.Text = "Author:";
            // 
            // Commit
            // 
            this.Commit.AutoSize = true;
            this.Commit.Location = new System.Drawing.Point(30, 30);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(44, 13);
            this.Commit.TabIndex = 6;
            this.Commit.Text = "Commit:";
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(12, 9);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(97, 13);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Revert this commit:";
            // 
            // Revert
            // 
            this.Revert.Location = new System.Drawing.Point(453, 135);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(109, 23);
            this.Revert.TabIndex = 10;
            this.Revert.Text = "Revert this commit";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // AutoCommit
            // 
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(276, 139);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(171, 17);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically creates a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // FormRevertCommitSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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