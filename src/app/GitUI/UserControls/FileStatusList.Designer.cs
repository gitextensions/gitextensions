using GitCommands;
using GitUI.UserControls;

namespace GitUI
{
    partial class FileStatusList
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
            if (disposing && (components is not null))
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
            components = new System.ComponentModel.Container();
            FileStatusListView = new MultiSelectTreeView();
            columnHeader = new ColumnHeader();
            NoFiles = new Label();
            LoadingFiles = new Label();
            _NO_TRANSLATE_FilterComboBox = new ComboBox();
            FilterWatermarkLabel = new Label();
            FilterToolTip = new ToolTip(components);
            lblSplitter = new Label();
            DeleteFilterButton = new Label();
            cboFindInCommitFilesGitGrep = new ComboBox();
            lblFindInCommitFilesGitGrepWatermark = new Label();
            DeleteSearchButton = new Label();
            Toolbar = new ToolStripEx();
            btnCollapseGroups = new ToolStripButton();
            sepRefresh = new ToolStripSeparator();
            btnRefresh = new ToolStripSplitButton();
            tsmiRefreshOnFormFocus = new ToolStripMenuItem();
            sepAsTree = new ToolStripSeparator();
            btnAsTree = new ToolStripSplitButton();
            tsmiGroupByFilePathTree = new ToolStripMenuItem();
            tsmiGroupByFilePathFlat = new ToolStripMenuItem();
            tsmiGroupByFileExtensionTree = new ToolStripMenuItem();
            tsmiGroupByFileExtensionFlat = new ToolStripMenuItem();
            tsmiGroupByFileStatusTree = new ToolStripMenuItem();
            tsmiGroupByFileStatusFlat = new ToolStripMenuItem();
            sepListOptions = new ToolStripSeparator();
            tsmiDenseTree = new ToolStripMenuItem();
            tsmiShowGroupNodesInFlatList = new ToolStripMenuItem();
            sepToolbar = new ToolStripSeparator();
            tsmiToolbar = new ToolStripMenuItem();
            sepGroupBy = new ToolStripSeparator();
            btnByPath = new ToolStripButton();
            btnByExtension = new ToolStripButton();
            btnByStatus = new ToolStripButton();
            sepFilter = new ToolStripSeparator();
            btnUnequalChange = new ToolStripButton();
            btnOnlyB = new ToolStripButton();
            btnOnlyA = new ToolStripButton();
            btnSameChange = new ToolStripButton();
            sepOptions = new ToolStripSeparator();
            btnFindInFilesGitGrep = new ToolStripSplitButton();
            tsmiFindUsingDialog = new ToolStripMenuItem();
            tsmiFindUsingInputBox = new ToolStripMenuItem();
            tsmiFindUsingBoth = new ToolStripMenuItem();
            Toolbar.SuspendLayout();
            SuspendLayout();
            // 
            // FileStatusListView
            // 
            FileStatusListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FileStatusListView.BorderStyle = BorderStyle.None;
            FileStatusListView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            FileStatusListView.FullRowSelect = true;
            FileStatusListView.HideSelection = false;
            FileStatusListView.Location = new Point(0, 46);
            FileStatusListView.Margin = new Padding(0);
            FileStatusListView.Name = "FileStatusListView";
            FileStatusListView.ShowRootLines = false;
            FileStatusListView.Size = new Size(682, 439);
            FileStatusListView.TabIndex = 9;
            FileStatusListView.DrawNode += FileStatusListView_DrawNode;
            FileStatusListView.DoubleClick += FileStatusListView_DoubleClick;
            FileStatusListView.KeyDown += FileStatusListView_KeyDown;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.MouseUp += FileStatusListView_MouseUp;
            // 
            // columnHeader
            // 
            columnHeader.Text = "Files";
            columnHeader.Width = 678;
            // 
            // NoFiles
            // 
            NoFiles.AutoSize = true;
            NoFiles.BackColor = SystemColors.Window;
            NoFiles.ForeColor = SystemColors.GrayText;
            NoFiles.Location = new Point(0, 23);
            NoFiles.Name = "NoFiles";
            NoFiles.Padding = new Padding(2);
            NoFiles.Size = new Size(4, 19);
            NoFiles.TabIndex = 1;
            // 
            // LoadingFiles
            // 
            LoadingFiles.AutoSize = true;
            LoadingFiles.BackColor = SystemColors.Window;
            LoadingFiles.ForeColor = SystemColors.GrayText;
            LoadingFiles.Location = new Point(0, 46);
            LoadingFiles.Name = "LoadingFiles";
            LoadingFiles.Padding = new Padding(2);
            LoadingFiles.Size = new Size(4, 19);
            LoadingFiles.TabIndex = 2;
            // 
            // _NO_TRANSLATE_FilterComboBox
            // 
            _NO_TRANSLATE_FilterComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_FilterComboBox.FlatStyle = FlatStyle.Flat;
            _NO_TRANSLATE_FilterComboBox.FormattingEnabled = true;
            _NO_TRANSLATE_FilterComboBox.Location = new Point(0, 48);
            _NO_TRANSLATE_FilterComboBox.Margin = new Padding(0);
            _NO_TRANSLATE_FilterComboBox.Name = "_NO_TRANSLATE_FilterComboBox";
            _NO_TRANSLATE_FilterComboBox.Size = new Size(682, 23);
            _NO_TRANSLATE_FilterComboBox.TabIndex = 5;
            _NO_TRANSLATE_FilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            _NO_TRANSLATE_FilterComboBox.TextUpdate += FilterComboBox_TextUpdate;
            _NO_TRANSLATE_FilterComboBox.SizeChanged += FilterComboBox_SizeChanged;
            _NO_TRANSLATE_FilterComboBox.GotFocus += FilterComboBox_GotFocus;
            _NO_TRANSLATE_FilterComboBox.LostFocus += FilterComboBox_LostFocus;
            _NO_TRANSLATE_FilterComboBox.MouseEnter += FilterComboBox_MouseEnter;
            // 
            // FilterWatermarkLabel
            // 
            FilterWatermarkLabel.AutoSize = true;
            FilterWatermarkLabel.BackColor = SystemColors.Window;
            FilterWatermarkLabel.ForeColor = SystemColors.GrayText;
            FilterWatermarkLabel.Location = new Point(0, 48);
            FilterWatermarkLabel.Name = "FilterWatermarkLabel";
            FilterWatermarkLabel.Padding = new Padding(2, 3, 2, 0);
            FilterWatermarkLabel.Size = new Size(210, 20);
            FilterWatermarkLabel.TabIndex = 6;
            FilterWatermarkLabel.Text = "Filter files using a regular expression...";
            FilterWatermarkLabel.Click += FilterWatermarkLabel_Click;
            // 
            // FilterToolTip
            // 
            FilterToolTip.AutomaticDelay = 100;
            FilterToolTip.ShowAlways = true;
            FilterToolTip.ToolTipIcon = ToolTipIcon.Error;
            FilterToolTip.ToolTipTitle = "RegEx";
            FilterToolTip.UseAnimation = false;
            FilterToolTip.UseFading = false;
            // 
            // lblSplitter
            // 
            lblSplitter.Dock = DockStyle.Top;
            lblSplitter.Location = new Point(0, 25);
            lblSplitter.Name = "lblSplitter";
            lblSplitter.Size = new Size(682, 2);
            lblSplitter.TabIndex = 8;
            // 
            // DeleteFilterButton
            // 
            DeleteFilterButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteFilterButton.BackColor = SystemColors.Window;
            DeleteFilterButton.FlatStyle = FlatStyle.Flat;
            DeleteFilterButton.Image = Properties.Resources.DeleteText;
            DeleteFilterButton.Location = new Point(646, 48);
            DeleteFilterButton.Name = "DeleteFilterButton";
            DeleteFilterButton.Padding = new Padding(0, 1, 0, 0);
            DeleteFilterButton.Size = new Size(18, 23);
            DeleteFilterButton.TabIndex = 7;
            DeleteFilterButton.Click += DeleteFilterButton_Click;
            // 
            // cboFindInCommitFilesGitGrep
            // 
            cboFindInCommitFilesGitGrep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFindInCommitFilesGitGrep.FlatStyle = FlatStyle.Flat;
            cboFindInCommitFilesGitGrep.FormattingEnabled = true;
            cboFindInCommitFilesGitGrep.Location = new Point(0, 25);
            cboFindInCommitFilesGitGrep.Margin = new Padding(0);
            cboFindInCommitFilesGitGrep.Name = "cboFindInCommitFilesGitGrep";
            cboFindInCommitFilesGitGrep.Size = new Size(682, 23);
            cboFindInCommitFilesGitGrep.TabIndex = 0;
            cboFindInCommitFilesGitGrep.Tag = "ToolBar_group:Text search";
            cboFindInCommitFilesGitGrep.SelectedIndexChanged += cboFindInCommitFilesGitGrep_SelectedIndexChanged;
            cboFindInCommitFilesGitGrep.TextUpdate += cboFindInCommitFilesGitGrep_TextUpdate;
            cboFindInCommitFilesGitGrep.SizeChanged += cboFindInCommitFilesGitGrep_SizeChanged;
            cboFindInCommitFilesGitGrep.GotFocus += cboFindInCommitFilesGitGrep_GotFocus;
            cboFindInCommitFilesGitGrep.LostFocus += cboFindInCommitFilesGitGrep_LostFocus;
            // 
            // lblFindInCommitFilesGitGrepWatermark
            // 
            lblFindInCommitFilesGitGrepWatermark.AutoSize = true;
            lblFindInCommitFilesGitGrepWatermark.BackColor = SystemColors.Window;
            lblFindInCommitFilesGitGrepWatermark.ForeColor = SystemColors.GrayText;
            lblFindInCommitFilesGitGrepWatermark.Location = new Point(0, 25);
            lblFindInCommitFilesGitGrepWatermark.Name = "lblFindInCommitFilesGitGrepWatermark";
            lblFindInCommitFilesGitGrepWatermark.Padding = new Padding(2, 3, 2, 2);
            lblFindInCommitFilesGitGrepWatermark.Size = new Size(302, 20);
            lblFindInCommitFilesGitGrepWatermark.TabIndex = 3;
            lblFindInCommitFilesGitGrepWatermark.Text = "Find in commit files using git-grep regular expression...";
            lblFindInCommitFilesGitGrepWatermark.Click += lblFindInCommitFilesGitGrepWatermark_Click;
            // 
            // DeleteSearchButton
            // 
            DeleteSearchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteSearchButton.BackColor = SystemColors.Window;
            DeleteSearchButton.FlatStyle = FlatStyle.Flat;
            DeleteSearchButton.Image = Properties.Resources.DeleteText;
            DeleteSearchButton.Location = new Point(646, 25);
            DeleteSearchButton.Name = "DeleteSearchButton";
            DeleteSearchButton.Padding = new Padding(0, 1, 0, 0);
            DeleteSearchButton.Size = new Size(18, 23);
            DeleteSearchButton.TabIndex = 4;
            DeleteSearchButton.Click += DeleteSearchButton_Click;
            // 
            // Toolbar
            // 
            Toolbar.ClickThrough = true;
            Toolbar.DrawBorder = false;
            Toolbar.GripStyle = ToolStripGripStyle.Hidden;
            Toolbar.Items.AddRange(new ToolStripItem[] { btnCollapseGroups, sepRefresh, btnRefresh, sepAsTree, btnAsTree, sepGroupBy, btnByPath, btnByExtension, btnByStatus, sepFilter, btnUnequalChange, btnOnlyB, btnOnlyA, btnSameChange, sepOptions, btnFindInFilesGitGrep });
            Toolbar.Location = new Point(0, 0);
            Toolbar.Name = "Toolbar";
            Toolbar.RenderMode = ToolStripRenderMode.Professional;
            Toolbar.Size = new Size(682, 25);
            Toolbar.TabIndex = 0;
            // 
            // btnCollapseGroups
            // 
            btnCollapseGroups.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnCollapseGroups.Image = Properties.Images.CollapseAll;
            btnCollapseGroups.Name = "btnCollapseGroups";
            btnCollapseGroups.Size = new Size(23, 22);
            btnCollapseGroups.ToolTipText = "Collapse all groups, otherwise expand the selected group";
            btnCollapseGroups.Visible = false;
            btnCollapseGroups.Click += CollapseGroups_Click;
            // 
            // sepRefresh
            // 
            sepRefresh.Name = "sepRefresh";
            sepRefresh.Size = new Size(6, 25);
            sepRefresh.Visible = false;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnRefresh.DropDownItems.AddRange(new ToolStripItem[] { tsmiRefreshOnFormFocus });
            btnRefresh.Image = Properties.Images.ReloadRevisions;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(32, 22);
            btnRefresh.ToolTipText = "Refresh artificial commit";
            btnRefresh.Visible = false;
            // 
            // tsmiRefreshOnFormFocus
            // 
            tsmiRefreshOnFormFocus.CheckOnClick = true;
            tsmiRefreshOnFormFocus.Enabled = false;
            tsmiRefreshOnFormFocus.Name = "tsmiRefreshOnFormFocus";
            tsmiRefreshOnFormFocus.Size = new Size(286, 22);
            tsmiRefreshOnFormFocus.Text = "&Refresh artificial commits on form focus";
            tsmiRefreshOnFormFocus.Click += RefreshOnFormFocus_Click;
            // 
            // sepAsTree
            // 
            sepAsTree.Name = "sepAsTree";
            sepAsTree.Size = new Size(6, 25);
            // 
            // btnAsTree
            // 
            btnAsTree.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnAsTree.DropDownItems.AddRange(new ToolStripItem[] { tsmiGroupByFilePathTree, tsmiGroupByFilePathFlat, tsmiGroupByFileExtensionTree, tsmiGroupByFileExtensionFlat, tsmiGroupByFileStatusTree, tsmiGroupByFileStatusFlat, sepListOptions, tsmiDenseTree, tsmiShowGroupNodesInFlatList, sepToolbar, tsmiToolbar });
            btnAsTree.Image = Properties.Images.FileTree;
            btnAsTree.Name = "btnAsTree";
            btnAsTree.Size = new Size(32, 22);
            btnAsTree.ToolTipText = "Toggle flat list / tree";
            btnAsTree.ButtonClick += AsTree_ButtonClick;
            // 
            // tsmiGroupByFilePathTree
            // 
            tsmiGroupByFilePathTree.Image = Properties.Images.FolderClosed;
            tsmiGroupByFilePathTree.Name = "tsmiGroupByFilePathTree";
            tsmiGroupByFilePathTree.Size = new Size(340, 22);
            tsmiGroupByFilePathTree.Tag = DiffListSortType.FilePath;
            tsmiGroupByFilePathTree.Text = "Group by file &path - tree";
            tsmiGroupByFilePathTree.Click += GroupByToolStripMenuItem_Click;
            // 
            // tsmiGroupByFilePathFlat
            // 
            tsmiGroupByFilePathFlat.Name = "tsmiGroupByFilePathFlat";
            tsmiGroupByFilePathFlat.Size = new Size(340, 22);
            tsmiGroupByFilePathFlat.Tag = DiffListSortType.FilePathFlat;
            tsmiGroupByFilePathFlat.Text = "Group by &file path - flat";
            tsmiGroupByFilePathFlat.Click += GroupByToolStripMenuItem_Click;
            // 
            // tsmiGroupByFileExtensionTree
            // 
            tsmiGroupByFileExtensionTree.Image = Properties.Images.File;
            tsmiGroupByFileExtensionTree.Name = "tsmiGroupByFileExtensionTree";
            tsmiGroupByFileExtensionTree.Size = new Size(340, 22);
            tsmiGroupByFileExtensionTree.Tag = DiffListSortType.FileExtension;
            tsmiGroupByFileExtensionTree.Text = "Group by file &extension - tree";
            tsmiGroupByFileExtensionTree.Click += GroupByToolStripMenuItem_Click;
            // 
            // tsmiGroupByFileExtensionFlat
            // 
            tsmiGroupByFileExtensionFlat.Name = "tsmiGroupByFileExtensionFlat";
            tsmiGroupByFileExtensionFlat.Size = new Size(340, 22);
            tsmiGroupByFileExtensionFlat.Tag = DiffListSortType.FileExtensionFlat;
            tsmiGroupByFileExtensionFlat.Text = "Group by file e&xtension - flat";
            tsmiGroupByFileExtensionFlat.Click += GroupByToolStripMenuItem_Click;
            // 
            // tsmiGroupByFileStatusTree
            // 
            tsmiGroupByFileStatusTree.Image = Properties.Images.FileStatusModified;
            tsmiGroupByFileStatusTree.Name = "tsmiGroupByFileStatusTree";
            tsmiGroupByFileStatusTree.Size = new Size(340, 22);
            tsmiGroupByFileStatusTree.Tag = DiffListSortType.FileStatus;
            tsmiGroupByFileStatusTree.Text = "Group by file &status - tree";
            tsmiGroupByFileStatusTree.Click += GroupByToolStripMenuItem_Click;
            // 
            // tsmiGroupByFileStatusFlat
            // 
            tsmiGroupByFileStatusFlat.Name = "tsmiGroupByFileStatusFlat";
            tsmiGroupByFileStatusFlat.Size = new Size(340, 22);
            tsmiGroupByFileStatusFlat.Tag = DiffListSortType.FileStatusFlat;
            tsmiGroupByFileStatusFlat.Text = "Group by file s&tatus - flat";
            tsmiGroupByFileStatusFlat.Click += GroupByToolStripMenuItem_Click;
            // 
            // sepListOptions
            // 
            sepListOptions.Name = "sepListOptions";
            sepListOptions.Size = new Size(337, 6);
            // 
            // tsmiDenseTree
            // 
            tsmiDenseTree.CheckOnClick = true;
            tsmiDenseTree.Name = "tsmiDenseTree";
            tsmiDenseTree.Size = new Size(340, 22);
            tsmiDenseTree.Text = "&Dense tree (merge single item with its folder node)";
            tsmiDenseTree.Click += DenseTree_Click;
            // 
            // tsmiShowGroupNodesInFlatList
            // 
            tsmiShowGroupNodesInFlatList.CheckOnClick = true;
            tsmiShowGroupNodesInFlatList.Name = "tsmiShowGroupNodesInFlatList";
            tsmiShowGroupNodesInFlatList.Size = new Size(340, 22);
            tsmiShowGroupNodesInFlatList.Text = "Show &group nodes in flat list (if multiple)";
            tsmiShowGroupNodesInFlatList.Click += ShowGroupNodesInFlatList_Click;
            // 
            // sepToolbar
            // 
            sepToolbar.Name = "sepToolbar";
            sepToolbar.Size = new Size(337, 6);
            // 
            // tsmiToolbar
            // 
            tsmiToolbar.Name = "tsmiToolbar";
            tsmiToolbar.Size = new Size(340, 22);
            tsmiToolbar.Text = "Toolbar";
            // 
            // sepGroupBy
            // 
            sepGroupBy.Name = "sepGroupBy";
            sepGroupBy.Size = new Size(6, 25);
            // 
            // btnByPath
            // 
            btnByPath.Checked = true;
            btnByPath.CheckState = CheckState.Checked;
            btnByPath.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnByPath.Image = Properties.Images.FolderClosed;
            btnByPath.Name = "btnByPath";
            btnByPath.Size = new Size(23, 22);
            btnByPath.ToolTipText = "Group by file path";
            btnByPath.Click += GroupBy_Click;
            // 
            // btnByExtension
            // 
            btnByExtension.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnByExtension.Image = Properties.Images.File;
            btnByExtension.Name = "btnByExtension";
            btnByExtension.Size = new Size(23, 22);
            btnByExtension.ToolTipText = "Group by file type (extension)";
            btnByExtension.Click += GroupBy_Click;
            // 
            // btnByStatus
            // 
            btnByStatus.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnByStatus.Image = Properties.Images.FileStatusModified;
            btnByStatus.Name = "btnByStatus";
            btnByStatus.Size = new Size(23, 22);
            btnByStatus.ToolTipText = "Group by diff status";
            btnByStatus.Click += GroupBy_Click;
            // 
            // sepFilter
            // 
            sepFilter.Name = "sepFilter";
            sepFilter.Size = new Size(6, 25);
            // 
            // btnUnequalChange
            // 
            btnUnequalChange.Checked = true;
            btnUnequalChange.CheckOnClick = true;
            btnUnequalChange.CheckState = CheckState.Checked;
            btnUnequalChange.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnUnequalChange.ForeColor = Color.FromArgb(255, 0, 0);
            btnUnequalChange.Name = "btnUnequalChange";
            btnUnequalChange.Size = new Size(23, 22);
            btnUnequalChange.Text = "!";
            btnUnequalChange.ToolTipText = "Show files with different changes";
            btnUnequalChange.Click += Filter_ButtonClick;
            // 
            // btnOnlyB
            // 
            btnOnlyB.Checked = true;
            btnOnlyB.CheckOnClick = true;
            btnOnlyB.CheckState = CheckState.Checked;
            btnOnlyB.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOnlyB.ForeColor = Color.FromArgb(189, 124, 255);
            btnOnlyB.Name = "btnOnlyB";
            btnOnlyB.Size = new Size(23, 22);
            btnOnlyB.Text = "B";
            btnOnlyB.ToolTipText = "Show files changed in B only";
            btnOnlyB.Click += Filter_ButtonClick;
            // 
            // btnOnlyA
            // 
            btnOnlyA.Checked = true;
            btnOnlyA.CheckOnClick = true;
            btnOnlyA.CheckState = CheckState.Checked;
            btnOnlyA.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOnlyA.ForeColor = Color.FromArgb(168, 168, 0);
            btnOnlyA.Name = "btnOnlyA";
            btnOnlyA.Size = new Size(23, 22);
            btnOnlyA.Text = "A";
            btnOnlyA.ToolTipText = "Show files changed in A only";
            btnOnlyA.Click += Filter_ButtonClick;
            // 
            // btnSameChange
            // 
            btnSameChange.Checked = true;
            btnSameChange.CheckOnClick = true;
            btnSameChange.CheckState = CheckState.Checked;
            btnSameChange.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSameChange.ForeColor = Color.FromArgb(0, 168, 0);
            btnSameChange.Name = "btnSameChange";
            btnSameChange.Size = new Size(23, 22);
            btnSameChange.Text = "=";
            btnSameChange.ToolTipText = "Show files changed equally";
            btnSameChange.Click += Filter_ButtonClick;
            // 
            // sepOptions
            // 
            sepOptions.Name = "sepOptions";
            sepOptions.Size = new Size(6, 25);
            // 
            // btnFindInFilesGitGrep
            // 
            btnFindInFilesGitGrep.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnFindInFilesGitGrep.DropDownItems.AddRange(new ToolStripItem[] { tsmiFindUsingDialog, tsmiFindUsingInputBox, tsmiFindUsingBoth });
            btnFindInFilesGitGrep.Image = Properties.Images.ViewFile;
            btnFindInFilesGitGrep.Name = "btnFindInFilesGitGrep";
            btnFindInFilesGitGrep.Size = new Size(32, 22);
            btnFindInFilesGitGrep.ToolTipText = "Toggle 'Find in commit files using git-grep'";
            btnFindInFilesGitGrep.ButtonClick += FindInFilesGitGrep_ButtonClick;
            // 
            // tsmiFindUsingDialog
            // 
            tsmiFindUsingDialog.Name = "tsmiFindUsingDialog";
            tsmiFindUsingDialog.Size = new Size(158, 22);
            tsmiFindUsingDialog.Text = "Using &dialog";
            tsmiFindUsingDialog.Click += FindUsing_Click;
            // 
            // tsmiFindUsingInputBox
            // 
            tsmiFindUsingInputBox.Checked = true;
            tsmiFindUsingInputBox.CheckState = CheckState.Checked;
            tsmiFindUsingInputBox.Name = "tsmiFindUsingInputBox";
            tsmiFindUsingInputBox.Size = new Size(158, 22);
            tsmiFindUsingInputBox.Text = "Using &input box";
            tsmiFindUsingInputBox.Click += FindUsing_Click;
            // 
            // tsmiFindUsingBoth
            // 
            tsmiFindUsingBoth.Name = "tsmiFindUsingBoth";
            tsmiFindUsingBoth.Size = new Size(158, 22);
            tsmiFindUsingBoth.Text = "Using &both";
            tsmiFindUsingBoth.Click += FindUsing_Click;
            // 
            // FileStatusList
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(LoadingFiles);
            Controls.Add(NoFiles);
            Controls.Add(lblFindInCommitFilesGitGrepWatermark);
            Controls.Add(DeleteSearchButton);
            Controls.Add(FilterWatermarkLabel);
            Controls.Add(DeleteFilterButton);
            Controls.Add(cboFindInCommitFilesGitGrep);
            Controls.Add(FileStatusListView);
            Controls.Add(_NO_TRANSLATE_FilterComboBox);
            Controls.Add(lblSplitter);
            Controls.Add(Toolbar);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FileStatusList";
            Size = new Size(682, 485);
            Toolbar.ResumeLayout(false);
            Toolbar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MultiSelectTreeView FileStatusListView;
        private Label NoFiles;
        private Label LoadingFiles;
        private ColumnHeader columnHeader;
        private ComboBox _NO_TRANSLATE_FilterComboBox;
        private Label FilterWatermarkLabel;
        private ToolTip FilterToolTip;
        private Label lblSplitter;
        private Label DeleteFilterButton;
        private ComboBox cboFindInCommitFilesGitGrep;
        private Label lblFindInCommitFilesGitGrepWatermark;
        private Label DeleteSearchButton;
        private ToolStripEx Toolbar;
        private ToolStripButton btnCollapseGroups;
        private ToolStripSplitButton btnRefresh;
        private ToolStripMenuItem tsmiRefreshOnFormFocus;
        private ToolStripSplitButton btnAsTree;
        private ToolStripMenuItem tsmiDenseTree;
        private ToolStripMenuItem tsmiShowGroupNodesInFlatList;
        private ToolStripButton btnByPath;
        private ToolStripButton btnByExtension;
        private ToolStripButton btnByStatus;
        private ToolStripSeparator sepFilter;
        private ToolStripButton btnUnequalChange;
        private ToolStripButton btnOnlyB;
        private ToolStripButton btnOnlyA;
        private ToolStripButton btnSameChange;
        private ToolStripSeparator sepOptions;
        private ToolStripSplitButton btnFindInFilesGitGrep;
        private ToolStripMenuItem tsmiFindUsingDialog;
        private ToolStripMenuItem tsmiFindUsingInputBox;
        private ToolStripMenuItem tsmiFindUsingBoth;
        private ToolStripSeparator sepRefresh;
        private ToolStripSeparator sepAsTree;
        private ToolStripSeparator sepGroupBy;
        private ToolStripSeparator sepListOptions;
        private ToolStripSeparator sepToolbar;
        private ToolStripMenuItem tsmiGroupByFilePathTree;
        private ToolStripMenuItem tsmiGroupByFilePathFlat;
        private ToolStripMenuItem tsmiGroupByFileExtensionTree;
        private ToolStripMenuItem tsmiGroupByFileExtensionFlat;
        private ToolStripMenuItem tsmiGroupByFileStatusTree;
        private ToolStripMenuItem tsmiGroupByFileStatusFlat;
        private ToolStripMenuItem tsmiToolbar;
    }
}
