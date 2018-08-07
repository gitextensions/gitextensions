namespace GitUI.UserControls
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
            this.labelTags = new System.Windows.Forms.Label();
            this.labelBranches = new System.Windows.Forms.Label();
            this.labelTagsCaption = new System.Windows.Forms.Label();
            this.labelBranchesCaption = new System.Windows.Forms.Label();
            this.labelAuthorCaption = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelDate
            // 
            this.labelDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(232, 89);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(80, 15);
            this.labelDate.TabIndex = 13;
            this.labelDate.Text = "Commit date:";
            // 
            // labelMessage
            // 
            this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMessage.AutoEllipsis = true;
            this.labelMessage.Location = new System.Drawing.Point(12, 22);
            this.labelMessage.MaximumSize = new System.Drawing.Size(1000, 50);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(422, 50);
            this.labelMessage.TabIndex = 12;
            this.labelMessage.Text = "...";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelMessage.SizeChanged += new System.EventHandler(this.labelMessage_SizeChanged);
            // 
            // labelAuthor
            // 
            this.labelAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Location = new System.Drawing.Point(100, 89);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(19, 13);
            this.labelAuthor.TabIndex = 11;
            this.labelAuthor.Text = "...";
            // 
            // labelTags
            // 
            this.labelTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTags.AutoSize = true;
            this.labelTags.BackColor = System.Drawing.Color.LightSteelBlue;
            this.labelTags.Location = new System.Drawing.Point(100, 112);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(19, 13);
            this.labelTags.TabIndex = 14;
            this.labelTags.Text = "...";
            // 
            // labelBranches
            // 
            this.labelBranches.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelBranches.AutoSize = true;
            this.labelBranches.Location = new System.Drawing.Point(100, 135);
            this.labelBranches.Name = "labelBranches";
            this.labelBranches.Size = new System.Drawing.Size(19, 13);
            this.labelBranches.TabIndex = 15;
            this.labelBranches.Text = "...";
            // 
            // labelTagsCaption
            // 
            this.labelTagsCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTagsCaption.AutoSize = true;
            this.labelTagsCaption.Location = new System.Drawing.Point(11, 112);
            this.labelTagsCaption.Name = "labelTagsCaption";
            this.labelTagsCaption.Size = new System.Drawing.Size(43, 15);
            this.labelTagsCaption.TabIndex = 17;
            this.labelTagsCaption.Text = "Tag(s):";
            // 
            // labelBranchesCaption
            // 
            this.labelBranchesCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelBranchesCaption.AutoSize = true;
            this.labelBranchesCaption.Location = new System.Drawing.Point(11, 135);
            this.labelBranchesCaption.Name = "labelBranchesCaption";
            this.labelBranchesCaption.Size = new System.Drawing.Size(66, 15);
            this.labelBranchesCaption.TabIndex = 18;
            this.labelBranchesCaption.Text = "Branch(es):";
            // 
            // labelAuthorCaption
            // 
            this.labelAuthorCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAuthorCaption.AutoSize = true;
            this.labelAuthorCaption.Location = new System.Drawing.Point(12, 89);
            this.labelAuthorCaption.Name = "labelAuthorCaption";
            this.labelAuthorCaption.Size = new System.Drawing.Size(47, 15);
            this.labelAuthorCaption.TabIndex = 19;
            this.labelAuthorCaption.Text = "Author:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelAuthorCaption);
            this.groupBox1.Controls.Add(this.labelBranchesCaption);
            this.groupBox1.Controls.Add(this.labelTagsCaption);
            this.groupBox1.Controls.Add(this.labelBranches);
            this.groupBox1.Controls.Add(this.labelTags);
            this.groupBox1.Controls.Add(this.labelDate);
            this.groupBox1.Controls.Add(this.labelMessage);
            this.groupBox1.Controls.Add(this.labelAuthor);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 160);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "...";
            this.groupBox1.Resize += new System.EventHandler(this.groupBox1_Resize);
            // 
            // CommitSummaryUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(440, 160);
            this.Name = "CommitSummaryUserControl";
            this.Size = new System.Drawing.Size(440, 160);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.Label labelBranches;
        private System.Windows.Forms.Label labelTagsCaption;
        private System.Windows.Forms.Label labelBranchesCaption;
        private System.Windows.Forms.Label labelAuthorCaption;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
