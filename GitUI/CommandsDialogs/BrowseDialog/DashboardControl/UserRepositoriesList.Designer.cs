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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Pinned", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Recent", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Other", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "C:\\Users\\russkie\\AppData\\Roaming\\GitExtensions\\GitExtensions",
            "refs/heads/master",
            "Favourite"}, 0);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRepositoriesList));
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Default", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "C:\\Users\\russkie\\AppData\\Roaming\\GitExtensions\\GitExtensions",
            "refs/heads/master",
            "Favourite"}, 0);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.lblRecentRepositories = new System.Windows.Forms.Label();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.listViewRecentRepositories = new GitUI.UserControls.NativeListView();
            this.clmhdrPath = new System.Windows.Forms.ColumnHeader();
            this.clmhdrBranch = new System.Windows.Forms.ColumnHeader();
            this.clmhdrCategory = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStripRepository = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategories = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryNone = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveMissingReposFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListLarge = new System.Windows.Forms.ImageList(this.components);
            this.imageListSmall = new System.Windows.Forms.ImageList(this.components);
            this.listViewAllRepositories = new GitUI.UserControls.NativeListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStripCategory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCategoryRename = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryClear = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStripRecentMenu = new System.Windows.Forms.MenuStrip();
            this.mnuTop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConfigure = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlHeader.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.flowLayoutPanel.SuspendLayout();
            this.contextMenuStripRepository.SuspendLayout();
            this.contextMenuStripCategory.SuspendLayout();
            this.menuStripRecentMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(270, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(270, 6);
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxSearch, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblRecentRepositories, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 30);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(411, 32);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSearch.Location = new System.Drawing.Point(220, 3);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(188, 23);
            this.textBoxSearch.TabIndex = 0;
            // 
            // lblRecentRepositories
            // 
            this.lblRecentRepositories.AutoSize = true;
            this.lblRecentRepositories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecentRepositories.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRecentRepositories.ForeColor = System.Drawing.Color.DimGray;
            this.lblRecentRepositories.Location = new System.Drawing.Point(0, 0);
            this.lblRecentRepositories.Margin = new System.Windows.Forms.Padding(0);
            this.lblRecentRepositories.Name = "lblRecentRepositories";
            this.lblRecentRepositories.Size = new System.Drawing.Size(217, 32);
            this.lblRecentRepositories.TabIndex = 2;
            this.lblRecentRepositories.Text = "Recent repositories";
            // 
            // pnlBody
            // 
            this.pnlBody.Controls.Add(this.flowLayoutPanel);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 73);
            this.pnlBody.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20, 18, 20, 3);
            this.pnlBody.Size = new System.Drawing.Size(451, 210);
            this.pnlBody.TabIndex = 1;
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.flowLayoutPanel.Controls.Add(this.listViewRecentRepositories);
            this.flowLayoutPanel.Controls.Add(this.listViewAllRepositories);
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.Location = new System.Drawing.Point(20, 18);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(411, 189);
            this.flowLayoutPanel.TabIndex = 3;
            this.flowLayoutPanel.SizeChanged += new System.EventHandler(this.flowLayoutPanel_SizeChanged);
            // 
            // listViewRecentRepositories
            // 
            this.listViewRecentRepositories.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listViewRecentRepositories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewRecentRepositories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmhdrPath,
            this.clmhdrBranch,
            this.clmhdrCategory});
            this.listViewRecentRepositories.ContextMenuStrip = this.contextMenuStripRepository;
            this.listViewRecentRepositories.FullRowSelect = true;
            listViewGroup1.Header = "Pinned";
            listViewGroup1.Name = "lvgPinned";
            listViewGroup2.Header = "Recent";
            listViewGroup2.Name = "lvgAllRecent";
            listViewGroup3.Header = "Other";
            listViewGroup3.Name = "lvgOther";
            this.listViewRecentRepositories.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.listViewRecentRepositories.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewRecentRepositories.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listViewRecentRepositories.LargeImageList = this.imageListLarge;
            this.listViewRecentRepositories.Location = new System.Drawing.Point(0, 3);
            this.listViewRecentRepositories.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.listViewRecentRepositories.MinimumSize = new System.Drawing.Size(500, 120);
            this.listViewRecentRepositories.MultiSelect = false;
            this.listViewRecentRepositories.Name = "listViewRecentRepositories";
            this.listViewRecentRepositories.OwnerDraw = true;
            this.listViewRecentRepositories.ShowItemToolTips = true;
            this.listViewRecentRepositories.Size = new System.Drawing.Size(500, 120);
            this.listViewRecentRepositories.SmallImageList = this.imageListSmall;
            this.listViewRecentRepositories.TabIndex = 1;
            this.listViewRecentRepositories.TileSize = new System.Drawing.Size(350, 50);
            this.listViewRecentRepositories.UseCompatibleStateImageBehavior = false;
            this.listViewRecentRepositories.View = System.Windows.Forms.View.Tile;
            this.listViewRecentRepositories.GroupTaskLinkClick += new System.EventHandler<System.Windows.Forms.ListViewGroupEventArgs>(this.ListView1_GroupTaskLinkClick);
            this.listViewRecentRepositories.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listViewRecentRepositories.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            this.listViewRecentRepositories.MouseLeave += new System.EventHandler(this.listView1_MouseLeave);
            this.listViewRecentRepositories.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseMove);
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
            this.contextMenuStripRepository.Size = new System.Drawing.Size(274, 104);
            this.contextMenuStripRepository.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStrip_Closed);
            this.contextMenuStripRepository.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // tsmiOpenFolder
            // 
            this.tsmiOpenFolder.Image = global::GitUI.Properties.Images.BrowseFileExplorer;
            this.tsmiOpenFolder.Name = "tsmiOpenFolder";
            this.tsmiOpenFolder.Size = new System.Drawing.Size(273, 22);
            this.tsmiOpenFolder.Text = "Show in folder";
            this.tsmiOpenFolder.Click += new System.EventHandler(this.tsmiOpenFolder_Click);
            // 
            // tsmiCategories
            // 
            this.tsmiCategories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCategoryNone,
            this.tsmiCategoryAdd});
            this.tsmiCategories.Name = "tsmiCategories";
            this.tsmiCategories.Size = new System.Drawing.Size(273, 22);
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
            // tsmiRemoveFromList
            // 
            this.tsmiRemoveFromList.Name = "tsmiRemoveFromList";
            this.tsmiRemoveFromList.Size = new System.Drawing.Size(273, 22);
            this.tsmiRemoveFromList.Text = "Remove project from the list";
            this.tsmiRemoveFromList.Click += new System.EventHandler(this.tsmiRemoveFromList_Click);
            // 
            // tsmiRemoveMissingReposFromList
            // 
            this.tsmiRemoveMissingReposFromList.Name = "tsmiRemoveMissingReposFromList";
            this.tsmiRemoveMissingReposFromList.Size = new System.Drawing.Size(273, 22);
            this.tsmiRemoveMissingReposFromList.Text = "Remove missing projects from the list";
            this.tsmiRemoveMissingReposFromList.Click += new System.EventHandler(this.tsmiRemoveMissingReposFromList_Click);
            // 
            // imageListLarge
            // 
            this.imageListLarge.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageListLarge.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLarge.ImageStream")));
            this.imageListLarge.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListLarge.Images.SetKeyName(0, "source_code.png");
            // 
            // imageListSmall
            // 
            this.imageListSmall.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageListSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSmall.ImageStream")));
            this.imageListSmall.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSmall.Images.SetKeyName(0, "source_code.png");
            // 
            // listViewAllRepositories
            // 
            this.listViewAllRepositories.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listViewAllRepositories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewAllRepositories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewAllRepositories.ContextMenuStrip = this.contextMenuStripRepository;
            this.listViewAllRepositories.FullRowSelect = true;
            listViewGroup4.Header = "Default";
            listViewGroup4.Name = "lvgDefaultGroup";
            this.listViewAllRepositories.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup4});
            this.listViewAllRepositories.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewAllRepositories.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.listViewAllRepositories.LargeImageList = this.imageListLarge;
            this.listViewAllRepositories.Location = new System.Drawing.Point(0, 129);
            this.listViewAllRepositories.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.listViewAllRepositories.MinimumSize = new System.Drawing.Size(500, 120);
            this.listViewAllRepositories.MultiSelect = false;
            this.listViewAllRepositories.Name = "listViewAllRepositories";
            this.listViewAllRepositories.OwnerDraw = true;
            this.listViewAllRepositories.ShowItemToolTips = true;
            this.listViewAllRepositories.Size = new System.Drawing.Size(500, 120);
            this.listViewAllRepositories.SmallImageList = this.imageListSmall;
            this.listViewAllRepositories.TabIndex = 2;
            this.listViewAllRepositories.TileSize = new System.Drawing.Size(350, 50);
            this.listViewAllRepositories.UseCompatibleStateImageBehavior = false;
            this.listViewAllRepositories.View = System.Windows.Forms.View.Tile;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Path";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Branch";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Category";
            // 
            // contextMenuStripCategory
            // 
            this.contextMenuStripCategory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCategoryRename,
            this.tsmiCategoryDelete,
            this.tsmiCategoryClear});
            this.contextMenuStripCategory.Name = "contextMenuStripCategory";
            this.contextMenuStripCategory.Size = new System.Drawing.Size(217, 70);
            // 
            // tsmiCategoryRename
            // 
            this.tsmiCategoryRename.Image = global::GitUI.Properties.Images.FileStatusModified;
            this.tsmiCategoryRename.Name = "tsmiCategoryRename";
            this.tsmiCategoryRename.Size = new System.Drawing.Size(216, 22);
            this.tsmiCategoryRename.Text = "Rename category";
            this.tsmiCategoryRename.Click += new System.EventHandler(this.tsmiCategoryRename_Click);
            // 
            // tsmiCategoryDelete
            // 
            this.tsmiCategoryDelete.Image = global::GitUI.Properties.Images.StarRemove;
            this.tsmiCategoryDelete.Name = "tsmiCategoryDelete";
            this.tsmiCategoryDelete.Size = new System.Drawing.Size(216, 22);
            this.tsmiCategoryDelete.Text = "Delete category";
            this.tsmiCategoryDelete.Click += new System.EventHandler(this.tsmiCategoryDelete_Click);
            // 
            // tsmiCategoryClear
            // 
            this.tsmiCategoryClear.Image = global::GitUI.Properties.Images.CleanupRepo;
            this.tsmiCategoryClear.Name = "tsmiCategoryClear";
            this.tsmiCategoryClear.Size = new System.Drawing.Size(216, 22);
            this.tsmiCategoryClear.Text = "Clear all recent repositories";
            this.tsmiCategoryClear.Click += new System.EventHandler(this.tsmiCategoryClear_Click);
            // 
            // menuStripRecentMenu
            // 
            this.menuStripRecentMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTop});
            this.menuStripRecentMenu.Location = new System.Drawing.Point(0, 0);
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
            this.mnuTop.Size = new System.Drawing.Size(12, 20);
            // 
            // mnuConfigure
            // 
            this.mnuConfigure.Image = global::GitUI.Properties.Images.Settings;
            this.mnuConfigure.Name = "mnuConfigure";
            this.mnuConfigure.Size = new System.Drawing.Size(218, 22);
            this.mnuConfigure.Text = "Recent repositories &settings";
            this.mnuConfigure.Click += new System.EventHandler(this.mnuConfigure_Click);
            // 
            // UserRepositoriesList
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.menuStripRecentMenu);
            this.DoubleBuffered = true;
            this.Name = "UserRepositoriesList";
            this.Size = new System.Drawing.Size(451, 283);
            this.Load += new System.EventHandler(this.RecentRepositoriesList_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            this.flowLayoutPanel.ResumeLayout(false);
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
        private GitUI.UserControls.NativeListView listViewRecentRepositories;
        private System.Windows.Forms.ColumnHeader clmhdrPath;
        private System.Windows.Forms.ColumnHeader clmhdrBranch;
        private System.Windows.Forms.ColumnHeader clmhdrCategory;
        private System.Windows.Forms.ImageList imageListLarge;
        private System.Windows.Forms.MenuStrip menuStripRecentMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuTop;
        private System.Windows.Forms.ToolStripMenuItem mnuConfigure;
        private System.Windows.Forms.TextBox textBoxSearch;
        private UserControls.NativeListView listViewAllRepositories;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private FlowLayoutPanel flowLayoutPanel;
        private ImageList imageListSmall;
    }
}
