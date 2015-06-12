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
            this.lblCurrent = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AddFiles = new System.Windows.Forms.Button();
            this.Resolved = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.OptionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkInteractive = new System.Windows.Forms.CheckBox();
            this.chkPreserveMerges = new System.Windows.Forms.CheckBox();
            this.chkAutosquash = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkSpecificRange = new System.Windows.Forms.CheckBox();
            this.lblRangeFrom = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.btnChooseFromRevision = new System.Windows.Forms.Button();
            this.lblRangeTo = new System.Windows.Forms.Label();
            this.cboTo = new System.Windows.Forms.ComboBox();
            this.ShowOptions = new System.Windows.Forms.LinkLabel();
            this.patchGrid1 = new GitUI.PatchGrid();
            this.label3 = new System.Windows.Forms.Label();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.ContinuePanel = new System.Windows.Forms.Panel();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.helpImageDisplayUserControl1 = new GitUI.Help.HelpImageDisplayUserControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Ok = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.rebasePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.rbStash = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel2.SuspendLayout();
            this.OptionsPanel.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.ContinuePanel.SuspendLayout();
            this.MergeToolPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.rebasePanel.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.localChangesGB.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(261, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rebase current branch on top of another branch";
            // 
            // lblCurrent
            // 
            this.lblCurrent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Location = new System.Drawing.Point(3, 5);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(90, 15);
            this.lblCurrent.TabIndex = 2;
            this.lblCurrent.Text = "Current branch:";
            // 
            // Branches
            // 
            this.Branches.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(70, 3);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(218, 23);
            this.Branches.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Rebase on";
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(3, 167);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(162, 25);
            this.AddFiles.TabIndex = 14;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFilesClick);
            // 
            // Resolved
            // 
            this.Resolved.Location = new System.Drawing.Point(0, 4);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(161, 25);
            this.Resolved.TabIndex = 13;
            this.Resolved.Text = "Continue rebase";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.ResolvedClick);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(3, 290);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(162, 25);
            this.Abort.TabIndex = 12;
            this.Abort.Text = "Abort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.AbortClick);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(3, 259);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(162, 25);
            this.Skip.TabIndex = 11;
            this.Skip.Text = "Skip this commit";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.SkipClick);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(0, 4);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(161, 25);
            this.Mergetool.TabIndex = 10;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.lblCurrent);
            this.flowLayoutPanel2.Controls.Add(this.Currentbranch);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 15);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.flowLayoutPanel2.Size = new System.Drawing.Size(509, 25);
            this.flowLayoutPanel2.TabIndex = 31;
            this.flowLayoutPanel2.WrapContents = false;
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
            // OptionsPanel
            // 
            this.OptionsPanel.ColumnCount = 1;
            this.OptionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.OptionsPanel.Controls.Add(this.flowLayoutPanel4, 0, 0);
            this.OptionsPanel.Controls.Add(this.flowLayoutPanel5, 0, 1);
            this.OptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsPanel.Location = new System.Drawing.Point(3, 78);
            this.OptionsPanel.Name = "OptionsPanel";
            this.OptionsPanel.RowCount = 2;
            this.OptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.OptionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.OptionsPanel.Size = new System.Drawing.Size(503, 72);
            this.OptionsPanel.TabIndex = 30;
            this.OptionsPanel.Visible = false;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.chkInteractive);
            this.flowLayoutPanel4.Controls.Add(this.chkPreserveMerges);
            this.flowLayoutPanel4.Controls.Add(this.chkAutosquash);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(497, 25);
            this.flowLayoutPanel4.TabIndex = 0;
            this.flowLayoutPanel4.WrapContents = false;
            // 
            // chkInteractive
            // 
            this.chkInteractive.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkInteractive.AutoSize = true;
            this.chkInteractive.Location = new System.Drawing.Point(3, 3);
            this.chkInteractive.Name = "chkInteractive";
            this.chkInteractive.Size = new System.Drawing.Size(121, 19);
            this.chkInteractive.TabIndex = 7;
            this.chkInteractive.Text = "Interactive Rebase";
            this.chkInteractive.UseVisualStyleBackColor = true;
            this.chkInteractive.Click += new System.EventHandler(this.InteractiveRebaseClick);
            // 
            // chkPreserveMerges
            // 
            this.chkPreserveMerges.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPreserveMerges.AutoSize = true;
            this.chkPreserveMerges.Location = new System.Drawing.Point(130, 3);
            this.chkPreserveMerges.Name = "chkPreserveMerges";
            this.chkPreserveMerges.Size = new System.Drawing.Size(112, 19);
            this.chkPreserveMerges.TabIndex = 7;
            this.chkPreserveMerges.Text = "Preserve Merges";
            this.chkPreserveMerges.UseVisualStyleBackColor = true;
            this.chkPreserveMerges.CheckedChanged += new System.EventHandler(this.chkPreserveMerges_CheckedChanged);
            this.chkPreserveMerges.Click += new System.EventHandler(this.InteractiveRebaseClick);
            // 
            // chkAutosquash
            // 
            this.chkAutosquash.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAutosquash.AutoSize = true;
            this.chkAutosquash.Enabled = false;
            this.chkAutosquash.Location = new System.Drawing.Point(248, 3);
            this.chkAutosquash.Name = "chkAutosquash";
            this.chkAutosquash.Size = new System.Drawing.Size(89, 19);
            this.chkAutosquash.TabIndex = 7;
            this.chkAutosquash.Text = "Autosquash";
            this.chkAutosquash.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.AutoSize = true;
            this.flowLayoutPanel5.Controls.Add(this.chkSpecificRange);
            this.flowLayoutPanel5.Controls.Add(this.lblRangeFrom);
            this.flowLayoutPanel5.Controls.Add(this.txtFrom);
            this.flowLayoutPanel5.Controls.Add(this.btnChooseFromRevision);
            this.flowLayoutPanel5.Controls.Add(this.lblRangeTo);
            this.flowLayoutPanel5.Controls.Add(this.cboTo);
            this.flowLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel5.Location = new System.Drawing.Point(3, 34);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(497, 35);
            this.flowLayoutPanel5.TabIndex = 1;
            this.flowLayoutPanel5.WrapContents = false;
            // 
            // chkSpecificRange
            // 
            this.chkSpecificRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSpecificRange.AutoSize = true;
            this.chkSpecificRange.Location = new System.Drawing.Point(3, 5);
            this.chkSpecificRange.Name = "chkSpecificRange";
            this.chkSpecificRange.Size = new System.Drawing.Size(100, 19);
            this.chkSpecificRange.TabIndex = 12;
            this.chkSpecificRange.Text = "Specific range";
            this.chkSpecificRange.UseVisualStyleBackColor = true;
            this.chkSpecificRange.CheckedChanged += new System.EventHandler(this.chkUseFromOnto_CheckedChanged);
            // 
            // lblRangeFrom
            // 
            this.lblRangeFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRangeFrom.AutoSize = true;
            this.lblRangeFrom.Location = new System.Drawing.Point(109, 7);
            this.lblRangeFrom.Name = "lblRangeFrom";
            this.lblRangeFrom.Size = new System.Drawing.Size(66, 15);
            this.lblRangeFrom.TabIndex = 9;
            this.lblRangeFrom.Text = "From (exc.)";
            // 
            // txtFrom
            // 
            this.txtFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtFrom.Enabled = false;
            this.txtFrom.Location = new System.Drawing.Point(181, 3);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(80, 23);
            this.txtFrom.TabIndex = 8;
            // 
            // btnChooseFromRevision
            // 
            this.btnChooseFromRevision.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnChooseFromRevision.Enabled = false;
            this.btnChooseFromRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnChooseFromRevision.Location = new System.Drawing.Point(267, 3);
            this.btnChooseFromRevision.Name = "btnChooseFromRevision";
            this.btnChooseFromRevision.Size = new System.Drawing.Size(25, 24);
            this.btnChooseFromRevision.TabIndex = 30;
            this.btnChooseFromRevision.UseVisualStyleBackColor = true;
            this.btnChooseFromRevision.Click += new System.EventHandler(this.btnChooseFromRevision_Click);
            // 
            // lblRangeTo
            // 
            this.lblRangeTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRangeTo.AutoSize = true;
            this.lblRangeTo.Location = new System.Drawing.Point(298, 7);
            this.lblRangeTo.Name = "lblRangeTo";
            this.lblRangeTo.Size = new System.Drawing.Size(21, 15);
            this.lblRangeTo.TabIndex = 11;
            this.lblRangeTo.Text = "To";
            // 
            // cboTo
            // 
            this.cboTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cboTo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboTo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboTo.Enabled = false;
            this.cboTo.FormattingEnabled = true;
            this.cboTo.Location = new System.Drawing.Point(325, 4);
            this.cboTo.Name = "cboTo";
            this.cboTo.Size = new System.Drawing.Size(184, 23);
            this.cboTo.TabIndex = 29;
            // 
            // ShowOptions
            // 
            this.ShowOptions.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ShowOptions.AutoSize = true;
            this.ShowOptions.Location = new System.Drawing.Point(294, 7);
            this.ShowOptions.Name = "ShowOptions";
            this.ShowOptions.Size = new System.Drawing.Size(79, 15);
            this.ShowOptions.TabIndex = 27;
            this.ShowOptions.TabStop = true;
            this.ShowOptions.Text = "Show options";
            this.ShowOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowOptions_LinkClicked);
            // 
            // patchGrid1
            // 
            this.patchGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patchGrid1.Location = new System.Drawing.Point(3, 180);
            this.patchGrid1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.patchGrid1.Name = "patchGrid1";
            this.patchGrid1.Size = new System.Drawing.Size(503, 286);
            this.patchGrid1.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 153);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label3.Size = new System.Drawing.Size(120, 25);
            this.label3.TabIndex = 0;
            this.label3.Text = "Commits to re-apply:";
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(3, 321);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(161, 49);
            this.SolveMergeconflicts.TabIndex = 19;
            this.SolveMergeconflicts.Text = "There are unresolved mergeconflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeconflictsClick);
            // 
            // ContinuePanel
            // 
            this.ContinuePanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ContinuePanel.Controls.Add(this.Resolved);
            this.ContinuePanel.Location = new System.Drawing.Point(3, 219);
            this.ContinuePanel.Name = "ContinuePanel";
            this.ContinuePanel.Size = new System.Drawing.Size(160, 34);
            this.ContinuePanel.TabIndex = 7;
            // 
            // MergeToolPanel
            // 
            this.MergeToolPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.MergeToolPanel.Controls.Add(this.Mergetool);
            this.MergeToolPanel.Location = new System.Drawing.Point(3, 106);
            this.MergeToolPanel.Name = "MergeToolPanel";
            this.MergeToolPanel.Size = new System.Drawing.Size(161, 34);
            this.MergeToolPanel.TabIndex = 8;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.helpImageDisplayUserControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 555F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(984, 555);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // helpImageDisplayUserControl1
            // 
            this.helpImageDisplayUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpImageDisplayUserControl1.AutoSize = true;
            this.helpImageDisplayUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.helpImageDisplayUserControl1.Image1 = global::GitUI.Properties.Resources.HelpCommandRebase;
            this.helpImageDisplayUserControl1.Image2 = null;
            this.helpImageDisplayUserControl1.IsExpanded = true;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2 = false;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.helpImageDisplayUserControl1.Location = new System.Drawing.Point(3, 3);
            this.helpImageDisplayUserControl1.MinimumSize = new System.Drawing.Size(289, 418);
            this.helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";
            this.helpImageDisplayUserControl1.Size = new System.Drawing.Size(289, 549);
            this.helpImageDisplayUserControl1.TabIndex = 20;
            this.helpImageDisplayUserControl1.UniqueIsExpandedSettingsId = "Rebase";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.Controls.Add(this.Ok);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Controls.Add(this.MergeToolPanel);
            this.flowLayoutPanel1.Controls.Add(this.panel3);
            this.flowLayoutPanel1.Controls.Add(this.AddFiles);
            this.flowLayoutPanel1.Controls.Add(this.panel4);
            this.flowLayoutPanel1.Controls.Add(this.ContinuePanel);
            this.flowLayoutPanel1.Controls.Add(this.Skip);
            this.flowLayoutPanel1.Controls.Add(this.Abort);
            this.flowLayoutPanel1.Controls.Add(this.SolveMergeconflicts);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(813, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(168, 549);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(10, 45);
            this.panel1.TabIndex = 0;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(3, 54);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(162, 25);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "Rebase";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(3, 85);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 15);
            this.panel2.TabIndex = 20;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(3, 146);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 15);
            this.panel3.TabIndex = 21;
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(3, 198);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(10, 15);
            this.panel4.TabIndex = 22;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.OptionsPanel, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.rebasePanel, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.patchGrid1, 0, 7);
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel3, 0, 4);
			this.tableLayoutPanel3.Controls.Add(this.localChangesGB, 0, 6);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(298, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 7;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(509, 549);
            this.tableLayoutPanel3.TabIndex = 32;
            // 
            // rebasePanel
            // 
            this.rebasePanel.AutoSize = true;
            this.rebasePanel.Controls.Add(this.label2);
            this.rebasePanel.Controls.Add(this.Branches);
            this.rebasePanel.Controls.Add(this.ShowOptions);
            this.rebasePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rebasePanel.Location = new System.Drawing.Point(3, 43);
            this.rebasePanel.Name = "rebasePanel";
            this.rebasePanel.Size = new System.Drawing.Size(503, 29);
            this.rebasePanel.TabIndex = 32;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.localChangesGB);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 156);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(503, 75);
            this.flowLayoutPanel3.TabIndex = 33;
            // 
            // localChangesGB
            // 
            this.localChangesGB.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.localChangesGB, 2);
            this.localChangesGB.Controls.Add(this.tableLayoutPanel2);
            this.localChangesGB.Location = new System.Drawing.Point(2, 478);
            this.localChangesGB.Margin = new System.Windows.Forms.Padding(2, 10, 2, 2);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Padding = new System.Windows.Forms.Padding(6);
            this.localChangesGB.Size = new System.Drawing.Size(126, 51);
            this.localChangesGB.TabIndex = 33;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.rbReset, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.rbStash, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 22);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(114, 23);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // rbReset
            // 
            this.rbReset.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(59, 2);
            this.rbReset.Margin = new System.Windows.Forms.Padding(2);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(53, 19);
            this.rbReset.TabIndex = 4;
            this.rbReset.Text = "Reset";
            this.rbReset.UseVisualStyleBackColor = true;
            // 
            // rbStash
            // 
            this.rbStash.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbStash.AutoSize = true;
            this.rbStash.Checked = true;
            this.rbStash.Location = new System.Drawing.Point(2, 2);
            this.rbStash.Margin = new System.Windows.Forms.Padding(2);
            this.rbStash.Name = "rbStash";
            this.rbStash.Size = new System.Drawing.Size(53, 19);
            this.rbStash.TabIndex = 3;
            this.rbStash.TabStop = true;
            this.rbStash.Text = "Stash";
            this.rbStash.UseVisualStyleBackColor = true;
            // 
            // FormRebase
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(984, 555);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(1000, 510);
            this.Name = "FormRebase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rebase";
            this.Load += new System.EventHandler(this.FormRebaseLoad);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.OptionsPanel.ResumeLayout(false);
            this.OptionsPanel.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.ContinuePanel.ResumeLayout(false);
            this.MergeToolPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.rebasePanel.ResumeLayout(false);
            this.rebasePanel.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.localChangesGB.ResumeLayout(false);
            this.localChangesGB.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button Resolved;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.ComboBox Branches;
        private PatchGrid patchGrid1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.Panel ContinuePanel;
        private System.Windows.Forms.Panel MergeToolPanel;
        private System.Windows.Forms.CheckBox chkSpecificRange;
        private System.Windows.Forms.Label lblRangeTo;
        private System.Windows.Forms.Label lblRangeFrom;
        private System.Windows.Forms.TextBox txtFrom;
        private System.Windows.Forms.CheckBox chkInteractive;
        private System.Windows.Forms.CheckBox chkAutosquash;
        private System.Windows.Forms.CheckBox chkPreserveMerges;
        private System.Windows.Forms.LinkLabel ShowOptions;
        private System.Windows.Forms.ComboBox cboTo;
        private System.Windows.Forms.Button btnChooseFromRevision;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TableLayoutPanel OptionsPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel rebasePanel;
        private Help.HelpImageDisplayUserControl helpImageDisplayUserControl1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.GroupBox localChangesGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton rbReset;
        private System.Windows.Forms.RadioButton rbStash;		
    }
}