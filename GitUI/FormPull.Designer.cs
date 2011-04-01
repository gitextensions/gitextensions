namespace GitUI
{
    partial class FormPull
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
            this.PullImage = new System.Windows.Forms.PictureBox();
            this.UpdateSubmodules = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.AddRemote = new System.Windows.Forms.Button();
            this.Remotes = new System.Windows.Forms.ComboBox();
            this.BrowseSource = new System.Windows.Forms.Button();
            this.PullSource = new System.Windows.Forms.ComboBox();
            this.Pull = new System.Windows.Forms.Button();
            this.AutoStash = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Fetch = new System.Windows.Forms.RadioButton();
            this.Merge = new System.Windows.Forms.RadioButton();
            this.Rebase = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._NO_TRANSLATE_localBranch = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Stash = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PullImage)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.PullImage);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.UpdateSubmodules);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2.Controls.Add(this.Pull);
            this.splitContainer1.Panel2.Controls.Add(this.AutoStash);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel2.Controls.Add(this.Stash);
            this.splitContainer1.Panel2.Controls.Add(this.Mergetool);
            this.splitContainer1.Size = new System.Drawing.Size(639, 312);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.TabIndex = 15;
            // 
            // PullImage
            // 
            this.PullImage.BackColor = System.Drawing.Color.White;
            this.PullImage.BackgroundImage = global::GitUI.Properties.Resources.merge;
            this.PullImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PullImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PullImage.Location = new System.Drawing.Point(0, 0);
            this.PullImage.Name = "PullImage";
            this.PullImage.Size = new System.Drawing.Size(80, 312);
            this.PullImage.TabIndex = 18;
            this.PullImage.TabStop = false;
            // 
            // UpdateSubmodules
            // 
            this.UpdateSubmodules.AutoSize = true;
            this.UpdateSubmodules.Location = new System.Drawing.Point(326, 286);
            this.UpdateSubmodules.Name = "UpdateSubmodules";
            this.UpdateSubmodules.Size = new System.Drawing.Size(133, 19);
            this.UpdateSubmodules.TabIndex = 15;
            this.UpdateSubmodules.Text = "Update Submodules";
            this.UpdateSubmodules.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.PullFromUrl);
            this.groupBox2.Controls.Add(this.PullFromRemote);
            this.groupBox2.Controls.Add(this.AddRemote);
            this.groupBox2.Controls.Add(this.Remotes);
            this.groupBox2.Controls.Add(this.BrowseSource);
            this.groupBox2.Controls.Add(this.PullSource);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(541, 80);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pull from";
            // 
            // PullFromUrl
            // 
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 49);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(38, 17);
            this.PullFromUrl.TabIndex = 3;
            this.PullFromUrl.Text = "Url";
            this.Tooltip.SetToolTip(this.PullFromUrl, "Url to pull from");
            this.PullFromUrl.UseVisualStyleBackColor = true;
            this.PullFromUrl.CheckedChanged += new System.EventHandler(this.PullFromUrlCheckedChanged);
            // 
            // PullFromRemote
            // 
            this.PullFromRemote.AutoSize = true;
            this.PullFromRemote.Checked = true;
            this.PullFromRemote.Location = new System.Drawing.Point(7, 19);
            this.PullFromRemote.Name = "PullFromRemote";
            this.PullFromRemote.Size = new System.Drawing.Size(62, 17);
            this.PullFromRemote.TabIndex = 0;
            this.PullFromRemote.TabStop = true;
            this.PullFromRemote.Text = "Remote";
            this.Tooltip.SetToolTip(this.PullFromRemote, "Remote repository to pull from");
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemoteCheckedChanged);
            // 
            // AddRemote
            // 
            this.AddRemote.Location = new System.Drawing.Point(422, 14);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(110, 25);
            this.AddRemote.TabIndex = 2;
            this.AddRemote.Text = "Manage remotes";
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemoteClick);
            // 
            // Remotes
            // 
            this.Remotes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Remotes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(148, 16);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(268, 23);
            this.Remotes.TabIndex = 1;
            this.Remotes.TextChanged += new System.EventHandler(this.Remotes_TextChanged);
            this.Remotes.Validating += new System.ComponentModel.CancelEventHandler(this.RemotesValidating);
            // 
            // BrowseSource
            // 
            this.BrowseSource.Enabled = false;
            this.BrowseSource.Location = new System.Drawing.Point(422, 46);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(110, 25);
            this.BrowseSource.TabIndex = 5;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSourceClick);
            // 
            // PullSource
            // 
            this.PullSource.Enabled = false;
            this.PullSource.FormattingEnabled = true;
            this.PullSource.Location = new System.Drawing.Point(148, 48);
            this.PullSource.Name = "PullSource";
            this.PullSource.Size = new System.Drawing.Size(268, 23);
            this.PullSource.TabIndex = 4;
            this.PullSource.DropDown += new System.EventHandler(this.PullSourceDropDown);
            this.PullSource.Validating += new System.ComponentModel.CancelEventHandler(this.PullSourceValidating);
            // 
            // Pull
            // 
            this.Pull.Image = global::GitUI.Properties.Resources._4;
            this.Pull.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Pull.Location = new System.Drawing.Point(465, 282);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(79, 25);
            this.Pull.TabIndex = 14;
            this.Pull.Text = "&Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.PullClick);
            // 
            // AutoStash
            // 
            this.AutoStash.AutoSize = true;
            this.AutoStash.Location = new System.Drawing.Point(243, 287);
            this.AutoStash.Name = "AutoStash";
            this.AutoStash.Size = new System.Drawing.Size(82, 19);
            this.AutoStash.TabIndex = 12;
            this.AutoStash.Text = "Auto stash";
            this.AutoStash.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Fetch);
            this.groupBox1.Controls.Add(this.Merge);
            this.groupBox1.Controls.Add(this.Rebase);
            this.groupBox1.Location = new System.Drawing.Point(3, 174);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(541, 102);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge options";
            // 
            // Fetch
            // 
            this.Fetch.Location = new System.Drawing.Point(7, 76);
            this.Fetch.Name = "Fetch";
            this.Fetch.Size = new System.Drawing.Size(526, 21);
            this.Fetch.TabIndex = 9;
            this.Fetch.TabStop = true;
            this.Fetch.Text = "Do not merge, only &fetch remote branch";
            this.Fetch.UseVisualStyleBackColor = true;
            this.Fetch.CheckedChanged += new System.EventHandler(this.FetchCheckedChanged);
            // 
            // Merge
            // 
            this.Merge.Checked = true;
            this.Merge.Location = new System.Drawing.Point(7, 18);
            this.Merge.Name = "Merge";
            this.Merge.Size = new System.Drawing.Size(528, 24);
            this.Merge.TabIndex = 7;
            this.Merge.TabStop = true;
            this.Merge.Text = "&Merge remote branch to current branch";
            this.Merge.UseVisualStyleBackColor = true;
            this.Merge.CheckedChanged += new System.EventHandler(this.MergeCheckedChanged);
            // 
            // Rebase
            // 
            this.Rebase.Location = new System.Drawing.Point(7, 36);
            this.Rebase.Name = "Rebase";
            this.Rebase.Size = new System.Drawing.Size(525, 44);
            this.Rebase.TabIndex = 8;
            this.Rebase.Text = "&Rebase remote branch to current branch, creates linear history. It is recommende" +
                "d\r\nto choose a remote branch when using rebase. (use with caution)";
            this.Rebase.UseVisualStyleBackColor = true;
            this.Rebase.CheckedChanged += new System.EventHandler(this.RebaseCheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._NO_TRANSLATE_localBranch);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.Branches);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(3, 90);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(541, 78);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Branch";
            // 
            // _NO_TRANSLATE_localBranch
            // 
            this._NO_TRANSLATE_localBranch.AutoSize = true;
            this._NO_TRANSLATE_localBranch.Location = new System.Drawing.Point(148, 22);
            this._NO_TRANSLATE_localBranch.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_localBranch.Name = "_NO_TRANSLATE_localBranch";
            this._NO_TRANSLATE_localBranch.Size = new System.Drawing.Size(69, 15);
            this._NO_TRANSLATE_localBranch.TabIndex = 9;
            this._NO_TRANSLATE_localBranch.Text = "(no branch)";
            this.Tooltip.SetToolTip(this._NO_TRANSLATE_localBranch, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Local branch";
            this.Tooltip.SetToolTip(this.label1, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // Branches
            // 
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(148, 46);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(268, 23);
            this.Branches.TabIndex = 6;
            this.Branches.DropDown += new System.EventHandler(this.BranchesDropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Remote branch";
            this.Tooltip.SetToolTip(this.label2, "Remote branch to pull. Leave empty to pull all branches.");
            // 
            // Stash
            // 
            this.Stash.Location = new System.Drawing.Point(121, 282);
            this.Stash.Name = "Stash";
            this.Stash.Size = new System.Drawing.Size(117, 25);
            this.Stash.TabIndex = 11;
            this.Stash.Text = "Stash changes";
            this.Stash.UseVisualStyleBackColor = true;
            this.Stash.Click += new System.EventHandler(this.StashClick);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(3, 282);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(117, 25);
            this.Mergetool.TabIndex = 10;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // FormPull
            // 
            this.AcceptButton = this.Pull;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 312);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPull";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pull";
            this.Load += new System.EventHandler(this.FormPullLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PullImage)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.RadioButton Merge;
        private System.Windows.Forms.RadioButton Rebase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Fetch;
        private System.Windows.Forms.ComboBox PullSource;
        private System.Windows.Forms.Button Stash;
        private System.Windows.Forms.ComboBox Remotes;
        private System.Windows.Forms.Button AddRemote;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton PullFromUrl;
        private System.Windows.Forms.RadioButton PullFromRemote;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox AutoStash;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox PullImage;
        private System.Windows.Forms.ToolTip Tooltip;
        private System.Windows.Forms.Label _NO_TRANSLATE_localBranch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox UpdateSubmodules;
    }
}