namespace GitUI
{
    partial class CommitSummaryUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelDate = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.labelCommit = new System.Windows.Forms.Label();
            this.labelTags = new System.Windows.Forms.Label();
            this.labelBranches = new System.Windows.Forms.Label();
            this.labelMessageCaption = new System.Windows.Forms.Label();
            this.labelTagsCaption = new System.Windows.Forms.Label();
            this.labelBranchesCaption = new System.Windows.Forms.Label();
            this.labelAuthorCaption = new System.Windows.Forms.Label();
            this.labelCommitCaption = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(213, 26);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(68, 13);
            this.labelDate.TabIndex = 13;
            this.labelDate.Text = "Commit date:";
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMessage.Location = new System.Drawing.Point(4, 115);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(403, 45);
            this.labelMessage.TabIndex = 12;
            this.labelMessage.Text = "...";
            // 
            // labelAuthor
            // 
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Location = new System.Drawing.Point(93, 26);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(16, 13);
            this.labelAuthor.TabIndex = 11;
            this.labelAuthor.Text = "...";
            // 
            // labelCommit
            // 
            this.labelCommit.AutoSize = true;
            this.labelCommit.Location = new System.Drawing.Point(93, 3);
            this.labelCommit.Name = "labelCommit";
            this.labelCommit.Size = new System.Drawing.Size(16, 13);
            this.labelCommit.TabIndex = 10;
            this.labelCommit.Text = "...";
            // 
            // labelTags
            // 
            this.labelTags.AutoSize = true;
            this.labelTags.BackColor = System.Drawing.Color.LightSteelBlue;
            this.labelTags.Location = new System.Drawing.Point(93, 49);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(16, 13);
            this.labelTags.TabIndex = 14;
            this.labelTags.Text = "...";
            // 
            // labelBranches
            // 
            this.labelBranches.AutoSize = true;
            this.labelBranches.Location = new System.Drawing.Point(93, 72);
            this.labelBranches.Name = "labelBranches";
            this.labelBranches.Size = new System.Drawing.Size(16, 13);
            this.labelBranches.TabIndex = 15;
            this.labelBranches.Text = "...";
            // 
            // labelMessageCaption
            // 
            this.labelMessageCaption.AutoSize = true;
            this.labelMessageCaption.Location = new System.Drawing.Point(4, 95);
            this.labelMessageCaption.Name = "labelMessageCaption";
            this.labelMessageCaption.Size = new System.Drawing.Size(53, 13);
            this.labelMessageCaption.TabIndex = 16;
            this.labelMessageCaption.Text = "Message:";
            // 
            // labelTagsCaption
            // 
            this.labelTagsCaption.AutoSize = true;
            this.labelTagsCaption.Location = new System.Drawing.Point(4, 49);
            this.labelTagsCaption.Name = "labelTagsCaption";
            this.labelTagsCaption.Size = new System.Drawing.Size(34, 13);
            this.labelTagsCaption.TabIndex = 17;
            this.labelTagsCaption.Text = "Tags:";
            // 
            // labelBranchesCaption
            // 
            this.labelBranchesCaption.AutoSize = true;
            this.labelBranchesCaption.Location = new System.Drawing.Point(4, 72);
            this.labelBranchesCaption.Name = "labelBranchesCaption";
            this.labelBranchesCaption.Size = new System.Drawing.Size(55, 13);
            this.labelBranchesCaption.TabIndex = 18;
            this.labelBranchesCaption.Text = "Branches:";
            // 
            // labelAuthorCaption
            // 
            this.labelAuthorCaption.AutoSize = true;
            this.labelAuthorCaption.Location = new System.Drawing.Point(5, 26);
            this.labelAuthorCaption.Name = "labelAuthorCaption";
            this.labelAuthorCaption.Size = new System.Drawing.Size(41, 13);
            this.labelAuthorCaption.TabIndex = 19;
            this.labelAuthorCaption.Text = "Author:";
            // 
            // labelCommitCaption
            // 
            this.labelCommitCaption.AutoSize = true;
            this.labelCommitCaption.Location = new System.Drawing.Point(5, 3);
            this.labelCommitCaption.Name = "labelCommitCaption";
            this.labelCommitCaption.Size = new System.Drawing.Size(70, 13);
            this.labelCommitCaption.TabIndex = 20;
            this.labelCommitCaption.Text = "Commit hash:";
            // 
            // CommitSummaryUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.labelCommitCaption);
            this.Controls.Add(this.labelAuthorCaption);
            this.Controls.Add(this.labelBranchesCaption);
            this.Controls.Add(this.labelTagsCaption);
            this.Controls.Add(this.labelMessageCaption);
            this.Controls.Add(this.labelBranches);
            this.Controls.Add(this.labelTags);
            this.Controls.Add(this.labelDate);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelAuthor);
            this.Controls.Add(this.labelCommit);
            this.Name = "CommitSummaryUserControl";
            this.Size = new System.Drawing.Size(410, 160);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Label labelCommit;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.Label labelBranches;
        private System.Windows.Forms.Label labelMessageCaption;
        private System.Windows.Forms.Label labelTagsCaption;
        private System.Windows.Forms.Label labelBranchesCaption;
        private System.Windows.Forms.Label labelAuthorCaption;
        private System.Windows.Forms.Label labelCommitCaption;
    }
}
