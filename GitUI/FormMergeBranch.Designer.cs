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
            this.gbxAdvMergeOptions = new System.Windows.Forms.GroupBox();
            this.tbxAdvMergeOptions = new System.Windows.Forms.TextBox();
            this.advOptions = new System.Windows.Forms.ToolStrip();
            this.btnStrategy = new System.Windows.Forms.ToolStripDropDownButton();
            this.resolvetoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.recursivetoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.octopusToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.oursToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.subtreeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnnoff = new System.Windows.Forms.ToolStripButton();
            this.btnSquash = new System.Windows.Forms.ToolStripButton();
            this.btnNoCommit = new System.Windows.Forms.ToolStripButton();
            this.btnClear = new System.Windows.Forms.Button();
            this.advMergeOptions = new System.Windows.Forms.RadioButton();
            this.currentBranchLabel = new System.Windows.Forms.Label();
            this.noFastForward = new System.Windows.Forms.RadioButton();
            this.fastForward = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.strategyToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.mergetoolTip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.gbxAdvMergeOptions.SuspendLayout();
            this.advOptions.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(514, 256);
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
            this.pictureBox1.Size = new System.Drawing.Size(80, 256);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.gbxAdvMergeOptions);
            this.groupBox1.Controls.Add(this.advMergeOptions);
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
            this.groupBox1.Size = new System.Drawing.Size(430, 256);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge";
            // 
            // gbxAdvMergeOptions
            // 
            this.gbxAdvMergeOptions.Controls.Add(this.tbxAdvMergeOptions);
            this.gbxAdvMergeOptions.Controls.Add(this.advOptions);
            this.gbxAdvMergeOptions.Controls.Add(this.btnClear);
            this.gbxAdvMergeOptions.Location = new System.Drawing.Point(25, 152);
            this.gbxAdvMergeOptions.Name = "gbxAdvMergeOptions";
            this.gbxAdvMergeOptions.Size = new System.Drawing.Size(396, 72);
            this.gbxAdvMergeOptions.TabIndex = 15;
            this.gbxAdvMergeOptions.TabStop = false;
            this.gbxAdvMergeOptions.Text = "Advanced merge options";
            this.gbxAdvMergeOptions.Visible = false;
            // 
            // tbxAdvMergeOptions
            // 
            this.tbxAdvMergeOptions.AutoCompleteCustomSource.AddRange(new string[] {
            "--commit",
            "--no-commit",
            "--ff",
            "--no-ff",
            "--log",
            "--no-log",
            "--stat",
            "--no-stat",
            "--squash",
            "--no-squash",
            "--ff-only",
            "--strategy=",
            "--strategy-option=",
            "--summary",
            "--no-summary",
            "--quiet",
            "--verbose",
            "--rerere-autoupdate",
            "--no-rerere-autoupdate",
            "--abort"});
            this.tbxAdvMergeOptions.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbxAdvMergeOptions.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.tbxAdvMergeOptions.Location = new System.Drawing.Point(6, 45);
            this.tbxAdvMergeOptions.Name = "tbxAdvMergeOptions";
            this.tbxAdvMergeOptions.Size = new System.Drawing.Size(360, 23);
            this.tbxAdvMergeOptions.TabIndex = 3;
            // 
            // advOptions
            // 
            this.advOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStrategy,
            this.btnnoff,
            this.btnSquash,
            this.btnNoCommit});
            this.advOptions.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.advOptions.Location = new System.Drawing.Point(3, 19);
            this.advOptions.Name = "advOptions";
            this.advOptions.Size = new System.Drawing.Size(390, 22);
            this.advOptions.TabIndex = 1;
            this.advOptions.Text = "toolStrip1";
            // 
            // btnStrategy
            // 
            this.btnStrategy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnStrategy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resolvetoolStripMenuItem1,
            this.recursivetoolStripMenuItem1,
            this.octopusToolStripMenuItem1,
            this.oursToolStripMenuItem1,
            this.subtreeToolStripMenuItem1});
            this.btnStrategy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStrategy.Name = "btnStrategy";
            this.btnStrategy.Size = new System.Drawing.Size(128, 19);
            this.btnStrategy.Text = "non-default strategy";
            // 
            // resolvetoolStripMenuItem1
            // 
            this.resolvetoolStripMenuItem1.Name = "resolvetoolStripMenuItem1";
            this.resolvetoolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.resolvetoolStripMenuItem1.Text = "resolve";
            this.resolvetoolStripMenuItem1.Click += new System.EventHandler(this.strategyToolStripMenuItem_Click);
            // 
            // recursivetoolStripMenuItem1
            // 
            this.recursivetoolStripMenuItem1.Name = "recursivetoolStripMenuItem1";
            this.recursivetoolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.recursivetoolStripMenuItem1.Text = "recursive";
            this.recursivetoolStripMenuItem1.Click += new System.EventHandler(this.strategyToolStripMenuItem_Click);
            // 
            // octopusToolStripMenuItem1
            // 
            this.octopusToolStripMenuItem1.Name = "octopusToolStripMenuItem1";
            this.octopusToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.octopusToolStripMenuItem1.Text = "octopus";
            this.octopusToolStripMenuItem1.Click += new System.EventHandler(this.strategyToolStripMenuItem_Click);
            // 
            // oursToolStripMenuItem1
            // 
            this.oursToolStripMenuItem1.Name = "oursToolStripMenuItem1";
            this.oursToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.oursToolStripMenuItem1.Text = "ours";
            this.oursToolStripMenuItem1.Click += new System.EventHandler(this.strategyToolStripMenuItem_Click);
            // 
            // subtreeToolStripMenuItem1
            // 
            this.subtreeToolStripMenuItem1.Name = "subtreeToolStripMenuItem1";
            this.subtreeToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.subtreeToolStripMenuItem1.Text = "subtree";
            this.subtreeToolStripMenuItem1.Click += new System.EventHandler(this.strategyToolStripMenuItem_Click);
            // 
            // btnnoff
            // 
            this.btnnoff.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnnoff.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnnoff.Name = "btnnoff";
            this.btnnoff.Size = new System.Drawing.Size(91, 19);
            this.btnnoff.Text = "no fast forward";
            this.btnnoff.ToolTipText = "Always create a new merge commit";
            this.btnnoff.Click += new System.EventHandler(this.btnnoff_Click);
            // 
            // btnSquash
            // 
            this.btnSquash.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSquash.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSquash.Name = "btnSquash";
            this.btnSquash.Size = new System.Drawing.Size(49, 19);
            this.btnSquash.Text = "Squash";
            this.btnSquash.ToolTipText = "single commit on top of current branch";
            this.btnSquash.Click += new System.EventHandler(this.btnSquash_Click);
            // 
            // btnNoCommit
            // 
            this.btnNoCommit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnNoCommit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNoCommit.Name = "btnNoCommit";
            this.btnNoCommit.Size = new System.Drawing.Size(70, 19);
            this.btnNoCommit.Text = "no commit";
            this.btnNoCommit.ToolTipText = "merge but do not commit";
            this.btnNoCommit.Click += new System.EventHandler(this.btnNoCommit_Click);
            // 
            // btnClear
            // 
            this.btnClear.Image = global::GitUI.Properties.Resources.edit_clear;
            this.btnClear.Location = new System.Drawing.Point(368, 45);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(22, 23);
            this.btnClear.TabIndex = 2;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // advMergeOptions
            // 
            this.advMergeOptions.AutoSize = true;
            this.advMergeOptions.Location = new System.Drawing.Point(13, 150);
            this.advMergeOptions.Name = "advMergeOptions";
            this.advMergeOptions.Size = new System.Drawing.Size(158, 19);
            this.advMergeOptions.TabIndex = 13;
            this.advMergeOptions.TabStop = true;
            this.advMergeOptions.Text = "Advanced merge options";
            this.advMergeOptions.UseVisualStyleBackColor = true;
            this.advMergeOptions.CheckedChanged += new System.EventHandler(this.advMergeOptions_CheckedChanged);
            // 
            // currentBranchLabel
            // 
            this.currentBranchLabel.AutoSize = true;
            this.currentBranchLabel.Location = new System.Drawing.Point(126, 39);
            this.currentBranchLabel.Name = "currentBranchLabel";
            this.currentBranchLabel.Size = new System.Drawing.Size(12, 15);
            this.currentBranchLabel.TabIndex = 8;
            this.currentBranchLabel.Text = "?";
            // 
            // noFastForward
            // 
            this.noFastForward.AutoSize = true;
            this.noFastForward.Location = new System.Drawing.Point(13, 125);
            this.noFastForward.Name = "noFastForward";
            this.noFastForward.Size = new System.Drawing.Size(213, 19);
            this.noFastForward.TabIndex = 7;
            this.noFastForward.Text = "Always create a new merge commit";
            this.noFastForward.UseVisualStyleBackColor = true;
            // 
            // fastForward
            // 
            this.fastForward.AutoSize = true;
            this.fastForward.Checked = true;
            this.fastForward.Location = new System.Drawing.Point(13, 100);
            this.fastForward.Name = "fastForward";
            this.fastForward.Size = new System.Drawing.Size(286, 19);
            this.fastForward.TabIndex = 6;
            this.fastForward.TabStop = true;
            this.fastForward.Text = "Keep a single branch line if possible (fast forward)";
            this.fastForward.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(335, 227);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(87, 23);
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
            this.label1.Size = new System.Drawing.Size(232, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Merge current branch with another branch";
            // 
            // Branches
            // 
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(126, 64);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(296, 23);
            this.Branches.TabIndex = 3;
            // 
            // Currentbranch
            // 
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(10, 39);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(87, 15);
            this.Currentbranch.TabIndex = 1;
            this.Currentbranch.Text = "Current branch";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 256);
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
            this.gbxAdvMergeOptions.ResumeLayout(false);
            this.gbxAdvMergeOptions.PerformLayout();
            this.advOptions.ResumeLayout(false);
            this.advOptions.PerformLayout();
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
        private System.Windows.Forms.ToolTip strategyToolTip;
        private System.Windows.Forms.RadioButton advMergeOptions;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.GroupBox gbxAdvMergeOptions;
        private System.Windows.Forms.ToolStrip advOptions;
        private System.Windows.Forms.ToolStripButton btnnoff;
        private System.Windows.Forms.ToolStripButton btnSquash;
        private System.Windows.Forms.ToolStripButton btnNoCommit;
        private System.Windows.Forms.ToolStripDropDownButton btnStrategy;
        private System.Windows.Forms.ToolStripMenuItem resolvetoolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem recursivetoolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem octopusToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem oursToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem subtreeToolStripMenuItem1;
        private System.Windows.Forms.TextBox tbxAdvMergeOptions;
        private System.Windows.Forms.ToolTip mergetoolTip;
    }
}