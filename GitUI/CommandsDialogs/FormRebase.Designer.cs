namespace GitUI.CommandsDialogs
{
    partial class FormRebase
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
            FlowLayoutPanel flowLayoutPanel1;
            FlowLayoutPanel flowLayoutPanel2;
            FlowLayoutPanel PanelCurrentBranch;
            btnAddFiles = new Button();
            btnCommit = new Button();
            btnEditTodo = new Button();
            btnSkip = new Button();
            lblCurrent = new Label();
            Currentbranch = new Label();
            lblRebase = new Label();
            cboBranches = new ComboBox();
            label2 = new Label();
            btnContinueRebase = new Button();
            btnAbort = new Button();
            btnSolveConflicts = new Button();
            flpnlOptionsPanelTop = new FlowLayoutPanel();
            chkInteractive = new CheckBox();
            chkPreserveMerges = new CheckBox();
            chkAutosquash = new CheckBox();
            chkStash = new CheckBox();
            chkIgnoreDate = new CheckBox();
            chkCommitterDateIsAuthorDate = new CheckBox();
            checkBoxUpdateRefs = new CheckBox();
            flpnlOptionsPanelBottom = new FlowLayoutPanel();
            chkSpecificRange = new CheckBox();
            lblRangeFrom = new Label();
            txtFrom = new TextBox();
            btnChooseFromRevision = new Button();
            lblRangeTo = new Label();
            cboTo = new ComboBox();
            llblShowOptions = new LinkLabel();
            PatchGrid = new PatchGrid();
            lblCommitsToReapply = new Label();
            btnSolveMergeconflicts = new Button();
            MergeToolPanel = new Panel();
            MainLayout = new TableLayoutPanel();
            PanelLeftImage = new Help.HelpImageDisplayUserControl();
            PanelMiddle = new TableLayoutPanel();
            rebasePanel = new FlowLayoutPanel();
            tlpnlSecondaryControls = new TableLayoutPanel();
            btnRebase = new Button();
            toolTip1 = new ToolTip(components);
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            PanelCurrentBranch = new FlowLayoutPanel();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            PanelCurrentBranch.SuspendLayout();
            flpnlOptionsPanelTop.SuspendLayout();
            flpnlOptionsPanelBottom.SuspendLayout();
            MergeToolPanel.SuspendLayout();
            MainLayout.SuspendLayout();
            PanelMiddle.SuspendLayout();
            rebasePanel.SuspendLayout();
            tlpnlSecondaryControls.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(MainLayout);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(1034, 420);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(btnAbort);
            ControlsPanel.Controls.Add(btnContinueRebase);
            ControlsPanel.Controls.Add(MergeToolPanel);
            ControlsPanel.Controls.Add(btnRebase);
            ControlsPanel.Location = new Point(0, 420);
            ControlsPanel.Size = new Size(1034, 41);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(btnAddFiles);
            flowLayoutPanel1.Controls.Add(btnCommit);
            flowLayoutPanel1.Controls.Add(btnEditTodo);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(348, 31);
            flowLayoutPanel1.TabIndex = 27;
            flowLayoutPanel1.WrapContents = false;
            // 
            // btnAddFiles
            // 
            btnAddFiles.AutoSize = true;
            btnAddFiles.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAddFiles.Image = Properties.Images.BulletAdd;
            btnAddFiles.Location = new Point(3, 3);
            btnAddFiles.MinimumSize = new Size(79, 25);
            btnAddFiles.Name = "btnAddFiles";
            btnAddFiles.Size = new Size(79, 25);
            btnAddFiles.TabIndex = 34;
            btnAddFiles.Text = "&Add files";
            btnAddFiles.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAddFiles.UseVisualStyleBackColor = true;
            btnAddFiles.Click += AddFilesClick;
            // 
            // btnCommit
            // 
            btnCommit.AutoSize = true;
            btnCommit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCommit.Image = Properties.Images.RepoStateStaged;
            btnCommit.Location = new Point(88, 3);
            btnCommit.MinimumSize = new Size(86, 25);
            btnCommit.Name = "btnCommit";
            btnCommit.Size = new Size(86, 25);
            btnCommit.TabIndex = 35;
            btnCommit.Text = "C&ommit...";
            btnCommit.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCommit.UseVisualStyleBackColor = true;
            btnCommit.Click += Commit_Click;
            // 
            // btnEditTodo
            // 
            btnEditTodo.AutoSize = true;
            btnEditTodo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnEditTodo.Image = Properties.Images.EditFile;
            btnEditTodo.Location = new Point(180, 3);
            btnEditTodo.MinimumSize = new Size(90, 25);
            btnEditTodo.Name = "btnEditTodo";
            btnEditTodo.Size = new Size(90, 25);
            btnEditTodo.TabIndex = 36;
            btnEditTodo.Text = "&Edit todo...";
            btnEditTodo.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnEditTodo.UseVisualStyleBackColor = true;
            btnEditTodo.Click += EditTodoClick;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.Controls.Add(btnSkip);
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new Point(357, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(349, 31);
            flowLayoutPanel2.TabIndex = 28;
            flowLayoutPanel2.WrapContents = false;
            // 
            // btnSkip
            // 
            btnSkip.AutoSize = true;
            btnSkip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSkip.Location = new Point(163, 3);
            btnSkip.MinimumSize = new Size(183, 25);
            btnSkip.Name = "btnSkip";
            btnSkip.Size = new Size(183, 25);
            btnSkip.TabIndex = 39;
            btnSkip.Text = "S&kip currently applying commit";
            btnSkip.UseVisualStyleBackColor = true;
            btnSkip.Click += SkipClick;
            // 
            // PanelCurrentBranch
            // 
            PanelCurrentBranch.AutoSize = true;
            PanelCurrentBranch.Controls.Add(lblCurrent);
            PanelCurrentBranch.Controls.Add(Currentbranch);
            PanelCurrentBranch.Dock = DockStyle.Fill;
            PanelCurrentBranch.Location = new Point(0, 15);
            PanelCurrentBranch.Margin = new Padding(0);
            PanelCurrentBranch.Name = "PanelCurrentBranch";
            PanelCurrentBranch.Padding = new Padding(0, 5, 0, 5);
            PanelCurrentBranch.Size = new Size(715, 25);
            PanelCurrentBranch.TabIndex = 4;
            PanelCurrentBranch.WrapContents = false;
            // 
            // lblCurrent
            // 
            lblCurrent.AutoSize = true;
            lblCurrent.Dock = DockStyle.Fill;
            lblCurrent.Location = new Point(3, 5);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.Size = new Size(90, 15);
            lblCurrent.TabIndex = 5;
            lblCurrent.Text = "Current branch:";
            // 
            // Currentbranch
            // 
            Currentbranch.Anchor = AnchorStyles.Left;
            Currentbranch.AutoSize = true;
            Currentbranch.Location = new Point(99, 5);
            Currentbranch.Name = "Currentbranch";
            Currentbranch.Size = new Size(0, 15);
            Currentbranch.TabIndex = 3;
            // 
            // lblRebase
            // 
            lblRebase.AutoSize = true;
            lblRebase.Dock = DockStyle.Fill;
            lblRebase.Location = new Point(3, 0);
            lblRebase.Name = "lblRebase";
            lblRebase.Size = new Size(709, 15);
            lblRebase.TabIndex = 3;
            lblRebase.Text = "Rebase current branch on top of another branch";
            // 
            // cboBranches
            // 
            cboBranches.Anchor = AnchorStyles.Left;
            cboBranches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboBranches.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboBranches.FormattingEnabled = true;
            cboBranches.Location = new Point(70, 3);
            cboBranches.Name = "cboBranches";
            cboBranches.Size = new Size(270, 23);
            cboBranches.TabIndex = 8;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 7);
            label2.Name = "label2";
            label2.Size = new Size(61, 15);
            label2.TabIndex = 7;
            label2.Text = "&Rebase on";
            // 
            // btnContinueRebase
            // 
            btnContinueRebase.AutoSize = true;
            btnContinueRebase.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnContinueRebase.Location = new Point(837, 8);
            btnContinueRebase.MinimumSize = new Size(103, 25);
            btnContinueRebase.Name = "btnContinueRebase";
            btnContinueRebase.Size = new Size(103, 25);
            btnContinueRebase.TabIndex = 38;
            btnContinueRebase.Text = "&Continue rebase";
            btnContinueRebase.UseVisualStyleBackColor = true;
            btnContinueRebase.Click += ResolvedClick;
            // 
            // btnAbort
            // 
            btnAbort.AutoSize = true;
            btnAbort.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAbort.Location = new Point(946, 8);
            btnAbort.MinimumSize = new Size(75, 25);
            btnAbort.Name = "btnAbort";
            btnAbort.Size = new Size(75, 25);
            btnAbort.TabIndex = 40;
            btnAbort.Text = "A&bort";
            btnAbort.UseVisualStyleBackColor = true;
            btnAbort.Click += AbortClick;
            // 
            // btnSolveConflicts
            // 
            btnSolveConflicts.AutoSize = true;
            btnSolveConflicts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSolveConflicts.Location = new Point(0, 2);
            btnSolveConflicts.Margin = new Padding(1);
            btnSolveConflicts.MinimumSize = new Size(100, 25);
            btnSolveConflicts.Name = "btnSolveConflicts";
            btnSolveConflicts.Size = new Size(100, 25);
            btnSolveConflicts.TabIndex = 32;
            btnSolveConflicts.Text = "&Solve conflicts";
            btnSolveConflicts.UseVisualStyleBackColor = true;
            btnSolveConflicts.Click += MergetoolClick;
            // 
            // flpnlOptionsPanelTop
            // 
            flpnlOptionsPanelTop.AutoSize = true;
            flpnlOptionsPanelTop.Controls.Add(chkInteractive);
            flpnlOptionsPanelTop.Controls.Add(chkPreserveMerges);
            flpnlOptionsPanelTop.Controls.Add(chkAutosquash);
            flpnlOptionsPanelTop.Controls.Add(chkStash);
            flpnlOptionsPanelTop.Controls.Add(chkIgnoreDate);
            flpnlOptionsPanelTop.Controls.Add(chkCommitterDateIsAuthorDate);
            flpnlOptionsPanelTop.Controls.Add(checkBoxUpdateRefs);
            flpnlOptionsPanelTop.Dock = DockStyle.Fill;
            flpnlOptionsPanelTop.Location = new Point(3, 78);
            flpnlOptionsPanelTop.Name = "flpnlOptionsPanelTop";
            flpnlOptionsPanelTop.Size = new Size(709, 50);
            flpnlOptionsPanelTop.TabIndex = 11;
            // 
            // chkInteractive
            // 
            chkInteractive.Anchor = AnchorStyles.Left;
            chkInteractive.AutoSize = true;
            chkInteractive.Location = new Point(3, 3);
            chkInteractive.Name = "chkInteractive";
            chkInteractive.Size = new Size(121, 19);
            chkInteractive.TabIndex = 12;
            chkInteractive.Text = "&Interactive Rebase";
            chkInteractive.UseVisualStyleBackColor = true;
            chkInteractive.CheckedChanged += chkInteractive_CheckedChanged;
            // 
            // chkPreserveMerges
            // 
            chkPreserveMerges.Anchor = AnchorStyles.Left;
            chkPreserveMerges.AutoSize = true;
            chkPreserveMerges.Location = new Point(130, 3);
            chkPreserveMerges.Name = "chkPreserveMerges";
            chkPreserveMerges.Size = new Size(112, 19);
            chkPreserveMerges.TabIndex = 13;
            chkPreserveMerges.Text = "&Preserve Merges";
            chkPreserveMerges.UseVisualStyleBackColor = true;
            // 
            // chkAutosquash
            // 
            chkAutosquash.Anchor = AnchorStyles.Left;
            chkAutosquash.AutoSize = true;
            chkAutosquash.Enabled = false;
            chkAutosquash.Location = new Point(248, 3);
            chkAutosquash.Name = "chkAutosquash";
            chkAutosquash.Size = new Size(89, 19);
            chkAutosquash.TabIndex = 14;
            chkAutosquash.Text = "Autos&quash";
            chkAutosquash.UseVisualStyleBackColor = true;
            // 
            // chkStash
            // 
            chkStash.Anchor = AnchorStyles.Left;
            chkStash.AutoSize = true;
            chkStash.Enabled = false;
            chkStash.Location = new Point(343, 3);
            chkStash.Name = "chkStash";
            chkStash.Size = new Size(82, 19);
            chkStash.TabIndex = 15;
            chkStash.Text = "A&uto stash";
            chkStash.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreDate
            // 
            chkIgnoreDate.Anchor = AnchorStyles.Left;
            chkIgnoreDate.AutoSize = true;
            chkIgnoreDate.Location = new Point(431, 3);
            chkIgnoreDate.Name = "chkIgnoreDate";
            chkIgnoreDate.Size = new Size(86, 19);
            chkIgnoreDate.TabIndex = 16;
            chkIgnoreDate.Text = "Ignore &date";
            toolTip1.SetToolTip(chkIgnoreDate, "Sets the author date to the current date (same as\r\ncommit date), ignoring the original author date.");
            chkIgnoreDate.UseVisualStyleBackColor = true;
            chkIgnoreDate.CheckedChanged += chkIgnoreDate_CheckedChanged;
            // 
            // chkCommitterDateIsAuthorDate
            // 
            chkCommitterDateIsAuthorDate.Anchor = AnchorStyles.Left;
            chkCommitterDateIsAuthorDate.AutoSize = true;
            chkCommitterDateIsAuthorDate.Location = new Point(3, 28);
            chkCommitterDateIsAuthorDate.Name = "chkCommitterDateIsAuthorDate";
            chkCommitterDateIsAuthorDate.Size = new Size(185, 19);
            chkCommitterDateIsAuthorDate.TabIndex = 17;
            chkCommitterDateIsAuthorDate.Text = "Co&mmitter date is author date";
            toolTip1.SetToolTip(chkCommitterDateIsAuthorDate, "Sets the commit date to the original author date\r\n(instead of the current date).");
            chkCommitterDateIsAuthorDate.UseVisualStyleBackColor = true;
            chkCommitterDateIsAuthorDate.CheckedChanged += chkCommitterDateIsAuthorDate_CheckedChanged;
            // 
            // checkBoxUpdateRefs
            // 
            checkBoxUpdateRefs.AutoSize = true;
            checkBoxUpdateRefs.Location = new Point(194, 28);
            checkBoxUpdateRefs.Name = "checkBoxUpdateRefs";
            checkBoxUpdateRefs.Size = new Size(146, 19);
            checkBoxUpdateRefs.TabIndex = 18;
            checkBoxUpdateRefs.Text = "Update dependent r&efs";
            checkBoxUpdateRefs.UseVisualStyleBackColor = true;
            // 
            // flpnlOptionsPanelBottom
            // 
            flpnlOptionsPanelBottom.AutoSize = true;
            flpnlOptionsPanelBottom.Controls.Add(chkSpecificRange);
            flpnlOptionsPanelBottom.Controls.Add(lblRangeFrom);
            flpnlOptionsPanelBottom.Controls.Add(txtFrom);
            flpnlOptionsPanelBottom.Controls.Add(btnChooseFromRevision);
            flpnlOptionsPanelBottom.Controls.Add(lblRangeTo);
            flpnlOptionsPanelBottom.Controls.Add(cboTo);
            flpnlOptionsPanelBottom.Dock = DockStyle.Fill;
            flpnlOptionsPanelBottom.Location = new Point(3, 134);
            flpnlOptionsPanelBottom.Name = "flpnlOptionsPanelBottom";
            flpnlOptionsPanelBottom.Size = new Size(709, 30);
            flpnlOptionsPanelBottom.TabIndex = 18;
            flpnlOptionsPanelBottom.WrapContents = false;
            // 
            // chkSpecificRange
            // 
            chkSpecificRange.Anchor = AnchorStyles.Left;
            chkSpecificRange.AutoSize = true;
            chkSpecificRange.Location = new Point(3, 5);
            chkSpecificRange.Name = "chkSpecificRange";
            chkSpecificRange.Size = new Size(100, 19);
            chkSpecificRange.TabIndex = 19;
            chkSpecificRange.Text = "Specific ra&nge";
            chkSpecificRange.UseVisualStyleBackColor = true;
            chkSpecificRange.CheckedChanged += chkUseFromOnto_CheckedChanged;
            // 
            // lblRangeFrom
            // 
            lblRangeFrom.Anchor = AnchorStyles.Left;
            lblRangeFrom.AutoSize = true;
            lblRangeFrom.Location = new Point(109, 7);
            lblRangeFrom.Name = "lblRangeFrom";
            lblRangeFrom.Size = new Size(67, 15);
            lblRangeFrom.TabIndex = 20;
            lblRangeFrom.Text = "&From (exc.)";
            // 
            // txtFrom
            // 
            txtFrom.Anchor = AnchorStyles.Left;
            txtFrom.Enabled = false;
            txtFrom.Location = new Point(182, 3);
            txtFrom.Name = "txtFrom";
            txtFrom.Size = new Size(80, 23);
            txtFrom.TabIndex = 21;
            // 
            // btnChooseFromRevision
            // 
            btnChooseFromRevision.Anchor = AnchorStyles.Left;
            btnChooseFromRevision.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnChooseFromRevision.Enabled = false;
            btnChooseFromRevision.Image = Properties.Images.SelectRevision;
            btnChooseFromRevision.Location = new Point(268, 3);
            btnChooseFromRevision.MinimumSize = new Size(25, 24);
            btnChooseFromRevision.Name = "btnChooseFromRevision";
            btnChooseFromRevision.Size = new Size(25, 24);
            btnChooseFromRevision.TabIndex = 22;
            btnChooseFromRevision.UseVisualStyleBackColor = true;
            btnChooseFromRevision.Click += btnChooseFromRevision_Click;
            // 
            // lblRangeTo
            // 
            lblRangeTo.Anchor = AnchorStyles.Left;
            lblRangeTo.AutoSize = true;
            lblRangeTo.Location = new Point(299, 7);
            lblRangeTo.Name = "lblRangeTo";
            lblRangeTo.Size = new Size(19, 15);
            lblRangeTo.TabIndex = 23;
            lblRangeTo.Text = "&To";
            // 
            // cboTo
            // 
            cboTo.Anchor = AnchorStyles.Left;
            cboTo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboTo.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboTo.Enabled = false;
            cboTo.FormattingEnabled = true;
            cboTo.Location = new Point(324, 3);
            cboTo.Name = "cboTo";
            cboTo.Size = new Size(184, 23);
            cboTo.TabIndex = 24;
            // 
            // llblShowOptions
            // 
            llblShowOptions.Anchor = AnchorStyles.Left;
            llblShowOptions.AutoSize = true;
            llblShowOptions.Location = new Point(346, 7);
            llblShowOptions.Name = "llblShowOptions";
            llblShowOptions.Size = new Size(79, 15);
            llblShowOptions.TabIndex = 9;
            llblShowOptions.TabStop = true;
            llblShowOptions.Text = "Show options";
            llblShowOptions.LinkClicked += ShowOptions_LinkClicked;
            // 
            // PatchGrid
            // 
            PatchGrid.AutoSize = true;
            PatchGrid.Dock = DockStyle.Fill;
            PatchGrid.IsManagingRebase = true;
            PatchGrid.Location = new Point(3, 192);
            PatchGrid.Margin = new Padding(3, 2, 3, 2);
            PatchGrid.MinimumSize = new Size(0, 100);
            PatchGrid.Name = "PatchGrid";
            PatchGrid.Size = new Size(709, 159);
            PatchGrid.TabIndex = 26;
            // 
            // lblCommitsToReapply
            // 
            lblCommitsToReapply.AutoSize = true;
            lblCommitsToReapply.Dock = DockStyle.Fill;
            lblCommitsToReapply.Location = new Point(3, 175);
            lblCommitsToReapply.Name = "lblCommitsToReapply";
            lblCommitsToReapply.Size = new Size(709, 15);
            lblCommitsToReapply.TabIndex = 25;
            lblCommitsToReapply.Text = "Commits to re-apply:";
            // 
            // btnSolveMergeconflicts
            // 
            btnSolveMergeconflicts.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSolveMergeconflicts.AutoSize = true;
            btnSolveMergeconflicts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSolveMergeconflicts.BackColor = Color.Salmon;
            btnSolveMergeconflicts.FlatStyle = FlatStyle.Flat;
            btnSolveMergeconflicts.Location = new Point(12, 426);
            btnSolveMergeconflicts.MinimumSize = new Size(213, 27);
            btnSolveMergeconflicts.Name = "btnSolveMergeconflicts";
            btnSolveMergeconflicts.Size = new Size(213, 27);
            btnSolveMergeconflicts.TabIndex = 42;
            btnSolveMergeconflicts.Text = "There are unresolved merge conflicts\r\n";
            btnSolveMergeconflicts.UseVisualStyleBackColor = false;
            btnSolveMergeconflicts.Visible = false;
            btnSolveMergeconflicts.Click += SolveMergeConflictsClick;
            // 
            // MergeToolPanel
            // 
            MergeToolPanel.AutoSize = true;
            MergeToolPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MergeToolPanel.BackColor = SystemColors.ActiveCaption;
            MergeToolPanel.Controls.Add(btnSolveConflicts);
            MergeToolPanel.Location = new Point(732, 6);
            MergeToolPanel.Margin = new Padding(1);
            MergeToolPanel.Name = "MergeToolPanel";
            MergeToolPanel.Size = new Size(101, 28);
            MergeToolPanel.TabIndex = 31;
            // 
            // MainLayout
            // 
            MainLayout.ColumnCount = 2;
            MainLayout.ColumnStyles.Add(new ColumnStyle());
            MainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            MainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            MainLayout.Controls.Add(PanelLeftImage, 0, 0);
            MainLayout.Controls.Add(PanelMiddle, 1, 0);
            MainLayout.Dock = DockStyle.Fill;
            MainLayout.Location = new Point(9, 9);
            MainLayout.Name = "MainLayout";
            MainLayout.RowCount = 1;
            MainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            MainLayout.Size = new Size(1016, 402);
            MainLayout.TabIndex = 0;
            // 
            // PanelLeftImage
            // 
            PanelLeftImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PanelLeftImage.AutoSize = true;
            PanelLeftImage.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PanelLeftImage.Image1 = Properties.Images.HelpCommandRebase;
            PanelLeftImage.Image2 = null;
            PanelLeftImage.IsExpanded = true;
            PanelLeftImage.IsOnHoverShowImage2 = false;
            PanelLeftImage.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            PanelLeftImage.Location = new Point(3, 3);
            PanelLeftImage.MinimumSize = new Size(289, 418);
            PanelLeftImage.Name = "PanelLeftImage";
            PanelLeftImage.Size = new Size(289, 418);
            PanelLeftImage.TabIndex = 1;
            PanelLeftImage.UniqueIsExpandedSettingsId = "Rebase";
            // 
            // PanelMiddle
            // 
            PanelMiddle.ColumnCount = 1;
            PanelMiddle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            PanelMiddle.Controls.Add(lblRebase, 0, 0);
            PanelMiddle.Controls.Add(PanelCurrentBranch, 0, 1);
            PanelMiddle.Controls.Add(rebasePanel, 0, 2);
            PanelMiddle.Controls.Add(flpnlOptionsPanelTop, 0, 3);
            PanelMiddle.Controls.Add(flpnlOptionsPanelBottom, 0, 4);
            PanelMiddle.Controls.Add(lblCommitsToReapply, 0, 6);
            PanelMiddle.Controls.Add(PatchGrid, 0, 7);
            PanelMiddle.Controls.Add(tlpnlSecondaryControls, 0, 8);
            PanelMiddle.Dock = DockStyle.Fill;
            PanelMiddle.Location = new Point(298, 3);
            PanelMiddle.Name = "PanelMiddle";
            PanelMiddle.RowCount = 9;
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            PanelMiddle.RowStyles.Add(new RowStyle());
            PanelMiddle.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            PanelMiddle.Size = new Size(715, 396);
            PanelMiddle.TabIndex = 2;
            // 
            // rebasePanel
            // 
            rebasePanel.AutoSize = true;
            rebasePanel.Controls.Add(label2);
            rebasePanel.Controls.Add(cboBranches);
            rebasePanel.Controls.Add(llblShowOptions);
            rebasePanel.Dock = DockStyle.Fill;
            rebasePanel.Location = new Point(3, 43);
            rebasePanel.Name = "rebasePanel";
            rebasePanel.Size = new Size(709, 29);
            rebasePanel.TabIndex = 6;
            // 
            // tlpnlSecondaryControls
            // 
            tlpnlSecondaryControls.AutoSize = true;
            tlpnlSecondaryControls.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlSecondaryControls.ColumnCount = 2;
            tlpnlSecondaryControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpnlSecondaryControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpnlSecondaryControls.Controls.Add(flowLayoutPanel1, 0, 0);
            tlpnlSecondaryControls.Controls.Add(flowLayoutPanel2, 1, 0);
            tlpnlSecondaryControls.Dock = DockStyle.Fill;
            tlpnlSecondaryControls.Location = new Point(3, 356);
            tlpnlSecondaryControls.Name = "tlpnlSecondaryControls";
            tlpnlSecondaryControls.RowCount = 1;
            tlpnlSecondaryControls.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpnlSecondaryControls.Size = new Size(709, 37);
            tlpnlSecondaryControls.TabIndex = 28;
            // 
            // btnRebase
            // 
            btnRebase.AutoSize = true;
            btnRebase.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRebase.Image = Properties.Images.Rebase;
            btnRebase.Location = new Point(628, 8);
            btnRebase.MinimumSize = new Size(100, 25);
            btnRebase.Name = "btnRebase";
            btnRebase.Size = new Size(100, 25);
            btnRebase.TabIndex = 29;
            btnRebase.Text = "Rebase";
            btnRebase.TextAlign = ContentAlignment.MiddleRight;
            btnRebase.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnRebase.UseVisualStyleBackColor = true;
            btnRebase.Click += OkClick;
            // 
            // FormRebase
            // 
            AcceptButton = btnRebase;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(1034, 461);
            Controls.Add(btnSolveMergeconflicts);
            MinimumSize = new Size(1050, 500);
            Name = "FormRebase";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Rebase";
            Controls.SetChildIndex(ControlsPanel, 0);
            Controls.SetChildIndex(MainPanel, 0);
            Controls.SetChildIndex(btnSolveMergeconflicts, 0);
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            PanelCurrentBranch.ResumeLayout(false);
            PanelCurrentBranch.PerformLayout();
            flpnlOptionsPanelTop.ResumeLayout(false);
            flpnlOptionsPanelTop.PerformLayout();
            flpnlOptionsPanelBottom.ResumeLayout(false);
            flpnlOptionsPanelBottom.PerformLayout();
            MergeToolPanel.ResumeLayout(false);
            MergeToolPanel.PerformLayout();
            MainLayout.ResumeLayout(false);
            MainLayout.PerformLayout();
            PanelMiddle.ResumeLayout(false);
            PanelMiddle.PerformLayout();
            rebasePanel.ResumeLayout(false);
            rebasePanel.PerformLayout();
            tlpnlSecondaryControls.ResumeLayout(false);
            tlpnlSecondaryControls.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblRebase;
        private Label lblCurrent;
        private Label label2;
        private Button btnAddFiles;
        private Button btnContinueRebase;
        private Button btnAbort;
        private Button btnSkip;
        private Button btnSolveConflicts;
        private ComboBox cboBranches;
        private PatchGrid PatchGrid;
        private Label lblCommitsToReapply;
        private Button btnSolveMergeconflicts;
        private Panel MergeToolPanel;
        private CheckBox chkSpecificRange;
        private Label lblRangeTo;
        private Label lblRangeFrom;
        private TextBox txtFrom;
        private CheckBox chkInteractive;
        private CheckBox chkAutosquash;
        private CheckBox chkPreserveMerges;
        private LinkLabel llblShowOptions;
        private ComboBox cboTo;
        private Button btnChooseFromRevision;
        private TableLayoutPanel MainLayout;
        private Button btnRebase;
        private FlowLayoutPanel flpnlOptionsPanelTop;
        private FlowLayoutPanel flpnlOptionsPanelBottom;
        private Label Currentbranch;
        private TableLayoutPanel PanelMiddle;
        private FlowLayoutPanel rebasePanel;
        private Help.HelpImageDisplayUserControl PanelLeftImage;
        private CheckBox chkStash;
        private Button btnCommit;
        private Button btnEditTodo;
        private CheckBox chkIgnoreDate;
        private ToolTip toolTip1;
        private CheckBox chkCommitterDateIsAuthorDate;
        private TableLayoutPanel tlpnlSecondaryControls;
        private CheckBox checkBoxUpdateRefs;
    }
}
