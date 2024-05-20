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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRepositoriesList));
            ListViewGroup lvgPinned = new ListViewGroup("Pinned", HorizontalAlignment.Left);
            ListViewGroup lvgAllRecent = new ListViewGroup("Recent", HorizontalAlignment.Left);
            ListViewGroup lvgOther = new ListViewGroup("Other", HorizontalAlignment.Left);
            ListViewItem listViewItem1 = new ListViewItem(new string[] {
            "C:\\Users\\russkie\\AppData\\Roaming\\GitExtensions\\GitExtensions",
            "refs/heads/master",
            "Favourite"}, 0);
            toolStripMenuItem1 = new ToolStripSeparator();
            toolStripMenuItem2 = new ToolStripSeparator();
            pnlHeader = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblRecentRepositories = new Label();
            pnlBody = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            contextMenuStripRepository = new ContextMenuStrip(components);
            tsmiOpenFolder = new ToolStripMenuItem();
            tsmiCategories = new ToolStripMenuItem();
            tsmiCategoryNone = new ToolStripMenuItem();
            tsmiCategoryAdd = new ToolStripMenuItem();
            tsmiRemoveFromList = new ToolStripMenuItem();
            tsmiRemoveMissingReposFromList = new ToolStripMenuItem();
            imageList1 = new ImageList(components);
            textBoxSearch = new TextBox();
            contextMenuStripCategory = new ContextMenuStrip(components);
            tsmiCategoryRename = new ToolStripMenuItem();
            tsmiCategoryDelete = new ToolStripMenuItem();
            tsmiCategoryClear = new ToolStripMenuItem();
            menuStripRecentMenu = new MenuStrip();
            mnuTop = new ToolStripMenuItem();
            mnuConfigure = new ToolStripMenuItem();
            listView1 = new GitUI.UserControls.NativeListView();
            clmhdrPath = ((ColumnHeader)(new ColumnHeader()));
            clmhdrBranch = ((ColumnHeader)(new ColumnHeader()));
            clmhdrCategory = ((ColumnHeader)(new ColumnHeader()));
            pnlHeader.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            pnlBody.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            contextMenuStripRepository.SuspendLayout();
            contextMenuStripCategory.SuspendLayout();
            menuStripRecentMenu.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(270, 6);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(270, 6);
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(tableLayoutPanel1);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(0);
            pnlHeader.MinimumSize = new Size(450, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(20, 0, 20, 11);
            pnlHeader.Size = new Size(451, 73);
            pnlHeader.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(lblRecentRepositories, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Bottom;
            tableLayoutPanel1.Location = new Point(20, 30);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(411, 32);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // lblRecentRepositories
            // 
            lblRecentRepositories.AutoSize = true;
            lblRecentRepositories.Dock = DockStyle.Fill;
            lblRecentRepositories.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(204)));
            lblRecentRepositories.ForeColor = Color.DimGray;
            lblRecentRepositories.Location = new Point(0, 0);
            lblRecentRepositories.Margin = new Padding(0);
            lblRecentRepositories.Name = "lblRecentRepositories";
            lblRecentRepositories.Size = new Size(411, 32);
            lblRecentRepositories.TabIndex = 2;
            lblRecentRepositories.Text = "Recent repositories";
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(tableLayoutPanel2);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 73);
            pnlBody.Margin = new Padding(0);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new Padding(20, 18, 20, 3);
            pnlBody.Size = new Size(451, 210);
            pnlBody.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(listView1, 0, 1);
            tableLayoutPanel2.Controls.Add(textBoxSearch, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(20, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(411, 204);
            tableLayoutPanel2.TabIndex = 4;
            // 
            // contextMenuStripRepository
            // 
            contextMenuStripRepository.Items.AddRange(new ToolStripItem[] {
            tsmiOpenFolder,
            toolStripMenuItem1,
            tsmiCategories,
            toolStripMenuItem2,
            tsmiRemoveFromList,
            tsmiRemoveMissingReposFromList});
            contextMenuStripRepository.Name = "contextMenuStripRepository";
            contextMenuStripRepository.Size = new Size(274, 104);
            contextMenuStripRepository.Closed += contextMenuStrip_Closed;
            contextMenuStripRepository.Opening += contextMenuStrip_Opening;
            // 
            // tsmiOpenFolder
            // 
            tsmiOpenFolder.Image = Properties.Images.BrowseFileExplorer;
            tsmiOpenFolder.Name = "tsmiOpenFolder";
            tsmiOpenFolder.Size = new Size(273, 22);
            tsmiOpenFolder.Text = "Show in folder";
            tsmiOpenFolder.Click += tsmiOpenFolder_Click;
            // 
            // tsmiCategories
            // 
            tsmiCategories.DropDownItems.AddRange(new ToolStripItem[] {
            tsmiCategoryNone,
            tsmiCategoryAdd});
            tsmiCategories.Name = "tsmiCategories";
            tsmiCategories.Size = new Size(273, 22);
            tsmiCategories.Text = "Categories";
            tsmiCategories.DropDownOpening += tsmiCategories_DropDownOpening;
            // 
            // tsmiCategoryNone
            // 
            tsmiCategoryNone.Name = "tsmiCategoryNone";
            tsmiCategoryNone.Size = new Size(130, 22);
            tsmiCategoryNone.Text = "(none)";
            tsmiCategoryNone.Click += tsmiCategory_Click;
            // 
            // tsmiCategoryAdd
            // 
            tsmiCategoryAdd.Image = Properties.Images.BulletAdd;
            tsmiCategoryAdd.Name = "tsmiCategoryAdd";
            tsmiCategoryAdd.Size = new Size(130, 22);
            tsmiCategoryAdd.Text = "Add new...";
            tsmiCategoryAdd.Click += tsmiCategoryAdd_Click;
            // 
            // tsmiRemoveFromList
            // 
            tsmiRemoveFromList.Name = "tsmiRemoveFromList";
            tsmiRemoveFromList.Size = new Size(273, 22);
            tsmiRemoveFromList.Text = "Remove project from the list";
            tsmiRemoveFromList.Click += tsmiRemoveFromList_Click;
            // 
            // tsmiRemoveMissingReposFromList
            // 
            tsmiRemoveMissingReposFromList.Name = "tsmiRemoveMissingReposFromList";
            tsmiRemoveMissingReposFromList.Size = new Size(273, 22);
            tsmiRemoveMissingReposFromList.Text = "Remove missing projects from the list";
            tsmiRemoveMissingReposFromList.Click += tsmiRemoveMissingReposFromList_Click;
            // 
            // imageList1
            // 
            imageList1.ImageStream = ((ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "source_code.png");
            // 
            // textBoxSearch
            // 
            textBoxSearch.Dock = DockStyle.Fill;
            textBoxSearch.Location = new Point(3, 3);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(439, 20);
            textBoxSearch.TabIndex = 0;
            textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
            // 
            // contextMenuStripCategory
            // 
            contextMenuStripCategory.Items.AddRange(new ToolStripItem[] {
            tsmiCategoryRename,
            tsmiCategoryDelete,
            tsmiCategoryClear});
            contextMenuStripCategory.Name = "contextMenuStripCategory";
            contextMenuStripCategory.Size = new Size(217, 70);
            // 
            // tsmiCategoryRename
            // 
            tsmiCategoryRename.Image = Properties.Images.FileStatusModified;
            tsmiCategoryRename.Name = "tsmiCategoryRename";
            tsmiCategoryRename.Size = new Size(216, 22);
            tsmiCategoryRename.Text = "Rename category";
            tsmiCategoryRename.Click += tsmiCategoryRename_Click;
            // 
            // tsmiCategoryDelete
            // 
            tsmiCategoryDelete.Image = Properties.Images.StarRemove;
            tsmiCategoryDelete.Name = "tsmiCategoryDelete";
            tsmiCategoryDelete.Size = new Size(216, 22);
            tsmiCategoryDelete.Text = "Delete category";
            tsmiCategoryDelete.Click += tsmiCategoryDelete_Click;
            // 
            // tsmiCategoryClear
            // 
            tsmiCategoryClear.Image = Properties.Images.CleanupRepo;
            tsmiCategoryClear.Name = "tsmiCategoryClear";
            tsmiCategoryClear.Size = new Size(216, 22);
            tsmiCategoryClear.Text = "Clear all recent repositories";
            tsmiCategoryClear.Click += tsmiCategoryClear_Click;
            // 
            // menuStripRecentMenu
            // 
            menuStripRecentMenu.Items.AddRange(new ToolStripItem[] {
            mnuTop});
            menuStripRecentMenu.Location = new Point(0, 73);
            menuStripRecentMenu.Name = "menuStripRecentMenu";
            menuStripRecentMenu.Size = new Size(451, 24);
            menuStripRecentMenu.TabIndex = 3;
            menuStripRecentMenu.Visible = false;
            // 
            // mnuTop
            // 
            mnuTop.DropDownItems.AddRange(new ToolStripItem[] {
            mnuConfigure});
            mnuTop.Name = "mnuTop";
            mnuTop.Size = new Size(12, 20);
            // 
            // mnuConfigure
            // 
            mnuConfigure.Image = Properties.Images.Settings;
            mnuConfigure.Name = "mnuConfigure";
            mnuConfigure.Size = new Size(218, 22);
            mnuConfigure.Text = "Recent repositories &settings";
            mnuConfigure.Click += mnuConfigure_Click;
            // 
            // listView1
            // 
            listView1.Activation = ItemActivation.OneClick;
            listView1.BorderStyle = BorderStyle.None;
            listView1.Columns.AddRange(new ColumnHeader[] {
            clmhdrPath,
            clmhdrBranch,
            clmhdrCategory});
            listView1.ContextMenuStrip = contextMenuStripRepository;
            listView1.Dock = DockStyle.Fill;
            listView1.FullRowSelect = true;
            lvgPinned.Header = "Pinned";
            lvgPinned.Name = "lvgPinned";
            lvgAllRecent.Header = "Recent";
            lvgAllRecent.Name = "lvgAllRecent";
            lvgOther.Header = "Other";
            lvgOther.Name = "lvgOther";
            listView1.Groups.AddRange(new ListViewGroup[] {
            lvgPinned,
            lvgAllRecent,
            lvgOther});
            listView1.HeaderStyle = ColumnHeaderStyle.None;
            listView1.HideSelection = false;
            listView1.Items.AddRange(new ListViewItem[] {
            listViewItem1});
            listView1.LargeImageList = imageList1;
            listView1.Location = new Point(0, 33);
            listView1.Margin = new Padding(0, 3, 0, 3);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.OwnerDraw = true;
            listView1.ShowItemToolTips = true;
            listView1.Size = new Size(445, 168);
            listView1.TabIndex = 1;
            listView1.TileSize = new Size(350, 50);
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Tile;
            listView1.GroupTaskLinkClick += new System.EventHandler<ListViewGroupEventArgs>(ListView1_GroupTaskLinkClick);
            listView1.DrawItem += listView1_DrawItem;
            listView1.MouseClick += listView1_MouseClick;
            listView1.MouseLeave += listView1_MouseLeave;
            listView1.MouseMove += listView1_MouseMove;
            listView1.GotFocus += listView1_GotFocus;
            listView1.KeyDown += listView1_KeyDown;
            // 
            // clmhdrPath
            // 
            clmhdrPath.Text = "Path";
            // 
            // clmhdrBranch
            // 
            clmhdrBranch.Text = "Branch";
            // 
            // clmhdrCategory
            // 
            clmhdrCategory.Text = "Category";
            // 
            // UserRepositoriesList
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(menuStripRecentMenu);
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            DoubleBuffered = true;
            Name = "UserRepositoriesList";
            Size = new Size(451, 283);
            Load += RecentRepositoriesList_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            pnlBody.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            contextMenuStripRepository.ResumeLayout(false);
            contextMenuStripCategory.ResumeLayout(false);
            menuStripRecentMenu.ResumeLayout(false);
            menuStripRecentMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Panel pnlHeader;
        private Panel pnlBody;
        private Label lblRecentRepositories;
        private ContextMenuStrip contextMenuStripRepository;
        private ContextMenuStrip contextMenuStripCategory;
        private ToolStripMenuItem tsmiRemoveFromList;
        private ToolStripMenuItem tsmiOpenFolder;
        private ToolStripMenuItem tsmiCategoryAdd;
        private ToolStripMenuItem tsmiCategories;
        private ToolStripMenuItem tsmiCategoryNone;
        private ToolStripMenuItem tsmiRemoveMissingReposFromList;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem tsmiCategoryRename;
        private ToolStripMenuItem tsmiCategoryDelete;
        private ToolStripMenuItem tsmiCategoryClear;
        private TableLayoutPanel tableLayoutPanel1;
        private GitUI.UserControls.NativeListView listView1;
        private ColumnHeader clmhdrPath;
        private ColumnHeader clmhdrBranch;
        private ColumnHeader clmhdrCategory;
        private ImageList imageList1;
        private MenuStrip menuStripRecentMenu;
        private ToolStripMenuItem mnuTop;
        private ToolStripMenuItem mnuConfigure;
        private TableLayoutPanel tableLayoutPanel2;
        private TextBox textBoxSearch;
    }
}
