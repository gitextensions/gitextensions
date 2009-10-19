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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPull));
            this.BrowseSource = new System.Windows.Forms.Button();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Pull = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Merge = new System.Windows.Forms.RadioButton();
            this.Rebase = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Fetch = new System.Windows.Forms.RadioButton();
            this.PullSource = new System.Windows.Forms.ComboBox();
            this.Stash = new System.Windows.Forms.Button();
            this.Remotes = new System.Windows.Forms.ComboBox();
            this.AddRemote = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.AutoStash = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.PullImage = new System.Windows.Forms.PictureBox();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.Tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PullImage)).BeginInit();
            this.SuspendLayout();
            // 
            // BrowseSource
            // 
            this.BrowseSource.Enabled = false;
            this.BrowseSource.Location = new System.Drawing.Point(431, 44);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(102, 23);
            this.BrowseSource.TabIndex = 4;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSource_Click);
            // 
            // Branches
            // 
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(128, 19);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(297, 21);
            this.Branches.TabIndex = 5;
            this.Branches.DropDown += new System.EventHandler(this.Branches_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Remote branch";
            this.Tooltip.SetToolTip(this.label2, "Remote branch to pull. Leave empty to pull all branches.");
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // Pull
            // 
            this.Pull.Location = new System.Drawing.Point(447, 256);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(102, 23);
            this.Pull.TabIndex = 7;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.Pull_Click);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(3, 256);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(104, 23);
            this.Mergetool.TabIndex = 11;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Merge
            // 
            this.Merge.AutoSize = true;
            this.Merge.Checked = true;
            this.Merge.Location = new System.Drawing.Point(7, 20);
            this.Merge.Name = "Merge";
            this.Merge.Size = new System.Drawing.Size(210, 17);
            this.Merge.TabIndex = 0;
            this.Merge.TabStop = true;
            this.Merge.Text = "Merge remote branch to current branch";
            this.Merge.UseVisualStyleBackColor = true;
            this.Merge.CheckedChanged += new System.EventHandler(this.Merge_CheckedChanged);
            // 
            // Rebase
            // 
            this.Rebase.AutoSize = true;
            this.Rebase.Location = new System.Drawing.Point(7, 41);
            this.Rebase.Name = "Rebase";
            this.Rebase.Size = new System.Drawing.Size(458, 30);
            this.Rebase.TabIndex = 1;
            this.Rebase.Text = "Rebase remote branch to current branch, creates linear history. It is recommeded " +
                "to choose \r\na remote branch when using rebase. (use with caution)";
            this.Rebase.UseVisualStyleBackColor = true;
            this.Rebase.CheckedChanged += new System.EventHandler(this.Rebase_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Fetch);
            this.groupBox1.Controls.Add(this.Rebase);
            this.groupBox1.Controls.Add(this.Merge);
            this.groupBox1.Location = new System.Drawing.Point(3, 148);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(541, 102);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge options";
            // 
            // Fetch
            // 
            this.Fetch.AutoSize = true;
            this.Fetch.Location = new System.Drawing.Point(7, 75);
            this.Fetch.Name = "Fetch";
            this.Fetch.Size = new System.Drawing.Size(212, 17);
            this.Fetch.TabIndex = 2;
            this.Fetch.TabStop = true;
            this.Fetch.Text = "Do not merge, only fetch remote branch";
            this.Fetch.UseVisualStyleBackColor = true;
            this.Fetch.CheckedChanged += new System.EventHandler(this.Fetch_CheckedChanged);
            // 
            // PullSource
            // 
            this.PullSource.Enabled = false;
            this.PullSource.FormattingEnabled = true;
            this.PullSource.Location = new System.Drawing.Point(128, 46);
            this.PullSource.Name = "PullSource";
            this.PullSource.Size = new System.Drawing.Size(297, 21);
            this.PullSource.TabIndex = 13;
            this.PullSource.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.PullSource_DrawItem);
            this.PullSource.DropDown += new System.EventHandler(this.PullSource_DropDown);
            // 
            // Stash
            // 
            this.Stash.Location = new System.Drawing.Point(111, 256);
            this.Stash.Name = "Stash";
            this.Stash.Size = new System.Drawing.Size(104, 23);
            this.Stash.TabIndex = 14;
            this.Stash.Text = "Stash changes";
            this.Stash.UseVisualStyleBackColor = true;
            this.Stash.Click += new System.EventHandler(this.Stash_Click);
            // 
            // Remotes
            // 
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(128, 19);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(297, 21);
            this.Remotes.TabIndex = 16;
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.Remotes_SelectedIndexChanged);
            this.Remotes.Validated += new System.EventHandler(this.Remotes_Validated);
            this.Remotes.DropDown += new System.EventHandler(this.Remotes_DropDown);
            // 
            // AddRemote
            // 
            this.AddRemote.Location = new System.Drawing.Point(431, 18);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(101, 23);
            this.AddRemote.TabIndex = 17;
            this.AddRemote.Text = "Manage remotes";
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemote_Click);
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
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pull from";
            // 
            // PullFromUrl
            // 
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 47);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(38, 17);
            this.PullFromUrl.TabIndex = 19;
            this.PullFromUrl.Text = "Url";
            this.Tooltip.SetToolTip(this.PullFromUrl, "Url to pull from");
            this.PullFromUrl.UseVisualStyleBackColor = true;
            this.PullFromUrl.CheckedChanged += new System.EventHandler(this.PullFromUrl_CheckedChanged);
            // 
            // PullFromRemote
            // 
            this.PullFromRemote.AutoSize = true;
            this.PullFromRemote.Checked = true;
            this.PullFromRemote.Location = new System.Drawing.Point(7, 19);
            this.PullFromRemote.Name = "PullFromRemote";
            this.PullFromRemote.Size = new System.Drawing.Size(62, 17);
            this.PullFromRemote.TabIndex = 18;
            this.PullFromRemote.TabStop = true;
            this.PullFromRemote.Text = "Remote";
            this.Tooltip.SetToolTip(this.PullFromRemote, "Remote repository to pull from");
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemote_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.Branches);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(3, 90);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(541, 52);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Branch";
            // 
            // AutoStash
            // 
            this.AutoStash.AutoSize = true;
            this.AutoStash.Location = new System.Drawing.Point(219, 260);
            this.AutoStash.Name = "AutoStash";
            this.AutoStash.Size = new System.Drawing.Size(76, 17);
            this.AutoStash.TabIndex = 20;
            this.AutoStash.Text = "Auto stash";
            this.AutoStash.UseVisualStyleBackColor = true;
            this.AutoStash.CheckedChanged += new System.EventHandler(this.AutoStash_CheckedChanged);
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
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2.Controls.Add(this.LoadSSHKey);
            this.splitContainer1.Panel2.Controls.Add(this.Pull);
            this.splitContainer1.Panel2.Controls.Add(this.AutoStash);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel2.Controls.Add(this.Stash);
            this.splitContainer1.Panel2.Controls.Add(this.Mergetool);
            this.splitContainer1.Size = new System.Drawing.Size(639, 290);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.TabIndex = 25;
            // 
            // PullImage
            // 
            this.PullImage.BackColor = System.Drawing.Color.White;
            this.PullImage.BackgroundImage = global::GitUI.Properties.Resources.merge;
            this.PullImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PullImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PullImage.Location = new System.Drawing.Point(0, 0);
            this.PullImage.Name = "PullImage";
            this.PullImage.Size = new System.Drawing.Size(80, 290);
            this.PullImage.TabIndex = 0;
            this.PullImage.TabStop = false;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(320, 256);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(123, 23);
            this.LoadSSHKey.TabIndex = 24;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // FormPull
            // 
            this.AcceptButton = this.Pull;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 290);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPull";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pull";
            this.Load += new System.EventHandler(this.FormPull_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PullImage)).EndInit();
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
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox PullImage;
        private System.Windows.Forms.ToolTip Tooltip;
    }
}