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
            resources.ApplyResources(this.splitContainer5, "splitContainer5");
            this.splitContainer5.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer5.DataBindings.Add(new System.Windows.Forms.Binding("SplitterDistance", global::GitUI.Properties.Settings.Default, "Dashboard_MainSplitContainer_SplitterDistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            resources.ApplyResources(this.splitContainer5.Panel1, "splitContainer5.Panel1");
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            resources.ApplyResources(this.splitContainer5.Panel2, "splitContainer5.Panel2");
            this.splitContainer5.Panel2.Controls.Add(this.groupLayoutPanel);
            this.splitContainer5.SplitterDistance = global::GitUI.Properties.Settings.Default.Dashboard_MainSplitContainer_SplitterDistance;
            // 
            // splitContainer6
            // 
            resources.ApplyResources(this.splitContainer6, "splitContainer6");
            this.splitContainer6.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            resources.ApplyResources(this.splitContainer6.Panel1, "splitContainer6.Panel1");
            this.splitContainer6.Panel1.Controls.Add(this.CommonActions);
            // 
            // splitContainer6.Panel2
            // 
            resources.ApplyResources(this.splitContainer6.Panel2, "splitContainer6.Panel2");
            this.splitContainer6.Panel2.Controls.Add(this.splitContainer7);
            // 
            // CommonActions
            // 
            resources.ApplyResources(this.CommonActions, "CommonActions");
            this.CommonActions.BackColor = System.Drawing.Color.Transparent;
            this.CommonActions.Name = "CommonActions";
            this.CommonActions.RepositoryCategory = null;
            this.CommonActions.Title = "Common Actions";
            // 
            // splitContainer7
            // 
            resources.ApplyResources(this.splitContainer7, "splitContainer7");
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.Name = "splitContainer7";
            // 
            // splitContainer7.Panel1
            // 
            resources.ApplyResources(this.splitContainer7.Panel1, "splitContainer7.Panel1");
            this.splitContainer7.Panel1.Controls.Add(this.RecentRepositories);
            // 
            // splitContainer7.Panel2
            // 
            resources.ApplyResources(this.splitContainer7.Panel2, "splitContainer7.Panel2");
            this.splitContainer7.Panel2.Controls.Add(this.DonateCategory);
            // 
            // RecentRepositories
            // 
            resources.ApplyResources(this.RecentRepositories, "RecentRepositories");
            this.RecentRepositories.BackColor = System.Drawing.Color.Transparent;
            this.RecentRepositories.Name = "RecentRepositories";
            this.RecentRepositories.RepositoryCategory = null;
            this.RecentRepositories.Title = "Recent Repositories";
            // 
            // DonateCategory
            // 
            resources.ApplyResources(this.DonateCategory, "DonateCategory");
            this.DonateCategory.BackColor = System.Drawing.Color.Transparent;
            this.DonateCategory.Name = "DonateCategory";
            this.DonateCategory.RepositoryCategory = null;
            this.DonateCategory.Title = "Contribute";
            // 
            // groupLayoutPanel
            // 
            resources.ApplyResources(this.groupLayoutPanel, "groupLayoutPanel");
            this.groupLayoutPanel.Name = "groupLayoutPanel";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::GitUI.Properties.Resources.git_extensions_logo_final_128;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // Dashboard
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.splitContainer5);
            this.Name = "Dashboard";
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
