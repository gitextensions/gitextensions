namespace Gerrit
{
    partial class FormGerritDownload
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
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.Download = new System.Windows.Forms.Button();
            this.labelTopicBranch = new System.Windows.Forms.Label();
            this.AddRemote = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Remotes = new System.Windows.Forms.ComboBox();
            this._NO_TRANSLATE_TopicBranch = new System.Windows.Forms.TextBox();
            this.labelChange = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Change = new System.Windows.Forms.TextBox();
            this.labelRemote = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Download
            // 
            this.Download.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Download.Image = global::Gerrit.Properties.Resources.GerritDownload;
            this.Download.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Download.Location = new System.Drawing.Point(412, 120);
            this.Download.Name = "Download";
            this.Download.Size = new System.Drawing.Size(101, 25);
            this.Download.TabIndex = 7;
            this.Download.Text = "&Download";
            this.Download.UseVisualStyleBackColor = true;
            this.Download.Click += new System.EventHandler(this.DownloadClick);
            // 
            // labelTopicBranch
            // 
            this.labelTopicBranch.AutoSize = true;
            this.labelTopicBranch.Location = new System.Drawing.Point(16, 86);
            this.labelTopicBranch.Name = "labelTopicBranch";
            this.labelTopicBranch.Size = new System.Drawing.Size(80, 15);
            this.labelTopicBranch.TabIndex = 5;
            this.labelTopicBranch.Text = "Topic branch:";
            // 
            // AddRemote
            // 
            this.AddRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemote.Location = new System.Drawing.Point(408, 23);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(105, 25);
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
            this._NO_TRANSLATE_Remotes.Location = new System.Drawing.Point(116, 25);
            this._NO_TRANSLATE_Remotes.Name = "_NO_TRANSLATE_Remotes";
            this._NO_TRANSLATE_Remotes.Size = new System.Drawing.Size(286, 23);
            this._NO_TRANSLATE_Remotes.TabIndex = 1;
            // 
            // _NO_TRANSLATE_TopicBranch
            // 
            this._NO_TRANSLATE_TopicBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_TopicBranch.Location = new System.Drawing.Point(116, 83);
            this._NO_TRANSLATE_TopicBranch.Name = "_NO_TRANSLATE_TopicBranch";
            this._NO_TRANSLATE_TopicBranch.Size = new System.Drawing.Size(397, 23);
            this._NO_TRANSLATE_TopicBranch.TabIndex = 6;
            // 
            // labelChange
            // 
            this.labelChange.AutoSize = true;
            this.labelChange.Location = new System.Drawing.Point(16, 57);
            this.labelChange.Name = "labelChange";
            this.labelChange.Size = new System.Drawing.Size(51, 15);
            this.labelChange.TabIndex = 3;
            this.labelChange.Text = "Change:";
            // 
            // _NO_TRANSLATE_Change
            // 
            this._NO_TRANSLATE_Change.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Change.Location = new System.Drawing.Point(116, 54);
            this._NO_TRANSLATE_Change.Name = "_NO_TRANSLATE_Change";
            this._NO_TRANSLATE_Change.Size = new System.Drawing.Size(397, 23);
            this._NO_TRANSLATE_Change.TabIndex = 4;
            // 
            // labelRemote
            // 
            this.labelRemote.AutoSize = true;
            this.labelRemote.Location = new System.Drawing.Point(16, 28);
            this.labelRemote.Name = "labelRemote";
            this.labelRemote.Size = new System.Drawing.Size(51, 15);
            this.labelRemote.TabIndex = 0;
            this.labelRemote.Text = "Remote:";
            // 
            // FormGerritDownload
            // 
            this.AcceptButton = this.Download;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 157);
            this.Controls.Add(this.labelRemote);
            this.Controls.Add(this._NO_TRANSLATE_Change);
            this.Controls.Add(this.AddRemote);
            this.Controls.Add(this._NO_TRANSLATE_TopicBranch);
            this.Controls.Add(this._NO_TRANSLATE_Remotes);
            this.Controls.Add(this.labelChange);
            this.Controls.Add(this.Download);
            this.Controls.Add(this.labelTopicBranch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGerritDownload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Download Gerrit Change";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGerritDownload_FormClosing);
            this.Load += new System.EventHandler(this.FormGerritDownloadLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Download;
        private System.Windows.Forms.Button AddRemote;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Remotes;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label labelTopicBranch;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_TopicBranch;
        private System.Windows.Forms.Label labelChange;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Change;
        private System.Windows.Forms.Label labelRemote;
    }
}