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
            ToolStripMenuItem toolStripMenuItem2;
            ToolStripMenuItem toolStripMenuItem4;
            clearRecentRepositoriesListToolStripMenuItem = new ToolStripSeparator();
            tsmiRecentRepositoriesClear = new ToolStripMenuItem();
            initNewRepositoryToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            tsmiFavouriteRepositories = new ToolStripMenuItem();
            tsmiRecentRepositories = new ToolStripMenuItem();
            toolStripSeparator12 = new ToolStripSeparator();
            cloneToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(122, 22);
            toolStripMenuItem2.Text = "...";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(83, 22);
            toolStripMenuItem4.Text = "...";
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            initNewRepositoryToolStripMenuItem.Image = Properties.Images.RepoCreate;
            initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            initNewRepositoryToolStripMenuItem.Size = new Size(198, 22);
            initNewRepositoryToolStripMenuItem.Text = "&Create new repository...";
            initNewRepositoryToolStripMenuItem.Click += InitNewRepositoryToolStripMenuItemClick;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = Properties.Images.RepoOpen;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(198, 22);
            openToolStripMenuItem.Text = "&Open...";
            openToolStripMenuItem.Click += OpenToolStripMenuItemClick;
            // 
            // tsmiFavouriteRepositories
            // 
            tsmiFavouriteRepositories.DropDownItems.AddRange(new ToolStripItem[] {
            toolStripMenuItem4});
            tsmiFavouriteRepositories.Image = Properties.Images.Star;
            tsmiFavouriteRepositories.Name = "tsmiFavouriteRepositories";
            tsmiFavouriteRepositories.Size = new Size(198, 22);
            tsmiFavouriteRepositories.Text = "&Favorite repositories";
            tsmiFavouriteRepositories.DropDownOpening += tsmiFavouriteRepositories_DropDownOpening;
            // 
            // tsmiRecentRepositories
            // 
            tsmiRecentRepositories.DropDownItems.AddRange(new ToolStripItem[] {
            toolStripMenuItem2,
            clearRecentRepositoriesListToolStripMenuItem,
            tsmiRecentRepositoriesClear});
            tsmiRecentRepositories.Image = Properties.Images.RecentRepositories;
            tsmiRecentRepositories.Name = "tsmiRecentRepositories";
            tsmiRecentRepositories.Size = new Size(198, 22);
            tsmiRecentRepositories.Text = "&Recent repositories";
            tsmiRecentRepositories.DropDownOpening += tsmiRecentRepositories_DropDownOpening;
            // 
            // clearRecentRepositoriesListToolStripMenuItem
            // 
            clearRecentRepositoriesListToolStripMenuItem.Name = "clearRecentRepositoriesListToolStripMenuItem";
            clearRecentRepositoriesListToolStripMenuItem.Size = new Size(119, 6);
            // 
            // tsmiRecentRepositoriesClear
            // 
            tsmiRecentRepositoriesClear.Name = "tsmiRecentRepositoriesClear";
            tsmiRecentRepositoriesClear.Size = new Size(122, 22);
            tsmiRecentRepositoriesClear.Text = "Clear list";
            tsmiRecentRepositoriesClear.Click += tsmiRecentRepositoriesClear_Click;
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(195, 6);
            // 
            // cloneToolStripMenuItem
            // 
            cloneToolStripMenuItem.Image = Properties.Images.CloneRepoGit;
            cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            cloneToolStripMenuItem.Size = new Size(198, 22);
            cloneToolStripMenuItem.Text = "C&lone repository...";
            cloneToolStripMenuItem.Click += CloneToolStripMenuItemClick;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(195, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.Q)));
            exitToolStripMenuItem.Size = new Size(198, 22);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItemClick;
            // 
            // StartToolStripMenuItem
            // 
            DropDownItems.AddRange(new ToolStripItem[] {
            initNewRepositoryToolStripMenuItem,
            openToolStripMenuItem,
            tsmiFavouriteRepositories,
            tsmiRecentRepositories,
            toolStripSeparator12,
            cloneToolStripMenuItem,
            toolStripMenuItem1,
            exitToolStripMenuItem});
            Size = new Size(43, 20);
            Text = "&Start";
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
