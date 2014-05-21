﻿namespace GitUI.CommandsDialogs
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
            this.fastForwardOnly = new System.Windows.Forms.RadioButton();
            this.Branches = new GitUI.BranchComboBox();
            this.noCommit = new System.Windows.Forms.CheckBox();
            this.advanced = new System.Windows.Forms.CheckBox();
            this.NonDefaultMergeStrategy = new System.Windows.Forms.CheckBox();
            this.squash = new System.Windows.Forms.CheckBox();
            this.strategyHelp = new System.Windows.Forms.LinkLabel();
            this._NO_TRANSLATE_mergeStrategy = new System.Windows.Forms.ComboBox();
            this.currentBranchLabel = new System.Windows.Forms.Label();
            this.noFastForward = new System.Windows.Forms.RadioButton();
            this.fastForward = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.helpImageDisplayUserControl1 = new GitUI.Help.HelpImageDisplayUserControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // strategyToolTip
            // 
            this.strategyToolTip.AutomaticDelay = 1;
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
            this.Ok.Location = new System.Drawing.Point(345, 317);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 25);
            this.Ok.TabIndex = 10;
            this.Ok.Text = "&Merge";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.helpImageDisplayUserControl1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(704, 357);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.fastForwardOnly);
            this.groupBox1.Controls.Add(this.Branches);
            this.groupBox1.Controls.Add(this.noCommit);
            this.groupBox1.Controls.Add(this.advanced);
            this.groupBox1.Controls.Add(this.NonDefaultMergeStrategy);
            this.groupBox1.Controls.Add(this.squash);
            this.groupBox1.Controls.Add(this.strategyHelp);
            this.groupBox1.Controls.Add(this._NO_TRANSLATE_mergeStrategy);
            this.groupBox1.Controls.Add(this.currentBranchLabel);
            this.groupBox1.Controls.Add(this.noFastForward);
            this.groupBox1.Controls.Add(this.fastForward);
            this.groupBox1.Controls.Add(this.Ok);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.Currentbranch);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(298, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(461, 351);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge";
            // 
            // fastForwardOnly
            // 
            this.fastForwardOnly.AutoSize = true;
            this.fastForwardOnly.Location = new System.Drawing.Point(13, 105);
            this.fastForwardOnly.Name = "fastForwardOnly";
            this.fastForwardOnly.Size = new System.Drawing.Size(377, 19);
            this.fastForwardOnly.TabIndex = 1;
            this.fastForwardOnly.Text = "Only allow a straight line (fast forward) merge, abort if not possible";
            this.fastForwardOnly.UseVisualStyleBackColor = true;
            this.fastForwardOnly.CheckedChanged += new System.EventHandler(this.fastForwardOnly_CheckedChanged);
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.BranchesToSelect = null;
            this.Branches.Location = new System.Drawing.Point(130, 68);
            this.Branches.Margin = new System.Windows.Forms.Padding(0);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(322, 21);
            this.Branches.TabIndex = 0;
            // 
            // noCommit
            // 
            this.noCommit.AutoSize = true;
            this.noCommit.Location = new System.Drawing.Point(13, 193);
            this.noCommit.Name = "noCommit";
            this.noCommit.Size = new System.Drawing.Size(121, 23);
            this.noCommit.TabIndex = 4;
            this.noCommit.Text = "Do not commit";
            this.noCommit.UseVisualStyleBackColor = true;
            // 
            // advanced
            // 
            this.advanced.AutoSize = true;
            this.advanced.Location = new System.Drawing.Point(13, 230);
            this.advanced.Name = "advanced";
            this.advanced.Size = new System.Drawing.Size(173, 23);
            this.advanced.TabIndex = 5;
            this.advanced.Text = "Show advanced options";
            this.advanced.UseVisualStyleBackColor = true;
            this.advanced.CheckedChanged += new System.EventHandler(this.advanced_CheckedChanged);
            // 
            // NonDefaultMergeStrategy
            // 
            this.NonDefaultMergeStrategy.AutoSize = true;
            this.NonDefaultMergeStrategy.Location = new System.Drawing.Point(27, 258);
            this.NonDefaultMergeStrategy.Name = "NonDefaultMergeStrategy";
            this.NonDefaultMergeStrategy.Size = new System.Drawing.Size(224, 23);
            this.NonDefaultMergeStrategy.TabIndex = 6;
            this.NonDefaultMergeStrategy.Text = "Use non-default merge strategy";
            this.NonDefaultMergeStrategy.UseVisualStyleBackColor = true;
            this.NonDefaultMergeStrategy.Visible = false;
            this.NonDefaultMergeStrategy.CheckedChanged += new System.EventHandler(this.NonDefaultMergeStrategy_CheckedChanged);
            // 
            // squash
            // 
            this.squash.AutoSize = true;
            this.squash.Location = new System.Drawing.Point(27, 286);
            this.squash.Name = "squash";
            this.squash.Size = new System.Drawing.Size(128, 23);
            this.squash.TabIndex = 9;
            this.squash.Text = "Squash commits";
            this.squash.UseVisualStyleBackColor = true;
            this.squash.Visible = false;
            // 
            // strategyHelp
            // 
            this.strategyHelp.AutoSize = true;
            this.strategyHelp.Location = new System.Drawing.Point(418, 259);
            this.strategyHelp.Name = "strategyHelp";
            this.strategyHelp.Size = new System.Drawing.Size(37, 19);
            this.strategyHelp.TabIndex = 8;
            this.strategyHelp.TabStop = true;
            this.strategyHelp.Text = "Help";
            this.strategyHelp.Visible = false;
            this.strategyHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.strategyHelp_LinkClicked);
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
            this._NO_TRANSLATE_mergeStrategy.Location = new System.Drawing.Point(254, 256);
            this._NO_TRANSLATE_mergeStrategy.Name = "_NO_TRANSLATE_mergeStrategy";
            this._NO_TRANSLATE_mergeStrategy.Size = new System.Drawing.Size(158, 27);
            this._NO_TRANSLATE_mergeStrategy.TabIndex = 7;
            this._NO_TRANSLATE_mergeStrategy.Visible = false;
            // 
            // currentBranchLabel
            // 
            this.currentBranchLabel.AutoSize = true;
            this.currentBranchLabel.Location = new System.Drawing.Point(126, 42);
            this.currentBranchLabel.Name = "currentBranchLabel";
            this.currentBranchLabel.Size = new System.Drawing.Size(15, 19);
            this.currentBranchLabel.TabIndex = 0;
            this.currentBranchLabel.Text = "?";
            // 
            // noFastForward
            // 
            this.noFastForward.AutoSize = true;
            this.noFastForward.Location = new System.Drawing.Point(13, 155);
            this.noFastForward.Name = "noFastForward";
            this.noFastForward.Size = new System.Drawing.Size(243, 23);
            this.noFastForward.TabIndex = 3;
            this.noFastForward.Text = "Always create a new merge commit";
            this.noFastForward.UseVisualStyleBackColor = true;
            this.noFastForward.CheckedChanged += new System.EventHandler(this.noFastForward_CheckedChanged);
            // 
            // fastForward
            // 
            this.fastForward.AutoSize = true;
            this.fastForward.Checked = true;
            this.fastForward.Location = new System.Drawing.Point(13, 130);
            this.fastForward.Name = "fastForward";
            this.fastForward.Size = new System.Drawing.Size(327, 23);
            this.fastForward.TabIndex = 2;
            this.fastForward.TabStop = true;
            this.fastForward.Text = "Keep a single branch line if possible (fast forward)";
            this.fastForward.UseVisualStyleBackColor = true;
            this.fastForward.CheckedChanged += new System.EventHandler(this.fastForward_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Merge branch into current branch";
            // 
            // Currentbranch
            // 
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(10, 42);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(102, 19);
            this.Currentbranch.TabIndex = 0;
            this.Currentbranch.Text = "Current branch";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 19);
            this.label2.TabIndex = 0;
            this.label2.Text = "Merge with";
            // 
            // helpImageDisplayUserControl1
            // 
            this.helpImageDisplayUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpImageDisplayUserControl1.AutoSize = true;
            this.helpImageDisplayUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.helpImageDisplayUserControl1.Image1 = global::GitUI.Properties.Resources.HelpCommandMerge;
            this.helpImageDisplayUserControl1.Image2 = global::GitUI.Properties.Resources.HelpCommandMergeFastForward;
            this.helpImageDisplayUserControl1.IsExpanded = true;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;
            this.helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.helpImageDisplayUserControl1.Location = new System.Drawing.Point(3, 3);
            this.helpImageDisplayUserControl1.MinimumSize = new System.Drawing.Size(231, 338);
            this.helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";
            this.helpImageDisplayUserControl1.Size = new System.Drawing.Size(231, 351);
            this.helpImageDisplayUserControl1.TabIndex = 18;
            this.helpImageDisplayUserControl1.UniqueIsExpandedSettingsId = "MergeBranches";
            // 
            // FormMergeBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(704, 357);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(483, 396);
            this.Name = "FormMergeBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Merge branches";
            this.Load += new System.EventHandler(this.FormMergeBranchLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton noFastForward;
        private System.Windows.Forms.RadioButton fastForward;
        private System.Windows.Forms.Label currentBranchLabel;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_mergeStrategy;
        private System.Windows.Forms.LinkLabel strategyHelp;
        private System.Windows.Forms.ToolTip strategyToolTip;
        private System.Windows.Forms.CheckBox squash;
        private System.Windows.Forms.CheckBox advanced;
        private System.Windows.Forms.CheckBox NonDefaultMergeStrategy;
        private System.Windows.Forms.CheckBox noCommit;
        private BranchComboBox Branches;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Help.HelpImageDisplayUserControl helpImageDisplayUserControl1;
        private System.Windows.Forms.RadioButton fastForwardOnly;
    }
}