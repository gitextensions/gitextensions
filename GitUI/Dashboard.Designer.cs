namespace GitUI
{
    partial class Dashboard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.CommonActions = new GitUI.DashboardCategory();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.RecentRepositories = new GitUI.DashboardCategory();
            this.DonateCategory = new GitUI.DashboardCategory();
            this.groupLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer5
            // 
            this.splitContainer5.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer5.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.splitContainer5.Panel2.Controls.Add(this.groupLayoutPanel);
            this.splitContainer5.Size = new System.Drawing.Size(972, 623);
            this.splitContainer5.SplitterDistance = 314;
            this.splitContainer5.SplitterWidth = 5;
            this.splitContainer5.TabIndex = 9;
            // 
            // splitContainer6
            // 
            this.splitContainer6.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer6.Panel1.Controls.Add(this.CommonActions);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.splitContainer7);
            this.splitContainer6.Size = new System.Drawing.Size(314, 623);
            this.splitContainer6.SplitterDistance = 126;
            this.splitContainer6.SplitterWidth = 5;
            this.splitContainer6.TabIndex = 0;
            // 
            // CommonActions
            // 
            this.CommonActions.AutoSize = true;
            this.CommonActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CommonActions.BackColor = System.Drawing.Color.Transparent;
            this.CommonActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommonActions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.CommonActions.Location = new System.Drawing.Point(0, 0);
            this.CommonActions.Name = "CommonActions";
            this.CommonActions.RepositoryCategory = null;
            this.CommonActions.Size = new System.Drawing.Size(314, 126);
            this.CommonActions.TabIndex = 8;
            this.CommonActions.Title = "Common Actions";
            // 
            // splitContainer7
            // 
            this.splitContainer7.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer7.Panel1.Controls.Add(this.RecentRepositories);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer7.Panel2.Controls.Add(this.DonateCategory);
            this.splitContainer7.Size = new System.Drawing.Size(314, 492);
            this.splitContainer7.SplitterDistance = 412;
            this.splitContainer7.SplitterWidth = 5;
            this.splitContainer7.TabIndex = 0;
            // 
            // RecentRepositories
            // 
            this.RecentRepositories.AutoSize = true;
            this.RecentRepositories.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RecentRepositories.BackColor = System.Drawing.Color.Transparent;
            this.RecentRepositories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecentRepositories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.RecentRepositories.Location = new System.Drawing.Point(0, 0);
            this.RecentRepositories.Name = "RecentRepositories";
            this.RecentRepositories.RepositoryCategory = null;
            this.RecentRepositories.Size = new System.Drawing.Size(314, 412);
            this.RecentRepositories.TabIndex = 0;
            this.RecentRepositories.Title = "Recent Repositories";
            // 
            // DonateCategory
            // 
            this.DonateCategory.AutoSize = true;
            this.DonateCategory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DonateCategory.BackColor = System.Drawing.Color.Transparent;
            this.DonateCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DonateCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.DonateCategory.Location = new System.Drawing.Point(0, 0);
            this.DonateCategory.Name = "DonateCategory";
            this.DonateCategory.RepositoryCategory = null;
            this.DonateCategory.Size = new System.Drawing.Size(314, 75);
            this.DonateCategory.TabIndex = 0;
            this.DonateCategory.Title = "Contribute";
            // 
            // groupLayoutPanel
            // 
            this.groupLayoutPanel.AllowDrop = true;
            this.groupLayoutPanel.AutoScroll = true;
            this.groupLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.groupLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.groupLayoutPanel.Name = "groupLayoutPanel";
            this.groupLayoutPanel.Size = new System.Drawing.Size(653, 623);
            this.groupLayoutPanel.TabIndex = 0;
            this.groupLayoutPanel.WrapContents = false;
            this.groupLayoutPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.groupLayoutPanel_DragDrop);
            this.groupLayoutPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.groupLayoutPanel_DragEnter);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::GitUI.Properties.Resources.git_extensions_logo_final_128;
            this.pictureBox1.Location = new System.Drawing.Point(299, 328);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(128, 128);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.splitContainer5);
            this.Name = "Dashboard";
            this.Size = new System.Drawing.Size(972, 623);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel1.PerformLayout();
            this.splitContainer6.Panel2.ResumeLayout(false);
            this.splitContainer6.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel1.PerformLayout();
            this.splitContainer7.Panel2.ResumeLayout(false);
            this.splitContainer7.Panel2.PerformLayout();
            this.splitContainer7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private DashboardCategory RecentRepositories;
        private DashboardCategory CommonActions;
        private DashboardCategory DonateCategory;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.FlowLayoutPanel groupLayoutPanel;
    }
}
