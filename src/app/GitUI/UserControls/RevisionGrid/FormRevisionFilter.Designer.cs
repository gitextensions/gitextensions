namespace GitUI.UserControls.RevisionGrid
{
    partial class FormRevisionFilter
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
            Ok = new Button();
            _NO_TRANSLATE_lblSince = new Label();
            Since = new DateTimePicker();
            SinceCheck = new CheckBox();
            _NO_TRANSLATE_lblUntil = new Label();
            CheckUntil = new CheckBox();
            Until = new DateTimePicker();
            _NO_TRANSLATE_lblAuthor = new Label();
            AuthorCheck = new CheckBox();
            Author = new TextBox();
            _NO_TRANSLATE_lblCommitter = new Label();
            CommitterCheck = new CheckBox();
            Committer = new TextBox();
            _NO_TRANSLATE_lblMessage = new Label();
            MessageCheck = new CheckBox();
            Message = new TextBox();
            _NO_TRANSLATE_lblDiffContent = new Label();
            DiffContent = new TextBox();
            DiffContentCheck = new CheckBox();
            IgnoreCase = new CheckBox();
            _NO_TRANSLATE_lblLimit = new Label();
            CommitsLimitCheck = new CheckBox();
            _NO_TRANSLATE_CommitsLimit = new NumericUpDown();
            _NO_TRANSLATE_lblPathFilter = new Label();
            PathFilterCheck = new CheckBox();
            PathFilter = new TextBox();
            _NO_TRANSLATE_lblBranches = new Label();
            BranchFilterCheck = new CheckBox();
            BranchFilter = new TextBox();
            CurrentBranchOnlyCheck = new CheckBox();
            ReflogCheck = new CheckBox();
            OnlyFirstParentCheck = new CheckBox();
            HideMergeCommitsCheck = new CheckBox();
            SimplifyByDecorationCheck = new CheckBox();
            FullHistoryCheck = new CheckBox();
            SimplifyMergesCheck = new CheckBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            toolTip = new ToolTip(components);
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_CommitsLimit)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tableLayoutPanel1);
            MainPanel.Size = new Size(408, 483);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Ok);
            ControlsPanel.Location = new Point(0, 483);
            ControlsPanel.Size = new Size(408, 39);
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(320, 8);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 0;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // _NO_TRANSLATE_lblSince
            // 
            _NO_TRANSLATE_lblSince.AutoSize = true;
            _NO_TRANSLATE_lblSince.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblSince.Location = new Point(3, 0);
            _NO_TRANSLATE_lblSince.Name = "_NO_TRANSLATE_lblSince";
            _NO_TRANSLATE_lblSince.Size = new Size(67, 29);
            _NO_TRANSLATE_lblSince.TabIndex = 0;
            _NO_TRANSLATE_lblSince.Text = "Since";
            _NO_TRANSLATE_lblSince.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SinceCheck
            // 
            SinceCheck.Anchor = AnchorStyles.Left;
            SinceCheck.AutoSize = true;
            SinceCheck.Location = new Point(76, 7);
            SinceCheck.Name = "SinceCheck";
            SinceCheck.Size = new Size(14, 14);
            SinceCheck.TabIndex = 1;
            SinceCheck.UseVisualStyleBackColor = true;
            SinceCheck.CheckedChanged += option_CheckedChanged;
            // 
            // Since
            // 
            Since.Anchor = AnchorStyles.Left;
            Since.Location = new Point(96, 3);
            Since.Name = "Since";
            Since.Size = new Size(200, 23);
            Since.TabIndex = 2;
            // 
            // _NO_TRANSLATE_lblUntil
            // 
            _NO_TRANSLATE_lblUntil.AutoSize = true;
            _NO_TRANSLATE_lblUntil.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblUntil.Location = new Point(3, 29);
            _NO_TRANSLATE_lblUntil.Name = "_NO_TRANSLATE_lblUntil";
            _NO_TRANSLATE_lblUntil.Size = new Size(67, 29);
            _NO_TRANSLATE_lblUntil.TabIndex = 3;
            _NO_TRANSLATE_lblUntil.Text = "Until";
            _NO_TRANSLATE_lblUntil.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CheckUntil
            // 
            CheckUntil.Anchor = AnchorStyles.Left;
            CheckUntil.AutoSize = true;
            CheckUntil.Location = new Point(76, 36);
            CheckUntil.Name = "CheckUntil";
            CheckUntil.Size = new Size(14, 14);
            CheckUntil.TabIndex = 4;
            CheckUntil.UseVisualStyleBackColor = true;
            CheckUntil.CheckedChanged += option_CheckedChanged;
            // 
            // Until
            // 
            Until.Anchor = AnchorStyles.Left;
            Until.Location = new Point(96, 32);
            Until.Name = "Until";
            Until.Size = new Size(200, 23);
            Until.TabIndex = 5;
            // 
            // _NO_TRANSLATE_lblAuthor
            // 
            _NO_TRANSLATE_lblAuthor.AutoSize = true;
            _NO_TRANSLATE_lblAuthor.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblAuthor.Location = new Point(3, 58);
            _NO_TRANSLATE_lblAuthor.Name = "_NO_TRANSLATE_lblAuthor";
            _NO_TRANSLATE_lblAuthor.Size = new Size(67, 29);
            _NO_TRANSLATE_lblAuthor.TabIndex = 6;
            _NO_TRANSLATE_lblAuthor.Text = "Author";
            _NO_TRANSLATE_lblAuthor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AuthorCheck
            // 
            AuthorCheck.Anchor = AnchorStyles.Left;
            AuthorCheck.AutoSize = true;
            AuthorCheck.Location = new Point(76, 65);
            AuthorCheck.Name = "AuthorCheck";
            AuthorCheck.Size = new Size(14, 14);
            AuthorCheck.TabIndex = 7;
            AuthorCheck.UseVisualStyleBackColor = true;
            AuthorCheck.CheckedChanged += option_CheckedChanged;
            // 
            // Author
            // 
            Author.Dock = DockStyle.Fill;
            Author.Location = new Point(96, 61);
            Author.Name = "Author";
            Author.Size = new Size(285, 23);
            Author.TabIndex = 8;
            // 
            // _NO_TRANSLATE_lblCommitter
            // 
            _NO_TRANSLATE_lblCommitter.AutoSize = true;
            _NO_TRANSLATE_lblCommitter.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblCommitter.Location = new Point(3, 87);
            _NO_TRANSLATE_lblCommitter.Name = "_NO_TRANSLATE_lblCommitter";
            _NO_TRANSLATE_lblCommitter.Size = new Size(67, 29);
            _NO_TRANSLATE_lblCommitter.TabIndex = 9;
            _NO_TRANSLATE_lblCommitter.Text = "Committer";
            _NO_TRANSLATE_lblCommitter.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CommitterCheck
            // 
            CommitterCheck.Anchor = AnchorStyles.Left;
            CommitterCheck.AutoSize = true;
            CommitterCheck.Location = new Point(76, 94);
            CommitterCheck.Name = "CommitterCheck";
            CommitterCheck.Size = new Size(14, 14);
            CommitterCheck.TabIndex = 10;
            CommitterCheck.UseVisualStyleBackColor = true;
            CommitterCheck.CheckedChanged += option_CheckedChanged;
            // 
            // Committer
            // 
            Committer.Dock = DockStyle.Fill;
            Committer.Location = new Point(96, 90);
            Committer.Name = "Committer";
            Committer.Size = new Size(285, 23);
            Committer.TabIndex = 11;
            // 
            // _NO_TRANSLATE_lblMessage
            // 
            _NO_TRANSLATE_lblMessage.AutoSize = true;
            _NO_TRANSLATE_lblMessage.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblMessage.Location = new Point(3, 116);
            _NO_TRANSLATE_lblMessage.Name = "_NO_TRANSLATE_lblMessage";
            _NO_TRANSLATE_lblMessage.Size = new Size(67, 29);
            _NO_TRANSLATE_lblMessage.TabIndex = 12;
            _NO_TRANSLATE_lblMessage.Text = "Message";
            _NO_TRANSLATE_lblMessage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MessageCheck
            // 
            MessageCheck.Anchor = AnchorStyles.Left;
            MessageCheck.AutoSize = true;
            MessageCheck.Location = new Point(76, 123);
            MessageCheck.Name = "MessageCheck";
            MessageCheck.Size = new Size(14, 14);
            MessageCheck.TabIndex = 13;
            MessageCheck.UseVisualStyleBackColor = true;
            MessageCheck.CheckedChanged += option_CheckedChanged;
            // 
            // Message
            // 
            Message.Dock = DockStyle.Fill;
            Message.Location = new Point(96, 119);
            Message.Name = "Message";
            Message.Size = new Size(285, 23);
            Message.TabIndex = 14;
            // 
            // _NO_TRANSLATE_lblDiffContent
            // 
            _NO_TRANSLATE_lblDiffContent.AutoSize = true;
            _NO_TRANSLATE_lblDiffContent.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblDiffContent.Location = new Point(3, 145);
            _NO_TRANSLATE_lblDiffContent.Name = "_NO_TRANSLATE_lblDiffContent";
            _NO_TRANSLATE_lblDiffContent.Size = new Size(67, 29);
            _NO_TRANSLATE_lblDiffContent.TabIndex = 15;
            _NO_TRANSLATE_lblDiffContent.Text = "Diff contains";
            _NO_TRANSLATE_lblDiffContent.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // DiffContentCheck
            // 
            DiffContentCheck.Anchor = AnchorStyles.Left;
            DiffContentCheck.AutoSize = true;
            DiffContentCheck.Location = new Point(76, 152);
            DiffContentCheck.Name = "DiffContentCheck";
            DiffContentCheck.Size = new Size(14, 14);
            DiffContentCheck.TabIndex = 16;
            DiffContentCheck.UseVisualStyleBackColor = true;
            DiffContentCheck.CheckedChanged += option_CheckedChanged;
            // 
            // DiffContent
            // 
            DiffContent.Dock = DockStyle.Fill;
            DiffContent.Location = new Point(96, 148);
            DiffContent.Name = "DiffContent";
            DiffContent.Size = new Size(285, 23);
            DiffContent.TabIndex = 17;
            // 
            // IgnoreCase
            // 
            IgnoreCase.Anchor = AnchorStyles.Left;
            IgnoreCase.AutoSize = true;
            IgnoreCase.Checked = true;
            IgnoreCase.CheckState = CheckState.Checked;
            IgnoreCase.Location = new Point(76, 177);
            tableLayoutPanel1.SetColumnSpan(IgnoreCase, 2);
            IgnoreCase.Name = "IgnoreCase";
            IgnoreCase.Size = new Size(14, 14);
            IgnoreCase.TabIndex = 18;
            IgnoreCase.Text = "&Ignore case";
            IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_lblLimit
            // 
            _NO_TRANSLATE_lblLimit.AutoSize = true;
            _NO_TRANSLATE_lblLimit.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblLimit.Location = new Point(3, 194);
            _NO_TRANSLATE_lblLimit.Name = "_NO_TRANSLATE_lblLimit";
            _NO_TRANSLATE_lblLimit.Size = new Size(67, 29);
            _NO_TRANSLATE_lblLimit.TabIndex = 19;
            _NO_TRANSLATE_lblLimit.Text = "Limit";
            _NO_TRANSLATE_lblLimit.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CommitsLimitCheck
            // 
            CommitsLimitCheck.Anchor = AnchorStyles.Left;
            CommitsLimitCheck.AutoSize = true;
            CommitsLimitCheck.Location = new Point(76, 201);
            CommitsLimitCheck.Name = "CommitsLimitCheck";
            CommitsLimitCheck.Size = new Size(14, 14);
            CommitsLimitCheck.TabIndex = 20;
            CommitsLimitCheck.UseVisualStyleBackColor = true;
            CommitsLimitCheck.CheckedChanged += option_CheckedChanged;
            // 
            // _NO_TRANSLATE_CommitsLimit
            // 
            _NO_TRANSLATE_CommitsLimit.Dock = DockStyle.Left;
            _NO_TRANSLATE_CommitsLimit.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            _NO_TRANSLATE_CommitsLimit.Location = new Point(96, 197);
            _NO_TRANSLATE_CommitsLimit.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            _NO_TRANSLATE_CommitsLimit.Name = "_NO_TRANSLATE_CommitsLimit";
            _NO_TRANSLATE_CommitsLimit.Size = new Size(116, 23);
            _NO_TRANSLATE_CommitsLimit.TabIndex = 21;
            // 
            // _NO_TRANSLATE_lblPathFilter
            // 
            _NO_TRANSLATE_lblPathFilter.AutoSize = true;
            _NO_TRANSLATE_lblPathFilter.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblPathFilter.Location = new Point(3, 223);
            _NO_TRANSLATE_lblPathFilter.Name = "_NO_TRANSLATE_lblPathFilter";
            _NO_TRANSLATE_lblPathFilter.Size = new Size(67, 29);
            _NO_TRANSLATE_lblPathFilter.TabIndex = 22;
            _NO_TRANSLATE_lblPathFilter.Text = "Path filter";
            _NO_TRANSLATE_lblPathFilter.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PathFilterCheck
            // 
            PathFilterCheck.Anchor = AnchorStyles.Left;
            PathFilterCheck.AutoSize = true;
            PathFilterCheck.Location = new Point(76, 230);
            PathFilterCheck.Name = "PathFilterCheck";
            PathFilterCheck.Size = new Size(14, 14);
            PathFilterCheck.TabIndex = 23;
            PathFilterCheck.UseVisualStyleBackColor = true;
            PathFilterCheck.CheckedChanged += option_CheckedChanged;
            // 
            // PathFilter
            // 
            PathFilter.Dock = DockStyle.Fill;
            PathFilter.Location = new Point(96, 226);
            PathFilter.Name = "PathFilter";
            PathFilter.Size = new Size(285, 23);
            PathFilter.TabIndex = 24;
            // 
            // _NO_TRANSLATE_lblBranches
            // 
            _NO_TRANSLATE_lblBranches.AutoSize = true;
            _NO_TRANSLATE_lblBranches.Dock = DockStyle.Fill;
            _NO_TRANSLATE_lblBranches.Location = new Point(3, 302);
            _NO_TRANSLATE_lblBranches.Name = "_NO_TRANSLATE_lblBranches";
            _NO_TRANSLATE_lblBranches.Size = new Size(67, 29);
            _NO_TRANSLATE_lblBranches.TabIndex = 25;
            _NO_TRANSLATE_lblBranches.Text = "Branches";
            _NO_TRANSLATE_lblBranches.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BranchFilterCheck
            // 
            BranchFilterCheck.Anchor = AnchorStyles.Left;
            BranchFilterCheck.AutoSize = true;
            BranchFilterCheck.Location = new Point(76, 309);
            BranchFilterCheck.Name = "BranchFilterCheck";
            BranchFilterCheck.Size = new Size(14, 14);
            BranchFilterCheck.TabIndex = 26;
            BranchFilterCheck.UseVisualStyleBackColor = true;
            BranchFilterCheck.CheckedChanged += option_CheckedChanged;
            // 
            // BranchFilter
            // 
            BranchFilter.Dock = DockStyle.Fill;
            BranchFilter.Location = new Point(96, 305);
            BranchFilter.Name = "BranchFilter";
            BranchFilter.Size = new Size(285, 23);
            BranchFilter.TabIndex = 27;
            // 
            // CurrentBranchOnlyCheck
            // 
            CurrentBranchOnlyCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(CurrentBranchOnlyCheck, 2);
            CurrentBranchOnlyCheck.Dock = DockStyle.Fill;
            CurrentBranchOnlyCheck.Location = new Point(76, 280);
            CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            CurrentBranchOnlyCheck.Size = new Size(305, 19);
            CurrentBranchOnlyCheck.TabIndex = 28;
            CurrentBranchOnlyCheck.Text = "Show current branch &only";
            CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            CurrentBranchOnlyCheck.CheckedChanged += option_CheckedChanged;
            // 
            // ReflogCheck
            // 
            ReflogCheck.Anchor = AnchorStyles.Left;
            ReflogCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(ReflogCheck, 2);
            ReflogCheck.Location = new Point(76, 255);
            ReflogCheck.Name = "ReflogCheck";
            ReflogCheck.Size = new Size(89, 19);
            ReflogCheck.TabIndex = 29;
            ReflogCheck.Text = "Show &reflog";
            ReflogCheck.UseVisualStyleBackColor = true;
            ReflogCheck.CheckedChanged += option_CheckedChanged;
            // 
            // OnlyFirstParentCheck
            // 
            OnlyFirstParentCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(OnlyFirstParentCheck, 2);
            OnlyFirstParentCheck.Dock = DockStyle.Fill;
            OnlyFirstParentCheck.Location = new Point(76, 334);
            OnlyFirstParentCheck.Name = "OnlyFirstParentCheck";
            OnlyFirstParentCheck.Size = new Size(305, 19);
            OnlyFirstParentCheck.TabIndex = 30;
            OnlyFirstParentCheck.Text = "Show only &first parent";
            OnlyFirstParentCheck.UseVisualStyleBackColor = true;
            OnlyFirstParentCheck.CheckedChanged += option_CheckedChanged;
            // 
            // MergeCommitsCheck
            // 
            HideMergeCommitsCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(HideMergeCommitsCheck, 2);
            HideMergeCommitsCheck.Dock = DockStyle.Fill;
            HideMergeCommitsCheck.Location = new Point(76, 359);
            HideMergeCommitsCheck.Name = "MergeCommitsCheck";
            HideMergeCommitsCheck.Size = new Size(305, 19);
            HideMergeCommitsCheck.TabIndex = 31;
            HideMergeCommitsCheck.Text = "Hide merge commi&ts";
            HideMergeCommitsCheck.UseVisualStyleBackColor = true;
            HideMergeCommitsCheck.CheckedChanged += option_CheckedChanged;
            // 
            // SimplifyByDecorationCheck
            // 
            SimplifyByDecorationCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(SimplifyByDecorationCheck, 2);
            SimplifyByDecorationCheck.Dock = DockStyle.Fill;
            SimplifyByDecorationCheck.Location = new Point(76, 384);
            SimplifyByDecorationCheck.Name = "SimplifyByDecorationCheck";
            SimplifyByDecorationCheck.Size = new Size(305, 19);
            SimplifyByDecorationCheck.TabIndex = 32;
            SimplifyByDecorationCheck.Text = "Simplify b&y decoration";
            SimplifyByDecorationCheck.UseVisualStyleBackColor = true;
            SimplifyByDecorationCheck.CheckedChanged += option_CheckedChanged;
            // 
            // FullHistoryCheck
            // 
            FullHistoryCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(FullHistoryCheck, 2);
            FullHistoryCheck.Dock = DockStyle.Fill;
            FullHistoryCheck.Location = new Point(76, 409);
            FullHistoryCheck.Name = "FullHistoryCheck";
            FullHistoryCheck.Size = new Size(305, 19);
            FullHistoryCheck.TabIndex = 33;
            FullHistoryCheck.Text = "Full &history";
            FullHistoryCheck.UseVisualStyleBackColor = true;
            FullHistoryCheck.CheckedChanged += option_CheckedChanged;
            // 
            // SimplifyMergesCheck
            // 
            SimplifyMergesCheck.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(SimplifyMergesCheck, 2);
            SimplifyMergesCheck.Dock = DockStyle.Fill;
            SimplifyMergesCheck.Location = new Point(76, 434);
            SimplifyMergesCheck.Name = "SimplifyMergesCheck";
            SimplifyMergesCheck.Size = new Size(305, 22);
            SimplifyMergesCheck.TabIndex = 34;
            SimplifyMergesCheck.Text = "Simplify mer&ges";
            SimplifyMergesCheck.UseVisualStyleBackColor = true;
            SimplifyMergesCheck.CheckedChanged += option_CheckedChanged;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblSince, 0, 0);
            tableLayoutPanel1.Controls.Add(SinceCheck, 1, 0);
            tableLayoutPanel1.Controls.Add(Since, 2, 0);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblUntil, 0, 1);
            tableLayoutPanel1.Controls.Add(CheckUntil, 1, 1);
            tableLayoutPanel1.Controls.Add(Until, 2, 1);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblAuthor, 0, 2);
            tableLayoutPanel1.Controls.Add(AuthorCheck, 1, 2);
            tableLayoutPanel1.Controls.Add(Author, 2, 2);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblCommitter, 0, 3);
            tableLayoutPanel1.Controls.Add(CommitterCheck, 1, 3);
            tableLayoutPanel1.Controls.Add(Committer, 2, 3);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblMessage, 0, 4);
            tableLayoutPanel1.Controls.Add(MessageCheck, 1, 4);
            tableLayoutPanel1.Controls.Add(Message, 2, 4);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblDiffContent, 0, 5);
            tableLayoutPanel1.Controls.Add(DiffContentCheck, 1, 5);
            tableLayoutPanel1.Controls.Add(DiffContent, 2, 5);
            tableLayoutPanel1.Controls.Add(IgnoreCase, 1, 6);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblLimit, 0, 7);
            tableLayoutPanel1.Controls.Add(CommitsLimitCheck, 1, 7);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_CommitsLimit, 2, 7);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblPathFilter, 0, 8);
            tableLayoutPanel1.Controls.Add(PathFilterCheck, 1, 8);
            tableLayoutPanel1.Controls.Add(PathFilter, 2, 8);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_lblBranches, 0, 9);
            tableLayoutPanel1.Controls.Add(BranchFilterCheck, 1, 9);
            tableLayoutPanel1.Controls.Add(BranchFilter, 2, 9);
            tableLayoutPanel1.Controls.Add(CurrentBranchOnlyCheck, 1, 10);
            tableLayoutPanel1.Controls.Add(ReflogCheck, 1, 11);
            tableLayoutPanel1.Controls.Add(OnlyFirstParentCheck, 1, 12);
            tableLayoutPanel1.Controls.Add(HideMergeCommitsCheck, 1, 13);
            tableLayoutPanel1.Controls.Add(SimplifyByDecorationCheck, 1, 14);
            tableLayoutPanel1.Controls.Add(FullHistoryCheck, 1, 15);
            tableLayoutPanel1.Controls.Add(SimplifyMergesCheck, 1, 16);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 17;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(384, 459);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // FormRevisionFilter
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(408, 527);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormRevisionFilter";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Filter";
            MainPanel.ResumeLayout(false);
            ControlsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_CommitsLimit)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button Ok;
        private TableLayoutPanel tableLayoutPanel1;
        private Label _NO_TRANSLATE_lblSince;
        private CheckBox SinceCheck;
        private DateTimePicker Since;
        private Label _NO_TRANSLATE_lblUntil;
        private CheckBox CheckUntil;
        private DateTimePicker Until;
        private Label _NO_TRANSLATE_lblAuthor;
        private CheckBox AuthorCheck;
        private TextBox Author;
        private Label _NO_TRANSLATE_lblCommitter;
        private CheckBox CommitterCheck;
        private TextBox Committer;
        private Label _NO_TRANSLATE_lblMessage;
        private CheckBox MessageCheck;
        private TextBox Message;
        private Label _NO_TRANSLATE_lblDiffContent;
        private CheckBox DiffContentCheck;
        private TextBox DiffContent;
        private CheckBox IgnoreCase;
        private Label _NO_TRANSLATE_lblLimit;
        private CheckBox CommitsLimitCheck;
        private NumericUpDown _NO_TRANSLATE_CommitsLimit;
        private Label _NO_TRANSLATE_lblPathFilter;
        private CheckBox PathFilterCheck;
        private TextBox PathFilter;
        private Label _NO_TRANSLATE_lblBranches;
        private CheckBox BranchFilterCheck;
        private TextBox BranchFilter;
        private CheckBox CurrentBranchOnlyCheck;
        private CheckBox ReflogCheck;
        private CheckBox OnlyFirstParentCheck;
        private CheckBox HideMergeCommitsCheck;
        private CheckBox SimplifyByDecorationCheck;
        private CheckBox FullHistoryCheck;
        private CheckBox SimplifyMergesCheck;
        private ToolTip toolTip;
    }
}
