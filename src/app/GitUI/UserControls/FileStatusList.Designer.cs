using GitCommands;
using GitUI.UserControls;

namespace GitUI;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileStatusList));
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
        btnRefresh = new ToolStripButton();
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
        sepSettings = new ToolStripSeparator();
        btnSettings = new ToolStripDropDownButton();
        tsmiShowIgnoredFiles = new ToolStripMenuItem();
        tsmiShowSkipWorktreeFiles = new ToolStripMenuItem();
        tsmiShowAssumeUnchangedFiles = new ToolStripMenuItem();
        tsmiShowUntrackedFiles = new ToolStripMenuItem();
        sepShow = new ToolStripSeparator();
        tsmiEditGitIgnore = new ToolStripMenuItem();
        tmsiEditLocallyIgnoredFiles = new ToolStripMenuItem();
        sepEdit = new ToolStripSeparator();
        tsmiRefreshOnFormFocus = new ToolStripMenuItem();
        tsmiShowDiffForAllParents = new ToolStripMenuItem();
        sepToolbar = new ToolStripSeparator();
        tsmiToolbar = new ToolStripMenuItem();
        ItemContextMenu = new ContextMenuStrip(components);
        tsmiUpdateSubmodule = new ToolStripMenuItem();
        tsmiResetSubmoduleChanges = new ToolStripMenuItem();
        tsmiStashSubmoduleChanges = new ToolStripMenuItem();
        tsmiCommitSubmoduleChanges = new ToolStripMenuItem();
        sepSubmodule = new ToolStripSeparator();
        tsmiStageFile = new ToolStripMenuItem();
        tsmiUnstageFile = new ToolStripMenuItem();
        tsmiResetFileTo = new ToolStripMenuItem();
        tsmiResetFileToSelected = new ToolStripMenuItem();
        tsmiResetFileToParent = new ToolStripMenuItem();
        tsmiResetChunkOfFile = new ToolStripMenuItem();
        tsmiInteractiveAdd = new ToolStripMenuItem();
        tsmiCherryPickChanges = new ToolStripMenuItem();
        sepGit = new ToolStripSeparator();
        tsmiOpenWithDifftool = new ToolStripMenuItem();
        tsmiSecondDiffCaption = new ToolStripMenuItem();
        tsmiFirstDiffCaption = new ToolStripMenuItem();
        tsmiDiffFirstToSelected = new ToolStripMenuItem();
        tsmiDiffSelectedToLocal = new ToolStripMenuItem();
        tsmiDiffFirstToLocal = new ToolStripMenuItem();
        sepDifftoolRemember = new ToolStripSeparator();
        tsmiDiffTwoSelected = new ToolStripMenuItem();
        tsmiDiffWithRemembered = new ToolStripMenuItem();
        tsmiRememberSecondRevDiff = new ToolStripMenuItem();
        tsmiRememberFirstRevDiff = new ToolStripMenuItem();
        tsmiOpenWorkingDirectoryFile = new ToolStripMenuItem();
        tsmiOpenWorkingDirectoryFileWith = new ToolStripMenuItem();
        tsmiOpenRevisionFile = new ToolStripMenuItem();
        tsmiOpenRevisionFileWith = new ToolStripMenuItem();
        tsmiEditWorkingDirectoryFile = new ToolStripMenuItem();
        tsmiOpenInVisualStudio = new ToolStripMenuItem();
        tsmiSaveAs = new ToolStripMenuItem();
        tsmiMove = new ToolStripMenuItem();
        tsmiDeleteFile = new ToolStripMenuItem();
        sepFile = new ToolStripSeparator();
        tsmiCopyPaths = new GitUI.CommandsDialogs.Menus.CopyPathsToolStripMenuItem();
        tsmiShowInFolder = new ToolStripMenuItem();
        sepBrowse = new ToolStripSeparator();
        tsmiShowInFileTree = new ToolStripMenuItem();
        tsmiFilterFileInGrid = new ToolStripMenuItem();
        tsmiFileHistory = new ToolStripMenuItem();
        tsmiBlame = new ToolStripMenuItem();
        tsmiFindFile = new ToolStripMenuItem();
        tsmiOpenFindInCommitFilesGitGrepDialog = new ToolStripMenuItem();
        tsmiShowFindInCommitFilesGitGrep = new ToolStripMenuItem();
        sepIgnore = new ToolStripSeparator();
        tsmiAddFileToGitIgnore = new ToolStripMenuItem();
        tsmiAddFileToGitInfoExclude = new ToolStripMenuItem();
        tsmiSkipWorktree = new ToolStripMenuItem();
        tsmiAssumeUnchanged = new ToolStripMenuItem();
        tsmiStopTracking = new ToolStripMenuItem();
        sepScripts = new ToolStripSeparator();
        tsmiRunScript = new ToolStripMenuItem();
        Toolbar.SuspendLayout();
        ItemContextMenu.SuspendLayout();
        SuspendLayout();
        // 
        // FileStatusListView
        // 
        FileStatusListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        FileStatusListView.BorderStyle = BorderStyle.None;
        FileStatusListView.ContextMenuStrip = ItemContextMenu;
        FileStatusListView.DrawMode = TreeViewDrawMode.OwnerDrawText;
        FileStatusListView.FullRowSelect = true;
        FileStatusListView.HideSelection = false;
        FileStatusListView.Location = new Point(0, 46);
        FileStatusListView.Margin = new Padding(0);
        FileStatusListView.Name = "FileStatusListView";
        FileStatusListView.ShowRootLines = false;
        FileStatusListView.Size = new Size(682, 439);
        FileStatusListView.TabIndex = 9;
        FileStatusListView.BeforeExpand += FileStatusListView_BeforeExpand;
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
        FilterWatermarkLabel.Size = new Size(210, 18);
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
        Toolbar.Items.AddRange(new ToolStripItem[] { btnCollapseGroups, sepRefresh, btnRefresh, sepAsTree, btnAsTree, sepGroupBy, btnByPath, btnByExtension, btnByStatus, sepFilter, btnUnequalChange, btnOnlyB, btnOnlyA, btnSameChange, sepOptions, btnFindInFilesGitGrep, sepSettings, btnSettings });
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
        btnRefresh.Image = Properties.Images.ReloadRevisions;
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(23, 22);
        btnRefresh.ToolTipText = "Refresh artificial commit";
        btnRefresh.Visible = false;
        // 
        // sepAsTree
        // 
        sepAsTree.Name = "sepAsTree";
        sepAsTree.Size = new Size(6, 25);
        // 
        // btnAsTree
        // 
        btnAsTree.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnAsTree.DropDownItems.AddRange(new ToolStripItem[] { tsmiGroupByFilePathTree, tsmiGroupByFilePathFlat, tsmiGroupByFileExtensionTree, tsmiGroupByFileExtensionFlat, tsmiGroupByFileStatusTree, tsmiGroupByFileStatusFlat, sepListOptions, tsmiDenseTree, tsmiShowGroupNodesInFlatList });
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
        // sepSettings
        // 
        sepSettings.Name = "sepSettings";
        sepSettings.Size = new Size(6, 25);
        // 
        // btnSettings
        // 
        btnSettings.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnSettings.DropDownItems.AddRange(new ToolStripItem[] { tsmiShowIgnoredFiles, tsmiShowSkipWorktreeFiles, tsmiShowAssumeUnchangedFiles, tsmiShowUntrackedFiles, sepShow, tsmiEditGitIgnore, tmsiEditLocallyIgnoredFiles, sepEdit, tsmiRefreshOnFormFocus, tsmiShowDiffForAllParents, sepToolbar, tsmiToolbar });
        btnSettings.Image = Properties.Images.Settings;
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(29, 22);
        btnSettings.Text = "Settings";
        btnSettings.DropDownOpening += Settings_DropDownOpening;
        // 
        // tsmiShowIgnoredFiles
        // 
        tsmiShowIgnoredFiles.CheckOnClick = true;
        tsmiShowIgnoredFiles.Name = "tsmiShowIgnoredFiles";
        tsmiShowIgnoredFiles.Size = new Size(286, 22);
        tsmiShowIgnoredFiles.Text = "Show &ignored files";
        tsmiShowIgnoredFiles.Visible = false;
        tsmiShowIgnoredFiles.Click += ShowIgnoredFiles_Click;
        // 
        // tsmiShowSkipWorktreeFiles
        // 
        tsmiShowSkipWorktreeFiles.CheckOnClick = true;
        tsmiShowSkipWorktreeFiles.Name = "tsmiShowSkipWorktreeFiles";
        tsmiShowSkipWorktreeFiles.Size = new Size(286, 22);
        tsmiShowSkipWorktreeFiles.Text = "Show &skip-worktree files";
        tsmiShowSkipWorktreeFiles.Visible = false;
        tsmiShowSkipWorktreeFiles.Click += ShowSkipWorktreeFiles_Click;
        // 
        // tsmiShowAssumeUnchangedFiles
        // 
        tsmiShowAssumeUnchangedFiles.CheckOnClick = true;
        tsmiShowAssumeUnchangedFiles.Name = "tsmiShowAssumeUnchangedFiles";
        tsmiShowAssumeUnchangedFiles.Size = new Size(286, 22);
        tsmiShowAssumeUnchangedFiles.Text = "Show &assumed-unchanged files";
        tsmiShowAssumeUnchangedFiles.Visible = false;
        tsmiShowAssumeUnchangedFiles.Click += ShowAssumeUnchangedFiles_Click;
        // 
        // tsmiShowUntrackedFiles
        // 
        tsmiShowUntrackedFiles.Checked = true;
        tsmiShowUntrackedFiles.CheckOnClick = true;
        tsmiShowUntrackedFiles.CheckState = CheckState.Checked;
        tsmiShowUntrackedFiles.Name = "tsmiShowUntrackedFiles";
        tsmiShowUntrackedFiles.Size = new Size(286, 22);
        tsmiShowUntrackedFiles.Text = "Show &untracked files";
        tsmiShowUntrackedFiles.Visible = false;
        tsmiShowUntrackedFiles.Click += ShowUntrackedFiles_Click;
        // 
        // sepShow
        // 
        sepShow.Name = "sepShow";
        sepShow.Size = new Size(283, 6);
        sepShow.Visible = false;
        // 
        // tsmiEditGitIgnore
        // 
        tsmiEditGitIgnore.Image = Properties.Images.EditGitIgnore;
        tsmiEditGitIgnore.Name = "tsmiEditGitIgnore";
        tsmiEditGitIgnore.Size = new Size(286, 22);
        tsmiEditGitIgnore.Text = "&Edit ignored files";
        tsmiEditGitIgnore.Click += EditGitIgnore_Click;
        // 
        // tmsiEditLocallyIgnoredFiles
        // 
        tmsiEditLocallyIgnoredFiles.Image = Properties.Images.EditGitIgnore;
        tmsiEditLocallyIgnoredFiles.Name = "tmsiEditLocallyIgnoredFiles";
        tmsiEditLocallyIgnoredFiles.Size = new Size(286, 22);
        tmsiEditLocallyIgnoredFiles.Text = "Edit &locally ignored files";
        tmsiEditLocallyIgnoredFiles.Click += EditLocallyIgnoredFiles_Click;
        // 
        // sepEdit
        // 
        sepEdit.Name = "sepEdit";
        sepEdit.Size = new Size(283, 6);
        // 
        // tsmiRefreshOnFormFocus
        // 
        tsmiRefreshOnFormFocus.CheckOnClick = true;
        tsmiRefreshOnFormFocus.Name = "tsmiRefreshOnFormFocus";
        tsmiRefreshOnFormFocus.Size = new Size(286, 22);
        tsmiRefreshOnFormFocus.Text = "&Refresh artificial commits on form focus";
        tsmiRefreshOnFormFocus.Visible = false;
        tsmiRefreshOnFormFocus.Click += RefreshOnFormFocus_Click;
        // 
        // tsmiShowDiffForAllParents
        // 
        tsmiShowDiffForAllParents.CheckOnClick = true;
        tsmiShowDiffForAllParents.Name = "tsmiShowDiffForAllParents";
        tsmiShowDiffForAllParents.Size = new Size(286, 22);
        tsmiShowDiffForAllParents.Text = "&Show file differences for all parents";
        tsmiShowDiffForAllParents.Click += ShowDiffForAllParents_Click;
        // 
        // sepToolbar
        // 
        sepToolbar.Name = "sepToolbar";
        sepToolbar.Size = new Size(283, 6);
        sepToolbar.Visible = false;
        // 
        // tsmiToolbar
        // 
        tsmiToolbar.Name = "tsmiToolbar";
        tsmiToolbar.Size = new Size(286, 22);
        tsmiToolbar.Text = "&Toolbar";
        // 
        // ItemContextMenu
        // 
        ItemContextMenu.Items.AddRange(new ToolStripItem[] { tsmiUpdateSubmodule, tsmiResetSubmoduleChanges, tsmiStashSubmoduleChanges, tsmiCommitSubmoduleChanges, sepSubmodule, tsmiStageFile, tsmiUnstageFile, tsmiResetFileTo, tsmiResetChunkOfFile, tsmiInteractiveAdd, tsmiCherryPickChanges, sepGit, tsmiOpenWithDifftool, tsmiOpenWorkingDirectoryFile, tsmiOpenWorkingDirectoryFileWith, tsmiOpenRevisionFile, tsmiOpenRevisionFileWith, tsmiEditWorkingDirectoryFile, tsmiOpenInVisualStudio, tsmiSaveAs, tsmiMove, tsmiDeleteFile, sepFile, tsmiCopyPaths, tsmiShowInFolder, sepBrowse, tsmiShowInFileTree, tsmiFilterFileInGrid, tsmiFileHistory, tsmiBlame, tsmiFindFile, tsmiOpenFindInCommitFilesGitGrepDialog, tsmiShowFindInCommitFilesGitGrep, sepIgnore, tsmiAddFileToGitIgnore, tsmiAddFileToGitInfoExclude, tsmiSkipWorktree, tsmiAssumeUnchanged, tsmiStopTracking, sepScripts, tsmiRunScript });
        ItemContextMenu.Name = "DiffContextMenu";
        ItemContextMenu.Size = new Size(296, 832);
        ItemContextMenu.Opening += ItemContextMenu_Opening;
        // 
        // tsmiUpdateSubmodule
        // 
        tsmiUpdateSubmodule.Image = Properties.Images.SubmodulesUpdate;
        tsmiUpdateSubmodule.Name = "tsmiUpdateSubmodule";
        tsmiUpdateSubmodule.Size = new Size(295, 22);
        tsmiUpdateSubmodule.Tag = "1";
        tsmiUpdateSubmodule.Text = "&Update submodule";
        tsmiUpdateSubmodule.Click += UpdateSubmodule_Click;
        // 
        // tsmiResetSubmoduleChanges
        // 
        tsmiResetSubmoduleChanges.Image = Properties.Images.ResetWorkingDirChanges;
        tsmiResetSubmoduleChanges.Name = "tsmiResetSubmoduleChanges";
        tsmiResetSubmoduleChanges.Size = new Size(295, 22);
        tsmiResetSubmoduleChanges.Text = "R&eset submodule changes";
        tsmiResetSubmoduleChanges.Click += ResetSubmoduleChanges_Click;
        // 
        // tsmiStashSubmoduleChanges
        // 
        tsmiStashSubmoduleChanges.Image = Properties.Images.Stash;
        tsmiStashSubmoduleChanges.Name = "tsmiStashSubmoduleChanges";
        tsmiStashSubmoduleChanges.Size = new Size(295, 22);
        tsmiStashSubmoduleChanges.Text = "S&tash submodule changes";
        tsmiStashSubmoduleChanges.Click += StashSubmoduleChanges_Click;
        // 
        // tsmiCommitSubmoduleChanges
        // 
        tsmiCommitSubmoduleChanges.Image = Properties.Images.RepoStateDirtySubmodules;
        tsmiCommitSubmoduleChanges.Name = "tsmiCommitSubmoduleChanges";
        tsmiCommitSubmoduleChanges.Size = new Size(295, 22);
        tsmiCommitSubmoduleChanges.Text = "&Commit submodule changes";
        tsmiCommitSubmoduleChanges.Click += CommitSubmoduleChanges_Click;
        // 
        // sepSubmodule
        // 
        sepSubmodule.Name = "sepSubmodule";
        sepSubmodule.Size = new Size(292, 6);
        sepSubmodule.Tag = "1";
        // 
        // tsmiStageFile
        // 
        tsmiStageFile.Image = Properties.Images.Stage;
        tsmiStageFile.Name = "tsmiStageFile";
        tsmiStageFile.Size = new Size(295, 22);
        tsmiStageFile.Text = "&Stage selected";
        tsmiStageFile.Click += StageFile_Click;
        // 
        // tsmiUnstageFile
        // 
        tsmiUnstageFile.Image = Properties.Images.Unstage;
        tsmiUnstageFile.Name = "tsmiUnstageFile";
        tsmiUnstageFile.Size = new Size(295, 22);
        tsmiUnstageFile.Text = "&Unstage selected";
        tsmiUnstageFile.Click += UnstageFile_Click;
        // 
        // tsmiResetFileTo
        // 
        tsmiResetFileTo.DropDownItems.AddRange(new ToolStripItem[] { tsmiResetFileToSelected, tsmiResetFileToParent });
        tsmiResetFileTo.Image = Properties.Images.ResetWorkingDirChanges;
        tsmiResetFileTo.Name = "tsmiResetFileTo";
        tsmiResetFileTo.Size = new Size(295, 22);
        tsmiResetFileTo.Text = "&Reset file(s) to";
        tsmiResetFileTo.Click += ResetFile_Click;
        // 
        // tsmiResetFileToSelected
        // 
        tsmiResetFileToSelected.Name = "tsmiResetFileToSelected";
        tsmiResetFileToSelected.Size = new Size(67, 22);
        tsmiResetFileToSelected.Click += ResetFile_Click;
        // 
        // tsmiResetFileToParent
        // 
        tsmiResetFileToParent.Name = "tsmiResetFileToParent";
        tsmiResetFileToParent.Size = new Size(67, 22);
        tsmiResetFileToParent.Click += ResetFile_Click;
        // 
        // tsmiResetChunkOfFile
        // 
        tsmiResetChunkOfFile.Name = "tsmiResetChunkOfFile";
        tsmiResetChunkOfFile.Size = new Size(295, 22);
        tsmiResetChunkOfFile.Text = "Reset chunk of file...";
        tsmiResetChunkOfFile.Visible = false;
        tsmiResetChunkOfFile.Click += ResetChunkOfFile_Click;
        // 
        // tsmiInteractiveAdd
        // 
        tsmiInteractiveAdd.Name = "tsmiInteractiveAdd";
        tsmiInteractiveAdd.Size = new Size(295, 22);
        tsmiInteractiveAdd.Text = "Interactive add...";
        tsmiInteractiveAdd.Visible = false;
        tsmiInteractiveAdd.Click += InteractiveAdd_Click;
        // 
        // tsmiCherryPickChanges
        // 
        tsmiCherryPickChanges.Image = Properties.Images.CherryPick;
        tsmiCherryPickChanges.Name = "tsmiCherryPickChanges";
        tsmiCherryPickChanges.Size = new Size(295, 22);
        tsmiCherryPickChanges.Text = "Cherr&y pick changes";
        tsmiCherryPickChanges.Visible = false;
        tsmiCherryPickChanges.Click += CherryPickChanges_Click;
        // 
        // sepGit
        // 
        sepGit.Name = "sepGit";
        sepGit.Size = new Size(292, 6);
        // 
        // tsmiOpenWithDifftool
        // 
        tsmiOpenWithDifftool.DropDownItems.AddRange(new ToolStripItem[] { tsmiSecondDiffCaption, tsmiFirstDiffCaption, tsmiDiffFirstToSelected, tsmiDiffSelectedToLocal, tsmiDiffFirstToLocal, sepDifftoolRemember, tsmiDiffTwoSelected, tsmiDiffWithRemembered, tsmiRememberSecondRevDiff, tsmiRememberFirstRevDiff });
        tsmiOpenWithDifftool.Image = Properties.Images.Diff;
        tsmiOpenWithDifftool.Name = "tsmiOpenWithDifftool";
        tsmiOpenWithDifftool.Size = new Size(295, 22);
        tsmiOpenWithDifftool.Text = "Open with &difftool";
        tsmiOpenWithDifftool.DropDownOpening += OpenWithDifftool_DropDownOpening;
        tsmiOpenWithDifftool.Click += DiffFirstToSelected_Click;
        // 
        // tsmiSecondDiffCaption
        // 
        tsmiSecondDiffCaption.Enabled = false;
        tsmiSecondDiffCaption.Name = "tsmiSecondDiffCaption";
        tsmiSecondDiffCaption.Size = new Size(227, 22);
        // 
        // tsmiFirstDiffCaption
        // 
        tsmiFirstDiffCaption.Enabled = false;
        tsmiFirstDiffCaption.Name = "tsmiFirstDiffCaption";
        tsmiFirstDiffCaption.Size = new Size(227, 22);
        // 
        // tsmiDiffFirstToSelected
        // 
        tsmiDiffFirstToSelected.Name = "tsmiDiffFirstToSelected";
        tsmiDiffFirstToSelected.Size = new Size(227, 22);
        tsmiDiffFirstToSelected.Text = "&First -> Second";
        tsmiDiffFirstToSelected.Click += DiffFirstToSelected_Click;
        // 
        // tsmiDiffSelectedToLocal
        // 
        tsmiDiffSelectedToLocal.Name = "tsmiDiffSelectedToLocal";
        tsmiDiffSelectedToLocal.Size = new Size(227, 22);
        tsmiDiffSelectedToLocal.Text = "&Second -> Working directory";
        tsmiDiffSelectedToLocal.Click += DiffSelectedToLocal_Click;
        // 
        // tsmiDiffFirstToLocal
        // 
        tsmiDiffFirstToLocal.Name = "tsmiDiffFirstToLocal";
        tsmiDiffFirstToLocal.Size = new Size(227, 22);
        tsmiDiffFirstToLocal.Text = "First -> &Working directory";
        tsmiDiffFirstToLocal.Click += DiffFirstToLocal_Click;
        // 
        // sepDifftoolRemember
        // 
        sepDifftoolRemember.Name = "sepDifftoolRemember";
        sepDifftoolRemember.Size = new Size(224, 6);
        // 
        // tsmiDiffTwoSelected
        // 
        tsmiDiffTwoSelected.Name = "tsmiDiffTwoSelected";
        tsmiDiffTwoSelected.Size = new Size(227, 22);
        tsmiDiffTwoSelected.Text = "&Diff the selected files";
        tsmiDiffTwoSelected.Click += DiffTwoSelected_Click;
        // 
        // tsmiDiffWithRemembered
        // 
        tsmiDiffWithRemembered.Name = "tsmiDiffWithRemembered";
        tsmiDiffWithRemembered.Size = new Size(227, 22);
        tsmiDiffWithRemembered.Click += DiffWithRemembered_Click;
        // 
        // tsmiRememberSecondRevDiff
        // 
        tsmiRememberSecondRevDiff.Name = "tsmiRememberSecondRevDiff";
        tsmiRememberSecondRevDiff.Size = new Size(227, 22);
        tsmiRememberSecondRevDiff.Text = "&Remember Second for diff";
        tsmiRememberSecondRevDiff.Click += RememberSecondRevDiff_Click;
        // 
        // tsmiRememberFirstRevDiff
        // 
        tsmiRememberFirstRevDiff.Name = "tsmiRememberFirstRevDiff";
        tsmiRememberFirstRevDiff.Size = new Size(227, 22);
        tsmiRememberFirstRevDiff.Text = "R&emember First for diff";
        tsmiRememberFirstRevDiff.Click += RememberFirstRevDiff_Click;
        // 
        // tsmiOpenWorkingDirectoryFile
        // 
        tsmiOpenWorkingDirectoryFile.Image = Properties.Images.EditFile;
        tsmiOpenWorkingDirectoryFile.Name = "tsmiOpenWorkingDirectoryFile";
        tsmiOpenWorkingDirectoryFile.Size = new Size(295, 22);
        tsmiOpenWorkingDirectoryFile.Text = "&Open working directory file";
        tsmiOpenWorkingDirectoryFile.Click += OpenWorkingDirectoryFile_Click;
        // 
        // tsmiOpenWorkingDirectoryFileWith
        // 
        tsmiOpenWorkingDirectoryFileWith.Image = Properties.Images.EditFile;
        tsmiOpenWorkingDirectoryFileWith.Name = "tsmiOpenWorkingDirectoryFileWith";
        tsmiOpenWorkingDirectoryFileWith.Size = new Size(295, 22);
        tsmiOpenWorkingDirectoryFileWith.Text = "Open working directory file with...";
        tsmiOpenWorkingDirectoryFileWith.Click += OpenWorkingDirectoryFileWith_Click;
        // 
        // tsmiOpenRevisionFile
        // 
        tsmiOpenRevisionFile.Image = Properties.Images.ViewFile;
        tsmiOpenRevisionFile.Name = "tsmiOpenRevisionFile";
        tsmiOpenRevisionFile.Size = new Size(295, 22);
        tsmiOpenRevisionFile.Text = "Ope&n this revision (temp file)";
        tsmiOpenRevisionFile.Click += OpenRevisionFile_Click;
        // 
        // tsmiOpenRevisionFileWith
        // 
        tsmiOpenRevisionFileWith.Image = Properties.Images.ViewFile;
        tsmiOpenRevisionFileWith.Name = "tsmiOpenRevisionFileWith";
        tsmiOpenRevisionFileWith.Size = new Size(295, 22);
        tsmiOpenRevisionFileWith.Text = "Open this revision &with... (temp file)";
        tsmiOpenRevisionFileWith.Click += OpenRevisionFileWith_Click;
        // 
        // tsmiEditWorkingDirectoryFile
        // 
        tsmiEditWorkingDirectoryFile.Image = Properties.Images.EditFile;
        tsmiEditWorkingDirectoryFile.Name = "tsmiEditWorkingDirectoryFile";
        tsmiEditWorkingDirectoryFile.Size = new Size(295, 22);
        tsmiEditWorkingDirectoryFile.Text = "&Edit working directory file";
        tsmiEditWorkingDirectoryFile.Click += EditWorkingDirectoryFile_Click;
        // 
        // tsmiOpenInVisualStudio
        // 
        tsmiOpenInVisualStudio.Image = Properties.Images.VisualStudio16;
        tsmiOpenInVisualStudio.Name = "tsmiOpenInVisualStudio";
        tsmiOpenInVisualStudio.Size = new Size(295, 22);
        tsmiOpenInVisualStudio.Text = "Open in &Visual Studio";
        tsmiOpenInVisualStudio.Click += OpenInVisualStudio_Click;
        // 
        // tsmiSaveAs
        // 
        tsmiSaveAs.Image = Properties.Images.SaveAs;
        tsmiSaveAs.Name = "tsmiSaveAs";
        tsmiSaveAs.ShortcutKeys = Keys.Control | Keys.S;
        tsmiSaveAs.Size = new Size(295, 22);
        tsmiSaveAs.Text = "S&ave selected as...";
        tsmiSaveAs.Click += SaveAs_Click;
        // 
        // tsmiMove
        // 
        tsmiMove.Name = "tsmiMove";
        tsmiMove.Size = new Size(295, 22);
        tsmiMove.Text = "Rena&me / move";
        tsmiMove.Click += Move_Click;
        // 
        // tsmiDeleteFile
        // 
        tsmiDeleteFile.Image = Properties.Images.DeleteFile;
        tsmiDeleteFile.Name = "tsmiDeleteFile";
        tsmiDeleteFile.Size = new Size(295, 22);
        tsmiDeleteFile.Text = "De&lete file";
        tsmiDeleteFile.Click += DeleteFile_Click;
        // 
        // sepFile
        // 
        sepFile.Name = "sepFile";
        sepFile.Size = new Size(292, 6);
        sepFile.Tag = "1";
        // 
        // tsmiCopyPaths
        // 
        tsmiCopyPaths.Image = (Image)resources.GetObject("tsmiCopyPaths.Image");
        tsmiCopyPaths.Name = "CopyPathsToolStripMenuItem";
        tsmiCopyPaths.Size = new Size(295, 22);
        tsmiCopyPaths.Text = "Copy &path(s)";
        // 
        // tsmiShowInFolder
        // 
        tsmiShowInFolder.Image = Properties.Images.BrowseFileExplorer;
        tsmiShowInFolder.Name = "tsmiShowInFolder";
        tsmiShowInFolder.Size = new Size(295, 22);
        tsmiShowInFolder.Text = "Show &in folder";
        tsmiShowInFolder.Click += ShowInFolder_Click;
        // 
        // sepBrowse
        // 
        sepBrowse.Name = "sepBrowse";
        sepBrowse.Size = new Size(292, 6);
        // 
        // tsmiShowInFileTree
        // 
        tsmiShowInFileTree.Image = Properties.Images.FileTree;
        tsmiShowInFileTree.Name = "tsmiShowInFileTree";
        tsmiShowInFileTree.Size = new Size(295, 22);
        tsmiShowInFileTree.Text = "Show in File &tree";
        tsmiShowInFileTree.Visible = false;
        tsmiShowInFileTree.Click += ShowInFileTree_Click;
        // 
        // tsmiFilterFileInGrid
        // 
        tsmiFilterFileInGrid.Image = Properties.Images.FunnelPencil;
        tsmiFilterFileInGrid.Name = "tsmiFilterFileInGrid";
        tsmiFilterFileInGrid.Size = new Size(295, 22);
        tsmiFilterFileInGrid.Visible = false;
        tsmiFilterFileInGrid.Click += FilterFileInGrid_Click;
        // 
        // tsmiFileHistory
        // 
        tsmiFileHistory.Image = Properties.Images.FileHistory;
        tsmiFileHistory.Name = "tsmiFileHistory";
        tsmiFileHistory.Size = new Size(295, 22);
        tsmiFileHistory.Text = "File &history";
        tsmiFileHistory.Click += FileHistory_Click;
        // 
        // tsmiBlame
        // 
        tsmiBlame.Image = Properties.Images.Blame;
        tsmiBlame.Name = "tsmiBlame";
        tsmiBlame.Size = new Size(295, 22);
        tsmiBlame.Text = "&Blame";
        tsmiBlame.Click += Blame_Click;
        // 
        // tsmiFindFile
        // 
        tsmiFindFile.Name = "tsmiFindFile";
        tsmiFindFile.Size = new Size(295, 22);
        tsmiFindFile.Text = "&Find file...";
        tsmiFindFile.Click += FindFile_Click;
        // 
        // tsmiOpenFindInCommitFilesGitGrepDialog
        // 
        tsmiOpenFindInCommitFilesGitGrepDialog.Image = Properties.Images.ViewFile;
        tsmiOpenFindInCommitFilesGitGrepDialog.Name = "tsmiOpenFindInCommitFilesGitGrepDialog";
        tsmiOpenFindInCommitFilesGitGrepDialog.Size = new Size(295, 22);
        tsmiOpenFindInCommitFilesGitGrepDialog.Text = "Find in &commit files using git-grep...";
        tsmiOpenFindInCommitFilesGitGrepDialog.Visible = false;
        tsmiOpenFindInCommitFilesGitGrepDialog.Click += OpenFindInCommitFilesGitGrepDialog_Click;
        // 
        // tsmiShowFindInCommitFilesGitGrep
        // 
        tsmiShowFindInCommitFilesGitGrep.CheckOnClick = true;
        tsmiShowFindInCommitFilesGitGrep.Name = "tsmiShowFindInCommitFilesGitGrep";
        tsmiShowFindInCommitFilesGitGrep.Size = new Size(295, 22);
        tsmiShowFindInCommitFilesGitGrep.Text = "Show 'Find in commit fi&les using git-grep'";
        tsmiShowFindInCommitFilesGitGrep.Visible = false;
        tsmiShowFindInCommitFilesGitGrep.Click += ShowFindInCommitFilesGitGrep_Click;
        // 
        // sepIgnore
        // 
        sepIgnore.Name = "sepIgnore";
        sepIgnore.Size = new Size(292, 6);
        // 
        // tsmiAddFileToGitIgnore
        // 
        tsmiAddFileToGitIgnore.Image = Properties.Images.AddToGitIgnore;
        tsmiAddFileToGitIgnore.Name = "tsmiAddFileToGitIgnore";
        tsmiAddFileToGitIgnore.Size = new Size(295, 22);
        tsmiAddFileToGitIgnore.Text = "Add file to &.gitignore";
        tsmiAddFileToGitIgnore.Click += AddFileToGitIgnore_Click;
        // 
        // tsmiAddFileToGitInfoExclude
        // 
        tsmiAddFileToGitInfoExclude.Image = Properties.Images.AddToGitIgnore;
        tsmiAddFileToGitInfoExclude.Name = "tsmiAddFileToGitInfoExclude";
        tsmiAddFileToGitInfoExclude.Size = new Size(295, 22);
        tsmiAddFileToGitInfoExclude.Text = "Add file to .git/info/exclude";
        tsmiAddFileToGitInfoExclude.Click += AddFileToGitInfoExclude_Click;
        // 
        // tsmiSkipWorktree
        // 
        tsmiSkipWorktree.CheckOnClick = true;
        tsmiSkipWorktree.Name = "tsmiSkipWorktree";
        tsmiSkipWorktree.Size = new Size(295, 22);
        tsmiSkipWorktree.Text = "S&kip worktree";
        tsmiSkipWorktree.Click += SkipWorktree_Click;
        // 
        // tsmiAssumeUnchanged
        // 
        tsmiAssumeUnchanged.CheckOnClick = true;
        tsmiAssumeUnchanged.Name = "tsmiAssumeUnchanged";
        tsmiAssumeUnchanged.Size = new Size(295, 22);
        tsmiAssumeUnchanged.Text = "Assu&me unchanged";
        tsmiAssumeUnchanged.Click += AssumeUnchanged_Click;
        // 
        // tsmiStopTracking
        // 
        tsmiStopTracking.Image = Properties.Images.StopTrackingFile;
        tsmiStopTracking.Name = "tsmiStopTracking";
        tsmiStopTracking.Size = new Size(295, 22);
        tsmiStopTracking.Text = "Stop tracking this file";
        tsmiStopTracking.Click += StopTracking_Click;
        // 
        // sepScripts
        // 
        sepScripts.Name = "sepScripts";
        sepScripts.Size = new Size(292, 6);
        // 
        // tsmiRunScript
        // 
        tsmiRunScript.Image = Properties.Images.Console;
        tsmiRunScript.Name = "tsmiRunScript";
        tsmiRunScript.Size = new Size(295, 22);
        tsmiRunScript.Text = "Run script";
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
        ItemContextMenu.ResumeLayout(false);
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
    private ToolStripSeparator sepRefresh;
    private ToolStripButton btnRefresh;
    private ToolStripSeparator sepAsTree;
    private ToolStripSplitButton btnAsTree;
    private ToolStripSeparator sepListOptions;
    private ToolStripMenuItem tsmiDenseTree;
    private ToolStripMenuItem tsmiShowGroupNodesInFlatList;
    private ToolStripSeparator sepGroupBy;
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
    private ToolStripSeparator sepSettings;
    private ToolStripDropDownButton btnSettings;
    internal ToolStripMenuItem tsmiShowIgnoredFiles;
    internal ToolStripMenuItem tsmiShowSkipWorktreeFiles;
    internal ToolStripMenuItem tsmiShowAssumeUnchangedFiles;
    internal ToolStripMenuItem tsmiShowUntrackedFiles;
    private ToolStripSeparator sepShow;
    private ToolStripMenuItem tsmiEditGitIgnore;
    private ToolStripMenuItem tmsiEditLocallyIgnoredFiles;
    private ToolStripSeparator sepEdit;
    private ToolStripMenuItem tsmiRefreshOnFormFocus;
    private ToolStripMenuItem tsmiShowDiffForAllParents;
    private ToolStripSeparator sepToolbar;
    private ToolStripMenuItem tsmiToolbar;
    private ToolStripMenuItem tsmiGroupByFilePathTree;
    private ToolStripMenuItem tsmiGroupByFilePathFlat;
    private ToolStripMenuItem tsmiGroupByFileExtensionTree;
    private ToolStripMenuItem tsmiGroupByFileExtensionFlat;
    private ToolStripMenuItem tsmiGroupByFileStatusTree;
    private ToolStripMenuItem tsmiGroupByFileStatusFlat;
    private ContextMenuStrip ItemContextMenu;
    private ToolStripMenuItem tsmiUpdateSubmodule;
    private ToolStripMenuItem tsmiResetSubmoduleChanges;
    private ToolStripMenuItem tsmiStashSubmoduleChanges;
    private ToolStripMenuItem tsmiCommitSubmoduleChanges;
    private ToolStripSeparator sepSubmodule;
    private ToolStripMenuItem tsmiStageFile;
    private ToolStripMenuItem tsmiUnstageFile;
    private ToolStripMenuItem tsmiResetFileTo;
    private ToolStripMenuItem tsmiResetFileToSelected;
    private ToolStripMenuItem tsmiResetFileToParent;
    private ToolStripMenuItem tsmiCherryPickChanges;
    private ToolStripSeparator sepGit;
    private ToolStripMenuItem tsmiOpenWithDifftool;
    private ToolStripMenuItem tsmiSecondDiffCaption;
    private ToolStripMenuItem tsmiFirstDiffCaption;
    internal ToolStripMenuItem tsmiDiffFirstToSelected;
    private ToolStripMenuItem tsmiDiffSelectedToLocal;
    private ToolStripMenuItem tsmiDiffFirstToLocal;
    private ToolStripSeparator sepDifftoolRemember;
    private ToolStripMenuItem tsmiDiffTwoSelected;
    private ToolStripMenuItem tsmiDiffWithRemembered;
    private ToolStripMenuItem tsmiRememberSecondRevDiff;
    private ToolStripMenuItem tsmiRememberFirstRevDiff;
    private ToolStripMenuItem tsmiOpenWorkingDirectoryFile;
    private ToolStripMenuItem tsmiOpenWorkingDirectoryFileWith;
    private ToolStripMenuItem tsmiOpenRevisionFile;
    private ToolStripMenuItem tsmiOpenRevisionFileWith;
    internal ToolStripMenuItem tsmiEditWorkingDirectoryFile;
    private ToolStripMenuItem tsmiOpenInVisualStudio;
    private ToolStripMenuItem tsmiSaveAs;
    private ToolStripMenuItem tsmiMove;
    private ToolStripMenuItem tsmiDeleteFile;
    private ToolStripSeparator sepFile;
    private CommandsDialogs.Menus.CopyPathsToolStripMenuItem tsmiCopyPaths;
    private ToolStripMenuItem tsmiShowInFolder;
    private ToolStripSeparator sepBrowse;
    private ToolStripMenuItem tsmiShowInFileTree;
    private ToolStripMenuItem tsmiFilterFileInGrid;
    private ToolStripMenuItem tsmiFileHistory;
    internal ToolStripMenuItem tsmiBlame;
    private ToolStripMenuItem tsmiFindFile;
    private ToolStripMenuItem tsmiOpenFindInCommitFilesGitGrepDialog;
    private ToolStripMenuItem tsmiShowFindInCommitFilesGitGrep;
    private ToolStripSeparator sepScripts;
    private ToolStripMenuItem tsmiRunScript;
    private ToolStripMenuItem tsmiResetChunkOfFile;
    private ToolStripMenuItem tsmiInteractiveAdd;
    private ToolStripSeparator sepIgnore;
    private ToolStripMenuItem tsmiAddFileToGitIgnore;
    private ToolStripMenuItem tsmiAddFileToGitInfoExclude;
    private ToolStripMenuItem tsmiSkipWorktree;
    private ToolStripMenuItem tsmiAssumeUnchanged;
    private ToolStripMenuItem tsmiStopTracking;
}
