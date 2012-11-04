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
            this.SuspendLayout();
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(4, 51);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(68, 13);
            this.labelDate.TabIndex = 13;
            this.labelDate.Text = "Commit date:";
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(4, 75);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(53, 13);
            this.labelMessage.TabIndex = 12;
            this.labelMessage.Text = "Message:";
            // 
            // labelAuthor
            // 
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Location = new System.Drawing.Point(4, 26);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(41, 13);
            this.labelAuthor.TabIndex = 11;
            this.labelAuthor.Text = "Author:";
            // 
            // labelCommit
            // 
            this.labelCommit.AutoSize = true;
            this.labelCommit.Location = new System.Drawing.Point(4, 3);
            this.labelCommit.Name = "labelCommit";
            this.labelCommit.Size = new System.Drawing.Size(44, 13);
            this.labelCommit.TabIndex = 10;
            this.labelCommit.Text = "Commit:";
            // 
            // labelTags
            // 
            this.labelTags.AutoSize = true;
            this.labelTags.Location = new System.Drawing.Point(4, 102);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(34, 13);
            this.labelTags.TabIndex = 14;
            this.labelTags.Text = "Tags:";
            // 
            // CommitSummaryUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.Controls.Add(this.labelTags);
            this.Controls.Add(this.labelDate);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelAuthor);
            this.Controls.Add(this.labelCommit);
            this.Name = "CommitSummaryUserControl";
            this.Size = new System.Drawing.Size(377, 124);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Label labelCommit;
        private System.Windows.Forms.Label labelTags;
    }
}
