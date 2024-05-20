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
            splitContainer2 = new SplitContainer();
            tableLayoutPanel2 = new TableLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            _chooseRepo = new Label();
            _selectHostedRepoCB = new ComboBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            _pullRequestsList = new GitUI.UserControls.NativeListView();
            columnHeaderId = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderHeading = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderBy = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderCreated = ((ColumnHeader)(new ColumnHeader()));
            flowLayoutPanel3 = new FlowLayoutPanel();
            _fetchBtn = new Button();
            _addAndFetchBtn = new Button();
            _closePullRequestBtn = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer3 = new SplitContainer();
            _fileStatusList = new GitUI.FileStatusList();
            _diffViewer = new GitUI.Editor.FileViewer();
            tabPage2 = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            _discussionWB = new GitUI.UserControls.WebBrowserControl();
            _postCommentText = new GitUI.SpellChecker.EditNetSpell();
            flowLayoutPanel1 = new FlowLayoutPanel();
            _refreshCommentsBtn = new Button();
            _postComment = new Button();
            columnHeaderBranch = ((ColumnHeader)(new ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer3)).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            tabPage2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(tableLayoutPanel2);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tabControl1);
            splitContainer2.Size = new Size(754, 511);
            splitContainer2.SplitterDistance = 146;
            splitContainer2.TabIndex = 6;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(flowLayoutPanel2, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(2);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(754, 146);
            tableLayoutPanel2.TabIndex = 7;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Controls.Add(_chooseRepo);
            flowLayoutPanel2.Controls.Add(_selectHostedRepoCB);
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.Location = new Point(2, 2);
            flowLayoutPanel2.Margin = new Padding(2);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(750, 27);
            flowLayoutPanel2.TabIndex = 0;
            // 
            // _chooseRepo
            // 
            _chooseRepo.Anchor = AnchorStyles.Left;
            _chooseRepo.AutoSize = true;
            _chooseRepo.Location = new Point(3, 7);
            _chooseRepo.Name = "_chooseRepo";
            _chooseRepo.Size = new Size(94, 13);
            _chooseRepo.TabIndex = 4;
            _chooseRepo.Text = "Choose repository:";
            // 
            // _selectHostedRepoCB
            // 
            _selectHostedRepoCB.DropDownStyle = ComboBoxStyle.DropDownList;
            _selectHostedRepoCB.FormattingEnabled = true;
            _selectHostedRepoCB.Location = new Point(103, 3);
            _selectHostedRepoCB.Name = "_selectHostedRepoCB";
            _selectHostedRepoCB.Size = new Size(258, 21);
            _selectHostedRepoCB.TabIndex = 0;
            _selectHostedRepoCB.SelectedIndexChanged += _selectedOwner_SelectedIndexChanged;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(_pullRequestsList, 0, 0);
            tableLayoutPanel3.Controls.Add(flowLayoutPanel3, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(2, 33);
            tableLayoutPanel3.Margin = new Padding(2);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(750, 111);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // _pullRequestsList
            // 
            _pullRequestsList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _pullRequestsList.Columns.AddRange(new ColumnHeader[] {
            columnHeaderId,
            columnHeaderHeading,
            columnHeaderBy,
            columnHeaderCreated,
            columnHeaderBranch});
            _pullRequestsList.FullRowSelect = true;
            _pullRequestsList.HideSelection = false;
            _pullRequestsList.Location = new Point(3, 3);
            _pullRequestsList.MultiSelect = false;
            _pullRequestsList.Name = "_pullRequestsList";
            _pullRequestsList.Size = new Size(580, 105);
            _pullRequestsList.TabIndex = 1;
            _pullRequestsList.UseCompatibleStateImageBehavior = false;
            _pullRequestsList.View = View.Details;
            _pullRequestsList.SelectedIndexChanged += _pullRequestsList_SelectedIndexChanged;
            // 
            // columnHeaderId
            // 
            columnHeaderId.Text = "#";
            columnHeaderId.Width = -2;
            // 
            // columnHeaderHeading
            // 
            columnHeaderHeading.Text = "Heading";
            columnHeaderHeading.Width = -2;
            // 
            // columnHeaderBy
            // 
            columnHeaderBy.Text = "By";
            columnHeaderBy.Width = -2;
            // 
            // columnHeaderCreated
            // 
            columnHeaderCreated.Text = "Created";
            columnHeaderCreated.Width = -2;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.Controls.Add(_fetchBtn);
            flowLayoutPanel3.Controls.Add(_addAndFetchBtn);
            flowLayoutPanel3.Controls.Add(_closePullRequestBtn);
            flowLayoutPanel3.Dock = DockStyle.Fill;
            flowLayoutPanel3.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel3.Location = new Point(588, 2);
            flowLayoutPanel3.Margin = new Padding(2);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(160, 107);
            flowLayoutPanel3.TabIndex = 0;
            flowLayoutPanel3.WrapContents = false;
            // 
            // _fetchBtn
            // 
            _fetchBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _fetchBtn.Location = new Point(3, 3);
            _fetchBtn.Name = "_fetchBtn";
            _fetchBtn.Size = new Size(155, 29);
            _fetchBtn.TabIndex = 2;
            _fetchBtn.Text = "Fetch to pr/ branch";
            _fetchBtn.UseVisualStyleBackColor = true;
            _fetchBtn.Click += _fetchBtn_Click;
            // 
            // _addAndFetchBtn
            // 
            _addAndFetchBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _addAndFetchBtn.Location = new Point(3, 38);
            _addAndFetchBtn.Name = "_addAndFetchBtn";
            _addAndFetchBtn.Size = new Size(155, 29);
            _addAndFetchBtn.TabIndex = 2;
            _addAndFetchBtn.Text = "Add remote and fetch";
            _addAndFetchBtn.UseVisualStyleBackColor = true;
            _addAndFetchBtn.Click += _addAsRemoteAndFetch_Click;
            // 
            // _closePullRequestBtn
            // 
            _closePullRequestBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _closePullRequestBtn.Location = new Point(3, 73);
            _closePullRequestBtn.Name = "_closePullRequestBtn";
            _closePullRequestBtn.Size = new Size(155, 29);
            _closePullRequestBtn.TabIndex = 3;
            _closePullRequestBtn.Text = "Close pull request";
            _closePullRequestBtn.UseVisualStyleBackColor = true;
            _closePullRequestBtn.Click += _closePullRequestBtn_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(754, 361);
            tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer3);
            tabPage1.Location = new Point(4, 22);
            tabPage1.Margin = new Padding(2);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(2);
            tabPage1.Size = new Size(746, 335);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Diffs";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(2, 2);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(_fileStatusList);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(_diffViewer);
            splitContainer3.Size = new Size(742, 331);
            splitContainer3.SplitterDistance = 116;
            splitContainer3.TabIndex = 0;
            // 
            // _fileStatusList
            // 
            _fileStatusList.Dock = DockStyle.Fill;
            _fileStatusList.Location = new Point(0, 0);
            _fileStatusList.Margin = new Padding(3, 4, 3, 4);
            _fileStatusList.Name = "_fileStatusList";
            _fileStatusList.Size = new Size(742, 116);
            _fileStatusList.TabIndex = 0;
            // 
            // _diffViewer
            // 
            _diffViewer.Dock = DockStyle.Fill;
            _diffViewer.Location = new Point(0, 0);
            _diffViewer.Margin = new Padding(3, 2, 3, 2);
            _diffViewer.Name = "_diffViewer";
            _diffViewer.Size = new Size(742, 211);
            _diffViewer.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(tableLayoutPanel1);
            tabPage2.Location = new Point(4, 22);
            tabPage2.Margin = new Padding(2);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(2);
            tabPage2.Size = new Size(746, 335);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Comments";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(_discussionWB, 0, 0);
            tableLayoutPanel1.Controls.Add(_postCommentText, 0, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(2, 2);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(742, 275);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // _discussionWB
            // 
            _discussionWB.Dock = DockStyle.Fill;
            _discussionWB.IsWebBrowserContextMenuEnabled = false;
            _discussionWB.Location = new Point(3, 3);
            _discussionWB.MinimumSize = new Size(20, 20);
            _discussionWB.Name = "_discussionWB";
            _discussionWB.ScriptErrorsSuppressed = true;
            _discussionWB.Size = new Size(736, 156);
            _discussionWB.TabIndex = 9;
            _discussionWB.WebBrowserShortcutsEnabled = false;
            // 
            // _postCommentText
            // 
            _postCommentText.Dock = DockStyle.Fill;
            _postCommentText.Location = new Point(2, 164);
            _postCommentText.Margin = new Padding(2);
            _postCommentText.Name = "_postCommentText";
            _postCommentText.Size = new Size(738, 76);
            _postCommentText.TabIndex = 10;
            _postCommentText.TextBoxFont = new Font("Segoe UI", 9F);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(_refreshCommentsBtn);
            flowLayoutPanel1.Controls.Add(_postComment);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(2, 244);
            flowLayoutPanel1.Margin = new Padding(2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(738, 29);
            flowLayoutPanel1.TabIndex = 11;
            // 
            // _refreshCommentsBtn
            // 
            _refreshCommentsBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _refreshCommentsBtn.Location = new Point(635, 3);
            _refreshCommentsBtn.Name = "_refreshCommentsBtn";
            _refreshCommentsBtn.Size = new Size(100, 23);
            _refreshCommentsBtn.TabIndex = 10;
            _refreshCommentsBtn.Text = "Refresh";
            _refreshCommentsBtn.UseVisualStyleBackColor = true;
            // 
            // _postComment
            // 
            _postComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _postComment.Location = new Point(498, 3);
            _postComment.Name = "_postComment";
            _postComment.Size = new Size(131, 23);
            _postComment.TabIndex = 11;
            _postComment.Text = "Post comment";
            _postComment.UseVisualStyleBackColor = true;
            // 
            // columnHeaderBranch
            // 
            columnHeaderBranch.Text = "Will be fetched to branch";
            columnHeaderBranch.Width = -2;
            // 
            // ViewPullRequestsForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(754, 511);
            Controls.Add(splitContainer2);
            Name = "ViewPullRequestsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "View Pull Requests";
            Load += ViewPullRequestsForm_Load;
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).EndInit();
            splitContainer2.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer3)).EndInit();
            splitContainer3.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private SplitContainer splitContainer3;
        private FileStatusList _fileStatusList;
        private Editor.FileViewer _diffViewer;
        private TabPage tabPage2;
        private TableLayoutPanel tableLayoutPanel1;
        private GitUI.UserControls.WebBrowserControl _discussionWB;
        private SpellChecker.EditNetSpell _postCommentText;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button _refreshCommentsBtn;
        private Button _postComment;
        private Label _chooseRepo;
        private UserControls.NativeListView _pullRequestsList;
        private ColumnHeader columnHeaderId;
        private ColumnHeader columnHeaderHeading;
        private ColumnHeader columnHeaderBy;
        private ColumnHeader columnHeaderCreated;
        private Button _fetchBtn;
        private Button _addAndFetchBtn;
        private ComboBox _selectHostedRepoCB;
        private Button _closePullRequestBtn;
        private SplitContainer splitContainer2;
        private TableLayoutPanel tableLayoutPanel2;
        private FlowLayoutPanel flowLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private FlowLayoutPanel flowLayoutPanel3;
        private ColumnHeader columnHeaderBranch;
    }
}
