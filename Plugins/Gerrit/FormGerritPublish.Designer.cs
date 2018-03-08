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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGerritPublish));
            this.Publish = new System.Windows.Forms.Button();
            this.AddRemote = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this.labelRemote = new System.Windows.Forms.Label();
            this.PublishDraft = new System.Windows.Forms.CheckBox();
            this.labelBranch = new System.Windows.Forms.Label();
            this.labelTopic = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Branch = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_Topic = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_Reviewers = new System.Windows.Forms.TextBox();
            this.labelReviewers = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Publish
            // 
            this.Publish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Publish.Image = ((System.Drawing.Image)(resources.GetObject("Publish.Image")));
            this.Publish.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Publish.Location = new System.Drawing.Point(522, 142);
            this.Publish.Name = "Publish";
            this.Publish.Size = new System.Drawing.Size(101, 25);
            this.Publish.TabIndex = 10;
            this.Publish.Text = "&Publish";
            this.Publish.UseVisualStyleBackColor = true;
            this.Publish.Click += new System.EventHandler(this.PublishClick);
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Location = new System.Drawing.Point(522, 18);
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
            this.labelRemote.Location = new System.Drawing.Point(13, 23);
            this.labelRemote.Name = "labelRemote";
            this.labelRemote.Size = new System.Drawing.Size(48, 13);
            this.labelRemote.TabIndex = 0;
            this.labelRemote.Text = "Remote:";
            // 
            // PublishDraft
            // 
            this.PublishDraft.AutoSize = true;
            this.PublishDraft.Location = new System.Drawing.Point(101, 144);
            this.PublishDraft.Name = "PublishDraft";
            this.PublishDraft.Size = new System.Drawing.Size(134, 17);
            this.PublishDraft.TabIndex = 9;
            this.PublishDraft.Text = "Submit review as draft";
            this.PublishDraft.UseVisualStyleBackColor = true;
            // 
            // labelBranch
            // 
            this.labelBranch.AutoSize = true;
            this.labelBranch.Location = new System.Drawing.Point(13, 52);
            this.labelBranch.Name = "labelBranch";
            this.labelBranch.Size = new System.Drawing.Size(44, 13);
            this.labelBranch.TabIndex = 3;
            this.labelBranch.Text = "Branch:";
            // 
            // labelTopic
            // 
            this.labelTopic.AutoSize = true;
            this.labelTopic.Location = new System.Drawing.Point(13, 81);
            this.labelTopic.Name = "labelTopic";
            this.labelTopic.Size = new System.Drawing.Size(36, 13);
            this.labelTopic.TabIndex = 5;
            this.labelTopic.Text = "Topic:";
            // 
            // _NO_TRANSLATE_Branch
            // 
            this._NO_TRANSLATE_Branch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Branch.Location = new System.Drawing.Point(101, 49);
            this._NO_TRANSLATE_Branch.Name = "_NO_TRANSLATE_Branch";
            this._NO_TRANSLATE_Branch.Size = new System.Drawing.Size(522, 21);
            this._NO_TRANSLATE_Branch.TabIndex = 4;
            // 
            // _NO_TRANSLATE_Topic
            // 
            this._NO_TRANSLATE_Topic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Topic.Location = new System.Drawing.Point(101, 78);
            this._NO_TRANSLATE_Topic.Name = "_NO_TRANSLATE_Topic";
            this._NO_TRANSLATE_Topic.Size = new System.Drawing.Size(522, 21);
            this._NO_TRANSLATE_Topic.TabIndex = 6;
            // 
            // _NO_TRANSLATE_Reviewers
            // 
            this._NO_TRANSLATE_Reviewers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Reviewers.Location = new System.Drawing.Point(101, 108);
            this._NO_TRANSLATE_Reviewers.Name = "_NO_TRANSLATE_Reviewers";
            this._NO_TRANSLATE_Reviewers.Size = new System.Drawing.Size(522, 21);
            this._NO_TRANSLATE_Reviewers.TabIndex = 8;
            // 
            // labelReviewers
            // 
            this.labelReviewers.AutoSize = true;
            this.labelReviewers.Location = new System.Drawing.Point(13, 111);
            this.labelReviewers.Name = "labelReviewers";
            this.labelReviewers.Size = new System.Drawing.Size(69, 13);
            this.labelReviewers.TabIndex = 7;
            this.labelReviewers.Text = "Reviewer(s):";
            // 
            // FormGerritPublish
            // 
            this.AcceptButton = this.Publish;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(644, 173);
            this.Controls.Add(this._NO_TRANSLATE_Reviewers);
            this.Controls.Add(this.labelReviewers);
            this.Controls.Add(this._NO_TRANSLATE_Topic);
            this.Controls.Add(this._NO_TRANSLATE_Branch);
            this.Controls.Add(this.labelTopic);
            this.Controls.Add(this.labelBranch);
            this.Controls.Add(this.PublishDraft);
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
        private System.Windows.Forms.CheckBox PublishDraft;
        private System.Windows.Forms.Label labelBranch;
        private System.Windows.Forms.Label labelTopic;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Branch;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Topic;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Reviewers;
        private System.Windows.Forms.Label labelReviewers;
    }
}