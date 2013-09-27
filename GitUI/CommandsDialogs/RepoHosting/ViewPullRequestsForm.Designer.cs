namespace GitUI.CommandsDialogs.RepoHosting
{
    partial class ViewPullRequestsForm
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
            loader.Cancel();

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
            this._selectHostedRepoCB = new System.Windows.Forms.ComboBox();
            this._pullRequestsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._chooseRepo = new System.Windows.Forms.Label();
            this._fetchBtn = new System.Windows.Forms.Button();
            this._postCommentText = new GitUI.SpellChecker.EditNetSpell();
            this._postComment = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._closePullRequestBtn = new System.Windows.Forms.Button();
            this._addAndFetchBtn = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this._fileStatusList = new GitUI.FileStatusList();
            this._diffViewer = new GitUI.Editor.FileViewer();
            this._refreshCommentsBtn = new System.Windows.Forms.Button();
            this._discussionWB = new System.Windows.Forms.WebBrowser();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
#endif
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
#endif
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
#endif
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _selectHostedRepoCB
            // 
            this._selectHostedRepoCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._selectHostedRepoCB.DisplayMember = "DisplayData";
            this._selectHostedRepoCB.FormattingEnabled = true;
            this._selectHostedRepoCB.Location = new System.Drawing.Point(130, 6);
            this._selectHostedRepoCB.Name = "_selectHostedRepoCB";
            this._selectHostedRepoCB.Size = new System.Drawing.Size(258, 23);
            this._selectHostedRepoCB.TabIndex = 0;
            this._selectHostedRepoCB.SelectedIndexChanged += new System.EventHandler(this._selectedOwner_SelectedIndexChanged);
            // 
            // _pullRequestsList
            // 
            this._pullRequestsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._pullRequestsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this._pullRequestsList.FullRowSelect = true;
            this._pullRequestsList.HideSelection = false;
            this._pullRequestsList.Location = new System.Drawing.Point(3, 33);
            this._pullRequestsList.MultiSelect = false;
            this._pullRequestsList.Name = "_pullRequestsList";
            this._pullRequestsList.Size = new System.Drawing.Size(643, 160);
            this._pullRequestsList.TabIndex = 1;
            this._pullRequestsList.UseCompatibleStateImageBehavior = false;
            this._pullRequestsList.View = System.Windows.Forms.View.Details;
            this._pullRequestsList.SelectedIndexChanged += new System.EventHandler(this._pullRequestsList_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "#";
            this.columnHeader1.Width = 41;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Heading";
            this.columnHeader2.Width = 286;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "By";
            this.columnHeader3.Width = 121;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Created";
            this.columnHeader4.Width = 133;
            // 
            // _chooseRepo
            // 
            this._chooseRepo.AutoSize = true;
            this._chooseRepo.Location = new System.Drawing.Point(12, 9);
            this._chooseRepo.Name = "_chooseRepo";
            this._chooseRepo.Size = new System.Drawing.Size(106, 15);
            this._chooseRepo.TabIndex = 4;
            this._chooseRepo.Text = "Choose repository:";
            // 
            // _fetchBtn
            // 
            this._fetchBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fetchBtn.Location = new System.Drawing.Point(652, 33);
            this._fetchBtn.Name = "_fetchBtn";
            this._fetchBtn.Size = new System.Drawing.Size(146, 29);
            this._fetchBtn.TabIndex = 2;
            this._fetchBtn.Text = "Fetch to pr/ branch";
            this._fetchBtn.UseVisualStyleBackColor = true;
            this._fetchBtn.Click += new System.EventHandler(this._fetchBtn_Click);
            // 
            // _postCommentText
            // 
            this._postCommentText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._postCommentText.Location = new System.Drawing.Point(3, 582);
            this._postCommentText.Margin = new System.Windows.Forms.Padding(2);
            this._postCommentText.Name = "_postCommentText";
            this._postCommentText.Size = new System.Drawing.Size(280, 66);
            this._postCommentText.TabIndex = 0;
            this._postCommentText.KeyUp += new System.Windows.Forms.KeyEventHandler(this._postCommentText_KeyUp);
            // 
            // _postComment
            // 
            this._postComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._postComment.Location = new System.Drawing.Point(150, 654);
            this._postComment.Name = "_postComment";
            this._postComment.Size = new System.Drawing.Size(131, 23);
            this._postComment.TabIndex = 1;
            this._postComment.Text = "Post comment";
            this._postComment.UseVisualStyleBackColor = true;
            this._postComment.Click += new System.EventHandler(this._postComment_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._refreshCommentsBtn);
            this.splitContainer1.Panel2.Controls.Add(this._discussionWB);
            this.splitContainer1.Panel2.Controls.Add(this._postComment);
            this.splitContainer1.Panel2.Controls.Add(this._postCommentText);
            this.splitContainer1.Size = new System.Drawing.Size(1089, 680);
            this.splitContainer1.SplitterDistance = 801;
            this.splitContainer1.TabIndex = 8;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._closePullRequestBtn);
            this.splitContainer2.Panel1.Controls.Add(this._selectHostedRepoCB);
            this.splitContainer2.Panel1.Controls.Add(this._addAndFetchBtn);
            this.splitContainer2.Panel1.Controls.Add(this._fetchBtn);
            this.splitContainer2.Panel1.Controls.Add(this._pullRequestsList);
            this.splitContainer2.Panel1.Controls.Add(this._chooseRepo);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(801, 680);
            this.splitContainer2.SplitterDistance = 196;
            this.splitContainer2.TabIndex = 6;
            // 
            // _closePullRequestBtn
            // 
            this._closePullRequestBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closePullRequestBtn.Location = new System.Drawing.Point(652, 164);
            this._closePullRequestBtn.Name = "_closePullRequestBtn";
            this._closePullRequestBtn.Size = new System.Drawing.Size(146, 29);
            this._closePullRequestBtn.TabIndex = 3;
            this._closePullRequestBtn.Text = "Close pull request";
            this._closePullRequestBtn.UseVisualStyleBackColor = true;
            this._closePullRequestBtn.Click += new System.EventHandler(this._closePullRequestBtn_Click);
            // 
            // _addAndFetchBtn
            // 
            this._addAndFetchBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addAndFetchBtn.Location = new System.Drawing.Point(652, 68);
            this._addAndFetchBtn.Name = "_addAndFetchBtn";
            this._addAndFetchBtn.Size = new System.Drawing.Size(146, 29);
            this._addAndFetchBtn.TabIndex = 2;
            this._addAndFetchBtn.Text = "Add remote and fetch";
            this._addAndFetchBtn.UseVisualStyleBackColor = true;
            this._addAndFetchBtn.Click += new System.EventHandler(this._addAsRemoteAndFetch_Click);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this._fileStatusList);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this._diffViewer);
            this.splitContainer3.Size = new System.Drawing.Size(801, 480);
            this.splitContainer3.SplitterDistance = 171;
            this.splitContainer3.TabIndex = 0;
            // 
            // _fileStatusList
            // 
            this._fileStatusList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileStatusList.Location = new System.Drawing.Point(0, 0);
            this._fileStatusList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._fileStatusList.Name = "_fileStatusList";
            this._fileStatusList.Size = new System.Drawing.Size(801, 171);
            this._fileStatusList.TabIndex = 0;
            // 
            // _diffViewer
            // 
            this._diffViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._diffViewer.Location = new System.Drawing.Point(0, 0);
            this._diffViewer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._diffViewer.Name = "_diffViewer";
            this._diffViewer.Size = new System.Drawing.Size(801, 305);
            this._diffViewer.TabIndex = 0;
            // 
            // _refreshCommentsBtn
            // 
            this._refreshCommentsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._refreshCommentsBtn.Location = new System.Drawing.Point(3, 654);
            this._refreshCommentsBtn.Name = "_refreshCommentsBtn";
            this._refreshCommentsBtn.Size = new System.Drawing.Size(100, 23);
            this._refreshCommentsBtn.TabIndex = 9;
            this._refreshCommentsBtn.Text = "Refresh";
            this._refreshCommentsBtn.UseVisualStyleBackColor = true;
            this._refreshCommentsBtn.Click += new System.EventHandler(this._refreshCommentsBtn_Click);
            // 
            // _discussionWB
            // 
            this._discussionWB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._discussionWB.IsWebBrowserContextMenuEnabled = false;
            this._discussionWB.Location = new System.Drawing.Point(3, 0);
            this._discussionWB.MinimumSize = new System.Drawing.Size(20, 20);
            this._discussionWB.Name = "_discussionWB";
            this._discussionWB.Size = new System.Drawing.Size(278, 576);
            this._discussionWB.TabIndex = 8;
            this._discussionWB.WebBrowserShortcutsEnabled = false;
            // 
            // ViewPullRequestsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1089, 680);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ViewPullRequestsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "View Pull Requests";
            this.Load += new System.EventHandler(this.ViewPullRequestsForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
#endif
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
#endif
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
#endif
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox _selectHostedRepoCB;
        private System.Windows.Forms.ListView _pullRequestsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private Editor.FileViewer _diffViewer;
        private System.Windows.Forms.Label _chooseRepo;
        private System.Windows.Forms.Button _fetchBtn;
        private FileStatusList _fileStatusList;
        private GitUI.SpellChecker.EditNetSpell _postCommentText;
        private System.Windows.Forms.Button _postComment;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Button _closePullRequestBtn;
        private System.Windows.Forms.WebBrowser _discussionWB;
        private System.Windows.Forms.Button _refreshCommentsBtn;
        private System.Windows.Forms.Button _addAndFetchBtn;
    }
}