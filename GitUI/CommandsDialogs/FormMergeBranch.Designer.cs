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
            this.strategyToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.advancedPanel = new System.Windows.Forms.TableLayoutPanel();
            this.mergeMessage = new System.Windows.Forms.TextBox();
            this.allowUnrelatedHistories = new System.Windows.Forms.CheckBox();
            this.squash = new System.Windows.Forms.CheckBox();
            this.NonDefaultMergeStrategy = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this._NO_TRANSLATE_mergeStrategy = new System.Windows.Forms.ComboBox();
            this.strategyHelp = new System.Windows.Forms.LinkLabel();
            this.nbMessages = new System.Windows.Forms.NumericUpDown();
            this.addLogMessages = new System.Windows.Forms.CheckBox();
            this.addMergeMessage = new System.Windows.Forms.CheckBox();
            this.advanced = new System.Windows.Forms.CheckBox();
            this.noCommit = new System.Windows.Forms.CheckBox();
            this.noFastForward = new System.Windows.Forms.RadioButton();
            this.fastForward = new System.Windows.Forms.RadioButton();
            this.currentBranchLabel = new System.Windows.Forms.Label();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.Branches = new GitUI.BranchComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.helpImageDisplayUserControl1 = new GitUI.Help.HelpImageDisplayUserControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.advancedPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbMessages)).BeginInit();
            this.SuspendLayout();
            // 
            // strategyToolTip
            // 
            this.strategyToolTip.AutomaticDelay = 100;
            this.strategyToolTip.AutoPopDelay = 0;
            this.strategyToolTip.InitialDelay = 1;
            this.strategyToolTip.ReshowDelay = 1;
            this.strategyToolTip.ShowAlways = true;
            this.strategyToolTip.UseAnimation = false;
            this.strategyToolTip.UseFading = false;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(657, 425);
            this.Ok.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 25);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "&Merge";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.helpImageDisplayUserControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Ok, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(772, 460);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(298, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(474, 412);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.advancedPanel, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.advanced, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.noCommit, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.noFastForward, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.fastForward, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.currentBranchLabel, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.Currentbranch, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.Branches, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(13, 20);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 7;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(455, 372);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // advancedPanel
            // 
            this.advancedPanel.AutoSize = true;
            this.advancedPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.advancedPanel.ColumnCount = 2;
            this.tableLayoutPanel2.SetColumnSpan(this.advancedPanel, 2);
            this.advancedPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.advancedPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.advancedPanel.Controls.Add(this.mergeMessage, 0, 6);
            this.advancedPanel.Controls.Add(this.allowUnrelatedHistories, 0, 2);
            this.advancedPanel.Controls.Add(this.squash, 0, 1);
            this.advancedPanel.Controls.Add(this.NonDefaultMergeStrategy, 0, 0);
            this.advancedPanel.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.advancedPanel.Controls.Add(this.nbMessages, 1, 4);
            this.advancedPanel.Controls.Add(this.addLogMessages, 0, 4);
            this.advancedPanel.Controls.Add(this.addMergeMessage, 0, 5);
            this.advancedPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advancedPanel.Location = new System.Drawing.Point(3, 165);
            this.advancedPanel.Name = "advancedPanel";
            this.advancedPanel.RowCount = 6;
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.advancedPanel.Size = new System.Drawing.Size(449, 204);
            this.advancedPanel.TabIndex = 0;
            // 
            // mergeMessage
            // 
            this.advancedPanel.SetColumnSpan(this.mergeMessage, 2);
            this.mergeMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mergeMessage.Enabled = false;
            this.mergeMessage.Location = new System.Drawing.Point(3, 124);
            this.mergeMessage.Multiline = true;
            this.mergeMessage.Name = "mergeMessage";
            this.mergeMessage.Size = new System.Drawing.Size(443, 77);
            this.mergeMessage.TabIndex = 7;
            // 
            // allowUnrelatedHistories
            // 
            this.allowUnrelatedHistories.AutoSize = true;
            this.advancedPanel.SetColumnSpan(this.allowUnrelatedHistories, 2);
            this.allowUnrelatedHistories.Location = new System.Drawing.Point(2, 52);
            this.allowUnrelatedHistories.Margin = new System.Windows.Forms.Padding(2);
            this.allowUnrelatedHistories.Name = "allowUnrelatedHistories";
            this.allowUnrelatedHistories.Size = new System.Drawing.Size(143, 17);
            this.allowUnrelatedHistories.TabIndex = 3;
            this.allowUnrelatedHistories.Text = "Allow unrelated histories";
            this.allowUnrelatedHistories.UseVisualStyleBackColor = true;
            // 
            // squash
            // 
            this.squash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.squash.AutoSize = true;
            this.advancedPanel.SetColumnSpan(this.squash, 2);
            this.squash.Location = new System.Drawing.Point(3, 30);
            this.squash.Name = "squash";
            this.squash.Size = new System.Drawing.Size(102, 17);
            this.squash.TabIndex = 2;
            this.squash.Text = "Squash commits";
            this.squash.UseVisualStyleBackColor = true;
            // 
            // NonDefaultMergeStrategy
            // 
            this.NonDefaultMergeStrategy.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.NonDefaultMergeStrategy.AutoSize = true;
            this.NonDefaultMergeStrategy.Location = new System.Drawing.Point(3, 5);
            this.NonDefaultMergeStrategy.Name = "NonDefaultMergeStrategy";
            this.NonDefaultMergeStrategy.Size = new System.Drawing.Size(180, 17);
            this.NonDefaultMergeStrategy.TabIndex = 0;
            this.NonDefaultMergeStrategy.Text = "Use non-default merge strategy";
            this.NonDefaultMergeStrategy.UseVisualStyleBackColor = true;
            this.NonDefaultMergeStrategy.CheckedChanged += new System.EventHandler(this.NonDefaultMergeStrategy_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this._NO_TRANSLATE_mergeStrategy);
            this.flowLayoutPanel1.Controls.Add(this.strategyHelp);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(189, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(192, 21);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // _NO_TRANSLATE_mergeStrategy
            // 
            this._NO_TRANSLATE_mergeStrategy.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_mergeStrategy.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_mergeStrategy.FormattingEnabled = true;
            this._NO_TRANSLATE_mergeStrategy.Items.AddRange(new object[] {
            "resolve",
            "recursive",
            "octopus",
            "ours",
            "subtree"});
            this._NO_TRANSLATE_mergeStrategy.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_mergeStrategy.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_mergeStrategy.Name = "_NO_TRANSLATE_mergeStrategy";
            this._NO_TRANSLATE_mergeStrategy.Size = new System.Drawing.Size(158, 21);
            this._NO_TRANSLATE_mergeStrategy.TabIndex = 0;
            this._NO_TRANSLATE_mergeStrategy.Visible = false;
            // 
            // strategyHelp
            // 
            this.strategyHelp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.strategyHelp.AutoSize = true;
            this.strategyHelp.Location = new System.Drawing.Point(161, 4);
            this.strategyHelp.Name = "strategyHelp";
            this.strategyHelp.Size = new System.Drawing.Size(28, 13);
            this.strategyHelp.TabIndex = 1;
            this.strategyHelp.TabStop = true;
            this.strategyHelp.Text = "Help";
            this.strategyHelp.Visible = false;
            this.strategyHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.strategyHelp_LinkClicked);
            // 
            // nbMessages
            // 
            this.nbMessages.Enabled = false;
            this.nbMessages.Location = new System.Drawing.Point(189, 74);
            this.nbMessages.Name = "nbMessages";
            this.nbMessages.Size = new System.Drawing.Size(53, 21);
            this.nbMessages.TabIndex = 5;
            this.nbMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nbMessages.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nbMessages.ValueChanged += new System.EventHandler(this.nbMessages_ValueChanged);
            // 
            // addLogMessages
            // 
            this.addLogMessages.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.addLogMessages.AutoSize = true;
            this.addLogMessages.Location = new System.Drawing.Point(3, 76);
            this.addLogMessages.Name = "addLogMessages";
            this.addLogMessages.Size = new System.Drawing.Size(112, 17);
            this.addLogMessages.TabIndex = 4;
            this.addLogMessages.Text = "Add log messages";
            this.addLogMessages.UseVisualStyleBackColor = true;
            this.addLogMessages.CheckedChanged += new System.EventHandler(this.addMessages_CheckedChanged);
            // 
            // addMergeMessage
            // 
            this.addMergeMessage.AutoSize = true;
            this.addMergeMessage.Location = new System.Drawing.Point(3, 101);
            this.addMergeMessage.Name = "addMergeMessage";
            this.addMergeMessage.Size = new System.Drawing.Size(139, 17);
            this.addMergeMessage.TabIndex = 6;
            this.addMergeMessage.Text = "Specify merge message";
            this.addMergeMessage.UseVisualStyleBackColor = true;
            this.addMergeMessage.CheckedChanged += new System.EventHandler(this.addMergeMessage_CheckedChanged);
            // 
            // advanced
            // 
            this.advanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.advanced.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.advanced, 2);
            this.advanced.Location = new System.Drawing.Point(3, 142);
            this.advanced.Name = "advanced";
            this.advanced.Size = new System.Drawing.Size(140, 17);
            this.advanced.TabIndex = 7;
            this.advanced.Text = "Show advanced options";
            this.advanced.UseVisualStyleBackColor = true;
            this.advanced.CheckedChanged += new System.EventHandler(this.advanced_CheckedChanged);
            // 
            // noCommit
            // 
            this.noCommit.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.noCommit, 2);
            this.noCommit.Location = new System.Drawing.Point(3, 119);
            this.noCommit.Name = "noCommit";
            this.noCommit.Size = new System.Drawing.Size(94, 17);
            this.noCommit.TabIndex = 6;
            this.noCommit.Text = "Do not commit";
            this.noCommit.UseVisualStyleBackColor = true;
            // 
            // noFastForward
            // 
            this.noFastForward.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.noFastForward, 2);
            this.noFastForward.Location = new System.Drawing.Point(3, 96);
            this.noFastForward.Name = "noFastForward";
            this.noFastForward.Size = new System.Drawing.Size(194, 17);
            this.noFastForward.TabIndex = 5;
            this.noFastForward.Text = "Always create a new merge commit";
            this.noFastForward.UseVisualStyleBackColor = true;
            this.noFastForward.CheckedChanged += new System.EventHandler(this.noFastForward_CheckedChanged);
            // 
            // fastForward
            // 
            this.fastForward.AutoSize = true;
            this.fastForward.Checked = true;
            this.tableLayoutPanel2.SetColumnSpan(this.fastForward, 2);
            this.fastForward.Location = new System.Drawing.Point(3, 73);
            this.fastForward.Name = "fastForward";
            this.fastForward.Size = new System.Drawing.Size(264, 17);
            this.fastForward.TabIndex = 4;
            this.fastForward.TabStop = true;
            this.fastForward.Text = "Keep a single branch line if possible (fast forward)";
            this.fastForward.UseVisualStyleBackColor = true;
            this.fastForward.CheckedChanged += new System.EventHandler(this.fastForward_CheckedChanged);
            // 
            // currentBranchLabel
            // 
            this.currentBranchLabel.AutoSize = true;
            this.currentBranchLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.currentBranchLabel.Location = new System.Drawing.Point(110, 24);
            this.currentBranchLabel.Name = "currentBranchLabel";
            this.currentBranchLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.currentBranchLabel.Size = new System.Drawing.Size(342, 46);
            this.currentBranchLabel.TabIndex = 3;
            this.currentBranchLabel.Text = "?";
            // 
            // Currentbranch
            // 
            this.Currentbranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(3, 24);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Padding = new System.Windows.Forms.Padding(0, 3, 0, 30);
            this.Currentbranch.Size = new System.Drawing.Size(101, 46);
            this.Currentbranch.TabIndex = 2;
            this.Currentbranch.Text = "Into current branch";
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.BranchesToSelect = null;
            this.Branches.Location = new System.Drawing.Point(107, 0);
            this.Branches.Margin = new System.Windows.Forms.Padding(0);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(348, 24);
            this.Branches.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 24);
            this.label2.TabIndex = 0;
            this.label2.Text = "Merge branch";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // helpImageDisplayUserControl1
            // 
            this.helpImageDisplayUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpImageDisplayUserControl1.AutoSize = true;
            this.helpImageDisplayUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.helpImageDisplayUserControl1.Image1 = global::GitUI.Properties.Images.HelpCommandMerge;
            this.helpImageDisplayUserControl1.Image2 = global::GitUI.Properties.Images.HelpCommandMergeFastForward;
            this.helpImageDisplayUserControl1.IsExpanded = true;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.helpImageDisplayUserControl1.Location = new System.Drawing.Point(3, 3);
            this.helpImageDisplayUserControl1.MinimumSize = new System.Drawing.Size(289, 416);
            this.helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";
            this.tableLayoutPanel1.SetRowSpan(this.helpImageDisplayUserControl1, 2);
            this.helpImageDisplayUserControl1.Size = new System.Drawing.Size(289, 454);
            this.helpImageDisplayUserControl1.TabIndex = 0;
            this.helpImageDisplayUserControl1.UniqueIsExpandedSettingsId = "MergeBranches";
            // 
            // FormMergeBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(772, 460);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(478, 380);
            this.Name = "FormMergeBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Merge branches";
            this.Load += new System.EventHandler(this.FormMergeBranchLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.advancedPanel.ResumeLayout(false);
            this.advancedPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbMessages)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolTip strategyToolTip;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Help.HelpImageDisplayUserControl helpImageDisplayUserControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label currentBranchLabel;
        private System.Windows.Forms.Label Currentbranch;
        private BranchComboBox Branches;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton noFastForward;
        private System.Windows.Forms.RadioButton fastForward;
        private System.Windows.Forms.CheckBox advanced;
        private System.Windows.Forms.CheckBox noCommit;
        private System.Windows.Forms.TableLayoutPanel advancedPanel;
        private System.Windows.Forms.CheckBox NonDefaultMergeStrategy;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_mergeStrategy;
        private System.Windows.Forms.LinkLabel strategyHelp;
        private System.Windows.Forms.CheckBox squash;
        private System.Windows.Forms.CheckBox allowUnrelatedHistories;
        private System.Windows.Forms.CheckBox addLogMessages;
        private System.Windows.Forms.NumericUpDown nbMessages;
        private System.Windows.Forms.TextBox mergeMessage;
        private System.Windows.Forms.CheckBox addMergeMessage;
        private System.Windows.Forms.Button Ok;
    }
}