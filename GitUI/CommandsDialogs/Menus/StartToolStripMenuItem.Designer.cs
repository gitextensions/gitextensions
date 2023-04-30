using System.Windows.Forms;

namespace GitUI.CommandsDialogs.Menus
{
    partial class StartToolStripMenuItem
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
            this.clearRecentRepositoriesListToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiRecentRepositoriesClear = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFavouriteRepositories = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRecentRepositories = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(122, 22);
            toolStripMenuItem2.Text = "...";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new System.Drawing.Size(83, 22);
            toolStripMenuItem4.Text = "...";
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Image = global::GitUI.Properties.Images.RepoCreate;
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "&Create new repository...";
            this.initNewRepositoryToolStripMenuItem.Click += new System.EventHandler(this.InitNewRepositoryToolStripMenuItemClick);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::GitUI.Properties.Images.RepoOpen;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // tsmiFavouriteRepositories
            // 
            this.tsmiFavouriteRepositories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripMenuItem4});
            this.tsmiFavouriteRepositories.Image = global::GitUI.Properties.Images.Star;
            this.tsmiFavouriteRepositories.Name = "tsmiFavouriteRepositories";
            this.tsmiFavouriteRepositories.Size = new System.Drawing.Size(198, 22);
            this.tsmiFavouriteRepositories.Text = "&Favorite repositories";
            this.tsmiFavouriteRepositories.DropDownOpening += new System.EventHandler(this.tsmiFavouriteRepositories_DropDownOpening);
            // 
            // tsmiRecentRepositories
            // 
            this.tsmiRecentRepositories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripMenuItem2,
            this.clearRecentRepositoriesListToolStripMenuItem,
            this.tsmiRecentRepositoriesClear});
            this.tsmiRecentRepositories.Image = global::GitUI.Properties.Images.RecentRepositories;
            this.tsmiRecentRepositories.Name = "tsmiRecentRepositories";
            this.tsmiRecentRepositories.Size = new System.Drawing.Size(198, 22);
            this.tsmiRecentRepositories.Text = "&Recent repositories";
            this.tsmiRecentRepositories.DropDownOpening += new System.EventHandler(this.tsmiRecentRepositories_DropDownOpening);
            // 
            // clearRecentRepositoriesListToolStripMenuItem
            // 
            this.clearRecentRepositoriesListToolStripMenuItem.Name = "clearRecentRepositoriesListToolStripMenuItem";
            this.clearRecentRepositoriesListToolStripMenuItem.Size = new System.Drawing.Size(119, 6);
            // 
            // tsmiRecentRepositoriesClear
            // 
            this.tsmiRecentRepositoriesClear.Name = "tsmiRecentRepositoriesClear";
            this.tsmiRecentRepositoriesClear.Size = new System.Drawing.Size(122, 22);
            this.tsmiRecentRepositoriesClear.Text = "Clear list";
            this.tsmiRecentRepositoriesClear.Click += new System.EventHandler(this.tsmiRecentRepositoriesClear_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(195, 6);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Image = global::GitUI.Properties.Images.CloneRepoGit;
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.cloneToolStripMenuItem.Text = "C&lone repository...";
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.CloneToolStripMenuItemClick);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(195, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // StartToolStripMenuItem
            // 
            this.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.initNewRepositoryToolStripMenuItem,
            this.openToolStripMenuItem,
            this.tsmiFavouriteRepositories,
            this.tsmiRecentRepositories,
            this.toolStripSeparator12,
            this.cloneToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.Size = new System.Drawing.Size(43, 20);
            this.Text = "&Start";
        }

        #endregion

        private ToolStripMenuItem initNewRepositoryToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem tsmiFavouriteRepositories;
        private ToolStripMenuItem tsmiRecentRepositories;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem cloneToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem tsmiRecentRepositoriesClear;
        private ToolStripSeparator clearRecentRepositoriesListToolStripMenuItem;
    }
}
