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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dashboard));
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.CommonActions = new GitUI.DashboardCategory();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.RecentRepositories = new GitUI.DashboardCategory();
            this.DonateCategory = new GitUI.DashboardCategory();
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
            this.splitContainer5.AccessibleDescription = null;
            this.splitContainer5.AccessibleName = null;
            resources.ApplyResources(this.splitContainer5, "splitContainer5");
            this.splitContainer5.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer5.BackgroundImage = null;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Font = null;
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.AccessibleDescription = null;
            this.splitContainer5.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer5.Panel1, "splitContainer5.Panel1");
            this.splitContainer5.Panel1.BackgroundImage = null;
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            this.splitContainer5.Panel1.Font = null;
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.AccessibleDescription = null;
            this.splitContainer5.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer5.Panel2, "splitContainer5.Panel2");
            this.splitContainer5.Panel2.BackgroundImage = null;
            this.splitContainer5.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer5.Panel2.Font = null;
            this.splitContainer5.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer5_SplitterMoved);
            // 
            // splitContainer6
            // 
            this.splitContainer6.AccessibleDescription = null;
            this.splitContainer6.AccessibleName = null;
            resources.ApplyResources(this.splitContainer6, "splitContainer6");
            this.splitContainer6.BackgroundImage = null;
            this.splitContainer6.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer6.Font = null;
            this.splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.AccessibleDescription = null;
            this.splitContainer6.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer6.Panel1, "splitContainer6.Panel1");
            this.splitContainer6.Panel1.BackgroundImage = null;
            this.splitContainer6.Panel1.Controls.Add(this.CommonActions);
            this.splitContainer6.Panel1.Font = null;
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.AccessibleDescription = null;
            this.splitContainer6.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer6.Panel2, "splitContainer6.Panel2");
            this.splitContainer6.Panel2.BackgroundImage = null;
            this.splitContainer6.Panel2.Controls.Add(this.splitContainer7);
            this.splitContainer6.Panel2.Font = null;
            // 
            // CommonActions
            // 
            this.CommonActions.AccessibleDescription = null;
            this.CommonActions.AccessibleName = null;
            resources.ApplyResources(this.CommonActions, "CommonActions");
            this.CommonActions.BackColor = System.Drawing.Color.Transparent;
            this.CommonActions.BackgroundImage = null;
            this.CommonActions.Font = null;
            this.CommonActions.Name = "CommonActions";
            this.CommonActions.RepositoryCategory = null;
            this.CommonActions.Title = "Common Actions";
            // 
            // splitContainer7
            // 
            this.splitContainer7.AccessibleDescription = null;
            this.splitContainer7.AccessibleName = null;
            resources.ApplyResources(this.splitContainer7, "splitContainer7");
            this.splitContainer7.BackgroundImage = null;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.Font = null;
            this.splitContainer7.Name = "splitContainer7";
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.AccessibleDescription = null;
            this.splitContainer7.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer7.Panel1, "splitContainer7.Panel1");
            this.splitContainer7.Panel1.BackgroundImage = null;
            this.splitContainer7.Panel1.Controls.Add(this.RecentRepositories);
            this.splitContainer7.Panel1.Font = null;
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.AccessibleDescription = null;
            this.splitContainer7.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer7.Panel2, "splitContainer7.Panel2");
            this.splitContainer7.Panel2.BackgroundImage = null;
            this.splitContainer7.Panel2.Controls.Add(this.DonateCategory);
            this.splitContainer7.Panel2.Font = null;
            // 
            // RecentRepositories
            // 
            this.RecentRepositories.AccessibleDescription = null;
            this.RecentRepositories.AccessibleName = null;
            resources.ApplyResources(this.RecentRepositories, "RecentRepositories");
            this.RecentRepositories.BackColor = System.Drawing.Color.Transparent;
            this.RecentRepositories.BackgroundImage = null;
            this.RecentRepositories.Font = null;
            this.RecentRepositories.Name = "RecentRepositories";
            this.RecentRepositories.RepositoryCategory = null;
            this.RecentRepositories.Title = "Recent Repositories";
            // 
            // DonateCategory
            // 
            this.DonateCategory.AccessibleDescription = null;
            this.DonateCategory.AccessibleName = null;
            resources.ApplyResources(this.DonateCategory, "DonateCategory");
            this.DonateCategory.BackColor = System.Drawing.Color.Transparent;
            this.DonateCategory.BackgroundImage = null;
            this.DonateCategory.Font = null;
            this.DonateCategory.Name = "DonateCategory";
            this.DonateCategory.RepositoryCategory = null;
            this.DonateCategory.Title = "Contribute";
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleDescription = null;
            this.pictureBox1.AccessibleName = null;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.BackgroundImage = null;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Font = null;
            this.pictureBox1.Image = global::GitUI.Properties.Resources.Cow1;
            this.pictureBox1.ImageLocation = null;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // Dashboard
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = null;
            this.Controls.Add(this.splitContainer5);
            this.Name = "Dashboard";
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            this.splitContainer6.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            this.splitContainer7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private DashboardCategory RecentRepositories;
        private DashboardCategory CommonActions;
        private DashboardCategory DonateCategory;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
