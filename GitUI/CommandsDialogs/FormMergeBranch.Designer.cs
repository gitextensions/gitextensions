namespace GitUI.CommandsDialogs
{
    partial class FormMergeBranch
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
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBox1 = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            advancedPanel = new TableLayoutPanel();
            mergeMessage = new TextBox();
            allowUnrelatedHistories = new CheckBox();
            squash = new CheckBox();
            NonDefaultMergeStrategy = new CheckBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            _NO_TRANSLATE_mergeStrategy = new ComboBox();
            strategyHelp = new LinkLabel();
            nbMessages = new NumericUpDown();
            addLogMessages = new CheckBox();
            addMergeMessage = new CheckBox();
            advanced = new CheckBox();
            noCommit = new CheckBox();
            noFastForward = new RadioButton();
            fastForward = new RadioButton();
            currentBranchLabel = new Label();
            Currentbranch = new Label();
            Branches = new GitUI.BranchComboBox();
            label2 = new Label();
            helpImageDisplayUserControl1 = new GitUI.Help.HelpImageDisplayUserControl();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            advancedPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(nbMessages)).BeginInit();
            SuspendLayout();
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Ok.Location = new Point(657, 425);
            Ok.Margin = new Padding(3, 3, 10, 10);
            Ok.Name = "Ok";
            Ok.Size = new Size(108, 25);
            Ok.TabIndex = 2;
            Ok.Text = "&Merge";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(groupBox1, 1, 0);
            tableLayoutPanel1.Controls.Add(helpImageDisplayUserControl1, 0, 0);
            tableLayoutPanel1.Controls.Add(Ok, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(772, 460);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(tableLayoutPanel2);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(298, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(474, 412);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Merge";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(advancedPanel, 0, 6);
            tableLayoutPanel2.Controls.Add(advanced, 0, 5);
            tableLayoutPanel2.Controls.Add(noCommit, 0, 4);
            tableLayoutPanel2.Controls.Add(noFastForward, 0, 3);
            tableLayoutPanel2.Controls.Add(fastForward, 0, 2);
            tableLayoutPanel2.Controls.Add(currentBranchLabel, 1, 1);
            tableLayoutPanel2.Controls.Add(Currentbranch, 0, 1);
            tableLayoutPanel2.Controls.Add(Branches, 1, 0);
            tableLayoutPanel2.Controls.Add(label2, 0, 0);
            tableLayoutPanel2.Location = new Point(13, 20);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 7;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(455, 372);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // advancedPanel
            // 
            advancedPanel.AutoSize = true;
            advancedPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            advancedPanel.ColumnCount = 2;
            tableLayoutPanel2.SetColumnSpan(advancedPanel, 2);
            advancedPanel.ColumnStyles.Add(new ColumnStyle());
            advancedPanel.ColumnStyles.Add(new ColumnStyle());
            advancedPanel.Controls.Add(mergeMessage, 0, 6);
            advancedPanel.Controls.Add(allowUnrelatedHistories, 0, 2);
            advancedPanel.Controls.Add(squash, 0, 1);
            advancedPanel.Controls.Add(NonDefaultMergeStrategy, 0, 0);
            advancedPanel.Controls.Add(flowLayoutPanel1, 1, 0);
            advancedPanel.Controls.Add(nbMessages, 1, 4);
            advancedPanel.Controls.Add(addLogMessages, 0, 4);
            advancedPanel.Controls.Add(addMergeMessage, 0, 5);
            advancedPanel.Dock = DockStyle.Fill;
            advancedPanel.Location = new Point(3, 165);
            advancedPanel.Name = "advancedPanel";
            advancedPanel.RowCount = 6;
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.RowStyles.Add(new RowStyle());
            advancedPanel.Size = new Size(449, 204);
            advancedPanel.TabIndex = 0;
            // 
            // mergeMessage
            // 
            advancedPanel.SetColumnSpan(mergeMessage, 2);
            mergeMessage.Dock = DockStyle.Fill;
            mergeMessage.Enabled = false;
            mergeMessage.Location = new Point(3, 124);
            mergeMessage.Multiline = true;
            mergeMessage.Name = "mergeMessage";
            mergeMessage.Size = new Size(443, 77);
            mergeMessage.TabIndex = 7;
            // 
            // allowUnrelatedHistories
            // 
            allowUnrelatedHistories.AutoSize = true;
            advancedPanel.SetColumnSpan(allowUnrelatedHistories, 2);
            allowUnrelatedHistories.Location = new Point(3, 52);
            allowUnrelatedHistories.Name = "allowUnrelatedHistories";
            allowUnrelatedHistories.Size = new Size(143, 17);
            allowUnrelatedHistories.TabIndex = 3;
            allowUnrelatedHistories.Text = "Allow unrelated histories";
            allowUnrelatedHistories.UseVisualStyleBackColor = true;
            // 
            // squash
            // 
            squash.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            squash.AutoSize = true;
            advancedPanel.SetColumnSpan(squash, 2);
            squash.Location = new Point(3, 30);
            squash.Name = "squash";
            squash.Size = new Size(102, 17);
            squash.TabIndex = 2;
            squash.Text = "Squash commits";
            squash.UseVisualStyleBackColor = true;
            // 
            // NonDefaultMergeStrategy
            // 
            NonDefaultMergeStrategy.Anchor = AnchorStyles.Left;
            NonDefaultMergeStrategy.AutoSize = true;
            NonDefaultMergeStrategy.Location = new Point(3, 5);
            NonDefaultMergeStrategy.Name = "NonDefaultMergeStrategy";
            NonDefaultMergeStrategy.Size = new Size(180, 17);
            NonDefaultMergeStrategy.TabIndex = 0;
            NonDefaultMergeStrategy.Text = "Use non-default merge strategy";
            NonDefaultMergeStrategy.UseVisualStyleBackColor = true;
            NonDefaultMergeStrategy.CheckedChanged += NonDefaultMergeStrategy_CheckedChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(_NO_TRANSLATE_mergeStrategy);
            flowLayoutPanel1.Controls.Add(strategyHelp);
            flowLayoutPanel1.Location = new Point(189, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(192, 21);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // _NO_TRANSLATE_mergeStrategy
            // 
            _NO_TRANSLATE_mergeStrategy.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_mergeStrategy.AutoCompleteSource = AutoCompleteSource.ListItems;
            _NO_TRANSLATE_mergeStrategy.FormattingEnabled = true;
            _NO_TRANSLATE_mergeStrategy.Items.AddRange(new object[] {
            "resolve",
            "recursive",
            "octopus",
            "ours",
            "subtree"});
            _NO_TRANSLATE_mergeStrategy.Location = new Point(0, 0);
            _NO_TRANSLATE_mergeStrategy.Margin = new Padding(0);
            _NO_TRANSLATE_mergeStrategy.Name = "_NO_TRANSLATE_mergeStrategy";
            _NO_TRANSLATE_mergeStrategy.Size = new Size(158, 21);
            _NO_TRANSLATE_mergeStrategy.TabIndex = 0;
            _NO_TRANSLATE_mergeStrategy.Visible = false;
            // 
            // strategyHelp
            // 
            strategyHelp.Anchor = AnchorStyles.Left;
            strategyHelp.AutoSize = true;
            strategyHelp.Location = new Point(161, 4);
            strategyHelp.Name = "strategyHelp";
            strategyHelp.Size = new Size(28, 13);
            strategyHelp.TabIndex = 1;
            strategyHelp.TabStop = true;
            strategyHelp.Text = "Help";
            strategyHelp.Visible = false;
            strategyHelp.LinkClicked += strategyHelp_LinkClicked;
            // 
            // nbMessages
            // 
            nbMessages.Enabled = false;
            nbMessages.Location = new Point(189, 74);
            nbMessages.Name = "nbMessages";
            nbMessages.Size = new Size(53, 21);
            nbMessages.TabIndex = 5;
            nbMessages.TextAlign = HorizontalAlignment.Center;
            nbMessages.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            nbMessages.ValueChanged += nbMessages_ValueChanged;
            // 
            // addLogMessages
            // 
            addLogMessages.Anchor = AnchorStyles.Left;
            addLogMessages.AutoSize = true;
            addLogMessages.Location = new Point(3, 76);
            addLogMessages.Name = "addLogMessages";
            addLogMessages.Size = new Size(112, 17);
            addLogMessages.TabIndex = 4;
            addLogMessages.Text = "Add log messages";
            addLogMessages.UseVisualStyleBackColor = true;
            addLogMessages.CheckedChanged += addMessages_CheckedChanged;
            // 
            // addMergeMessage
            // 
            addMergeMessage.AutoSize = true;
            addMergeMessage.Location = new Point(3, 101);
            addMergeMessage.Name = "addMergeMessage";
            addMergeMessage.Size = new Size(139, 17);
            addMergeMessage.TabIndex = 6;
            addMergeMessage.Text = "Specify merge message";
            addMergeMessage.UseVisualStyleBackColor = true;
            addMergeMessage.CheckedChanged += addMergeMessage_CheckedChanged;
            // 
            // advanced
            // 
            advanced.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            advanced.AutoSize = true;
            tableLayoutPanel2.SetColumnSpan(advanced, 2);
            advanced.Location = new Point(3, 142);
            advanced.Name = "advanced";
            advanced.Size = new Size(140, 17);
            advanced.TabIndex = 7;
            advanced.Text = "Show advanced options";
            advanced.UseVisualStyleBackColor = true;
            advanced.CheckedChanged += advanced_CheckedChanged;
            // 
            // noCommit
            // 
            noCommit.AutoSize = true;
            tableLayoutPanel2.SetColumnSpan(noCommit, 2);
            noCommit.Location = new Point(3, 119);
            noCommit.Name = "noCommit";
            noCommit.Size = new Size(94, 17);
            noCommit.TabIndex = 6;
            noCommit.Text = "Do not commit";
            noCommit.UseVisualStyleBackColor = true;
            // 
            // noFastForward
            // 
            noFastForward.AutoSize = true;
            tableLayoutPanel2.SetColumnSpan(noFastForward, 2);
            noFastForward.Location = new Point(3, 96);
            noFastForward.Name = "noFastForward";
            noFastForward.Size = new Size(194, 17);
            noFastForward.TabIndex = 5;
            noFastForward.Text = "Always create a new merge commit";
            noFastForward.UseVisualStyleBackColor = true;
            noFastForward.CheckedChanged += noFastForward_CheckedChanged;
            // 
            // fastForward
            // 
            fastForward.AutoSize = true;
            fastForward.Checked = true;
            tableLayoutPanel2.SetColumnSpan(fastForward, 2);
            fastForward.Location = new Point(3, 73);
            fastForward.Name = "fastForward";
            fastForward.Size = new Size(264, 17);
            fastForward.TabIndex = 4;
            fastForward.TabStop = true;
            fastForward.Text = "Keep a single branch line if possible (fast forward)";
            fastForward.UseVisualStyleBackColor = true;
            fastForward.CheckedChanged += fastForward_CheckedChanged;
            // 
            // currentBranchLabel
            // 
            currentBranchLabel.AutoSize = true;
            currentBranchLabel.Dock = DockStyle.Fill;
            currentBranchLabel.Location = new Point(110, 24);
            currentBranchLabel.Name = "currentBranchLabel";
            currentBranchLabel.Padding = new Padding(0, 3, 0, 0);
            currentBranchLabel.Size = new Size(342, 46);
            currentBranchLabel.TabIndex = 3;
            currentBranchLabel.Text = "?";
            // 
            // Currentbranch
            // 
            Currentbranch.Anchor = AnchorStyles.Left;
            Currentbranch.AutoSize = true;
            Currentbranch.Location = new Point(3, 24);
            Currentbranch.Name = "Currentbranch";
            Currentbranch.Padding = new Padding(0, 3, 0, 30);
            Currentbranch.Size = new Size(101, 46);
            Currentbranch.TabIndex = 2;
            Currentbranch.Text = "Into current branch";
            // 
            // Branches
            // 
            Branches.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Branches.BranchesToSelect = null;
            Branches.Location = new Point(107, 0);
            Branches.Margin = new Padding(0);
            Branches.Name = "Branches";
            Branches.Size = new Size(348, 24);
            Branches.TabIndex = 1;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(101, 24);
            label2.TabIndex = 0;
            label2.Text = "Merge branch";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // helpImageDisplayUserControl1
            // 
            helpImageDisplayUserControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            helpImageDisplayUserControl1.AutoSize = true;
            helpImageDisplayUserControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            helpImageDisplayUserControl1.Image1 = Properties.Images.HelpCommandMerge;
            helpImageDisplayUserControl1.Image2 = Properties.Images.HelpCommandMergeFastForward;
            helpImageDisplayUserControl1.IsExpanded = true;
            helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
            helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            helpImageDisplayUserControl1.Location = new Point(3, 3);
            helpImageDisplayUserControl1.MinimumSize = new Size(289, 416);
            helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";
            tableLayoutPanel1.SetRowSpan(helpImageDisplayUserControl1, 2);
            helpImageDisplayUserControl1.Size = new Size(289, 454);
            helpImageDisplayUserControl1.TabIndex = 0;
            helpImageDisplayUserControl1.UniqueIsExpandedSettingsId = "MergeBranches";
            // 
            // FormMergeBranch
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(772, 460);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(478, 380);
            Name = "FormMergeBranch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Merge branches";
            Load += FormMergeBranchLoad;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            advancedPanel.ResumeLayout(false);
            advancedPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(nbMessages)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel1;
        private Help.HelpImageDisplayUserControl helpImageDisplayUserControl1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label currentBranchLabel;
        private Label Currentbranch;
        private BranchComboBox Branches;
        private Label label2;
        private RadioButton noFastForward;
        private RadioButton fastForward;
        private CheckBox advanced;
        private CheckBox noCommit;
        private TableLayoutPanel advancedPanel;
        private CheckBox NonDefaultMergeStrategy;
        private FlowLayoutPanel flowLayoutPanel1;
        private ComboBox _NO_TRANSLATE_mergeStrategy;
        private LinkLabel strategyHelp;
        private CheckBox squash;
        private CheckBox allowUnrelatedHistories;
        private CheckBox addLogMessages;
        private NumericUpDown nbMessages;
        private TextBox mergeMessage;
        private CheckBox addMergeMessage;
        private Button Ok;
    }
}
