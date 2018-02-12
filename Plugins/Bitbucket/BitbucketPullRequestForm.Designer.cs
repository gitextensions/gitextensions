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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblReviewers = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.ReviewersDataGrid = new System.Windows.Forms.DataGridView();
            this.GridColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ddlRepositorySource = new System.Windows.Forms.ComboBox();
            this.ddlBranchSource = new System.Windows.Forms.ComboBox();
            this.lblCommitInfoSource = new System.Windows.Forms.Label();
            this.ddlBranchTarget = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ddlRepositoryTarget = new System.Windows.Forms.ComboBox();
            this.lblCommitInfoTarget = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabCreate = new System.Windows.Forms.TabPage();
            this.tabView = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.lbxPullRequests = new System.Windows.Forms.ListBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.lblPRState = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.lblPRAuthor = new System.Windows.Forms.Label();
            this.btnMerge = new System.Windows.Forms.Button();
            this.btnApprove = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.txtPRDescription = new System.Windows.Forms.TextBox();
            this.txtPRTitle = new System.Windows.Forms.TextBox();
            this.txtPRReviewers = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblPRSourceBranch = new System.Windows.Forms.Label();
            this.lblPRSourceRepo = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblPRDestBranch = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lblPRDestRepo = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ReviewersDataGrid)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabCreate.SuspendLayout();
            this.tabView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Branch";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Branch";
            // 
            // lblReviewers
            // 
            this.lblReviewers.AutoSize = true;
            this.lblReviewers.Location = new System.Drawing.Point(15, 38);
            this.lblReviewers.Name = "lblReviewers";
            this.lblReviewers.Size = new System.Drawing.Size(65, 13);
            this.lblReviewers.TabIndex = 4;
            this.lblReviewers.Text = "Reviewer(s)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 224);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Description";
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
            this.btnCreate.Location = new System.Drawing.Point(648, 588);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(134, 23);
            this.btnCreate.TabIndex = 2;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.BtnCreateClick);
            // 
            // ReviewersDataGrid
            // 
            this.ReviewersDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReviewersDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ReviewersDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GridColumnName});
            this.ReviewersDataGrid.Location = new System.Drawing.Point(129, 38);
            this.ReviewersDataGrid.Name = "ReviewersDataGrid";
            this.ReviewersDataGrid.Size = new System.Drawing.Size(593, 137);
            this.ReviewersDataGrid.TabIndex = 0;
            this.ReviewersDataGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.ReviewersDataGridEditingControlShowing);
            // 
            // GridColumnName
            // 
            this.GridColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.GridColumnName.DataPropertyName = "Slug";
            this.GridColumnName.HeaderText = "Name";
            this.GridColumnName.Name = "GridColumnName";
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtTitle.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtTitle.Location = new System.Drawing.Point(129, 189);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(593, 21);
            this.txtTitle.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Title";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Repository";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ddlRepositorySource);
            this.groupBox1.Controls.Add(this.ddlBranchSource);
            this.groupBox1.Controls.Add(this.lblCommitInfoSource);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(382, 169);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // ddlRepositorySource
            // 
            this.ddlRepositorySource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlRepositorySource.DisplayMember = "DisplayName";
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
            this.ddlBranchSource.DisplayMember = "DisplayName";
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
            this.ddlBranchTarget.DisplayMember = "DisplayName";
            this.ddlBranchTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlBranchTarget.FormattingEnabled = true;
            this.ddlBranchTarget.Location = new System.Drawing.Point(127, 58);
            this.ddlBranchTarget.Name = "ddlBranchTarget";
            this.ddlBranchTarget.Size = new System.Drawing.Size(178, 21);
            this.ddlBranchTarget.TabIndex = 0;
            this.ddlBranchTarget.SelectedValueChanged += new System.EventHandler(this.DdlBranchTargetSelectedValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ddlRepositoryTarget);
            this.groupBox2.Controls.Add(this.lblCommitInfoTarget);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.ddlBranchTarget);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(390, 169);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Target";
            // 
            // ddlRepositoryTarget
            // 
            this.ddlRepositoryTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlRepositoryTarget.DisplayMember = "DisplayName";
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
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Repository";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblReviewers);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.txtDescription);
            this.groupBox3.Controls.Add(this.txtTitle);
            this.groupBox3.Controls.Add(this.ReviewersDataGrid);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(6, 181);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(776, 401);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Pull Request Info";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(6, 6);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(776, 169);
            this.splitContainer1.SplitterDistance = 382;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabCreate);
            this.tabControl1.Controls.Add(this.tabView);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(803, 643);
            this.tabControl1.TabIndex = 3;
            // 
            // tabCreate
            // 
            this.tabCreate.Controls.Add(this.splitContainer1);
            this.tabCreate.Controls.Add(this.btnCreate);
            this.tabCreate.Controls.Add(this.groupBox3);
            this.tabCreate.Location = new System.Drawing.Point(4, 22);
            this.tabCreate.Name = "tabCreate";
            this.tabCreate.Padding = new System.Windows.Forms.Padding(3);
            this.tabCreate.Size = new System.Drawing.Size(795, 617);
            this.tabCreate.TabIndex = 0;
            this.tabCreate.Text = "Create Pull Request";
            this.tabCreate.UseVisualStyleBackColor = true;
            // 
            // tabView
            // 
            this.tabView.Controls.Add(this.splitContainer3);
            this.tabView.Controls.Add(this.btnMerge);
            this.tabView.Controls.Add(this.btnApprove);
            this.tabView.Controls.Add(this.groupBox6);
            this.tabView.Controls.Add(this.splitContainer2);
            this.tabView.Location = new System.Drawing.Point(4, 22);
            this.tabView.Name = "tabView";
            this.tabView.Padding = new System.Windows.Forms.Padding(3);
            this.tabView.Size = new System.Drawing.Size(795, 617);
            this.tabView.TabIndex = 1;
            this.tabView.Text = "View Pull Requests";
            this.tabView.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Location = new System.Drawing.Point(6, 3);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.lbxPullRequests);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.groupBox9);
            this.splitContainer3.Size = new System.Drawing.Size(776, 83);
            this.splitContainer3.SplitterDistance = 382;
            this.splitContainer3.TabIndex = 8;
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
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.lblPRState);
            this.groupBox9.Controls.Add(this.label12);
            this.groupBox9.Controls.Add(this.label16);
            this.groupBox9.Controls.Add(this.lblPRAuthor);
            this.groupBox9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox9.Location = new System.Drawing.Point(0, 0);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(390, 83);
            this.groupBox9.TabIndex = 0;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Information";
            // 
            // lblPRState
            // 
            this.lblPRState.AutoSize = true;
            this.lblPRState.Location = new System.Drawing.Point(130, 53);
            this.lblPRState.Name = "lblPRState";
            this.lblPRState.Size = new System.Drawing.Size(0, 13);
            this.lblPRState.TabIndex = 10;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 23);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(40, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "Author";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(16, 53);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(33, 13);
            this.label16.TabIndex = 8;
            this.label16.Text = "State";
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
            this.btnMerge.Location = new System.Drawing.Point(648, 588);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(134, 23);
            this.btnMerge.TabIndex = 7;
            this.btnMerge.Text = "Merge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.BtnMergeClick);
            // 
            // btnApprove
            // 
            this.btnApprove.Location = new System.Drawing.Point(508, 588);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(134, 23);
            this.btnApprove.TabIndex = 7;
            this.btnApprove.Text = "Approve";
            this.btnApprove.UseVisualStyleBackColor = true;
            this.btnApprove.Click += new System.EventHandler(this.BtnApproveClick);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.txtPRDescription);
            this.groupBox6.Controls.Add(this.txtPRTitle);
            this.groupBox6.Controls.Add(this.txtPRReviewers);
            this.groupBox6.Controls.Add(this.label10);
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.label14);
            this.groupBox6.Location = new System.Drawing.Point(6, 181);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(776, 401);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Pull Request Info";
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
            this.txtPRTitle.Size = new System.Drawing.Size(593, 21);
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
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(15, 38);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(109, 13);
            this.label10.TabIndex = 4;
            this.label10.Text = "Reviewer (approved)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(14, 224);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 6;
            this.label13.Text = "Description";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(15, 192);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(27, 13);
            this.label14.TabIndex = 11;
            this.label14.Text = "Title";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Location = new System.Drawing.Point(6, 92);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox4);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox5);
            this.splitContainer2.Size = new System.Drawing.Size(776, 83);
            this.splitContainer2.SplitterDistance = 382;
            this.splitContainer2.TabIndex = 2;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblPRSourceBranch);
            this.groupBox4.Controls.Add(this.lblPRSourceRepo);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(382, 83);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Source";
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
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Repository";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(14, 55);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Branch";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lblPRDestBranch);
            this.groupBox5.Controls.Add(this.label17);
            this.groupBox5.Controls.Add(this.lblPRDestRepo);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(0, 0);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(390, 83);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Target";
            // 
            // lblPRDestBranch
            // 
            this.lblPRDestBranch.AutoSize = true;
            this.lblPRDestBranch.Location = new System.Drawing.Point(130, 55);
            this.lblPRDestBranch.Name = "lblPRDestBranch";
            this.lblPRDestBranch.Size = new System.Drawing.Size(0, 13);
            this.lblPRDestBranch.TabIndex = 6;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(16, 25);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(59, 13);
            this.label17.TabIndex = 3;
            this.label17.Text = "Repository";
            // 
            // lblPRDestRepo
            // 
            this.lblPRDestRepo.AutoSize = true;
            this.lblPRDestRepo.Location = new System.Drawing.Point(130, 25);
            this.lblPRDestRepo.Name = "lblPRDestRepo";
            this.lblPRDestRepo.Size = new System.Drawing.Size(0, 13);
            this.lblPRDestRepo.TabIndex = 5;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(16, 55);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(40, 13);
            this.label18.TabIndex = 4;
            this.label18.Text = "Branch";
            // 
            // BitbucketPullRequestForm
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(827, 667);
            this.Controls.Add(this.tabControl1);
            this.Name = "BitbucketPullRequestForm";
            this.Text = "Create Pull Request";
            this.Load += BitbucketPullRequestFormLoad;
            this.Load += BitbucketViewPullRequestFormLoad;
            ((System.ComponentModel.ISupportInitialize)(this.ReviewersDataGrid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabCreate.ResumeLayout(false);
            this.tabView.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblReviewers;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.DataGridView ReviewersDataGrid;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridColumnName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblCommitInfoSource;
        private System.Windows.Forms.Label lblCommitInfoTarget;
        private System.Windows.Forms.ComboBox ddlRepositorySource;
        private System.Windows.Forms.ComboBox ddlBranchSource;
        private System.Windows.Forms.ComboBox ddlBranchTarget;
        private System.Windows.Forms.ComboBox ddlRepositoryTarget;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCreate;
        private System.Windows.Forms.TabPage tabView;
        private System.Windows.Forms.ListBox lbxPullRequests;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox txtPRDescription;
        private System.Windows.Forms.TextBox txtPRTitle;
        private System.Windows.Forms.TextBox txtPRReviewers;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblPRSourceBranch;
        private System.Windows.Forms.Label lblPRSourceRepo;
        private System.Windows.Forms.Label lblPRDestBranch;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblPRDestRepo;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lblPRAuthor;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Label lblPRState;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label16;
    }
}