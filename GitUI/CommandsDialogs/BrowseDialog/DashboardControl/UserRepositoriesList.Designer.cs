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
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "C:\\Users\\russkie\\AppData\\Roaming\\GitExtensions\\GitExtensions", "refs/heads/master", "Favourite" }, 0);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRepositoriesList));
            ListViewGroup listViewGroup1 = new ListViewGroup("Default", HorizontalAlignment.Left);
            ListViewItem listViewItem2 = new ListViewItem(new string[] { "C:\\Users\\russkie\\AppData\\Roaming\\GitExtensions\\GitExtensions", "refs/heads/master", "Favourite" }, 0);
            toolStripMenuItem1 = new ToolStripSeparator();
            toolStripMenuItem2 = new ToolStripSeparator();
            pnlHeader = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblRecentRepositories = new Label();
            textBoxSearch = new TextBox();
            comboChooseView = new ComboBox();
            pnlBody = new Panel();
            flowLayoutPanel = new FlowLayoutPanel();
            listViewRecentRepositories = new UserControls.NativeListView();
            clmhdrPath = new ColumnHeader();
            clmhdrBranch = new ColumnHeader();
            clmhdrCategory = new ColumnHeader();
            contextMenuStripRepository = new ContextMenuStrip(components);
            tsmiOpenFolder = new ToolStripMenuItem();
            tsmiCategories = new ToolStripMenuItem();
            tsmiCategoryNone = new ToolStripMenuItem();
            tsmiCategoryAdd = new ToolStripMenuItem();
            tsmiRemoveFromList = new ToolStripMenuItem();
            tsmiRemoveMissingReposFromList = new ToolStripMenuItem();
            imageListLarge = new ImageList(components);
            imageListSmall = new ImageList(components);
            listViewFavouriteRepositories = new UserControls.NativeListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            contextMenuStripCategory = new ContextMenuStrip(components);
            tsmiCategoryRename = new ToolStripMenuItem();
            tsmiCategoryDelete = new ToolStripMenuItem();
            tsmiCategoryClear = new ToolStripMenuItem();
            menuStripRecentMenu = new MenuStrip();
            mnuTop = new ToolStripMenuItem();
            mnuConfigure = new ToolStripMenuItem();
            pnlHeader.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            pnlBody.SuspendLayout();
            flowLayoutPanel.SuspendLayout();
            contextMenuStripRepository.SuspendLayout();
            contextMenuStripCategory.SuspendLayout();
            menuStripRecentMenu.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(286, 6);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(286, 6);
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
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
            tableLayoutPanel1.Controls.Add(lblRecentRepositories, 0, 0);
            tableLayoutPanel1.Controls.Add(textBoxSearch, 1, 0);
            tableLayoutPanel1.Controls.Add(comboChooseView, 2, 0);
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
            lblRecentRepositories.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point);
            lblRecentRepositories.ForeColor = Color.DimGray;
            lblRecentRepositories.Location = new Point(0, 0);
            lblRecentRepositories.Margin = new Padding(0);
            lblRecentRepositories.Name = "lblRecentRepositories";
            lblRecentRepositories.Size = new Size(300, 32);
            lblRecentRepositories.TabIndex = 2;
            lblRecentRepositories.Text = "Recent repositories";
            // 
            // textBoxSearch
            // 
            textBoxSearch.Dock = DockStyle.Fill;
            textBoxSearch.Location = new Point(303, 3);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(61, 23);
            textBoxSearch.TabIndex = 0;
            textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
            // 
            // comboChooseView
            // 
            comboChooseView.FormattingEnabled = true;
            comboChooseView.Items.AddRange(new object[] { "Tiles", "Detailed" });
            comboChooseView.Location = new Point(369, 2);
            comboChooseView.Margin = new Padding(2);
            comboChooseView.Name = "comboChooseView";
            comboChooseView.Size = new Size(40, 23);
            comboChooseView.TabIndex = 4;
            comboChooseView.SelectedIndexChanged += comboChooseView_SelectedIndexChanged;
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(flowLayoutPanel);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 73);
            pnlBody.Margin = new Padding(0);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new Padding(20, 18, 20, 3);
            pnlBody.Size = new Size(451, 210);
            pnlBody.TabIndex = 1;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Controls.Add(listViewRecentRepositories);
            flowLayoutPanel.Controls.Add(listViewFavouriteRepositories);
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.Location = new Point(20, 18);
            flowLayoutPanel.Margin = new Padding(0);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(411, 189);
            flowLayoutPanel.TabIndex = 3;
            flowLayoutPanel.SizeChanged += flowLayoutPanel_SizeChanged;
            // 
            // listViewRecentRepositories
            // 
            listViewRecentRepositories.Activation = ItemActivation.OneClick;
            listViewRecentRepositories.BorderStyle = BorderStyle.None;
            listViewRecentRepositories.Columns.AddRange(new ColumnHeader[] { clmhdrPath, clmhdrBranch, clmhdrCategory });
            listViewRecentRepositories.ContextMenuStrip = contextMenuStripRepository;
            listViewRecentRepositories.FullRowSelect = true;
            listViewRecentRepositories.HeaderStyle = ColumnHeaderStyle.None;
            listViewRecentRepositories.Items.AddRange(new ListViewItem[] { listViewItem1 });
            listViewRecentRepositories.LargeImageList = imageListLarge;
            listViewRecentRepositories.Location = new Point(0, 3);
            listViewRecentRepositories.Margin = new Padding(0, 3, 0, 3);
            listViewRecentRepositories.MinimumSize = new Size(175, 120);
            listViewRecentRepositories.MultiSelect = false;
            listViewRecentRepositories.Name = "listViewRecentRepositories";
            listViewRecentRepositories.OwnerDraw = true;
            listViewRecentRepositories.ShowItemToolTips = true;
            listViewRecentRepositories.Size = new Size(500, 120);
            listViewRecentRepositories.SmallImageList = imageListSmall;
            listViewRecentRepositories.TabIndex = 1;
            listViewRecentRepositories.TileSize = new Size(350, 50);
            listViewRecentRepositories.UseCompatibleStateImageBehavior = false;
            listViewRecentRepositories.View = View.Tile;
            listViewRecentRepositories.GroupTaskLinkClick += ListViewRecentRepositories_GroupTaskLinkClick;
            listViewRecentRepositories.DrawItem += listView_DrawItem;
            listViewRecentRepositories.MouseClick += ListViewRecentRepositories_MouseClick;
            listViewRecentRepositories.MouseLeave += ListView_MouseLeave;
            listViewRecentRepositories.MouseMove += ListView_MouseMove;
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
            // contextMenuStripRepository
            // 
            contextMenuStripRepository.ImageScalingSize = new Size(32, 32);
            contextMenuStripRepository.Items.AddRange(new ToolStripItem[] { tsmiOpenFolder, toolStripMenuItem1, tsmiCategories, toolStripMenuItem2, tsmiRemoveFromList, tsmiRemoveMissingReposFromList });
            contextMenuStripRepository.Name = "contextMenuStripRepository";
            contextMenuStripRepository.Size = new Size(290, 168);
            contextMenuStripRepository.Closed += contextMenuStrip_Closed;
            contextMenuStripRepository.Opening += contextMenuStrip_Opening;
            // 
            // tsmiOpenFolder
            // 
            tsmiOpenFolder.Image = Properties.Images.BrowseFileExplorer;
            tsmiOpenFolder.Name = "tsmiOpenFolder";
            tsmiOpenFolder.Size = new Size(289, 38);
            tsmiOpenFolder.Text = "Show in folder";
            tsmiOpenFolder.Click += tsmiOpenFolder_Click;
            // 
            // tsmiCategories
            // 
            tsmiCategories.DropDownItems.AddRange(new ToolStripItem[] { tsmiCategoryNone, tsmiCategoryAdd });
            tsmiCategories.Name = "tsmiCategories";
            tsmiCategories.Size = new Size(289, 38);
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
            tsmiRemoveFromList.Size = new Size(289, 38);
            tsmiRemoveFromList.Text = "Remove project from the list";
            tsmiRemoveFromList.Click += tsmiRemoveFromList_Click;
            // 
            // tsmiRemoveMissingReposFromList
            // 
            tsmiRemoveMissingReposFromList.Name = "tsmiRemoveMissingReposFromList";
            tsmiRemoveMissingReposFromList.Size = new Size(289, 38);
            tsmiRemoveMissingReposFromList.Text = "Remove missing projects from the list";
            tsmiRemoveMissingReposFromList.Click += tsmiRemoveMissingReposFromList_Click;
            // 
            // imageListLarge
            // 
            imageListLarge.ColorDepth = ColorDepth.Depth16Bit;
            imageListLarge.ImageStream = (ImageListStreamer)resources.GetObject("imageListLarge.ImageStream");
            imageListLarge.TransparentColor = Color.Transparent;
            imageListLarge.Images.SetKeyName(0, "source_code.png");
            // 
            // imageListSmall
            // 
            imageListSmall.ColorDepth = ColorDepth.Depth16Bit;
            imageListSmall.ImageStream = (ImageListStreamer)resources.GetObject("imageListSmall.ImageStream");
            imageListSmall.TransparentColor = Color.Transparent;
            imageListSmall.Images.SetKeyName(0, "source_code.png");
            // 
            // listViewFavouriteRepositories
            // 
            listViewFavouriteRepositories.Activation = ItemActivation.OneClick;
            listViewFavouriteRepositories.BorderStyle = BorderStyle.None;
            listViewFavouriteRepositories.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
            listViewFavouriteRepositories.ContextMenuStrip = contextMenuStripRepository;
            listViewFavouriteRepositories.FullRowSelect = true;
            listViewGroup1.Header = "Default";
            listViewGroup1.Name = "lvgDefaultGroup";
            listViewFavouriteRepositories.Groups.AddRange(new ListViewGroup[] { listViewGroup1 });
            listViewFavouriteRepositories.HeaderStyle = ColumnHeaderStyle.None;
            listViewFavouriteRepositories.Items.AddRange(new ListViewItem[] { listViewItem2 });
            listViewFavouriteRepositories.LargeImageList = imageListLarge;
            listViewFavouriteRepositories.Location = new Point(0, 126);
            listViewFavouriteRepositories.Margin = new Padding(0);
            listViewFavouriteRepositories.MinimumSize = new Size(175, 120);
            listViewFavouriteRepositories.MultiSelect = false;
            listViewFavouriteRepositories.Name = "listViewFavouriteRepositories";
            listViewFavouriteRepositories.OwnerDraw = true;
            listViewFavouriteRepositories.ShowItemToolTips = true;
            listViewFavouriteRepositories.Size = new Size(500, 120);
            listViewFavouriteRepositories.SmallImageList = imageListSmall;
            listViewFavouriteRepositories.TabIndex = 2;
            listViewFavouriteRepositories.TileSize = new Size(350, 50);
            listViewFavouriteRepositories.UseCompatibleStateImageBehavior = false;
            listViewFavouriteRepositories.View = View.Tile;
            listViewFavouriteRepositories.GroupTaskLinkClick += ListViewFavouriteRepositories_GroupTaskLinkClick;
            listViewFavouriteRepositories.DrawItem += listView_DrawItem;
            listViewFavouriteRepositories.MouseClick += ListViewAllRepositories_MouseClick;
            listViewFavouriteRepositories.MouseLeave += ListView_MouseLeave;
            listViewFavouriteRepositories.MouseMove += ListView_MouseMove;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Path";
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Branch";
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Category";
            // 
            // contextMenuStripCategory
            // 
            contextMenuStripCategory.ImageScalingSize = new Size(32, 32);
            contextMenuStripCategory.Items.AddRange(new ToolStripItem[] { tsmiCategoryRename, tsmiCategoryDelete, tsmiCategoryClear });
            contextMenuStripCategory.Name = "contextMenuStripCategory";
            contextMenuStripCategory.Size = new Size(233, 118);
            // 
            // tsmiCategoryRename
            // 
            tsmiCategoryRename.Image = Properties.Images.FileStatusModified;
            tsmiCategoryRename.Name = "tsmiCategoryRename";
            tsmiCategoryRename.Size = new Size(232, 38);
            tsmiCategoryRename.Text = "Rename category";
            tsmiCategoryRename.Click += tsmiCategoryRename_Click;
            // 
            // tsmiCategoryDelete
            // 
            tsmiCategoryDelete.Image = Properties.Images.StarRemove;
            tsmiCategoryDelete.Name = "tsmiCategoryDelete";
            tsmiCategoryDelete.Size = new Size(232, 38);
            tsmiCategoryDelete.Text = "Delete category";
            tsmiCategoryDelete.Click += tsmiCategoryDelete_Click;
            // 
            // tsmiCategoryClear
            // 
            tsmiCategoryClear.Image = Properties.Images.CleanupRepo;
            tsmiCategoryClear.Name = "tsmiCategoryClear";
            tsmiCategoryClear.Size = new Size(232, 38);
            tsmiCategoryClear.Text = "Clear all recent repositories";
            tsmiCategoryClear.Click += tsmiCategoryClear_Click;
            // 
            // menuStripRecentMenu
            // 
            menuStripRecentMenu.ImageScalingSize = new Size(32, 32);
            menuStripRecentMenu.Items.AddRange(new ToolStripItem[] { mnuTop });
            menuStripRecentMenu.Location = new Point(0, 0);
            menuStripRecentMenu.Name = "menuStripRecentMenu";
            menuStripRecentMenu.Size = new Size(451, 24);
            menuStripRecentMenu.TabIndex = 3;
            menuStripRecentMenu.Visible = false;
            // 
            // mnuTop
            // 
            mnuTop.DropDownItems.AddRange(new ToolStripItem[] { mnuConfigure });
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
            // UserRepositoriesList
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = SystemColors.Control;
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            Controls.Add(menuStripRecentMenu);
            DoubleBuffered = true;
            Name = "UserRepositoriesList";
            Size = new Size(451, 283);
            Load += RecentRepositoriesList_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            pnlBody.ResumeLayout(false);
            flowLayoutPanel.ResumeLayout(false);
            contextMenuStripRepository.ResumeLayout(false);
            contextMenuStripCategory.ResumeLayout(false);
            menuStripRecentMenu.ResumeLayout(false);
            menuStripRecentMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private UserControls.NativeListView listViewFavouriteRepositories;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private FlowLayoutPanel flowLayoutPanel;
        private ImageList imageListSmall;
        private ComboBox comboChooseView;
    }
}
