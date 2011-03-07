namespace GitUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.advanced = new System.Windows.Forms.CheckBox();
            this.NonDefaultMergeStrategy = new System.Windows.Forms.CheckBox();
            this.squash = new System.Windows.Forms.CheckBox();
            this.strategyHelp = new System.Windows.Forms.LinkLabel();
            this._NO_TRANSLATE_mergeStrategy = new System.Windows.Forms.ComboBox();
            this.currentBranchLabel = new System.Windows.Forms.Label();
            this.noFastForward = new System.Windows.Forms.RadioButton();
            this.fastForward = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.strategyToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(514, 260);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.TabIndex = 5;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BackgroundImage = global::GitUI.Properties.Resources.merge;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(80, 260);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
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
            this.groupBox1.Controls.Add(this.Branches);
            this.groupBox1.Controls.Add(this.Currentbranch);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(430, 260);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge";
            // 
            // advanced
            // 
            this.advanced.AutoSize = true;
            this.advanced.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.advanced.Location = new System.Drawing.Point(13, 160);
            this.advanced.Name = "advanced";
            this.advanced.Size = new System.Drawing.Size(74, 17);
            this.advanced.TabIndex = 15;
            this.advanced.Text = "Advanced";
            this.advanced.UseVisualStyleBackColor = true;
            this.advanced.CheckedChanged += new System.EventHandler(this.advanced_CheckedChanged);
            // 
            // NonDefaultMergeStrategy
            // 
            this.NonDefaultMergeStrategy.AutoSize = true;
            this.NonDefaultMergeStrategy.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.NonDefaultMergeStrategy.Location = new System.Drawing.Point(27, 185);
            this.NonDefaultMergeStrategy.Name = "NonDefaultMergeStrategy";
            this.NonDefaultMergeStrategy.Size = new System.Drawing.Size(180, 17);
            this.NonDefaultMergeStrategy.TabIndex = 14;
            this.NonDefaultMergeStrategy.Text = "Use non-default merge strategy";
            this.NonDefaultMergeStrategy.UseVisualStyleBackColor = true;
            this.NonDefaultMergeStrategy.Visible = false;
            this.NonDefaultMergeStrategy.CheckedChanged += new System.EventHandler(this.NonDefaultMergeStrategy_CheckedChanged);
            // 
            // squash
            // 
            this.squash.AutoSize = true;
            this.squash.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.squash.Location = new System.Drawing.Point(27, 210);
            this.squash.Name = "squash";
            this.squash.Size = new System.Drawing.Size(102, 17);
            this.squash.TabIndex = 13;
            this.squash.Text = "Squash commits";
            this.squash.UseVisualStyleBackColor = true;
            this.squash.Visible = false;
            // 
            // strategyHelp
            // 
            this.strategyHelp.AutoSize = true;
            this.strategyHelp.Location = new System.Drawing.Point(390, 189);
            this.strategyHelp.Name = "strategyHelp";
            this.strategyHelp.Size = new System.Drawing.Size(28, 13);
            this.strategyHelp.TabIndex = 12;
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
            this._NO_TRANSLATE_mergeStrategy.Location = new System.Drawing.Point(252, 186);
            this._NO_TRANSLATE_mergeStrategy.Name = "_NO_TRANSLATE_mergeStrategy";
            this._NO_TRANSLATE_mergeStrategy.Size = new System.Drawing.Size(132, 21);
            this._NO_TRANSLATE_mergeStrategy.TabIndex = 10;
            this._NO_TRANSLATE_mergeStrategy.Visible = false;
            // 
            // currentBranchLabel
            // 
            this.currentBranchLabel.AutoSize = true;
            this.currentBranchLabel.Location = new System.Drawing.Point(126, 42);
            this.currentBranchLabel.Name = "currentBranchLabel";
            this.currentBranchLabel.Size = new System.Drawing.Size(12, 13);
            this.currentBranchLabel.TabIndex = 8;
            this.currentBranchLabel.Text = "?";
            // 
            // noFastForward
            // 
            this.noFastForward.AutoSize = true;
            this.noFastForward.Location = new System.Drawing.Point(13, 135);
            this.noFastForward.Name = "noFastForward";
            this.noFastForward.Size = new System.Drawing.Size(194, 17);
            this.noFastForward.TabIndex = 7;
            this.noFastForward.Text = "Always create a new merge commit";
            this.noFastForward.UseVisualStyleBackColor = true;
            // 
            // fastForward
            // 
            this.fastForward.AutoSize = true;
            this.fastForward.Checked = true;
            this.fastForward.Location = new System.Drawing.Point(13, 110);
            this.fastForward.Name = "fastForward";
            this.fastForward.Size = new System.Drawing.Size(264, 17);
            this.fastForward.TabIndex = 6;
            this.fastForward.TabStop = true;
            this.fastForward.Text = "Keep a single branch line if possible (fast forward)";
            this.fastForward.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(314, 225);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 25);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "&Merge";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Merge branch into current branch";
            // 
            // Branches
            // 
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(126, 64);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(296, 21);
            this.Branches.TabIndex = 3;
            // 
            // Currentbranch
            // 
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(10, 42);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(80, 13);
            this.Currentbranch.TabIndex = 1;
            this.Currentbranch.Text = "Current branch";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Merge with";
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
            // FormMergeBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 260);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMergeBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Merge branches";
            this.Load += new System.EventHandler(this.FormMergeBranchLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
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
    }
}