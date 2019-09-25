namespace Gerrit
{
    partial class FormGerritPublish
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
            this.Publish = new System.Windows.Forms.Button();
            this.AddRemote = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this.labelRemote = new System.Windows.Forms.Label();
            this.labelBranch = new System.Windows.Forms.Label();
            this.labelTopic = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Branch = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_Topic = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_Reviewers = new System.Windows.Forms.TextBox();
            this.labelReviewers = new System.Windows.Forms.Label();
            this.PublishType = new System.Windows.Forms.ComboBox();
            this.labelPublishType = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Cc = new System.Windows.Forms.TextBox();
            this.labelCc = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Hashtag = new System.Windows.Forms.TextBox();
            this.labelHashtag = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Publish
            // 
            this.Publish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Publish.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Publish.Location = new System.Drawing.Point(531, 206);
            this.Publish.Name = "Publish";
            this.Publish.Size = new System.Drawing.Size(101, 25);
            this.Publish.TabIndex = 15;
            this.Publish.Text = "&Publish";
            this.Publish.UseVisualStyleBackColor = true;
            this.Publish.Click += new System.EventHandler(this.PublishClick);
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Location = new System.Drawing.Point(531, 17);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(101, 25);
            this.AddRemote.TabIndex = 2;
            this.AddRemote.Text = "Manage remotes";
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemoteClick);
            // 
            // _NO_TRANSLATE_Remotes
            // 
            this._NO_TRANSLATE_Remotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Remotes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Remotes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_Remotes.FormattingEnabled = true;
            this._NO_TRANSLATE_Remotes.Location = new System.Drawing.Point(101, 20);
            this._NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            this._NO_TRANSLATE_Remotes.Size = new System.Drawing.Size(415, 21);
            this._NO_TRANSLATE_Remotes.TabIndex = 1;
            // 
            // labelRemote
            // 
            this.labelRemote.AutoSize = true;
            this.labelRemote.Location = new System.Drawing.Point(12, 23);
            this.labelRemote.Name = "labelRemote";
            this.labelRemote.Size = new System.Drawing.Size(47, 13);
            this.labelRemote.TabIndex = 0;
            this.labelRemote.Text = "Remote:";
            // 
            // labelBranch
            // 
            this.labelBranch.AutoSize = true;
            this.labelBranch.Location = new System.Drawing.Point(12, 51);
            this.labelBranch.Name = "labelBranch";
            this.labelBranch.Size = new System.Drawing.Size(44, 13);
            this.labelBranch.TabIndex = 3;
            this.labelBranch.Text = "Branch:";
            // 
            // labelTopic
            // 
            this.labelTopic.AutoSize = true;
            this.labelTopic.Location = new System.Drawing.Point(12, 77);
            this.labelTopic.Name = "labelTopic";
            this.labelTopic.Size = new System.Drawing.Size(37, 13);
            this.labelTopic.TabIndex = 5;
            this.labelTopic.Text = "Topic:";
            // 
            // _NO_TRANSLATE_Branch
            // 
            this._NO_TRANSLATE_Branch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Branch.Location = new System.Drawing.Point(101, 48);
            this._NO_TRANSLATE_Branch.Name = "_NO_TRANSLATE_Branch";
            this._NO_TRANSLATE_Branch.Size = new System.Drawing.Size(531, 20);
            this._NO_TRANSLATE_Branch.TabIndex = 4;
            // 
            // _NO_TRANSLATE_Topic
            // 
            this._NO_TRANSLATE_Topic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Topic.Location = new System.Drawing.Point(101, 74);
            this._NO_TRANSLATE_Topic.Name = "_NO_TRANSLATE_Topic";
            this._NO_TRANSLATE_Topic.Size = new System.Drawing.Size(531, 20);
            this._NO_TRANSLATE_Topic.TabIndex = 6;
            // 
            // _NO_TRANSLATE_Reviewers
            // 
            this._NO_TRANSLATE_Reviewers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Reviewers.Location = new System.Drawing.Point(101, 100);
            this._NO_TRANSLATE_Reviewers.Name = "_NO_TRANSLATE_Reviewers";
            this._NO_TRANSLATE_Reviewers.Size = new System.Drawing.Size(531, 20);
            this._NO_TRANSLATE_Reviewers.TabIndex = 8;
            // 
            // labelReviewers
            // 
            this.labelReviewers.AutoSize = true;
            this.labelReviewers.Location = new System.Drawing.Point(12, 103);
            this.labelReviewers.Name = "labelReviewers";
            this.labelReviewers.Size = new System.Drawing.Size(66, 13);
            this.labelReviewers.TabIndex = 7;
            this.labelReviewers.Text = "Reviewer(s):";
            // 
            // PublishType
            // 
            this.PublishType.DisplayMember = "Key";
            this.PublishType.FormattingEnabled = true;
            this.PublishType.Location = new System.Drawing.Point(101, 152);
            this.PublishType.Margin = new System.Windows.Forms.Padding(4);
            this.PublishType.Name = "PublishType";
            this.PublishType.Size = new System.Drawing.Size(121, 21);
            this.PublishType.TabIndex = 12;
            this.PublishType.ValueMember = "Value";
            // 
            // labelPublishType
            // 
            this.labelPublishType.AutoSize = true;
            this.labelPublishType.Location = new System.Drawing.Point(12, 156);
            this.labelPublishType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPublishType.Name = "labelPublishType";
            this.labelPublishType.Size = new System.Drawing.Size(71, 13);
            this.labelPublishType.TabIndex = 11;
            this.labelPublishType.Text = "Publish Type:";
            // 
            // _NO_TRANSLATE_Cc
            // 
            this._NO_TRANSLATE_Cc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Cc.Location = new System.Drawing.Point(101, 126);
            this._NO_TRANSLATE_Cc.Name = "_NO_TRANSLATE_Cc";
            this._NO_TRANSLATE_Cc.Size = new System.Drawing.Size(531, 20);
            this._NO_TRANSLATE_Cc.TabIndex = 10;
            // 
            // labelCc
            // 
            this.labelCc.AutoSize = true;
            this.labelCc.Location = new System.Drawing.Point(12, 129);
            this.labelCc.Name = "labelCc";
            this.labelCc.Size = new System.Drawing.Size(24, 13);
            this.labelCc.TabIndex = 9;
            this.labelCc.Text = "CC:";
            // 
            // _NO_TRANSLATE_Hashtag
            // 
            this._NO_TRANSLATE_Hashtag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Hashtag.Location = new System.Drawing.Point(101, 180);
            this._NO_TRANSLATE_Hashtag.Name = "_NO_TRANSLATE_Hashtag";
            this._NO_TRANSLATE_Hashtag.Size = new System.Drawing.Size(121, 20);
            this._NO_TRANSLATE_Hashtag.TabIndex = 14;
            // 
            // labelHashtag
            // 
            this.labelHashtag.AutoSize = true;
            this.labelHashtag.Location = new System.Drawing.Point(12, 183);
            this.labelHashtag.Name = "labelHashtag";
            this.labelHashtag.Size = new System.Drawing.Size(50, 13);
            this.labelHashtag.TabIndex = 13;
            this.labelHashtag.Text = "Hashtag:";
            // 
            // FormGerritPublish
            // 
            this.AcceptButton = this.Publish;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(644, 243);
            this.Controls.Add(this._NO_TRANSLATE_Hashtag);
            this.Controls.Add(this.labelHashtag);
            this.Controls.Add(this._NO_TRANSLATE_Cc);
            this.Controls.Add(this.labelCc);
            this.Controls.Add(this.labelPublishType);
            this.Controls.Add(this.PublishType);
            this.Controls.Add(this._NO_TRANSLATE_Reviewers);
            this.Controls.Add(this.labelReviewers);
            this.Controls.Add(this._NO_TRANSLATE_Topic);
            this.Controls.Add(this._NO_TRANSLATE_Branch);
            this.Controls.Add(this.labelTopic);
            this.Controls.Add(this.labelBranch);
            this.Controls.Add(this.labelRemote);
            this.Controls.Add(this.AddRemote);
            this.Controls.Add(this._NO_TRANSLATE_Remotes);
            this.Controls.Add(this.Publish);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGerritPublish";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Publish Gerrit Change";
            this.Load += new System.EventHandler(this.FormGerritPublishLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Publish;
        private System.Windows.Forms.Button AddRemote;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Remotes;
        private System.Windows.Forms.Label labelRemote;
        private System.Windows.Forms.Label labelBranch;
        private System.Windows.Forms.Label labelTopic;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Branch;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Topic;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Reviewers;
        private System.Windows.Forms.Label labelReviewers;
        private System.Windows.Forms.ComboBox PublishType;
        private System.Windows.Forms.Label labelPublishType;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Cc;
        private System.Windows.Forms.Label labelCc;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Hashtag;
        private System.Windows.Forms.Label labelHashtag;
    }
}