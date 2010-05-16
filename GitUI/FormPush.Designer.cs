﻿namespace GitUI
{
    partial class FormPush
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPush));
            this.BrowseSource = new System.Windows.Forms.Button();
            this.Push = new System.Windows.Forms.Button();
            this.PushDestination = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Branch = new System.Windows.Forms.ComboBox();
            this.PushAllBranches = new System.Windows.Forms.CheckBox();
            this.ForcePushBranches = new System.Windows.Forms.CheckBox();
            this.ForcePushTags = new System.Windows.Forms.CheckBox();
            this.Pull = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PullFromUrl = new System.Windows.Forms.RadioButton();
            this.PullFromRemote = new System.Windows.Forms.RadioButton();
            this.AddRemote = new System.Windows.Forms.Button();
            this.Remotes = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.Tag = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.PushAllTags = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TabControlTagBranch = new System.Windows.Forms.TabControl();
            this.BranchTab = new System.Windows.Forms.TabPage();
            this.TagTab = new System.Windows.Forms.TabPage();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.TabControlTagBranch.SuspendLayout();
            this.BranchTab.SuspendLayout();
            this.TagTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseSource
            // 
            resources.ApplyResources(this.BrowseSource, "BrowseSource");
            this.BrowseSource.Enabled = false;
            this.BrowseSource.Location = new System.Drawing.Point(431, 45);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(101, 23);
            this.BrowseSource.TabIndex = 13;
            this.toolTip1.SetToolTip(this.BrowseSource, resources.GetString("BrowseSource.ToolTip"));
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSource_Click);
            // 
            // Push
            // 
            resources.ApplyResources(this.Push, "Push");
            this.Push.Image = global::GitUI.Properties.Resources._31;
            this.Push.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Push.Location = new System.Drawing.Point(447, 247);
            this.Push.Name = "Push";
            this.Push.Size = new System.Drawing.Size(101, 23);
            this.Push.TabIndex = 15;
            this.toolTip1.SetToolTip(this.Push, resources.GetString("Push.ToolTip"));
            this.Push.UseVisualStyleBackColor = true;
            this.Push.Click += new System.EventHandler(this.Push_Click);
            // 
            // PushDestination
            // 
            resources.ApplyResources(this.PushDestination, "PushDestination");
            this.PushDestination.Enabled = false;
            this.PushDestination.FormattingEnabled = true;
            this.PushDestination.Location = new System.Drawing.Point(128, 46);
            this.PushDestination.Name = "PushDestination";
            this.PushDestination.Size = new System.Drawing.Size(297, 21);
            this.PushDestination.TabIndex = 16;
            this.toolTip1.SetToolTip(this.PushDestination, resources.GetString("PushDestination.ToolTip"));
            this.PushDestination.DropDown += new System.EventHandler(this.PushDestination_DropDown);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 17;
            this.toolTip1.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
            // 
            // Branch
            // 
            resources.ApplyResources(this.Branch, "Branch");
            this.Branch.FormattingEnabled = true;
            this.Branch.Location = new System.Drawing.Point(127, 19);
            this.Branch.Name = "Branch";
            this.Branch.Size = new System.Drawing.Size(297, 21);
            this.Branch.TabIndex = 18;
            this.toolTip1.SetToolTip(this.Branch, resources.GetString("Branch.ToolTip"));
            this.Branch.DropDown += new System.EventHandler(this.Branch_DropDown);
            // 
            // PushAllBranches
            // 
            resources.ApplyResources(this.PushAllBranches, "PushAllBranches");
            this.PushAllBranches.AutoSize = true;
            this.PushAllBranches.Location = new System.Drawing.Point(127, 51);
            this.PushAllBranches.Name = "PushAllBranches";
            this.PushAllBranches.Size = new System.Drawing.Size(110, 17);
            this.PushAllBranches.TabIndex = 19;
            this.toolTip1.SetToolTip(this.PushAllBranches, resources.GetString("PushAllBranches.ToolTip"));
            this.PushAllBranches.UseVisualStyleBackColor = true;
            // 
            // ForcePushBranches
            // 
            resources.ApplyResources(this.ForcePushBranches, "ForcePushBranches");
            this.ForcePushBranches.AutoSize = true;
            this.ForcePushBranches.Location = new System.Drawing.Point(127, 74);
            this.ForcePushBranches.Name = "ForcePushBranches";
            this.ForcePushBranches.Size = new System.Drawing.Size(80, 17);
            this.ForcePushBranches.TabIndex = 19;
            this.toolTip1.SetToolTip(this.ForcePushBranches, resources.GetString("ForcePushBranches.ToolTip"));
            this.ForcePushBranches.UseVisualStyleBackColor = true;
            this.ForcePushBranches.CheckedChanged += new System.EventHandler(this.ForcePushBranches_CheckedChanged);
            // 
            // ForcePushTags
            // 
            resources.ApplyResources(this.ForcePushTags, "ForcePushTags");
            this.ForcePushTags.AutoSize = true;
            this.ForcePushTags.Location = new System.Drawing.Point(127, 74);
            this.ForcePushTags.Name = "ForcePushTags";
            this.ForcePushTags.Size = new System.Drawing.Size(80, 17);
            this.ForcePushTags.TabIndex = 19;
            this.toolTip1.SetToolTip(this.ForcePushTags, resources.GetString("ForcePushTags.ToolTip"));
            this.ForcePushTags.UseVisualStyleBackColor = true;
            this.ForcePushTags.CheckedChanged += new System.EventHandler(this.ForcePushTags_CheckedChanged);
            // 
            // Pull
            // 
            resources.ApplyResources(this.Pull, "Pull");
            this.Pull.Location = new System.Drawing.Point(12, 247);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(101, 23);
            this.Pull.TabIndex = 20;
            this.toolTip1.SetToolTip(this.Pull, resources.GetString("Pull.ToolTip"));
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.Pull_Click);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.PullFromUrl);
            this.groupBox2.Controls.Add(this.PullFromRemote);
            this.groupBox2.Controls.Add(this.AddRemote);
            this.groupBox2.Controls.Add(this.Remotes);
            this.groupBox2.Controls.Add(this.BrowseSource);
            this.groupBox2.Controls.Add(this.PushDestination);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(541, 80);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.toolTip1.SetToolTip(this.groupBox2, resources.GetString("groupBox2.ToolTip"));
            // 
            // PullFromUrl
            // 
            resources.ApplyResources(this.PullFromUrl, "PullFromUrl");
            this.PullFromUrl.AutoSize = true;
            this.PullFromUrl.Location = new System.Drawing.Point(7, 47);
            this.PullFromUrl.Name = "PullFromUrl";
            this.PullFromUrl.Size = new System.Drawing.Size(38, 17);
            this.PullFromUrl.TabIndex = 19;
            this.toolTip1.SetToolTip(this.PullFromUrl, resources.GetString("PullFromUrl.ToolTip"));
            this.PullFromUrl.UseVisualStyleBackColor = true;
            this.PullFromUrl.CheckedChanged += new System.EventHandler(this.PullFromUrl_CheckedChanged);
            // 
            // PullFromRemote
            // 
            resources.ApplyResources(this.PullFromRemote, "PullFromRemote");
            this.PullFromRemote.AutoSize = true;
            this.PullFromRemote.Checked = true;
            this.PullFromRemote.Location = new System.Drawing.Point(7, 19);
            this.PullFromRemote.Name = "PullFromRemote";
            this.PullFromRemote.Size = new System.Drawing.Size(62, 17);
            this.PullFromRemote.TabIndex = 18;
            this.PullFromRemote.TabStop = true;
            this.toolTip1.SetToolTip(this.PullFromRemote, resources.GetString("PullFromRemote.ToolTip"));
            this.PullFromRemote.UseVisualStyleBackColor = true;
            this.PullFromRemote.CheckedChanged += new System.EventHandler(this.PullFromRemote_CheckedChanged);
            // 
            // AddRemote
            // 
            resources.ApplyResources(this.AddRemote, "AddRemote");
            this.AddRemote.Location = new System.Drawing.Point(431, 18);
            this.AddRemote.Name = "AddRemote";
            this.AddRemote.Size = new System.Drawing.Size(101, 23);
            this.AddRemote.TabIndex = 17;
            this.toolTip1.SetToolTip(this.AddRemote, resources.GetString("AddRemote.ToolTip"));
            this.AddRemote.UseVisualStyleBackColor = true;
            this.AddRemote.Click += new System.EventHandler(this.AddRemote_Click);
            // 
            // Remotes
            // 
            resources.ApplyResources(this.Remotes, "Remotes");
            this.Remotes.FormattingEnabled = true;
            this.Remotes.Location = new System.Drawing.Point(128, 19);
            this.Remotes.Name = "Remotes";
            this.Remotes.Size = new System.Drawing.Size(297, 21);
            this.Remotes.TabIndex = 16;
            this.toolTip1.SetToolTip(this.Remotes, resources.GetString("Remotes.ToolTip"));
            this.Remotes.SelectedIndexChanged += new System.EventHandler(this.Remotes_SelectedIndexChanged);
            this.Remotes.Validated += new System.EventHandler(this.Remotes_Validated);
            this.Remotes.DropDown += new System.EventHandler(this.Remotes_DropDown);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Branch);
            this.groupBox1.Controls.Add(this.PushAllBranches);
            this.groupBox1.Controls.Add(this.ForcePushBranches);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(527, 111);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.groupBox1, resources.GetString("groupBox1.ToolTip"));
            // 
            // LoadSSHKey
            // 
            resources.ApplyResources(this.LoadSSHKey, "LoadSSHKey");
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(313, 247);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(123, 23);
            this.LoadSSHKey.TabIndex = 23;
            this.toolTip1.SetToolTip(this.LoadSSHKey, resources.GetString("LoadSSHKey.ToolTip"));
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // Tag
            // 
            resources.ApplyResources(this.Tag, "Tag");
            this.Tag.FormattingEnabled = true;
            this.Tag.Location = new System.Drawing.Point(127, 19);
            this.Tag.Name = "Tag";
            this.Tag.Size = new System.Drawing.Size(297, 21);
            this.Tag.TabIndex = 18;
            this.toolTip1.SetToolTip(this.Tag, resources.GetString("Tag.ToolTip"));
            this.Tag.DropDown += new System.EventHandler(this.Tag_DropDown);
            // 
            // groupBox3
            // 
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Controls.Add(this.PushAllTags);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.ForcePushTags);
            this.groupBox3.Controls.Add(this.Tag);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(527, 111);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.toolTip1.SetToolTip(this.groupBox3, resources.GetString("groupBox3.ToolTip"));
            // 
            // PushAllTags
            // 
            resources.ApplyResources(this.PushAllTags, "PushAllTags");
            this.PushAllTags.AutoSize = true;
            this.PushAllTags.Location = new System.Drawing.Point(127, 51);
            this.PushAllTags.Name = "PushAllTags";
            this.PushAllTags.Size = new System.Drawing.Size(86, 17);
            this.PushAllTags.TabIndex = 22;
            this.toolTip1.SetToolTip(this.PushAllTags, resources.GetString("PushAllTags.ToolTip"));
            this.PushAllTags.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 17;
            this.toolTip1.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // TabControlTagBranch
            // 
            resources.ApplyResources(this.TabControlTagBranch, "TabControlTagBranch");
            this.TabControlTagBranch.Controls.Add(this.BranchTab);
            this.TabControlTagBranch.Controls.Add(this.TagTab);
            this.TabControlTagBranch.HotTrack = true;
            this.TabControlTagBranch.ItemSize = new System.Drawing.Size(57, 18);
            this.TabControlTagBranch.Location = new System.Drawing.Point(12, 98);
            this.TabControlTagBranch.Multiline = true;
            this.TabControlTagBranch.Name = "TabControlTagBranch";
            this.TabControlTagBranch.SelectedIndex = 0;
            this.TabControlTagBranch.ShowToolTips = true;
            this.TabControlTagBranch.Size = new System.Drawing.Size(541, 143);
            this.TabControlTagBranch.TabIndex = 24;
            this.toolTip1.SetToolTip(this.TabControlTagBranch, resources.GetString("TabControlTagBranch.ToolTip"));
            // 
            // BranchTab
            // 
            resources.ApplyResources(this.BranchTab, "BranchTab");
            this.BranchTab.BackColor = System.Drawing.Color.Transparent;
            this.BranchTab.Controls.Add(this.groupBox1);
            this.BranchTab.Location = new System.Drawing.Point(4, 22);
            this.BranchTab.Name = "BranchTab";
            this.BranchTab.Padding = new System.Windows.Forms.Padding(3);
            this.BranchTab.Size = new System.Drawing.Size(533, 117);
            this.BranchTab.TabIndex = 0;
            this.toolTip1.SetToolTip(this.BranchTab, resources.GetString("BranchTab.ToolTip"));
            this.BranchTab.UseVisualStyleBackColor = true;
            // 
            // TagTab
            // 
            resources.ApplyResources(this.TagTab, "TagTab");
            this.TagTab.BackColor = System.Drawing.Color.Transparent;
            this.TagTab.Controls.Add(this.groupBox3);
            this.TagTab.Location = new System.Drawing.Point(4, 22);
            this.TagTab.Name = "TagTab";
            this.TagTab.Padding = new System.Windows.Forms.Padding(3);
            this.TagTab.Size = new System.Drawing.Size(533, 117);
            this.TagTab.TabIndex = 1;
            this.toolTip1.SetToolTip(this.TagTab, resources.GetString("TagTab.ToolTip"));
            this.TagTab.UseVisualStyleBackColor = true;
            // 
            // FormPush
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 282);
            this.Controls.Add(this.TabControlTagBranch);
            this.Controls.Add(this.LoadSSHKey);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.Push);
            this.Controls.Add(this.Pull);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            //this.Icon = global::GitUI.Properties.Resources.cow_head;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPush";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.FormPush_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.TabControlTagBranch.ResumeLayout(false);
            this.BranchTab.ResumeLayout(false);
            this.TagTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.Button Push;
        private System.Windows.Forms.ComboBox PushDestination;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Branch;
        private System.Windows.Forms.CheckBox PushAllBranches;
        private System.Windows.Forms.CheckBox ForcePushBranches;
        private System.Windows.Forms.CheckBox ForcePushTags;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton PullFromUrl;
        private System.Windows.Forms.RadioButton PullFromRemote;
        private System.Windows.Forms.Button AddRemote;
        private System.Windows.Forms.ComboBox Remotes;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button LoadSSHKey;
        private new System.Windows.Forms.ComboBox Tag;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl TabControlTagBranch;
        private System.Windows.Forms.TabPage BranchTab;
        private System.Windows.Forms.TabPage TagTab;
        private System.Windows.Forms.CheckBox PushAllTags;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}