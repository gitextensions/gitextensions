namespace Bitbucket
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
            this.lblSourceBranch = new System.Windows.Forms.Label();
            this.lblTargetBranch = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSourceRepository = new System.Windows.Forms.Label();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.ddlRepositorySource = new System.Windows.Forms.ComboBox();
            this.ddlBranchSource = new System.Windows.Forms.ComboBox();
            this.lblCommitInfoSource = new System.Windows.Forms.Label();
            this.ddlBranchTarget = new System.Windows.Forms.ComboBox();
            this.groupBoxTarget = new System.Windows.Forms.GroupBox();
            this.ddlRepositoryTarget = new System.Windows.Forms.ComboBox();
            this.lblCommitInfoTarget = new System.Windows.Forms.Label();
            this.lblTargetRepository = new System.Windows.Forms.Label();
            this.groupBoxCreatePullRequest = new System.Windows.Forms.GroupBox();
            this._NO_TRANSLATE_lblLinkCreatePull = new System.Windows.Forms.LinkLabel();
            this.splitContainerCreate = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabView = new System.Windows.Forms.TabPage();
            this.splitContainerView1 = new System.Windows.Forms.SplitContainer();
            this.lbxPullRequests = new System.Windows.Forms.ListBox();
            this.groupBoxInformation = new System.Windows.Forms.GroupBox();
            this.lblPRState = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.lblPRAuthor = new System.Windows.Forms.Label();
            this.btnMerge = new System.Windows.Forms.Button();
            this.btnApprove = new System.Windows.Forms.Button();
            this.groupBoxPullRequestInfo = new System.Windows.Forms.GroupBox();
            this._NO_TRANSLATE_lblLinkViewPull = new System.Windows.Forms.LinkLabel();
            this.txtPRDescription = new System.Windows.Forms.TextBox();
            this.txtPRTitle = new System.Windows.Forms.TextBox();
            this.txtPRReviewers = new System.Windows.Forms.TextBox();
            this.lblReviewerView = new System.Windows.Forms.Label();
            this.lblDescriptionView = new System.Windows.Forms.Label();
            this.lblTitleView = new System.Windows.Forms.Label();
            this.splitContainerView2 = new System.Windows.Forms.SplitContainer();
            this.groupBoxSourceRepoView = new System.Windows.Forms.GroupBox();
            this.lblPRSourceBranch = new System.Windows.Forms.Label();
            this.lblPRSourceRepo = new System.Windows.Forms.Label();
            this.lblSourceRepoView = new System.Windows.Forms.Label();
            this.lblTargetRepoView = new System.Windows.Forms.Label();
            this.groupBoxTargetRepoView = new System.Windows.Forms.GroupBox();
            this.lblPRDestBranch = new System.Windows.Forms.Label();
            this.lblRepositoryView = new System.Windows.Forms.Label();
            this.lblPRDestRepo = new System.Windows.Forms.Label();
            this.lblBranchView = new System.Windows.Forms.Label();
            this.tabCreate = new System.Windows.Forms.TabPage();
            this.toolTipLink = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxSource.SuspendLayout();
            this.groupBoxTarget.SuspendLayout();
            this.groupBoxCreatePullRequest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCreate)).BeginInit();
            this.splitContainerCreate.Panel1.SuspendLayout();
            this.splitContainerCreate.Panel2.SuspendLayout();
            this.splitContainerCreate.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerView1)).BeginInit();
            this.splitContainerView1.Panel1.SuspendLayout();
            this.splitContainerView1.Panel2.SuspendLayout();
            this.splitContainerView1.SuspendLayout();
            this.groupBoxInformation.SuspendLayout();
            this.groupBoxPullRequestInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerView2)).BeginInit();
            this.splitContainerView2.Panel1.SuspendLayout();
            this.splitContainerView2.Panel2.SuspendLayout();
            this.splitContainerView2.SuspendLayout();
            this.groupBoxSourceRepoView.SuspendLayout();
            this.groupBoxTargetRepoView.SuspendLayout();
            this.tabCreate.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSourceBranch
            // 
            this.lblSourceBranch.AutoSize = true;
            this.lblSourceBranch.Location = new System.Drawing.Point(14, 59);
            this.lblSourceBranch.Name = "lblSourceBranch";
            this.lblSourceBranch.Size = new System.Drawing.Size(41, 13);
            this.lblSourceBranch.TabIndex = 0;
            this.lblSourceBranch.Text = "Branch";
            // 
            // lblTargetBranch
            // 
            this.lblTargetBranch.AutoSize = true;
            this.lblTargetBranch.Location = new System.Drawing.Point(14, 59);
            this.lblTargetBranch.Name = "lblTargetBranch";
            this.lblTargetBranch.Size = new System.Drawing.Size(41, 13);
            this.lblTargetBranch.TabIndex = 1;
            this.lblTargetBranch.Text = "Branch";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(14, 224);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(60, 13);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "Description";
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(128, 221);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(593, 158);
            this.txtDescription.TabIndex = 2;
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(641, 592);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(134, 23);
            this.btnCreate.TabIndex = 2;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.BtnCreateClick);
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtTitle.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtTitle.Location = new System.Drawing.Point(129, 189);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(593, 20);
            this.txtTitle.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(15, 192);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(27, 13);
            this.lblTitle.TabIndex = 11;
            this.lblTitle.Text = "Title";
            // 
            // lblSourceRepository
            // 
            this.lblSourceRepository.AutoSize = true;
            this.lblSourceRepository.Location = new System.Drawing.Point(14, 29);
            this.lblSourceRepository.Name = "lblSourceRepository";
            this.lblSourceRepository.Size = new System.Drawing.Size(57, 13);
            this.lblSourceRepository.TabIndex = 0;
            this.lblSourceRepository.Text = "Repository";
            // 
            // groupBoxSource
            // 
            this.groupBoxSource.Controls.Add(this.ddlRepositorySource);
            this.groupBoxSource.Controls.Add(this.ddlBranchSource);
            this.groupBoxSource.Controls.Add(this.lblCommitInfoSource);
            this.groupBoxSource.Controls.Add(this.lblSourceRepository);
            this.groupBoxSource.Controls.Add(this.lblSourceBranch);
            this.groupBoxSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSource.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(382, 169);
            this.groupBoxSource.TabIndex = 0;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Source";
            // 
            // ddlRepositorySource
            // 
            this.ddlRepositorySource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlRepositorySource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlRepositorySource.Enabled = false;
            this.ddlRepositorySource.FormattingEnabled = true;
            this.ddlRepositorySource.Location = new System.Drawing.Point(128, 26);
            this.ddlRepositorySource.Name = "ddlRepositorySource";
            this.ddlRepositorySource.Size = new System.Drawing.Size(178, 21);
            this.ddlRepositorySource.TabIndex = 0;
            this.ddlRepositorySource.SelectedValueChanged += new System.EventHandler(this.DdlRepositorySourceSelectedValueChanged);
            // 
            // ddlBranchSource
            // 
            this.ddlBranchSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlBranchSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlBranchSource.FormattingEnabled = true;
            this.ddlBranchSource.Location = new System.Drawing.Point(128, 58);
            this.ddlBranchSource.Name = "ddlBranchSource";
            this.ddlBranchSource.Size = new System.Drawing.Size(178, 21);
            this.ddlBranchSource.TabIndex = 0;
            this.ddlBranchSource.SelectedValueChanged += new System.EventHandler(this.DdlBranchSourceSelectedValueChanged);
            // 
            // lblCommitInfoSource
            // 
            this.lblCommitInfoSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCommitInfoSource.Location = new System.Drawing.Point(127, 90);
            this.lblCommitInfoSource.Name = "lblCommitInfoSource";
            this.lblCommitInfoSource.Size = new System.Drawing.Size(237, 62);
            this.lblCommitInfoSource.TabIndex = 2;
            // 
            // ddlBranchTarget
            // 
            this.ddlBranchTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlBranchTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlBranchTarget.FormattingEnabled = true;
            this.ddlBranchTarget.Location = new System.Drawing.Point(127, 58);
            this.ddlBranchTarget.Name = "ddlBranchTarget";
            this.ddlBranchTarget.Size = new System.Drawing.Size(178, 21);
            this.ddlBranchTarget.TabIndex = 0;
            this.ddlBranchTarget.SelectedValueChanged += new System.EventHandler(this.DdlBranchTargetSelectedValueChanged);
            // 
            // groupBoxTarget
            // 
            this.groupBoxTarget.Controls.Add(this.ddlRepositoryTarget);
            this.groupBoxTarget.Controls.Add(this.lblCommitInfoTarget);
            this.groupBoxTarget.Controls.Add(this.lblTargetRepository);
            this.groupBoxTarget.Controls.Add(this.lblTargetBranch);
            this.groupBoxTarget.Controls.Add(this.ddlBranchTarget);
            this.groupBoxTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTarget.Location = new System.Drawing.Point(0, 0);
            this.groupBoxTarget.Name = "groupBoxTarget";
            this.groupBoxTarget.Size = new System.Drawing.Size(390, 169);
            this.groupBoxTarget.TabIndex = 0;
            this.groupBoxTarget.TabStop = false;
            this.groupBoxTarget.Text = "Target";
            // 
            // ddlRepositoryTarget
            // 
            this.ddlRepositoryTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlRepositoryTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlRepositoryTarget.Enabled = false;
            this.ddlRepositoryTarget.FormattingEnabled = true;
            this.ddlRepositoryTarget.Location = new System.Drawing.Point(128, 26);
            this.ddlRepositoryTarget.Name = "ddlRepositoryTarget";
            this.ddlRepositoryTarget.Size = new System.Drawing.Size(178, 21);
            this.ddlRepositoryTarget.TabIndex = 0;
            this.ddlRepositoryTarget.SelectedValueChanged += new System.EventHandler(this.DdlRepositoryTargetSelectedValueChanged);
            // 
            // lblCommitInfoTarget
            // 
            this.lblCommitInfoTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCommitInfoTarget.Location = new System.Drawing.Point(127, 90);
            this.lblCommitInfoTarget.Name = "lblCommitInfoTarget";
            this.lblCommitInfoTarget.Size = new System.Drawing.Size(237, 62);
            this.lblCommitInfoTarget.TabIndex = 2;
            // 
            // lblTargetRepository
            // 
            this.lblTargetRepository.AutoSize = true;
            this.lblTargetRepository.Location = new System.Drawing.Point(14, 29);
            this.lblTargetRepository.Name = "lblTargetRepository";
            this.lblTargetRepository.Size = new System.Drawing.Size(57, 13);
            this.lblTargetRepository.TabIndex = 12;
            this.lblTargetRepository.Text = "Repository";
            // 
            // groupBoxCreatePullRequest
            // 
            this.groupBoxCreatePullRequest.Controls.Add(this._NO_TRANSLATE_lblLinkCreatePull);
            this.groupBoxCreatePullRequest.Controls.Add(this.lblDescription);
            this.groupBoxCreatePullRequest.Controls.Add(this.txtDescription);
            this.groupBoxCreatePullRequest.Controls.Add(this.txtTitle);
            this.groupBoxCreatePullRequest.Controls.Add(this.lblTitle);
            this.groupBoxCreatePullRequest.Location = new System.Drawing.Point(6, 181);
            this.groupBoxCreatePullRequest.Name = "groupBoxCreatePullRequest";
            this.groupBoxCreatePullRequest.Size = new System.Drawing.Size(776, 401);
            this.groupBoxCreatePullRequest.TabIndex = 1;
            this.groupBoxCreatePullRequest.TabStop = false;
            this.groupBoxCreatePullRequest.Text = "Pull Request Info";
            // 
            // _NO_TRANSLATE_lblLinkCreatePull
            // 
            this._NO_TRANSLATE_lblLinkCreatePull.AutoSize = true;
            this._NO_TRANSLATE_lblLinkCreatePull.Location = new System.Drawing.Point(128, 382);
            this._NO_TRANSLATE_lblLinkCreatePull.Name = "_NO_TRANSLATE_lblLinkCreatePull";
            this._NO_TRANSLATE_lblLinkCreatePull.Size = new System.Drawing.Size(0, 13);
            this._NO_TRANSLATE_lblLinkCreatePull.TabIndex = 12;
            this._NO_TRANSLATE_lblLinkCreatePull.TabStop = true;
            this._NO_TRANSLATE_lblLinkCreatePull.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.textLinkLabel_LinkClicked);
            // 
            // splitContainerCreate
            // 
            this.splitContainerCreate.Location = new System.Drawing.Point(6, 6);
            this.splitContainerCreate.Name = "splitContainerCreate";
            // 
            // splitContainerCreate.Panel1
            // 
            this.splitContainerCreate.Panel1.Controls.Add(this.groupBoxSource);
            // 
            // splitContainerCreate.Panel2
            // 
            this.splitContainerCreate.Panel2.Controls.Add(this.groupBoxTarget);
            this.splitContainerCreate.Size = new System.Drawing.Size(776, 169);
            this.splitContainerCreate.SplitterDistance = 382;
            this.splitContainerCreate.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabView);
            this.tabControl1.Controls.Add(this.tabCreate);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(796, 644);
            this.tabControl1.TabIndex = 3;
            // 
            // tabView
            // 
            this.tabView.Controls.Add(this.splitContainerView1);
            this.tabView.Controls.Add(this.btnMerge);
            this.tabView.Controls.Add(this.btnApprove);
            this.tabView.Controls.Add(this.groupBoxPullRequestInfo);
            this.tabView.Controls.Add(this.splitContainerView2);
            this.tabView.Location = new System.Drawing.Point(4, 22);
            this.tabView.Name = "tabView";
            this.tabView.Padding = new System.Windows.Forms.Padding(3);
            this.tabView.Size = new System.Drawing.Size(788, 618);
            this.tabView.TabIndex = 1;
            this.tabView.Text = "View Pull Requests";
            this.tabView.UseVisualStyleBackColor = true;
            // 
            // splitContainerView1
            // 
            this.splitContainerView1.Location = new System.Drawing.Point(6, 3);
            this.splitContainerView1.Name = "splitContainerView1";
            // 
            // splitContainerView1.Panel1
            // 
            this.splitContainerView1.Panel1.Controls.Add(this.lbxPullRequests);
            // 
            // splitContainerView1.Panel2
            // 
            this.splitContainerView1.Panel2.Controls.Add(this.groupBoxInformation);
            this.splitContainerView1.Size = new System.Drawing.Size(776, 83);
            this.splitContainerView1.SplitterDistance = 382;
            this.splitContainerView1.TabIndex = 8;
            // 
            // lbxPullRequests
            // 
            this.lbxPullRequests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxPullRequests.FormattingEnabled = true;
            this.lbxPullRequests.Location = new System.Drawing.Point(0, 0);
            this.lbxPullRequests.Name = "lbxPullRequests";
            this.lbxPullRequests.Size = new System.Drawing.Size(382, 83);
            this.lbxPullRequests.TabIndex = 1;
            this.lbxPullRequests.SelectedIndexChanged += new System.EventHandler(this.PullRequestChanged);
            // 
            // groupBoxInformation
            // 
            this.groupBoxInformation.Controls.Add(this.lblPRState);
            this.groupBoxInformation.Controls.Add(this.lblAuthor);
            this.groupBoxInformation.Controls.Add(this.lblState);
            this.groupBoxInformation.Controls.Add(this.lblPRAuthor);
            this.groupBoxInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInformation.Location = new System.Drawing.Point(0, 0);
            this.groupBoxInformation.Name = "groupBoxInformation";
            this.groupBoxInformation.Size = new System.Drawing.Size(390, 83);
            this.groupBoxInformation.TabIndex = 0;
            this.groupBoxInformation.TabStop = false;
            this.groupBoxInformation.Text = "Information";
            // 
            // lblPRState
            // 
            this.lblPRState.AutoSize = true;
            this.lblPRState.Location = new System.Drawing.Point(130, 53);
            this.lblPRState.Name = "lblPRState";
            this.lblPRState.Size = new System.Drawing.Size(0, 13);
            this.lblPRState.TabIndex = 10;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(16, 23);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(38, 13);
            this.lblAuthor.TabIndex = 7;
            this.lblAuthor.Text = "Author";
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(16, 53);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(32, 13);
            this.lblState.TabIndex = 8;
            this.lblState.Text = "State";
            // 
            // lblPRAuthor
            // 
            this.lblPRAuthor.AutoSize = true;
            this.lblPRAuthor.Location = new System.Drawing.Point(130, 23);
            this.lblPRAuthor.Name = "lblPRAuthor";
            this.lblPRAuthor.Size = new System.Drawing.Size(0, 13);
            this.lblPRAuthor.TabIndex = 5;
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(648, 584);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(134, 23);
            this.btnMerge.TabIndex = 7;
            this.btnMerge.Text = "Merge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.BtnMergeClick);
            // 
            // btnApprove
            // 
            this.btnApprove.Location = new System.Drawing.Point(508, 584);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(134, 23);
            this.btnApprove.TabIndex = 7;
            this.btnApprove.Text = "Approve";
            this.btnApprove.UseVisualStyleBackColor = true;
            this.btnApprove.Click += new System.EventHandler(this.BtnApproveClick);
            // 
            // groupBoxPullRequestInfo
            // 
            this.groupBoxPullRequestInfo.Controls.Add(this._NO_TRANSLATE_lblLinkViewPull);
            this.groupBoxPullRequestInfo.Controls.Add(this.txtPRDescription);
            this.groupBoxPullRequestInfo.Controls.Add(this.txtPRTitle);
            this.groupBoxPullRequestInfo.Controls.Add(this.txtPRReviewers);
            this.groupBoxPullRequestInfo.Controls.Add(this.lblReviewerView);
            this.groupBoxPullRequestInfo.Controls.Add(this.lblDescriptionView);
            this.groupBoxPullRequestInfo.Controls.Add(this.lblTitleView);
            this.groupBoxPullRequestInfo.Location = new System.Drawing.Point(6, 181);
            this.groupBoxPullRequestInfo.Name = "groupBoxPullRequestInfo";
            this.groupBoxPullRequestInfo.Size = new System.Drawing.Size(776, 401);
            this.groupBoxPullRequestInfo.TabIndex = 4;
            this.groupBoxPullRequestInfo.TabStop = false;
            this.groupBoxPullRequestInfo.Text = "Pull Request Info";
            // 
            // _NO_TRANSLATE_lblLinkViewPull
            // 
            this._NO_TRANSLATE_lblLinkViewPull.AutoSize = true;
            this._NO_TRANSLATE_lblLinkViewPull.Location = new System.Drawing.Point(17, 382);
            this._NO_TRANSLATE_lblLinkViewPull.Name = "_NO_TRANSLATE_lblLinkViewPull";
            this._NO_TRANSLATE_lblLinkViewPull.Size = new System.Drawing.Size(55, 13);
            this._NO_TRANSLATE_lblLinkViewPull.TabIndex = 15;
            this._NO_TRANSLATE_lblLinkViewPull.TabStop = true;
            this._NO_TRANSLATE_lblLinkViewPull.Text = "linkLabel1";
            this._NO_TRANSLATE_lblLinkViewPull.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.textLinkLabel_LinkClicked);
            // 
            // txtPRDescription
            // 
            this.txtPRDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPRDescription.Location = new System.Drawing.Point(131, 224);
            this.txtPRDescription.Multiline = true;
            this.txtPRDescription.Name = "txtPRDescription";
            this.txtPRDescription.ReadOnly = true;
            this.txtPRDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPRDescription.Size = new System.Drawing.Size(594, 138);
            this.txtPRDescription.TabIndex = 14;
            // 
            // txtPRTitle
            // 
            this.txtPRTitle.Location = new System.Drawing.Point(131, 192);
            this.txtPRTitle.Name = "txtPRTitle";
            this.txtPRTitle.ReadOnly = true;
            this.txtPRTitle.Size = new System.Drawing.Size(593, 20);
            this.txtPRTitle.TabIndex = 13;
            // 
            // txtPRReviewers
            // 
            this.txtPRReviewers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPRReviewers.Location = new System.Drawing.Point(128, 38);
            this.txtPRReviewers.Multiline = true;
            this.txtPRReviewers.Name = "txtPRReviewers";
            this.txtPRReviewers.ReadOnly = true;
            this.txtPRReviewers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPRReviewers.Size = new System.Drawing.Size(594, 138);
            this.txtPRReviewers.TabIndex = 12;
            // 
            // lblReviewerView
            // 
            this.lblReviewerView.AutoSize = true;
            this.lblReviewerView.Location = new System.Drawing.Point(15, 38);
            this.lblReviewerView.Name = "lblReviewerView";
            this.lblReviewerView.Size = new System.Drawing.Size(106, 13);
            this.lblReviewerView.TabIndex = 4;
            this.lblReviewerView.Text = "Reviewer (approved)";
            // 
            // lblDescriptionView
            // 
            this.lblDescriptionView.AutoSize = true;
            this.lblDescriptionView.Location = new System.Drawing.Point(14, 224);
            this.lblDescriptionView.Name = "lblDescriptionView";
            this.lblDescriptionView.Size = new System.Drawing.Size(60, 13);
            this.lblDescriptionView.TabIndex = 6;
            this.lblDescriptionView.Text = "Description";
            // 
            // lblTitleView
            // 
            this.lblTitleView.AutoSize = true;
            this.lblTitleView.Location = new System.Drawing.Point(15, 192);
            this.lblTitleView.Name = "lblTitleView";
            this.lblTitleView.Size = new System.Drawing.Size(27, 13);
            this.lblTitleView.TabIndex = 11;
            this.lblTitleView.Text = "Title";
            // 
            // splitContainerView2
            // 
            this.splitContainerView2.Location = new System.Drawing.Point(6, 92);
            this.splitContainerView2.Name = "splitContainerView2";
            // 
            // splitContainerView2.Panel1
            // 
            this.splitContainerView2.Panel1.Controls.Add(this.groupBoxSourceRepoView);
            // 
            // splitContainerView2.Panel2
            // 
            this.splitContainerView2.Panel2.Controls.Add(this.groupBoxTargetRepoView);
            this.splitContainerView2.Size = new System.Drawing.Size(776, 83);
            this.splitContainerView2.SplitterDistance = 382;
            this.splitContainerView2.TabIndex = 2;
            // 
            // groupBoxSourceRepoView
            // 
            this.groupBoxSourceRepoView.Controls.Add(this.lblPRSourceBranch);
            this.groupBoxSourceRepoView.Controls.Add(this.lblPRSourceRepo);
            this.groupBoxSourceRepoView.Controls.Add(this.lblSourceRepoView);
            this.groupBoxSourceRepoView.Controls.Add(this.lblTargetRepoView);
            this.groupBoxSourceRepoView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSourceRepoView.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSourceRepoView.Name = "groupBoxSourceRepoView";
            this.groupBoxSourceRepoView.Size = new System.Drawing.Size(382, 83);
            this.groupBoxSourceRepoView.TabIndex = 0;
            this.groupBoxSourceRepoView.TabStop = false;
            this.groupBoxSourceRepoView.Text = "Source";
            // 
            // lblPRSourceBranch
            // 
            this.lblPRSourceBranch.AutoSize = true;
            this.lblPRSourceBranch.Location = new System.Drawing.Point(128, 55);
            this.lblPRSourceBranch.Name = "lblPRSourceBranch";
            this.lblPRSourceBranch.Size = new System.Drawing.Size(0, 13);
            this.lblPRSourceBranch.TabIndex = 2;
            // 
            // lblPRSourceRepo
            // 
            this.lblPRSourceRepo.AutoSize = true;
            this.lblPRSourceRepo.Location = new System.Drawing.Point(128, 25);
            this.lblPRSourceRepo.Name = "lblPRSourceRepo";
            this.lblPRSourceRepo.Size = new System.Drawing.Size(0, 13);
            this.lblPRSourceRepo.TabIndex = 1;
            // 
            // lblSourceRepoView
            // 
            this.lblSourceRepoView.AutoSize = true;
            this.lblSourceRepoView.Location = new System.Drawing.Point(14, 25);
            this.lblSourceRepoView.Name = "lblSourceRepoView";
            this.lblSourceRepoView.Size = new System.Drawing.Size(57, 13);
            this.lblSourceRepoView.TabIndex = 0;
            this.lblSourceRepoView.Text = "Repository";
            // 
            // lblTargetRepoView
            // 
            this.lblTargetRepoView.AutoSize = true;
            this.lblTargetRepoView.Location = new System.Drawing.Point(14, 55);
            this.lblTargetRepoView.Name = "lblTargetRepoView";
            this.lblTargetRepoView.Size = new System.Drawing.Size(41, 13);
            this.lblTargetRepoView.TabIndex = 0;
            this.lblTargetRepoView.Text = "Branch";
            // 
            // groupBoxTargetRepoView
            // 
            this.groupBoxTargetRepoView.Controls.Add(this.lblPRDestBranch);
            this.groupBoxTargetRepoView.Controls.Add(this.lblRepositoryView);
            this.groupBoxTargetRepoView.Controls.Add(this.lblPRDestRepo);
            this.groupBoxTargetRepoView.Controls.Add(this.lblBranchView);
            this.groupBoxTargetRepoView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTargetRepoView.Location = new System.Drawing.Point(0, 0);
            this.groupBoxTargetRepoView.Name = "groupBoxTargetRepoView";
            this.groupBoxTargetRepoView.Size = new System.Drawing.Size(390, 83);
            this.groupBoxTargetRepoView.TabIndex = 0;
            this.groupBoxTargetRepoView.TabStop = false;
            this.groupBoxTargetRepoView.Text = "Target";
            // 
            // lblPRDestBranch
            // 
            this.lblPRDestBranch.AutoSize = true;
            this.lblPRDestBranch.Location = new System.Drawing.Point(130, 55);
            this.lblPRDestBranch.Name = "lblPRDestBranch";
            this.lblPRDestBranch.Size = new System.Drawing.Size(0, 13);
            this.lblPRDestBranch.TabIndex = 6;
            // 
            // lblRepositoryView
            // 
            this.lblRepositoryView.AutoSize = true;
            this.lblRepositoryView.Location = new System.Drawing.Point(16, 25);
            this.lblRepositoryView.Name = "lblRepositoryView";
            this.lblRepositoryView.Size = new System.Drawing.Size(57, 13);
            this.lblRepositoryView.TabIndex = 3;
            this.lblRepositoryView.Text = "Repository";
            // 
            // lblPRDestRepo
            // 
            this.lblPRDestRepo.AutoSize = true;
            this.lblPRDestRepo.Location = new System.Drawing.Point(130, 25);
            this.lblPRDestRepo.Name = "lblPRDestRepo";
            this.lblPRDestRepo.Size = new System.Drawing.Size(0, 13);
            this.lblPRDestRepo.TabIndex = 5;
            // 
            // lblBranchView
            // 
            this.lblBranchView.AutoSize = true;
            this.lblBranchView.Location = new System.Drawing.Point(16, 55);
            this.lblBranchView.Name = "lblBranchView";
            this.lblBranchView.Size = new System.Drawing.Size(41, 13);
            this.lblBranchView.TabIndex = 4;
            this.lblBranchView.Text = "Branch";
            // 
            // tabCreate
            // 
            this.tabCreate.Controls.Add(this.splitContainerCreate);
            this.tabCreate.Controls.Add(this.btnCreate);
            this.tabCreate.Controls.Add(this.groupBoxCreatePullRequest);
            this.tabCreate.Location = new System.Drawing.Point(4, 22);
            this.tabCreate.Name = "tabCreate";
            this.tabCreate.Padding = new System.Windows.Forms.Padding(3);
            this.tabCreate.Size = new System.Drawing.Size(788, 618);
            this.tabCreate.TabIndex = 0;
            this.tabCreate.Text = "Create Pull Request";
            this.tabCreate.UseVisualStyleBackColor = true;
            // 
            // BitbucketPullRequestForm
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(820, 668);
            this.Controls.Add(this.tabControl1);
            this.Name = "BitbucketPullRequestForm";
            this.Text = "Bitbucket Server";
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            this.groupBoxTarget.ResumeLayout(false);
            this.groupBoxTarget.PerformLayout();
            this.groupBoxCreatePullRequest.ResumeLayout(false);
            this.groupBoxCreatePullRequest.PerformLayout();
            this.splitContainerCreate.Panel1.ResumeLayout(false);
            this.splitContainerCreate.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCreate)).EndInit();
            this.splitContainerCreate.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabView.ResumeLayout(false);
            this.splitContainerView1.Panel1.ResumeLayout(false);
            this.splitContainerView1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerView1)).EndInit();
            this.splitContainerView1.ResumeLayout(false);
            this.groupBoxInformation.ResumeLayout(false);
            this.groupBoxInformation.PerformLayout();
            this.groupBoxPullRequestInfo.ResumeLayout(false);
            this.groupBoxPullRequestInfo.PerformLayout();
            this.splitContainerView2.Panel1.ResumeLayout(false);
            this.splitContainerView2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerView2)).EndInit();
            this.splitContainerView2.ResumeLayout(false);
            this.groupBoxSourceRepoView.ResumeLayout(false);
            this.groupBoxSourceRepoView.PerformLayout();
            this.groupBoxTargetRepoView.ResumeLayout(false);
            this.groupBoxTargetRepoView.PerformLayout();
            this.tabCreate.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblSourceBranch;
        private System.Windows.Forms.Label lblTargetBranch;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSourceRepository;
        private System.Windows.Forms.GroupBox groupBoxSource;
        private System.Windows.Forms.GroupBox groupBoxTarget;
        private System.Windows.Forms.Label lblTargetRepository;
        private System.Windows.Forms.GroupBox groupBoxCreatePullRequest;
        private System.Windows.Forms.Label lblCommitInfoSource;
        private System.Windows.Forms.Label lblCommitInfoTarget;
        private System.Windows.Forms.ComboBox ddlRepositorySource;
        private System.Windows.Forms.ComboBox ddlBranchSource;
        private System.Windows.Forms.ComboBox ddlBranchTarget;
        private System.Windows.Forms.ComboBox ddlRepositoryTarget;
        private System.Windows.Forms.SplitContainer splitContainerCreate;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCreate;
        private System.Windows.Forms.TabPage tabView;
        private System.Windows.Forms.ListBox lbxPullRequests;
        private System.Windows.Forms.SplitContainer splitContainerView2;
        private System.Windows.Forms.GroupBox groupBoxSourceRepoView;
        private System.Windows.Forms.Label lblSourceRepoView;
        private System.Windows.Forms.Label lblTargetRepoView;
        private System.Windows.Forms.GroupBox groupBoxTargetRepoView;
        private System.Windows.Forms.GroupBox groupBoxPullRequestInfo;
        private System.Windows.Forms.TextBox txtPRDescription;
        private System.Windows.Forms.TextBox txtPRTitle;
        private System.Windows.Forms.TextBox txtPRReviewers;
        private System.Windows.Forms.Label lblReviewerView;
        private System.Windows.Forms.Label lblDescriptionView;
        private System.Windows.Forms.Label lblTitleView;
        private System.Windows.Forms.Label lblPRSourceBranch;
        private System.Windows.Forms.Label lblPRSourceRepo;
        private System.Windows.Forms.Label lblPRDestBranch;
        private System.Windows.Forms.Label lblRepositoryView;
        private System.Windows.Forms.Label lblPRDestRepo;
        private System.Windows.Forms.Label lblBranchView;
        private System.Windows.Forms.Label lblPRAuthor;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.SplitContainer splitContainerView1;
        private System.Windows.Forms.GroupBox groupBoxInformation;
        private System.Windows.Forms.Label lblPRState;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_lblLinkCreatePull;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_lblLinkViewPull;
        private System.Windows.Forms.ToolTip toolTipLink;
    }
}