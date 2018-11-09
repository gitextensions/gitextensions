using GitUI.UserControls;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class UserRepositoriesList
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
            if (disposing)
            {
                components?.Dispose();

                _foreColorBrush?.Dispose();
                _branchNameColorBrush?.Dispose();
                _favouriteColorBrush?.Dispose();
                _hoverColorBrush?.Dispose();
                _secondaryFont?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Most Recent", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Less Recent", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Other", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "C:\\Users\\russkie\\AppData\\Roaming\\GitExtensions\\GitExtensions",
            "refs/heads/master",
            "Favourite"}, 0);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRepositoriesList));
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblRecentRepositories = new System.Windows.Forms.Label();
            this.tsmiCategories = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryNone = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripRepository = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStripCategory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiRemoveFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveMissingReposFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryRename = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryClear = new System.Windows.Forms.ToolStripMenuItem();
            this.listView1 = new ExListView();
            this.clmhdrPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmhdrBranch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmhdrCategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStripRecentMenu = new System.Windows.Forms.MenuStrip();
            this.mnuTop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConfigure = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStripRepository.SuspendLayout();
            this.contextMenuStripCategory.SuspendLayout();
            this.menuStripRecentMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(221, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(221, 6);
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.tableLayoutPanel1);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeader.MinimumSize = new System.Drawing.Size(450, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20, 0, 20, 11);
            this.pnlHeader.Size = new System.Drawing.Size(451, 73);
            this.pnlHeader.TabIndex = 0;
            // 
            // pnlBody
            // 
            this.pnlBody.Controls.Add(this.listView1);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 0);
            this.pnlBody.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBody.Size = new System.Drawing.Size(451, 73);
            this.pnlBody.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.lblRecentRepositories, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 30);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(411, 32);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // lblRecentRepositories
            // 
            this.lblRecentRepositories.AutoSize = true;
            this.lblRecentRepositories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecentRepositories.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblRecentRepositories.ForeColor = System.Drawing.Color.DimGray;
            this.lblRecentRepositories.Location = new System.Drawing.Point(0, 0);
            this.lblRecentRepositories.Margin = new System.Windows.Forms.Padding(0);
            this.lblRecentRepositories.Name = "lblRecentRepositories";
            this.lblRecentRepositories.Size = new System.Drawing.Size(411, 32);
            this.lblRecentRepositories.TabIndex = 1;
            this.lblRecentRepositories.Text = "Recent Repositories";
            // 
            // tsmiCategories
            // 
            this.tsmiCategories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCategoryNone,
            this.tsmiCategoryAdd});
            this.tsmiCategories.Name = "tsmiCategories";
            this.tsmiCategories.Size = new System.Drawing.Size(224, 22);
            this.tsmiCategories.Text = "Categories";
            this.tsmiCategories.DropDownOpening += new System.EventHandler(this.tsmiCategories_DropDownOpening);
            // 
            // tsmiCategoryNone
            // 
            this.tsmiCategoryNone.Name = "tsmiCategoryNone";
            this.tsmiCategoryNone.Size = new System.Drawing.Size(130, 22);
            this.tsmiCategoryNone.Text = "(none)";
            this.tsmiCategoryNone.Click += new System.EventHandler(this.tsmiCategory_Click);
            // 
            // tsmiCategoryAdd
            // 
            this.tsmiCategoryAdd.Image = global::GitUI.Properties.Images.BulletAdd;
            this.tsmiCategoryAdd.Name = "tsmiCategoryAdd";
            this.tsmiCategoryAdd.Size = new System.Drawing.Size(130, 22);
            this.tsmiCategoryAdd.Text = "Add new...";
            this.tsmiCategoryAdd.Click += new System.EventHandler(this.tsmiCategoryAdd_Click);
            // 
            // contextMenuStripRepository
            // 
            this.contextMenuStripRepository.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiOpenFolder,
            this.toolStripMenuItem1,
            this.tsmiCategories,
            this.toolStripMenuItem2,
            this.tsmiRemoveFromList,
            this.tsmiRemoveMissingReposFromList});
            this.contextMenuStripRepository.Name = "contextMenuStripRepository";
            this.contextMenuStripRepository.Size = new System.Drawing.Size(225, 126);
            this.contextMenuStripRepository.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStrip_Closed);
            this.contextMenuStripRepository.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // contextMenuStripCategory
            // 
            this.contextMenuStripCategory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsmiCategoryRename,
                this.tsmiCategoryDelete,
                this.tsmiCategoryClear});
            this.contextMenuStripCategory.Name = "contextMenuStripCategory";
            this.contextMenuStripRepository.Size = new System.Drawing.Size(225, 66);
            // 
            // tsmiRemoveFromList
            // 
            this.tsmiRemoveFromList.Name = "tsmiRemoveFromList";
            this.tsmiRemoveFromList.Size = new System.Drawing.Size(224, 22);
            this.tsmiRemoveFromList.Text = "Remove project from the list";
            this.tsmiRemoveFromList.Click += new System.EventHandler(this.tsmiRemoveFromList_Click);
            // 
            // tsmiRemoveMissingReposFromList
            // 
            this.tsmiRemoveMissingReposFromList.Name = "tsmiRemoveMissingReposFromList";
            this.tsmiRemoveMissingReposFromList.Size = new System.Drawing.Size(224, 22);
            this.tsmiRemoveMissingReposFromList.Text = "Remove missing projects from the list";
            this.tsmiRemoveMissingReposFromList.Click += new System.EventHandler(this.tsmiRemoveMissingReposFromList_Click);
            // 
            // tsmiOpenFolder
            // 
            this.tsmiOpenFolder.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.tsmiOpenFolder.Name = "tsmiOpenFolder";
            this.tsmiOpenFolder.Size = new System.Drawing.Size(224, 22);
            this.tsmiOpenFolder.Text = "Show in folder";
            this.tsmiOpenFolder.Click += new System.EventHandler(this.tsmiOpenFolder_Click);
            // 
            // tsmiCategoryRename
            // 
            this.tsmiCategoryRename.Image = global::GitUI.Properties.Images.FileStatusModified;
            this.tsmiCategoryRename.Name = "tsmiCategoryRename";
            this.tsmiCategoryRename.Size = new System.Drawing.Size(224, 22);
            this.tsmiCategoryRename.Text = "Rename category";
            this.tsmiCategoryRename.Click += new System.EventHandler(this.tsmiCategoryRename_Click);
            // 
            // tsmiCategoryClear
            // 
            this.tsmiCategoryClear.Image = global::GitUI.Properties.Images.CleanupRepo;
            this.tsmiCategoryClear.Name = "tsmiCategoryClear";
            this.tsmiCategoryClear.Size = new System.Drawing.Size(224, 22);
            this.tsmiCategoryClear.Text = "Clear all recent repositories";
            this.tsmiCategoryClear.Click += new System.EventHandler(this.tsmiCategoryClear_Click);
            // 
            // tsmiCategoryDelete
            // 
            this.tsmiCategoryDelete.Image = global::GitUI.Properties.Images.StarRemove;
            this.tsmiCategoryDelete.Name = "tsmiCategoryDelete";
            this.tsmiCategoryDelete.Size = new System.Drawing.Size(224, 22);
            this.tsmiCategoryDelete.Text = "Delete category";
            this.tsmiCategoryDelete.Click += new System.EventHandler(this.tsmiCategoryDelete_Click);
            // 
            // listView1
            // 
            this.listView1.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmhdrPath,
            this.clmhdrBranch,
            this.clmhdrCategory});
            this.listView1.ContextMenuStrip = this.contextMenuStripRepository;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            listViewGroup1.Header = "Most Recent";
            listViewGroup1.Name = "lvgMostRecent";
            listViewGroup2.Header = "Less Recent";
            listViewGroup2.Name = "lvgLessRecent";
            listViewGroup3.Header = "Other";
            listViewGroup3.Name = "lvgOther";
            this.listView1.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(24, 80);
            this.listView1.Margin = new System.Windows.Forms.Padding(24, 3, 24, 3);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(403, 195);
            this.listView1.TabIndex = 2;
            this.listView1.TileSize = new System.Drawing.Size(350, 50);
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Tile;
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            this.listView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseMove);
            this.listView1.MouseLeave += new System.EventHandler(this.listView1_MouseLeave);
            this.listView1.GroupMouseUp += new System.EventHandler<ListViewGroupMouseEventArgs>(this.listView1_GroupMouseUp);
            // 
            // clmhdrPath
            // 
            this.clmhdrPath.Text = "Path";
            // 
            // clmhdrBranch
            // 
            this.clmhdrBranch.Text = "Branch";
            // 
            // clmhdrCategory
            // 
            this.clmhdrCategory.Text = "Category";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "source_code.png");
            // 
            // menuStripRecentMenu
            // 
            this.menuStripRecentMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTop});
            this.menuStripRecentMenu.Location = new System.Drawing.Point(0, 73);
            this.menuStripRecentMenu.Name = "menuStripRecentMenu";
            this.menuStripRecentMenu.Size = new System.Drawing.Size(451, 24);
            this.menuStripRecentMenu.TabIndex = 3;
            this.menuStripRecentMenu.Visible = false;
            // 
            // mnuTop
            // 
            this.mnuTop.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuConfigure});
            this.mnuTop.Name = "mnuTop";
            this.mnuTop.Size = new System.Drawing.Size(71, 20);
            // 
            // mnuConfigure
            // 
            this.mnuConfigure.Image = global::GitUI.Properties.Images.Settings;
            this.mnuConfigure.Name = "mnuConfigure";
            this.mnuConfigure.Size = new System.Drawing.Size(218, 22);
            this.mnuConfigure.Text = "&Recent repositories settings";
            this.mnuConfigure.Click += new System.EventHandler(this.mnuConfigure_Click);
            // 
            // RecentRepositoriesList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.menuStripRecentMenu);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlHeader);
            this.DoubleBuffered = true;
            this.Name = "RecentRepositoriesList";
            this.Size = new System.Drawing.Size(451, 283);
            this.Load += new System.EventHandler(this.RecentRepositoriesList_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStripRepository.ResumeLayout(false);
            this.contextMenuStripCategory.ResumeLayout(false);
            this.menuStripRecentMenu.ResumeLayout(false);
            this.menuStripRecentMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Label lblRecentRepositories;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRepository;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripCategory;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveFromList;
        private System.Windows.Forms.ToolStripMenuItem tsmiOpenFolder;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategories;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryNone;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveMissingReposFromList;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryRename;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryDelete;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryClear;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ExListView listView1;
        private System.Windows.Forms.ColumnHeader clmhdrPath;
        private System.Windows.Forms.ColumnHeader clmhdrBranch;
        private System.Windows.Forms.ColumnHeader clmhdrCategory;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.MenuStrip menuStripRecentMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuTop;
        private System.Windows.Forms.ToolStripMenuItem mnuConfigure;
    }
}
