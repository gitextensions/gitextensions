namespace GitUI.CommandsDialogs.RepoHosting
{
    partial class ViewPullRequestsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this._chooseRepo = new System.Windows.Forms.Label();
            this._selectHostedRepoCB = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._pullRequestsList = new UserControls.NativeListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this._fetchBtn = new System.Windows.Forms.Button();
            this._addAndFetchBtn = new System.Windows.Forms.Button();
            this._closePullRequestBtn = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this._fileStatusList = new GitUI.FileStatusList();
            this._diffViewer = new GitUI.Editor.FileViewer();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._discussionWB = new GitUI.UserControls.WebBrowserControl();
            this._postCommentText = new GitUI.SpellChecker.EditNetSpell();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this._refreshCommentsBtn = new System.Windows.Forms.Button();
            this._postComment = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer2.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(754, 511);
            this.splitContainer2.SplitterDistance = 146;
            this.splitContainer2.TabIndex = 6;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(754, 146);
            this.tableLayoutPanel2.TabIndex = 7;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this._chooseRepo);
            this.flowLayoutPanel2.Controls.Add(this._selectHostedRepoCB);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(2, 2);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(750, 29);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // _chooseRepo
            // 
            this._chooseRepo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._chooseRepo.AutoSize = true;
            this._chooseRepo.Location = new System.Drawing.Point(3, 7);
            this._chooseRepo.Name = "_chooseRepo";
            this._chooseRepo.Size = new System.Drawing.Size(106, 15);
            this._chooseRepo.TabIndex = 4;
            this._chooseRepo.Text = "Choose repository:";
            // 
            // _selectHostedRepoCB
            // 
            this._selectHostedRepoCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._selectHostedRepoCB.FormattingEnabled = true;
            this._selectHostedRepoCB.Location = new System.Drawing.Point(115, 3);
            this._selectHostedRepoCB.Name = "_selectHostedRepoCB";
            this._selectHostedRepoCB.Size = new System.Drawing.Size(258, 23);
            this._selectHostedRepoCB.TabIndex = 0;
            this._selectHostedRepoCB.SelectedIndexChanged += new System.EventHandler(this._selectedOwner_SelectedIndexChanged);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this._pullRequestsList, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel3, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 35);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(750, 109);
            this.tableLayoutPanel3.TabIndex = 1;
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
            this._pullRequestsList.Location = new System.Drawing.Point(3, 3);
            this._pullRequestsList.MultiSelect = false;
            this._pullRequestsList.Name = "_pullRequestsList";
            this._pullRequestsList.Size = new System.Drawing.Size(580, 103);
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
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this._fetchBtn);
            this.flowLayoutPanel3.Controls.Add(this._addAndFetchBtn);
            this.flowLayoutPanel3.Controls.Add(this._closePullRequestBtn);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(588, 2);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(160, 105);
            this.flowLayoutPanel3.TabIndex = 0;
            this.flowLayoutPanel3.WrapContents = false;
            // 
            // _fetchBtn
            // 
            this._fetchBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fetchBtn.Location = new System.Drawing.Point(3, 3);
            this._fetchBtn.Name = "_fetchBtn";
            this._fetchBtn.Size = new System.Drawing.Size(155, 29);
            this._fetchBtn.TabIndex = 2;
            this._fetchBtn.Text = "Fetch to pr/ branch";
            this._fetchBtn.UseVisualStyleBackColor = true;
            this._fetchBtn.Click += new System.EventHandler(this._fetchBtn_Click);
            // 
            // _addAndFetchBtn
            // 
            this._addAndFetchBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addAndFetchBtn.Location = new System.Drawing.Point(3, 38);
            this._addAndFetchBtn.Name = "_addAndFetchBtn";
            this._addAndFetchBtn.Size = new System.Drawing.Size(155, 29);
            this._addAndFetchBtn.TabIndex = 2;
            this._addAndFetchBtn.Text = "Add remote and fetch";
            this._addAndFetchBtn.UseVisualStyleBackColor = true;
            this._addAndFetchBtn.Click += new System.EventHandler(this._addAsRemoteAndFetch_Click);
            // 
            // _closePullRequestBtn
            // 
            this._closePullRequestBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closePullRequestBtn.Location = new System.Drawing.Point(3, 73);
            this._closePullRequestBtn.Name = "_closePullRequestBtn";
            this._closePullRequestBtn.Size = new System.Drawing.Size(155, 29);
            this._closePullRequestBtn.TabIndex = 3;
            this._closePullRequestBtn.Text = "Close pull request";
            this._closePullRequestBtn.UseVisualStyleBackColor = true;
            this._closePullRequestBtn.Click += new System.EventHandler(this._closePullRequestBtn_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(754, 361);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer3);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(746, 333);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Diffs";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(2, 2);
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
            this.splitContainer3.Size = new System.Drawing.Size(742, 329);
            this.splitContainer3.SplitterDistance = 116;
            this.splitContainer3.TabIndex = 0;
            // 
            // _fileStatusList
            // 
            this._fileStatusList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileStatusList.FilterVisible = false;
            this._fileStatusList.Location = new System.Drawing.Point(0, 0);
            this._fileStatusList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._fileStatusList.Name = "_fileStatusList";
            this._fileStatusList.Size = new System.Drawing.Size(742, 116);
            this._fileStatusList.TabIndex = 0;
            // 
            // _diffViewer
            // 
            this._diffViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._diffViewer.Location = new System.Drawing.Point(0, 0);
            this._diffViewer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._diffViewer.Name = "_diffViewer";
            this._diffViewer.Size = new System.Drawing.Size(742, 209);
            this._diffViewer.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(746, 279);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Comments";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._discussionWB, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._postCommentText, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(742, 275);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // _discussionWB
            // 
            this._discussionWB.Dock = System.Windows.Forms.DockStyle.Fill;
            this._discussionWB.IsWebBrowserContextMenuEnabled = false;
            this._discussionWB.Location = new System.Drawing.Point(3, 3);
            this._discussionWB.MinimumSize = new System.Drawing.Size(20, 20);
            this._discussionWB.Name = "_discussionWB";
            this._discussionWB.ScriptErrorsSuppressed = true;
            this._discussionWB.Size = new System.Drawing.Size(736, 156);
            this._discussionWB.TabIndex = 9;
            this._discussionWB.WebBrowserShortcutsEnabled = false;
            // 
            // _postCommentText
            // 
            this._postCommentText.Dock = System.Windows.Forms.DockStyle.Fill;
            this._postCommentText.Location = new System.Drawing.Point(2, 164);
            this._postCommentText.Margin = new System.Windows.Forms.Padding(2);
            this._postCommentText.Name = "_postCommentText";
            this._postCommentText.Size = new System.Drawing.Size(738, 76);
            this._postCommentText.TabIndex = 10;
            this._postCommentText.TextBoxFont = new System.Drawing.Font("Segoe UI", 9F);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this._refreshCommentsBtn);
            this.flowLayoutPanel1.Controls.Add(this._postComment);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 244);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(738, 29);
            this.flowLayoutPanel1.TabIndex = 11;
            // 
            // _refreshCommentsBtn
            // 
            this._refreshCommentsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._refreshCommentsBtn.Location = new System.Drawing.Point(635, 3);
            this._refreshCommentsBtn.Name = "_refreshCommentsBtn";
            this._refreshCommentsBtn.Size = new System.Drawing.Size(100, 23);
            this._refreshCommentsBtn.TabIndex = 10;
            this._refreshCommentsBtn.Text = "Refresh";
            this._refreshCommentsBtn.UseVisualStyleBackColor = true;
            // 
            // _postComment
            // 
            this._postComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._postComment.Location = new System.Drawing.Point(498, 3);
            this._postComment.Name = "_postComment";
            this._postComment.Size = new System.Drawing.Size(131, 23);
            this._postComment.TabIndex = 11;
            this._postComment.Text = "Post comment";
            this._postComment.UseVisualStyleBackColor = true;
            // 
            // ViewPullRequestsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(754, 511);
            this.Controls.Add(this.splitContainer2);
            this.Name = "ViewPullRequestsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "View Pull Requests";
            this.Load += new System.EventHandler(this.ViewPullRequestsForm_Load);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private FileStatusList _fileStatusList;
        private Editor.FileViewer _diffViewer;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private GitUI.UserControls.WebBrowserControl _discussionWB;
        private SpellChecker.EditNetSpell _postCommentText;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button _refreshCommentsBtn;
        private System.Windows.Forms.Button _postComment;
        private System.Windows.Forms.Label _chooseRepo;
        private UserControls.NativeListView _pullRequestsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button _fetchBtn;
        private System.Windows.Forms.Button _addAndFetchBtn;
        private System.Windows.Forms.ComboBox _selectHostedRepoCB;
        private System.Windows.Forms.Button _closePullRequestBtn;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;

    }
}