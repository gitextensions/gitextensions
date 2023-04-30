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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
            System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
            System.Windows.Forms.FlowLayoutPanel PanelCurrentBranch;
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.btnCommit = new System.Windows.Forms.Button();
            this.btnEditTodo = new System.Windows.Forms.Button();
            this.btnSkip = new System.Windows.Forms.Button();
            this.lblCurrent = new System.Windows.Forms.Label();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.lblRebase = new System.Windows.Forms.Label();
            this.cboBranches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnContinueRebase = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnSolveConflicts = new System.Windows.Forms.Button();
            this.flpnlOptionsPanelTop = new System.Windows.Forms.FlowLayoutPanel();
            this.chkInteractive = new System.Windows.Forms.CheckBox();
            this.chkPreserveMerges = new System.Windows.Forms.CheckBox();
            this.chkAutosquash = new System.Windows.Forms.CheckBox();
            this.chkStash = new System.Windows.Forms.CheckBox();
            this.chkIgnoreDate = new System.Windows.Forms.CheckBox();
            this.chkCommitterDateIsAuthorDate = new System.Windows.Forms.CheckBox();
            this.flpnlOptionsPanelBottom = new System.Windows.Forms.FlowLayoutPanel();
            this.chkSpecificRange = new System.Windows.Forms.CheckBox();
            this.lblRangeFrom = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.btnChooseFromRevision = new System.Windows.Forms.Button();
            this.lblRangeTo = new System.Windows.Forms.Label();
            this.cboTo = new System.Windows.Forms.ComboBox();
            this.llblShowOptions = new System.Windows.Forms.LinkLabel();
            this.PatchGrid = new GitUI.PatchGrid();
            this.lblCommitsToReapply = new System.Windows.Forms.Label();
            this.btnSolveMergeconflicts = new System.Windows.Forms.Button();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.PanelLeftImage = new GitUI.Help.HelpImageDisplayUserControl();
            this.PanelMiddle = new System.Windows.Forms.TableLayoutPanel();
            this.rebasePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpnlSecondaryControls = new System.Windows.Forms.TableLayoutPanel();
            this.btnRebase = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            PanelCurrentBranch = new System.Windows.Forms.FlowLayoutPanel();
            this.MainPanel.SuspendLayout();
            this.ControlsPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            PanelCurrentBranch.SuspendLayout();
            this.flpnlOptionsPanelTop.SuspendLayout();
            this.flpnlOptionsPanelBottom.SuspendLayout();
            this.MergeToolPanel.SuspendLayout();
            this.MainLayout.SuspendLayout();
            this.PanelMiddle.SuspendLayout();
            this.rebasePanel.SuspendLayout();
            this.tlpnlSecondaryControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.MainLayout);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(9);
            this.MainPanel.Size = new System.Drawing.Size(1034, 420);
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Controls.Add(this.btnAbort);
            this.ControlsPanel.Controls.Add(this.btnContinueRebase);
            this.ControlsPanel.Controls.Add(this.MergeToolPanel);
            this.ControlsPanel.Controls.Add(this.btnRebase);
            this.ControlsPanel.Location = new System.Drawing.Point(0, 420);
            this.ControlsPanel.Size = new System.Drawing.Size(1034, 41);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(this.btnAddFiles);
            flowLayoutPanel1.Controls.Add(this.btnCommit);
            flowLayoutPanel1.Controls.Add(this.btnEditTodo);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(348, 31);
            flowLayoutPanel1.TabIndex = 27;
            flowLayoutPanel1.WrapContents = false;
            // 
            // btnAddFiles
            // 
            this.btnAddFiles.AutoSize = true;
            this.btnAddFiles.Image = global::GitUI.Properties.Images.BulletAdd;
            this.btnAddFiles.Location = new System.Drawing.Point(3, 3);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(79, 25);
            this.btnAddFiles.TabIndex = 34;
            this.btnAddFiles.Text = "&Add files";
            this.btnAddFiles.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.AddFilesClick);
            // 
            // btnCommit
            // 
            this.btnCommit.AutoSize = true;
            this.btnCommit.Image = global::GitUI.Properties.Images.RepoStateStaged;
            this.btnCommit.Location = new System.Drawing.Point(88, 3);
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(86, 25);
            this.btnCommit.TabIndex = 35;
            this.btnCommit.Text = "C&ommit...";
            this.btnCommit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCommit.UseVisualStyleBackColor = true;
            this.btnCommit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // btnEditTodo
            // 
            this.btnEditTodo.AutoSize = true;
            this.btnEditTodo.Image = global::GitUI.Properties.Images.EditFile;
            this.btnEditTodo.Location = new System.Drawing.Point(180, 3);
            this.btnEditTodo.Name = "btnEditTodo";
            this.btnEditTodo.Size = new System.Drawing.Size(79, 25);
            this.btnEditTodo.TabIndex = 36;
            this.btnEditTodo.Text = "&Edit todo...";
            this.btnEditTodo.UseVisualStyleBackColor = true;
            this.btnEditTodo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnEditTodo.Click += new System.EventHandler(this.EditTodoClick);
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.Controls.Add(this.btnSkip);
            flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new System.Drawing.Point(357, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(349, 31);
            flowLayoutPanel2.TabIndex = 28;
            flowLayoutPanel2.WrapContents = false;
            // 
            // btnSkip
            // 
            this.btnSkip.AutoSize = true;
            this.btnSkip.Location = new System.Drawing.Point(163, 3);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(183, 25);
            this.btnSkip.TabIndex = 39;
            this.btnSkip.Text = "S&kip currently applying commit";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.SkipClick);
            // 
            // PanelCurrentBranch
            // 
            PanelCurrentBranch.AutoSize = true;
            PanelCurrentBranch.Controls.Add(this.lblCurrent);
            PanelCurrentBranch.Controls.Add(this.Currentbranch);
            PanelCurrentBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            PanelCurrentBranch.Location = new System.Drawing.Point(0, 15);
            PanelCurrentBranch.Margin = new System.Windows.Forms.Padding(0);
            PanelCurrentBranch.Name = "PanelCurrentBranch";
            PanelCurrentBranch.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            PanelCurrentBranch.Size = new System.Drawing.Size(715, 25);
            PanelCurrentBranch.TabIndex = 4;
            PanelCurrentBranch.WrapContents = false;
            // 
            // lblCurrent
            // 
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrent.Location = new System.Drawing.Point(3, 5);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(90, 15);
            this.lblCurrent.TabIndex = 5;
            this.lblCurrent.Text = "Current branch:";
            // 
            // Currentbranch
            // 
            this.Currentbranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(99, 5);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(0, 15);
            this.Currentbranch.TabIndex = 3;
            // 
            // lblRebase
            // 
            this.lblRebase.AutoSize = true;
            this.lblRebase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRebase.Location = new System.Drawing.Point(3, 0);
            this.lblRebase.Name = "lblRebase";
            this.lblRebase.Size = new System.Drawing.Size(709, 15);
            this.lblRebase.TabIndex = 3;
            this.lblRebase.Text = "Rebase current branch on top of another branch";
            // 
            // cboBranches
            // 
            this.cboBranches.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboBranches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboBranches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboBranches.FormattingEnabled = true;
            this.cboBranches.Location = new System.Drawing.Point(70, 3);
            this.cboBranches.Name = "cboBranches";
            this.cboBranches.Size = new System.Drawing.Size(270, 23);
            this.cboBranches.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "&Rebase on";
            // 
            // btnContinueRebase
            // 
            this.btnContinueRebase.AutoSize = true;
            this.btnContinueRebase.Location = new System.Drawing.Point(837, 8);
            this.btnContinueRebase.Name = "btnContinueRebase";
            this.btnContinueRebase.Size = new System.Drawing.Size(103, 25);
            this.btnContinueRebase.TabIndex = 38;
            this.btnContinueRebase.Text = "&Continue rebase";
            this.btnContinueRebase.UseVisualStyleBackColor = true;
            this.btnContinueRebase.Click += new System.EventHandler(this.ResolvedClick);
            // 
            // btnAbort
            // 
            this.btnAbort.AutoSize = true;
            this.btnAbort.Location = new System.Drawing.Point(946, 8);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 25);
            this.btnAbort.TabIndex = 40;
            this.btnAbort.Text = "A&bort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.AbortClick);
            // 
            // btnSolveConflicts
            // 
            this.btnSolveConflicts.AutoSize = true;
            this.btnSolveConflicts.Location = new System.Drawing.Point(0, 2);
            this.btnSolveConflicts.Margin = new System.Windows.Forms.Padding(1);
            this.btnSolveConflicts.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnSolveConflicts.Name = "btnSolveConflicts";
            this.btnSolveConflicts.Size = new System.Drawing.Size(100, 25);
            this.btnSolveConflicts.TabIndex = 32;
            this.btnSolveConflicts.Text = "&Solve conflicts";
            this.btnSolveConflicts.UseVisualStyleBackColor = true;
            this.btnSolveConflicts.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // flpnlOptionsPanelTop
            // 
            this.flpnlOptionsPanelTop.AutoSize = true;
            this.flpnlOptionsPanelTop.Controls.Add(this.chkInteractive);
            this.flpnlOptionsPanelTop.Controls.Add(this.chkPreserveMerges);
            this.flpnlOptionsPanelTop.Controls.Add(this.chkAutosquash);
            this.flpnlOptionsPanelTop.Controls.Add(this.chkStash);
            this.flpnlOptionsPanelTop.Controls.Add(this.chkIgnoreDate);
            this.flpnlOptionsPanelTop.Controls.Add(this.chkCommitterDateIsAuthorDate);
            this.flpnlOptionsPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlOptionsPanelTop.Location = new System.Drawing.Point(3, 78);
            this.flpnlOptionsPanelTop.Name = "flpnlOptionsPanelTop";
            this.flpnlOptionsPanelTop.Size = new System.Drawing.Size(709, 25);
            this.flpnlOptionsPanelTop.TabIndex = 11;
            this.flpnlOptionsPanelTop.WrapContents = false;
            // 
            // chkInteractive
            // 
            this.chkInteractive.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkInteractive.AutoSize = true;
            this.chkInteractive.Location = new System.Drawing.Point(3, 3);
            this.chkInteractive.Name = "chkInteractive";
            this.chkInteractive.Size = new System.Drawing.Size(121, 19);
            this.chkInteractive.TabIndex = 12;
            this.chkInteractive.Text = "&Interactive Rebase";
            this.chkInteractive.UseVisualStyleBackColor = true;
            this.chkInteractive.CheckedChanged += new System.EventHandler(this.chkInteractive_CheckedChanged);
            // 
            // chkPreserveMerges
            // 
            this.chkPreserveMerges.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPreserveMerges.AutoSize = true;
            this.chkPreserveMerges.Location = new System.Drawing.Point(130, 3);
            this.chkPreserveMerges.Name = "chkPreserveMerges";
            this.chkPreserveMerges.Size = new System.Drawing.Size(112, 19);
            this.chkPreserveMerges.TabIndex = 13;
            this.chkPreserveMerges.Text = "&Preserve Merges";
            this.chkPreserveMerges.UseVisualStyleBackColor = true;
            // 
            // chkAutosquash
            // 
            this.chkAutosquash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAutosquash.AutoSize = true;
            this.chkAutosquash.Enabled = false;
            this.chkAutosquash.Location = new System.Drawing.Point(248, 3);
            this.chkAutosquash.Name = "chkAutosquash";
            this.chkAutosquash.Size = new System.Drawing.Size(89, 19);
            this.chkAutosquash.TabIndex = 14;
            this.chkAutosquash.Text = "Autos&quash";
            this.chkAutosquash.UseVisualStyleBackColor = true;
            // 
            // chkStash
            // 
            this.chkStash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkStash.AutoSize = true;
            this.chkStash.Enabled = false;
            this.chkStash.Location = new System.Drawing.Point(343, 3);
            this.chkStash.Name = "chkStash";
            this.chkStash.Size = new System.Drawing.Size(82, 19);
            this.chkStash.TabIndex = 15;
            this.chkStash.Text = "A&uto stash";
            this.chkStash.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreDate
            // 
            this.chkIgnoreDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIgnoreDate.AutoSize = true;
            this.chkIgnoreDate.Location = new System.Drawing.Point(431, 3);
            this.chkIgnoreDate.Name = "chkIgnoreDate";
            this.chkIgnoreDate.Size = new System.Drawing.Size(86, 19);
            this.chkIgnoreDate.TabIndex = 16;
            this.chkIgnoreDate.Text = "Ignore &date";
            this.toolTip1.SetToolTip(this.chkIgnoreDate, "Sets the author date to the current date (same as\r\ncommit date), ignoring the ori" +
        "ginal author date.");
            this.chkIgnoreDate.UseVisualStyleBackColor = true;
            this.chkIgnoreDate.CheckedChanged += new System.EventHandler(this.chkIgnoreDate_CheckedChanged);
            // 
            // chkCommitterDateIsAuthorDate
            // 
            this.chkCommitterDateIsAuthorDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCommitterDateIsAuthorDate.AutoSize = true;
            this.chkCommitterDateIsAuthorDate.Location = new System.Drawing.Point(523, 3);
            this.chkCommitterDateIsAuthorDate.Name = "chkCommitterDateIsAuthorDate";
            this.chkCommitterDateIsAuthorDate.Size = new System.Drawing.Size(185, 19);
            this.chkCommitterDateIsAuthorDate.TabIndex = 17;
            this.chkCommitterDateIsAuthorDate.Text = "Co&mmitter date is author date";
            this.toolTip1.SetToolTip(this.chkCommitterDateIsAuthorDate, "Sets the commit date to the original author date\r\n(instead of the current date).");
            this.chkCommitterDateIsAuthorDate.UseVisualStyleBackColor = true;
            this.chkCommitterDateIsAuthorDate.CheckedChanged += new System.EventHandler(this.chkCommitterDateIsAuthorDate_CheckedChanged);
            // 
            // flpnlOptionsPanelBottom
            // 
            this.flpnlOptionsPanelBottom.AutoSize = true;
            this.flpnlOptionsPanelBottom.Controls.Add(this.chkSpecificRange);
            this.flpnlOptionsPanelBottom.Controls.Add(this.lblRangeFrom);
            this.flpnlOptionsPanelBottom.Controls.Add(this.txtFrom);
            this.flpnlOptionsPanelBottom.Controls.Add(this.btnChooseFromRevision);
            this.flpnlOptionsPanelBottom.Controls.Add(this.lblRangeTo);
            this.flpnlOptionsPanelBottom.Controls.Add(this.cboTo);
            this.flpnlOptionsPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlOptionsPanelBottom.Location = new System.Drawing.Point(3, 109);
            this.flpnlOptionsPanelBottom.Name = "flpnlOptionsPanelBottom";
            this.flpnlOptionsPanelBottom.Size = new System.Drawing.Size(709, 30);
            this.flpnlOptionsPanelBottom.TabIndex = 18;
            this.flpnlOptionsPanelBottom.WrapContents = false;
            // 
            // chkSpecificRange
            // 
            this.chkSpecificRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSpecificRange.AutoSize = true;
            this.chkSpecificRange.Location = new System.Drawing.Point(3, 5);
            this.chkSpecificRange.Name = "chkSpecificRange";
            this.chkSpecificRange.Size = new System.Drawing.Size(100, 19);
            this.chkSpecificRange.TabIndex = 19;
            this.chkSpecificRange.Text = "Specific ra&nge";
            this.chkSpecificRange.UseVisualStyleBackColor = true;
            this.chkSpecificRange.CheckedChanged += new System.EventHandler(this.chkUseFromOnto_CheckedChanged);
            // 
            // lblRangeFrom
            // 
            this.lblRangeFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRangeFrom.AutoSize = true;
            this.lblRangeFrom.Location = new System.Drawing.Point(109, 7);
            this.lblRangeFrom.Name = "lblRangeFrom";
            this.lblRangeFrom.Size = new System.Drawing.Size(67, 15);
            this.lblRangeFrom.TabIndex = 20;
            this.lblRangeFrom.Text = "&From (exc.)";
            // 
            // txtFrom
            // 
            this.txtFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtFrom.Enabled = false;
            this.txtFrom.Location = new System.Drawing.Point(182, 3);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(80, 23);
            this.txtFrom.TabIndex = 21;
            // 
            // btnChooseFromRevision
            // 
            this.btnChooseFromRevision.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnChooseFromRevision.Enabled = false;
            this.btnChooseFromRevision.Image = global::GitUI.Properties.Images.SelectRevision;
            this.btnChooseFromRevision.Location = new System.Drawing.Point(268, 3);
            this.btnChooseFromRevision.Name = "btnChooseFromRevision";
            this.btnChooseFromRevision.Size = new System.Drawing.Size(25, 24);
            this.btnChooseFromRevision.TabIndex = 22;
            this.btnChooseFromRevision.UseVisualStyleBackColor = true;
            this.btnChooseFromRevision.Click += new System.EventHandler(this.btnChooseFromRevision_Click);
            // 
            // lblRangeTo
            // 
            this.lblRangeTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRangeTo.AutoSize = true;
            this.lblRangeTo.Location = new System.Drawing.Point(299, 7);
            this.lblRangeTo.Name = "lblRangeTo";
            this.lblRangeTo.Size = new System.Drawing.Size(19, 15);
            this.lblRangeTo.TabIndex = 23;
            this.lblRangeTo.Text = "&To";
            // 
            // cboTo
            // 
            this.cboTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboTo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboTo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboTo.Enabled = false;
            this.cboTo.FormattingEnabled = true;
            this.cboTo.Location = new System.Drawing.Point(324, 3);
            this.cboTo.Name = "cboTo";
            this.cboTo.Size = new System.Drawing.Size(184, 23);
            this.cboTo.TabIndex = 24;
            // 
            // llblShowOptions
            // 
            this.llblShowOptions.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.llblShowOptions.AutoSize = true;
            this.llblShowOptions.Location = new System.Drawing.Point(346, 7);
            this.llblShowOptions.Name = "llblShowOptions";
            this.llblShowOptions.Size = new System.Drawing.Size(79, 15);
            this.llblShowOptions.TabIndex = 9;
            this.llblShowOptions.TabStop = true;
            this.llblShowOptions.Text = "Show options";
            this.llblShowOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowOptions_LinkClicked);
            // 
            // PatchGrid
            // 
            this.PatchGrid.AutoSize = true;
            this.PatchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PatchGrid.IsManagingRebase = true;
            this.PatchGrid.Location = new System.Drawing.Point(3, 177);
            this.PatchGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PatchGrid.MinimumSize = new System.Drawing.Size(0, 100);
            this.PatchGrid.Name = "PatchGrid";
            this.PatchGrid.Size = new System.Drawing.Size(709, 174);
            this.PatchGrid.TabIndex = 26;
            // 
            // lblCommitsToReapply
            // 
            this.lblCommitsToReapply.AutoSize = true;
            this.lblCommitsToReapply.Location = new System.Drawing.Point(3, 150);
            this.lblCommitsToReapply.Name = "lblCommitsToReapply";
            this.lblCommitsToReapply.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lblCommitsToReapply.Size = new System.Drawing.Size(120, 25);
            this.lblCommitsToReapply.TabIndex = 25;
            this.lblCommitsToReapply.Text = "Commits to re-apply:";
            // 
            // btnSolveMergeconflicts
            // 
            this.btnSolveMergeconflicts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSolveMergeconflicts.AutoSize = true;
            this.btnSolveMergeconflicts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSolveMergeconflicts.BackColor = System.Drawing.Color.Salmon;
            this.btnSolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSolveMergeconflicts.Location = new System.Drawing.Point(12, 426);
            this.btnSolveMergeconflicts.Name = "btnSolveMergeconflicts";
            this.btnSolveMergeconflicts.Size = new System.Drawing.Size(213, 27);
            this.btnSolveMergeconflicts.TabIndex = 42;
            this.btnSolveMergeconflicts.Text = "There are unresolved merge conflicts\r\n";
            this.btnSolveMergeconflicts.UseVisualStyleBackColor = false;
            this.btnSolveMergeconflicts.Visible = false;
            this.btnSolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeConflictsClick);
            // 
            // MergeToolPanel
            // 
            this.MergeToolPanel.AutoSize = true;
            this.MergeToolPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MergeToolPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.MergeToolPanel.Controls.Add(this.btnSolveConflicts);
            this.MergeToolPanel.Location = new System.Drawing.Point(732, 6);
            this.MergeToolPanel.Margin = new System.Windows.Forms.Padding(1);
            this.MergeToolPanel.Name = "MergeToolPanel";
            this.MergeToolPanel.Size = new System.Drawing.Size(101, 28);
            this.MergeToolPanel.TabIndex = 31;
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.Controls.Add(this.PanelLeftImage, 0, 0);
            this.MainLayout.Controls.Add(this.PanelMiddle, 1, 0);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(9, 9);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 1;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Size = new System.Drawing.Size(1016, 402);
            this.MainLayout.TabIndex = 0;
            // 
            // PanelLeftImage
            // 
            this.PanelLeftImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelLeftImage.AutoSize = true;
            this.PanelLeftImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelLeftImage.Image1 = global::GitUI.Properties.Images.HelpCommandRebase;
            this.PanelLeftImage.Image2 = null;
            this.PanelLeftImage.IsExpanded = true;
            this.PanelLeftImage.IsOnHoverShowImage2 = false;
            this.PanelLeftImage.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.PanelLeftImage.Location = new System.Drawing.Point(3, 3);
            this.PanelLeftImage.MinimumSize = new System.Drawing.Size(289, 418);
            this.PanelLeftImage.Name = "PanelLeftImage";
            this.PanelLeftImage.Size = new System.Drawing.Size(289, 418);
            this.PanelLeftImage.TabIndex = 1;
            this.PanelLeftImage.UniqueIsExpandedSettingsId = "Rebase";
            // 
            // PanelMiddle
            // 
            this.PanelMiddle.ColumnCount = 1;
            this.PanelMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PanelMiddle.Controls.Add(this.lblRebase, 0, 0);
            this.PanelMiddle.Controls.Add(PanelCurrentBranch, 0, 1);
            this.PanelMiddle.Controls.Add(this.rebasePanel, 0, 2);
            this.PanelMiddle.Controls.Add(this.flpnlOptionsPanelTop, 0, 3);
            this.PanelMiddle.Controls.Add(this.flpnlOptionsPanelBottom, 0, 4);
            this.PanelMiddle.Controls.Add(this.lblCommitsToReapply, 0, 6);
            this.PanelMiddle.Controls.Add(this.PatchGrid, 0, 7);
            this.PanelMiddle.Controls.Add(this.tlpnlSecondaryControls, 0, 8);
            this.PanelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelMiddle.Location = new System.Drawing.Point(298, 3);
            this.PanelMiddle.Name = "PanelMiddle";
            this.PanelMiddle.RowCount = 9;
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.PanelMiddle.Size = new System.Drawing.Size(715, 396);
            this.PanelMiddle.TabIndex = 2;
            // 
            // rebasePanel
            // 
            this.rebasePanel.AutoSize = true;
            this.rebasePanel.Controls.Add(this.label2);
            this.rebasePanel.Controls.Add(this.cboBranches);
            this.rebasePanel.Controls.Add(this.llblShowOptions);
            this.rebasePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rebasePanel.Location = new System.Drawing.Point(3, 43);
            this.rebasePanel.Name = "rebasePanel";
            this.rebasePanel.Size = new System.Drawing.Size(709, 29);
            this.rebasePanel.TabIndex = 6;
            // 
            // tlpnlSecondaryControls
            // 
            this.tlpnlSecondaryControls.AutoSize = true;
            this.tlpnlSecondaryControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlSecondaryControls.ColumnCount = 2;
            this.tlpnlSecondaryControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpnlSecondaryControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpnlSecondaryControls.Controls.Add(flowLayoutPanel1, 0, 0);
            this.tlpnlSecondaryControls.Controls.Add(flowLayoutPanel2, 1, 0);
            this.tlpnlSecondaryControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlSecondaryControls.Location = new System.Drawing.Point(3, 356);
            this.tlpnlSecondaryControls.Name = "tlpnlSecondaryControls";
            this.tlpnlSecondaryControls.RowCount = 1;
            this.tlpnlSecondaryControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpnlSecondaryControls.Size = new System.Drawing.Size(709, 37);
            this.tlpnlSecondaryControls.TabIndex = 28;
            // 
            // btnRebase
            // 
            this.btnRebase.AutoSize = true;
            this.btnRebase.Image = global::GitUI.Properties.Images.Rebase;
            this.btnRebase.Location = new System.Drawing.Point(628, 8);
            this.btnRebase.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnRebase.Name = "btnRebase";
            this.btnRebase.Size = new System.Drawing.Size(100, 25);
            this.btnRebase.TabIndex = 29;
            this.btnRebase.Text = "Rebase";
            this.btnRebase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRebase.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRebase.UseVisualStyleBackColor = true;
            this.btnRebase.Click += new System.EventHandler(this.OkClick);
            // 
            // FormRebase
            // 
            this.AcceptButton = this.btnRebase;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1034, 461);
            this.Controls.Add(this.btnSolveMergeconflicts);
            this.MinimumSize = new System.Drawing.Size(1050, 500);
            this.Name = "FormRebase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rebase";
            this.Controls.SetChildIndex(this.ControlsPanel, 0);
            this.Controls.SetChildIndex(this.MainPanel, 0);
            this.Controls.SetChildIndex(this.btnSolveMergeconflicts, 0);
            this.MainPanel.ResumeLayout(false);
            this.ControlsPanel.ResumeLayout(false);
            this.ControlsPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            PanelCurrentBranch.ResumeLayout(false);
            PanelCurrentBranch.PerformLayout();
            this.flpnlOptionsPanelTop.ResumeLayout(false);
            this.flpnlOptionsPanelTop.PerformLayout();
            this.flpnlOptionsPanelBottom.ResumeLayout(false);
            this.flpnlOptionsPanelBottom.PerformLayout();
            this.MergeToolPanel.ResumeLayout(false);
            this.MergeToolPanel.PerformLayout();
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.PanelMiddle.ResumeLayout(false);
            this.PanelMiddle.PerformLayout();
            this.rebasePanel.ResumeLayout(false);
            this.rebasePanel.PerformLayout();
            this.tlpnlSecondaryControls.ResumeLayout(false);
            this.tlpnlSecondaryControls.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRebase;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.Button btnContinueRebase;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnSkip;
        private System.Windows.Forms.Button btnSolveConflicts;
        private System.Windows.Forms.ComboBox cboBranches;
        private PatchGrid PatchGrid;
        private System.Windows.Forms.Label lblCommitsToReapply;
        private System.Windows.Forms.Button btnSolveMergeconflicts;
        private System.Windows.Forms.Panel MergeToolPanel;
        private System.Windows.Forms.CheckBox chkSpecificRange;
        private System.Windows.Forms.Label lblRangeTo;
        private System.Windows.Forms.Label lblRangeFrom;
        private System.Windows.Forms.TextBox txtFrom;
        private System.Windows.Forms.CheckBox chkInteractive;
        private System.Windows.Forms.CheckBox chkAutosquash;
        private System.Windows.Forms.CheckBox chkPreserveMerges;
        private System.Windows.Forms.LinkLabel llblShowOptions;
        private System.Windows.Forms.ComboBox cboTo;
        private System.Windows.Forms.Button btnChooseFromRevision;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.Button btnRebase;
        private System.Windows.Forms.FlowLayoutPanel flpnlOptionsPanelTop;
        private System.Windows.Forms.FlowLayoutPanel flpnlOptionsPanelBottom;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.TableLayoutPanel PanelMiddle;
        private System.Windows.Forms.FlowLayoutPanel rebasePanel;
        private Help.HelpImageDisplayUserControl PanelLeftImage;
        private System.Windows.Forms.CheckBox chkStash;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.Button btnEditTodo;
        private System.Windows.Forms.CheckBox chkIgnoreDate;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkCommitterDateIsAuthorDate;
        private System.Windows.Forms.TableLayoutPanel tlpnlSecondaryControls;
    }
}
