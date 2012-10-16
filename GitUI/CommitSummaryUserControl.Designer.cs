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
            this.Date = new System.Windows.Forms.Label();
            this.Message = new System.Windows.Forms.Label();
            this.Author = new System.Windows.Forms.Label();
            this.Commit = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Date
            // 
            this.Date.AutoSize = true;
            this.Date.Location = new System.Drawing.Point(4, 51);
            this.Date.Name = "Date";
            this.Date.Size = new System.Drawing.Size(68, 13);
            this.Date.TabIndex = 13;
            this.Date.Text = "Commit date:";
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(4, 75);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(53, 13);
            this.Message.TabIndex = 12;
            this.Message.Text = "Message:";
            // 
            // Author
            // 
            this.Author.AutoSize = true;
            this.Author.Location = new System.Drawing.Point(4, 26);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(41, 13);
            this.Author.TabIndex = 11;
            this.Author.Text = "Author:";
            // 
            // Commit
            // 
            this.Commit.AutoSize = true;
            this.Commit.Location = new System.Drawing.Point(4, 3);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(44, 13);
            this.Commit.TabIndex = 10;
            this.Commit.Text = "Commit:";
            // 
            // CommitSummaryUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.Date);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.Author);
            this.Controls.Add(this.Commit);
            this.Name = "CommitSummaryUserControl";
            this.Size = new System.Drawing.Size(377, 96);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Date;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.Label Author;
        private System.Windows.Forms.Label Commit;
    }
}
