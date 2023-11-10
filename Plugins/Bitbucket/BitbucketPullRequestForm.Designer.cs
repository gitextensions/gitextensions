namespace GitExtensions.Plugins.Bitbucket
{
    partial class BitbucketPullRequestForm
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
            if (disposing && (components is not null))
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
            components = new System.ComponentModel.Container();
            lblSourceBranch = new Label();
            lblTargetBranch = new Label();
            lblDescription = new Label();
            txtDescription = new TextBox();
            btnCreate = new Button();
            txtTitle = new TextBox();
            lblTitle = new Label();
            lblSourceRepository = new Label();
            groupBoxSource = new GroupBox();
            ddlRepositorySource = new ComboBox();
            ddlBranchSource = new ComboBox();
            lblCommitInfoSource = new Label();
            ddlBranchTarget = new ComboBox();
            groupBoxTarget = new GroupBox();
            ddlRepositoryTarget = new ComboBox();
            lblCommitInfoTarget = new Label();
            lblTargetRepository = new Label();
            groupBoxCreatePullRequest = new GroupBox();
            _NO_TRANSLATE_lblLinkCreatePull = new LinkLabel();
            splitContainerCreate = new SplitContainer();
            tabControl1 = new TabControl();
            tabView = new TabPage();
            splitContainerView1 = new SplitContainer();
            lbxPullRequests = new ListBox();
            groupBoxInformation = new GroupBox();
            lblPRState = new Label();
            lblAuthor = new Label();
            lblState = new Label();
            lblPRAuthor = new Label();
            btnMerge = new Button();
            btnApprove = new Button();
            groupBoxPullRequestInfo = new GroupBox();
            _NO_TRANSLATE_lblLinkViewPull = new LinkLabel();
            txtPRDescription = new TextBox();
            txtPRTitle = new TextBox();
            txtPRReviewers = new TextBox();
            lblReviewerView = new Label();
            lblDescriptionView = new Label();
            lblTitleView = new Label();
            splitContainerView2 = new SplitContainer();
            groupBoxSourceRepoView = new GroupBox();
            lblPRSourceBranch = new Label();
            lblPRSourceRepo = new Label();
            lblSourceRepoView = new Label();
            lblTargetRepoView = new Label();
            groupBoxTargetRepoView = new GroupBox();
            lblPRDestBranch = new Label();
            lblRepositoryView = new Label();
            lblPRDestRepo = new Label();
            lblBranchView = new Label();
            tabCreate = new TabPage();
            toolTipLink = new ToolTip(components);
            groupBoxSource.SuspendLayout();
            groupBoxTarget.SuspendLayout();
            groupBoxCreatePullRequest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainerCreate)).BeginInit();
            splitContainerCreate.Panel1.SuspendLayout();
            splitContainerCreate.Panel2.SuspendLayout();
            splitContainerCreate.SuspendLayout();
            tabControl1.SuspendLayout();
            tabView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainerView1)).BeginInit();
            splitContainerView1.Panel1.SuspendLayout();
            splitContainerView1.Panel2.SuspendLayout();
            splitContainerView1.SuspendLayout();
            groupBoxInformation.SuspendLayout();
            groupBoxPullRequestInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainerView2)).BeginInit();
            splitContainerView2.Panel1.SuspendLayout();
            splitContainerView2.Panel2.SuspendLayout();
            splitContainerView2.SuspendLayout();
            groupBoxSourceRepoView.SuspendLayout();
            groupBoxTargetRepoView.SuspendLayout();
            tabCreate.SuspendLayout();
            SuspendLayout();
            // 
            // lblSourceBranch
            // 
            lblSourceBranch.AutoSize = true;
            lblSourceBranch.Location = new Point(14, 59);
            lblSourceBranch.Name = "lblSourceBranch";
            lblSourceBranch.Size = new Size(41, 13);
            lblSourceBranch.TabIndex = 0;
            lblSourceBranch.Text = "Branch";
            // 
            // lblTargetBranch
            // 
            lblTargetBranch.AutoSize = true;
            lblTargetBranch.Location = new Point(14, 59);
            lblTargetBranch.Name = "lblTargetBranch";
            lblTargetBranch.Size = new Size(41, 13);
            lblTargetBranch.TabIndex = 1;
            lblTargetBranch.Text = "Branch";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(14, 224);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(60, 13);
            lblDescription.TabIndex = 6;
            lblDescription.Text = "Description";
            // 
            // txtDescription
            // 
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescription.Location = new Point(128, 221);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(593, 158);
            txtDescription.TabIndex = 2;
            // 
            // btnCreate
            // 
            btnCreate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCreate.Location = new Point(641, 592);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(134, 23);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += BtnCreateClick;
            // 
            // txtTitle
            // 
            txtTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTitle.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtTitle.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtTitle.Location = new Point(129, 189);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(593, 20);
            txtTitle.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(15, 192);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(27, 13);
            lblTitle.TabIndex = 11;
            lblTitle.Text = "Title";
            // 
            // lblSourceRepository
            // 
            lblSourceRepository.AutoSize = true;
            lblSourceRepository.Location = new Point(14, 29);
            lblSourceRepository.Name = "lblSourceRepository";
            lblSourceRepository.Size = new Size(57, 13);
            lblSourceRepository.TabIndex = 0;
            lblSourceRepository.Text = "Repository";
            // 
            // groupBoxSource
            // 
            groupBoxSource.Controls.Add(ddlRepositorySource);
            groupBoxSource.Controls.Add(ddlBranchSource);
            groupBoxSource.Controls.Add(lblCommitInfoSource);
            groupBoxSource.Controls.Add(lblSourceRepository);
            groupBoxSource.Controls.Add(lblSourceBranch);
            groupBoxSource.Dock = DockStyle.Fill;
            groupBoxSource.Location = new Point(0, 0);
            groupBoxSource.Name = "groupBoxSource";
            groupBoxSource.Size = new Size(382, 169);
            groupBoxSource.TabIndex = 0;
            groupBoxSource.TabStop = false;
            groupBoxSource.Text = "Source";
            // 
            // ddlRepositorySource
            // 
            ddlRepositorySource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ddlRepositorySource.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlRepositorySource.Enabled = false;
            ddlRepositorySource.FormattingEnabled = true;
            ddlRepositorySource.Location = new Point(128, 26);
            ddlRepositorySource.Name = "ddlRepositorySource";
            ddlRepositorySource.Size = new Size(178, 21);
            ddlRepositorySource.TabIndex = 0;
            ddlRepositorySource.SelectedValueChanged += DdlRepositorySourceSelectedValueChanged;
            // 
            // ddlBranchSource
            // 
            ddlBranchSource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ddlBranchSource.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlBranchSource.FormattingEnabled = true;
            ddlBranchSource.Location = new Point(128, 58);
            ddlBranchSource.Name = "ddlBranchSource";
            ddlBranchSource.Size = new Size(178, 21);
            ddlBranchSource.TabIndex = 0;
            ddlBranchSource.SelectedValueChanged += DdlBranchSourceSelectedValueChanged;
            // 
            // lblCommitInfoSource
            // 
            lblCommitInfoSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblCommitInfoSource.Location = new Point(127, 90);
            lblCommitInfoSource.Name = "lblCommitInfoSource";
            lblCommitInfoSource.Size = new Size(237, 62);
            lblCommitInfoSource.TabIndex = 2;
            // 
            // ddlBranchTarget
            // 
            ddlBranchTarget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ddlBranchTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlBranchTarget.FormattingEnabled = true;
            ddlBranchTarget.Location = new Point(127, 58);
            ddlBranchTarget.Name = "ddlBranchTarget";
            ddlBranchTarget.Size = new Size(178, 21);
            ddlBranchTarget.TabIndex = 0;
            ddlBranchTarget.SelectedValueChanged += DdlBranchTargetSelectedValueChanged;
            // 
            // groupBoxTarget
            // 
            groupBoxTarget.Controls.Add(ddlRepositoryTarget);
            groupBoxTarget.Controls.Add(lblCommitInfoTarget);
            groupBoxTarget.Controls.Add(lblTargetRepository);
            groupBoxTarget.Controls.Add(lblTargetBranch);
            groupBoxTarget.Controls.Add(ddlBranchTarget);
            groupBoxTarget.Dock = DockStyle.Fill;
            groupBoxTarget.Location = new Point(0, 0);
            groupBoxTarget.Name = "groupBoxTarget";
            groupBoxTarget.Size = new Size(390, 169);
            groupBoxTarget.TabIndex = 0;
            groupBoxTarget.TabStop = false;
            groupBoxTarget.Text = "Target";
            // 
            // ddlRepositoryTarget
            // 
            ddlRepositoryTarget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ddlRepositoryTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlRepositoryTarget.Enabled = false;
            ddlRepositoryTarget.FormattingEnabled = true;
            ddlRepositoryTarget.Location = new Point(128, 26);
            ddlRepositoryTarget.Name = "ddlRepositoryTarget";
            ddlRepositoryTarget.Size = new Size(178, 21);
            ddlRepositoryTarget.TabIndex = 0;
            ddlRepositoryTarget.SelectedValueChanged += DdlRepositoryTargetSelectedValueChanged;
            // 
            // lblCommitInfoTarget
            // 
            lblCommitInfoTarget.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblCommitInfoTarget.Location = new Point(127, 90);
            lblCommitInfoTarget.Name = "lblCommitInfoTarget";
            lblCommitInfoTarget.Size = new Size(237, 62);
            lblCommitInfoTarget.TabIndex = 2;
            // 
            // lblTargetRepository
            // 
            lblTargetRepository.AutoSize = true;
            lblTargetRepository.Location = new Point(14, 29);
            lblTargetRepository.Name = "lblTargetRepository";
            lblTargetRepository.Size = new Size(57, 13);
            lblTargetRepository.TabIndex = 12;
            lblTargetRepository.Text = "Repository";
            // 
            // groupBoxCreatePullRequest
            // 
            groupBoxCreatePullRequest.Controls.Add(_NO_TRANSLATE_lblLinkCreatePull);
            groupBoxCreatePullRequest.Controls.Add(lblDescription);
            groupBoxCreatePullRequest.Controls.Add(txtDescription);
            groupBoxCreatePullRequest.Controls.Add(txtTitle);
            groupBoxCreatePullRequest.Controls.Add(lblTitle);
            groupBoxCreatePullRequest.Location = new Point(6, 181);
            groupBoxCreatePullRequest.Name = "groupBoxCreatePullRequest";
            groupBoxCreatePullRequest.Size = new Size(776, 401);
            groupBoxCreatePullRequest.TabIndex = 1;
            groupBoxCreatePullRequest.TabStop = false;
            groupBoxCreatePullRequest.Text = "Pull Request Info";
            // 
            // _NO_TRANSLATE_lblLinkCreatePull
            // 
            _NO_TRANSLATE_lblLinkCreatePull.AutoSize = true;
            _NO_TRANSLATE_lblLinkCreatePull.Location = new Point(128, 382);
            _NO_TRANSLATE_lblLinkCreatePull.Name = "_NO_TRANSLATE_lblLinkCreatePull";
            _NO_TRANSLATE_lblLinkCreatePull.Size = new Size(0, 13);
            _NO_TRANSLATE_lblLinkCreatePull.TabIndex = 12;
            _NO_TRANSLATE_lblLinkCreatePull.TabStop = true;
            _NO_TRANSLATE_lblLinkCreatePull.LinkClicked += textLinkLabel_LinkClicked;
            // 
            // splitContainerCreate
            // 
            splitContainerCreate.Location = new Point(6, 6);
            splitContainerCreate.Name = "splitContainerCreate";
            // 
            // splitContainerCreate.Panel1
            // 
            splitContainerCreate.Panel1.Controls.Add(groupBoxSource);
            // 
            // splitContainerCreate.Panel2
            // 
            splitContainerCreate.Panel2.Controls.Add(groupBoxTarget);
            splitContainerCreate.Size = new Size(776, 169);
            splitContainerCreate.SplitterDistance = 382;
            splitContainerCreate.TabIndex = 0;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabView);
            tabControl1.Controls.Add(tabCreate);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Margin = new Padding(0);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(0, 0);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(796, 644);
            tabControl1.TabIndex = 3;
            // 
            // tabView
            // 
            tabView.Controls.Add(splitContainerView1);
            tabView.Controls.Add(btnMerge);
            tabView.Controls.Add(btnApprove);
            tabView.Controls.Add(groupBoxPullRequestInfo);
            tabView.Controls.Add(splitContainerView2);
            tabView.Location = new Point(4, 22);
            tabView.Name = "tabView";
            tabView.Padding = new Padding(3);
            tabView.Size = new Size(788, 618);
            tabView.TabIndex = 1;
            tabView.Text = "View Pull Requests";
            tabView.UseVisualStyleBackColor = true;
            // 
            // splitContainerView1
            // 
            splitContainerView1.Location = new Point(6, 3);
            splitContainerView1.Name = "splitContainerView1";
            // 
            // splitContainerView1.Panel1
            // 
            splitContainerView1.Panel1.Controls.Add(lbxPullRequests);
            // 
            // splitContainerView1.Panel2
            // 
            splitContainerView1.Panel2.Controls.Add(groupBoxInformation);
            splitContainerView1.Size = new Size(776, 83);
            splitContainerView1.SplitterDistance = 382;
            splitContainerView1.TabIndex = 8;
            // 
            // lbxPullRequests
            // 
            lbxPullRequests.Dock = DockStyle.Fill;
            lbxPullRequests.FormattingEnabled = true;
            lbxPullRequests.Location = new Point(0, 0);
            lbxPullRequests.Name = "lbxPullRequests";
            lbxPullRequests.Size = new Size(382, 83);
            lbxPullRequests.TabIndex = 1;
            lbxPullRequests.SelectedIndexChanged += PullRequestChanged;
            // 
            // groupBoxInformation
            // 
            groupBoxInformation.Controls.Add(lblPRState);
            groupBoxInformation.Controls.Add(lblAuthor);
            groupBoxInformation.Controls.Add(lblState);
            groupBoxInformation.Controls.Add(lblPRAuthor);
            groupBoxInformation.Dock = DockStyle.Fill;
            groupBoxInformation.Location = new Point(0, 0);
            groupBoxInformation.Name = "groupBoxInformation";
            groupBoxInformation.Size = new Size(390, 83);
            groupBoxInformation.TabIndex = 0;
            groupBoxInformation.TabStop = false;
            groupBoxInformation.Text = "Information";
            // 
            // lblPRState
            // 
            lblPRState.AutoSize = true;
            lblPRState.Location = new Point(130, 53);
            lblPRState.Name = "lblPRState";
            lblPRState.Size = new Size(0, 13);
            lblPRState.TabIndex = 10;
            // 
            // lblAuthor
            // 
            lblAuthor.AutoSize = true;
            lblAuthor.Location = new Point(16, 23);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(38, 13);
            lblAuthor.TabIndex = 7;
            lblAuthor.Text = "Author";
            // 
            // lblState
            // 
            lblState.AutoSize = true;
            lblState.Location = new Point(16, 53);
            lblState.Name = "lblState";
            lblState.Size = new Size(32, 13);
            lblState.TabIndex = 8;
            lblState.Text = "State";
            // 
            // lblPRAuthor
            // 
            lblPRAuthor.AutoSize = true;
            lblPRAuthor.Location = new Point(130, 23);
            lblPRAuthor.Name = "lblPRAuthor";
            lblPRAuthor.Size = new Size(0, 13);
            lblPRAuthor.TabIndex = 5;
            // 
            // btnMerge
            // 
            btnMerge.Location = new Point(648, 584);
            btnMerge.Name = "btnMerge";
            btnMerge.Size = new Size(134, 23);
            btnMerge.TabIndex = 7;
            btnMerge.Text = "Merge";
            btnMerge.UseVisualStyleBackColor = true;
            btnMerge.Click += BtnMergeClick;
            // 
            // btnApprove
            // 
            btnApprove.Location = new Point(508, 584);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(134, 23);
            btnApprove.TabIndex = 7;
            btnApprove.Text = "Approve";
            btnApprove.UseVisualStyleBackColor = true;
            btnApprove.Click += BtnApproveClick;
            // 
            // groupBoxPullRequestInfo
            // 
            groupBoxPullRequestInfo.Controls.Add(_NO_TRANSLATE_lblLinkViewPull);
            groupBoxPullRequestInfo.Controls.Add(txtPRDescription);
            groupBoxPullRequestInfo.Controls.Add(txtPRTitle);
            groupBoxPullRequestInfo.Controls.Add(txtPRReviewers);
            groupBoxPullRequestInfo.Controls.Add(lblReviewerView);
            groupBoxPullRequestInfo.Controls.Add(lblDescriptionView);
            groupBoxPullRequestInfo.Controls.Add(lblTitleView);
            groupBoxPullRequestInfo.Location = new Point(6, 181);
            groupBoxPullRequestInfo.Name = "groupBoxPullRequestInfo";
            groupBoxPullRequestInfo.Size = new Size(776, 401);
            groupBoxPullRequestInfo.TabIndex = 4;
            groupBoxPullRequestInfo.TabStop = false;
            groupBoxPullRequestInfo.Text = "Pull Request Info";
            // 
            // _NO_TRANSLATE_lblLinkViewPull
            // 
            _NO_TRANSLATE_lblLinkViewPull.AutoSize = true;
            _NO_TRANSLATE_lblLinkViewPull.Location = new Point(17, 382);
            _NO_TRANSLATE_lblLinkViewPull.Name = "_NO_TRANSLATE_lblLinkViewPull";
            _NO_TRANSLATE_lblLinkViewPull.Size = new Size(55, 13);
            _NO_TRANSLATE_lblLinkViewPull.TabIndex = 15;
            _NO_TRANSLATE_lblLinkViewPull.TabStop = true;
            _NO_TRANSLATE_lblLinkViewPull.Text = "linkLabel1";
            _NO_TRANSLATE_lblLinkViewPull.LinkClicked += textLinkLabel_LinkClicked;
            // 
            // txtPRDescription
            // 
            txtPRDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPRDescription.Location = new Point(131, 224);
            txtPRDescription.Multiline = true;
            txtPRDescription.Name = "txtPRDescription";
            txtPRDescription.ReadOnly = true;
            txtPRDescription.ScrollBars = ScrollBars.Vertical;
            txtPRDescription.Size = new Size(594, 138);
            txtPRDescription.TabIndex = 14;
            // 
            // txtPRTitle
            // 
            txtPRTitle.Location = new Point(131, 192);
            txtPRTitle.Name = "txtPRTitle";
            txtPRTitle.ReadOnly = true;
            txtPRTitle.Size = new Size(593, 20);
            txtPRTitle.TabIndex = 13;
            // 
            // txtPRReviewers
            // 
            txtPRReviewers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPRReviewers.Location = new Point(128, 38);
            txtPRReviewers.Multiline = true;
            txtPRReviewers.Name = "txtPRReviewers";
            txtPRReviewers.ReadOnly = true;
            txtPRReviewers.ScrollBars = ScrollBars.Vertical;
            txtPRReviewers.Size = new Size(594, 138);
            txtPRReviewers.TabIndex = 12;
            // 
            // lblReviewerView
            // 
            lblReviewerView.AutoSize = true;
            lblReviewerView.Location = new Point(15, 38);
            lblReviewerView.Name = "lblReviewerView";
            lblReviewerView.Size = new Size(106, 13);
            lblReviewerView.TabIndex = 4;
            lblReviewerView.Text = "Reviewer (approved)";
            // 
            // lblDescriptionView
            // 
            lblDescriptionView.AutoSize = true;
            lblDescriptionView.Location = new Point(14, 224);
            lblDescriptionView.Name = "lblDescriptionView";
            lblDescriptionView.Size = new Size(60, 13);
            lblDescriptionView.TabIndex = 6;
            lblDescriptionView.Text = "Description";
            // 
            // lblTitleView
            // 
            lblTitleView.AutoSize = true;
            lblTitleView.Location = new Point(15, 192);
            lblTitleView.Name = "lblTitleView";
            lblTitleView.Size = new Size(27, 13);
            lblTitleView.TabIndex = 11;
            lblTitleView.Text = "Title";
            // 
            // splitContainerView2
            // 
            splitContainerView2.Location = new Point(6, 92);
            splitContainerView2.Name = "splitContainerView2";
            // 
            // splitContainerView2.Panel1
            // 
            splitContainerView2.Panel1.Controls.Add(groupBoxSourceRepoView);
            // 
            // splitContainerView2.Panel2
            // 
            splitContainerView2.Panel2.Controls.Add(groupBoxTargetRepoView);
            splitContainerView2.Size = new Size(776, 83);
            splitContainerView2.SplitterDistance = 382;
            splitContainerView2.TabIndex = 2;
            // 
            // groupBoxSourceRepoView
            // 
            groupBoxSourceRepoView.Controls.Add(lblPRSourceBranch);
            groupBoxSourceRepoView.Controls.Add(lblPRSourceRepo);
            groupBoxSourceRepoView.Controls.Add(lblSourceRepoView);
            groupBoxSourceRepoView.Controls.Add(lblTargetRepoView);
            groupBoxSourceRepoView.Dock = DockStyle.Fill;
            groupBoxSourceRepoView.Location = new Point(0, 0);
            groupBoxSourceRepoView.Name = "groupBoxSourceRepoView";
            groupBoxSourceRepoView.Size = new Size(382, 83);
            groupBoxSourceRepoView.TabIndex = 0;
            groupBoxSourceRepoView.TabStop = false;
            groupBoxSourceRepoView.Text = "Source";
            // 
            // lblPRSourceBranch
            // 
            lblPRSourceBranch.AutoSize = true;
            lblPRSourceBranch.Location = new Point(128, 55);
            lblPRSourceBranch.Name = "lblPRSourceBranch";
            lblPRSourceBranch.Size = new Size(0, 13);
            lblPRSourceBranch.TabIndex = 2;
            // 
            // lblPRSourceRepo
            // 
            lblPRSourceRepo.AutoSize = true;
            lblPRSourceRepo.Location = new Point(128, 25);
            lblPRSourceRepo.Name = "lblPRSourceRepo";
            lblPRSourceRepo.Size = new Size(0, 13);
            lblPRSourceRepo.TabIndex = 1;
            // 
            // lblSourceRepoView
            // 
            lblSourceRepoView.AutoSize = true;
            lblSourceRepoView.Location = new Point(14, 25);
            lblSourceRepoView.Name = "lblSourceRepoView";
            lblSourceRepoView.Size = new Size(57, 13);
            lblSourceRepoView.TabIndex = 0;
            lblSourceRepoView.Text = "Repository";
            // 
            // lblTargetRepoView
            // 
            lblTargetRepoView.AutoSize = true;
            lblTargetRepoView.Location = new Point(14, 55);
            lblTargetRepoView.Name = "lblTargetRepoView";
            lblTargetRepoView.Size = new Size(41, 13);
            lblTargetRepoView.TabIndex = 0;
            lblTargetRepoView.Text = "Branch";
            // 
            // groupBoxTargetRepoView
            // 
            groupBoxTargetRepoView.Controls.Add(lblPRDestBranch);
            groupBoxTargetRepoView.Controls.Add(lblRepositoryView);
            groupBoxTargetRepoView.Controls.Add(lblPRDestRepo);
            groupBoxTargetRepoView.Controls.Add(lblBranchView);
            groupBoxTargetRepoView.Dock = DockStyle.Fill;
            groupBoxTargetRepoView.Location = new Point(0, 0);
            groupBoxTargetRepoView.Name = "groupBoxTargetRepoView";
            groupBoxTargetRepoView.Size = new Size(390, 83);
            groupBoxTargetRepoView.TabIndex = 0;
            groupBoxTargetRepoView.TabStop = false;
            groupBoxTargetRepoView.Text = "Target";
            // 
            // lblPRDestBranch
            // 
            lblPRDestBranch.AutoSize = true;
            lblPRDestBranch.Location = new Point(130, 55);
            lblPRDestBranch.Name = "lblPRDestBranch";
            lblPRDestBranch.Size = new Size(0, 13);
            lblPRDestBranch.TabIndex = 6;
            // 
            // lblRepositoryView
            // 
            lblRepositoryView.AutoSize = true;
            lblRepositoryView.Location = new Point(16, 25);
            lblRepositoryView.Name = "lblRepositoryView";
            lblRepositoryView.Size = new Size(57, 13);
            lblRepositoryView.TabIndex = 3;
            lblRepositoryView.Text = "Repository";
            // 
            // lblPRDestRepo
            // 
            lblPRDestRepo.AutoSize = true;
            lblPRDestRepo.Location = new Point(130, 25);
            lblPRDestRepo.Name = "lblPRDestRepo";
            lblPRDestRepo.Size = new Size(0, 13);
            lblPRDestRepo.TabIndex = 5;
            // 
            // lblBranchView
            // 
            lblBranchView.AutoSize = true;
            lblBranchView.Location = new Point(16, 55);
            lblBranchView.Name = "lblBranchView";
            lblBranchView.Size = new Size(41, 13);
            lblBranchView.TabIndex = 4;
            lblBranchView.Text = "Branch";
            // 
            // tabCreate
            // 
            tabCreate.Controls.Add(splitContainerCreate);
            tabCreate.Controls.Add(btnCreate);
            tabCreate.Controls.Add(groupBoxCreatePullRequest);
            tabCreate.Location = new Point(4, 22);
            tabCreate.Name = "tabCreate";
            tabCreate.Padding = new Padding(3);
            tabCreate.Size = new Size(788, 618);
            tabCreate.TabIndex = 0;
            tabCreate.Text = "Create Pull Request";
            tabCreate.UseVisualStyleBackColor = true;
            // 
            // BitbucketPullRequestForm
            // 
            AcceptButton = btnCreate;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(820, 668);
            Controls.Add(tabControl1);
            Name = "BitbucketPullRequestForm";
            Text = "Bitbucket Server";
            groupBoxSource.ResumeLayout(false);
            groupBoxSource.PerformLayout();
            groupBoxTarget.ResumeLayout(false);
            groupBoxTarget.PerformLayout();
            groupBoxCreatePullRequest.ResumeLayout(false);
            groupBoxCreatePullRequest.PerformLayout();
            splitContainerCreate.Panel1.ResumeLayout(false);
            splitContainerCreate.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainerCreate)).EndInit();
            splitContainerCreate.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabView.ResumeLayout(false);
            splitContainerView1.Panel1.ResumeLayout(false);
            splitContainerView1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainerView1)).EndInit();
            splitContainerView1.ResumeLayout(false);
            groupBoxInformation.ResumeLayout(false);
            groupBoxInformation.PerformLayout();
            groupBoxPullRequestInfo.ResumeLayout(false);
            groupBoxPullRequestInfo.PerformLayout();
            splitContainerView2.Panel1.ResumeLayout(false);
            splitContainerView2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainerView2)).EndInit();
            splitContainerView2.ResumeLayout(false);
            groupBoxSourceRepoView.ResumeLayout(false);
            groupBoxSourceRepoView.PerformLayout();
            groupBoxTargetRepoView.ResumeLayout(false);
            groupBoxTargetRepoView.PerformLayout();
            tabCreate.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Label lblSourceBranch;
        private Label lblTargetBranch;
        private Label lblDescription;
        private TextBox txtDescription;
        private Button btnCreate;
        private TextBox txtTitle;
        private Label lblTitle;
        private Label lblSourceRepository;
        private GroupBox groupBoxSource;
        private GroupBox groupBoxTarget;
        private Label lblTargetRepository;
        private GroupBox groupBoxCreatePullRequest;
        private Label lblCommitInfoSource;
        private Label lblCommitInfoTarget;
        private ComboBox ddlRepositorySource;
        private ComboBox ddlBranchSource;
        private ComboBox ddlBranchTarget;
        private ComboBox ddlRepositoryTarget;
        private SplitContainer splitContainerCreate;
        private TabControl tabControl1;
        private TabPage tabCreate;
        private TabPage tabView;
        private ListBox lbxPullRequests;
        private SplitContainer splitContainerView2;
        private GroupBox groupBoxSourceRepoView;
        private Label lblSourceRepoView;
        private Label lblTargetRepoView;
        private GroupBox groupBoxTargetRepoView;
        private GroupBox groupBoxPullRequestInfo;
        private TextBox txtPRDescription;
        private TextBox txtPRTitle;
        private TextBox txtPRReviewers;
        private Label lblReviewerView;
        private Label lblDescriptionView;
        private Label lblTitleView;
        private Label lblPRSourceBranch;
        private Label lblPRSourceRepo;
        private Label lblPRDestBranch;
        private Label lblRepositoryView;
        private Label lblPRDestRepo;
        private Label lblBranchView;
        private Label lblPRAuthor;
        private Button btnMerge;
        private Button btnApprove;
        private SplitContainer splitContainerView1;
        private GroupBox groupBoxInformation;
        private Label lblPRState;
        private Label lblAuthor;
        private Label lblState;
        private LinkLabel _NO_TRANSLATE_lblLinkCreatePull;
        private LinkLabel _NO_TRANSLATE_lblLinkViewPull;
        private ToolTip toolTipLink;
    }
}
