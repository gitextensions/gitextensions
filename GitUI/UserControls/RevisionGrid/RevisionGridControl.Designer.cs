using System.Windows.Forms;
using GitUI.Hotkey;
using GitUI.UserControls.RevisionGrid;

namespace GitUI
{
    partial class RevisionGridControl
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGridControl));
            _gridView = new RevisionDataGridView();
            mainContextMenu = new ContextMenuStrip(components);
            markRevisionAsBadToolStripMenuItem = new ToolStripMenuItem();
            markRevisionAsGoodToolStripMenuItem = new ToolStripMenuItem();
            bisectSkipRevisionToolStripMenuItem = new ToolStripMenuItem();
            stopBisectToolStripMenuItem = new ToolStripMenuItem();
            bisectSeparator = new ToolStripSeparator();
            copyToClipboardToolStripMenuItem = new GitUI.UserControls.RevisionGrid.CopyContextMenuItem();
            stashStripSeparator = new ToolStripSeparator();
            applyStashToolStripMenuItem = new ToolStripMenuItem();
            popStashToolStripMenuItem = new ToolStripMenuItem();
            dropStashToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            checkoutBranchToolStripMenuItem = new ToolStripMenuItem();
            mergeBranchToolStripMenuItem = new ToolStripMenuItem();
            resetCurrentBranchToHereToolStripMenuItem = new ToolStripMenuItem();
            resetAnotherBranchToHereToolStripMenuItem = new ToolStripMenuItem();
            rebaseOnToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            createNewBranchToolStripMenuItem = new ToolStripMenuItem();
            deleteBranchToolStripMenuItem = new ToolStripMenuItem();
            renameBranchToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            openCommitsWithDiffToolMenuItem = new ToolStripMenuItem();
            compareStripSeparator = new ToolStripSeparator();
            compareToBranchToolStripMenuItem = new ToolStripMenuItem();
            compareToolStripMenuItem = new ToolStripMenuItem();
            compareWithCurrentBranchToolStripMenuItem = new ToolStripMenuItem();
            compareToBaseToolStripMenuItem = new ToolStripMenuItem();
            compareToWorkingDirectoryMenuItem = new ToolStripMenuItem();
            compareSelectedCommitsMenuItem = new ToolStripMenuItem();
            selectAsBaseToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            createTagToolStripMenuItem = new ToolStripMenuItem();
            deleteTagToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            checkoutRevisionToolStripMenuItem = new ToolStripMenuItem();
            revertCommitToolStripMenuItem = new ToolStripMenuItem();
            cherryPickCommitToolStripMenuItem = new ToolStripMenuItem();
            archiveRevisionToolStripMenuItem = new ToolStripMenuItem();
            manipulateCommitToolStripMenuItem = new ToolStripMenuItem();
            rewordCommitToolStripMenuItem = new ToolStripMenuItem();
            fixupCommitToolStripMenuItem = new ToolStripMenuItem();
            squashCommitToolStripMenuItem = new ToolStripMenuItem();
            amendCommitToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            navigateToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            runScriptToolStripMenuItem = new ToolStripMenuItem();
            openBuildReportToolStripMenuItem = new ToolStripMenuItem();
            openPullRequestPageStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            getHelpOnHowToUseTheseFeaturesToolStripMenuItem = new ToolStripMenuItem();
            editCommitToolStripMenuItem = new ToolStripMenuItem();
            rebaseToolStripMenuItem = new ToolStripMenuItem();
            rebaseInteractivelyToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            rebaseWithAdvOptionsToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(_gridView)).BeginInit();
            mainContextMenu.SuspendLayout();
            SuspendLayout();
            //
            // Graph
            //
            _gridView.AllowUserToAddRows = false;
            _gridView.AllowUserToDeleteRows = false;
            _gridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            _gridView.BackgroundColor = SystemColors.Window;
            _gridView.BorderStyle = BorderStyle.None;
            _gridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            _gridView.ColumnHeadersVisible = false;
            _gridView.ContextMenuStrip = mainContextMenu;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.GradientActiveCaption;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            _gridView.DefaultCellStyle = dataGridViewCellStyle4;
            _gridView.Dock = DockStyle.Fill;
            _gridView.GridColor = SystemColors.Window;
            _gridView.Location = new Point(0, 0);
            _gridView.Name = "_gridView";
            _gridView.ReadOnly = true;
            _gridView.RowHeadersVisible = false;
            _gridView.RowsDefaultCellStyle = dataGridViewCellStyle6;
            _gridView.RowTemplate.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            _gridView.RowTemplate.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            _gridView.RowTemplate.Resizable = DataGridViewTriState.False;
            _gridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridView.Size = new Size(682, 235);
            _gridView.StandardTab = true;
            _gridView.TabIndex = 0;
            _gridView.VirtualMode = true;
            // 
            // mainContextMenu
            // 
            mainContextMenu.Items.AddRange(new ToolStripItem[] {
            markRevisionAsBadToolStripMenuItem,
            markRevisionAsGoodToolStripMenuItem,
            bisectSkipRevisionToolStripMenuItem,
            stopBisectToolStripMenuItem,
            bisectSeparator,
            copyToClipboardToolStripMenuItem,
            toolStripSeparator8,
            applyStashToolStripMenuItem,
            popStashToolStripMenuItem,
            dropStashToolStripMenuItem,
            stashStripSeparator,
            checkoutBranchToolStripMenuItem,
            mergeBranchToolStripMenuItem,
            rebaseOnToolStripMenuItem,
            resetCurrentBranchToHereToolStripMenuItem,
            toolStripSeparator3,
            createNewBranchToolStripMenuItem,
            renameBranchToolStripMenuItem,
            deleteBranchToolStripMenuItem,
            resetAnotherBranchToHereToolStripMenuItem,
            toolStripSeparator4,
            createTagToolStripMenuItem,
            deleteTagToolStripMenuItem,
            toolStripSeparator2,
            checkoutRevisionToolStripMenuItem,
            revertCommitToolStripMenuItem,
            cherryPickCommitToolStripMenuItem,
            archiveRevisionToolStripMenuItem,
            manipulateCommitToolStripMenuItem,
            toolStripSeparator1,
            compareToolStripMenuItem,
            toolStripSeparator5,
            navigateToolStripMenuItem,
            viewToolStripMenuItem,
            runScriptToolStripMenuItem,
            openBuildReportToolStripMenuItem,
            openPullRequestPageStripMenuItem
            });
            mainContextMenu.Name = "CreateTag";
            mainContextMenu.Size = new Size(265, 620);
            mainContextMenu.Opening += ContextMenuOpening;
            // 
            // markRevisionAsBadToolStripMenuItem
            // 
            markRevisionAsBadToolStripMenuItem.Image = GitUI.Properties.Images.BisectBad;
            markRevisionAsBadToolStripMenuItem.Name = "markRevisionAsBadToolStripMenuItem";
            markRevisionAsBadToolStripMenuItem.Size = new Size(264, 24);
            markRevisionAsBadToolStripMenuItem.Text = "Mark revision as bad";
            markRevisionAsBadToolStripMenuItem.Click += MarkRevisionAsBadToolStripMenuItemClick;
            // 
            // markRevisionAsGoodToolStripMenuItem
            // 
            markRevisionAsGoodToolStripMenuItem.Image = GitUI.Properties.Images.BisectGood;
            markRevisionAsGoodToolStripMenuItem.Name = "markRevisionAsGoodToolStripMenuItem";
            markRevisionAsGoodToolStripMenuItem.Size = new Size(264, 24);
            markRevisionAsGoodToolStripMenuItem.Text = "Mark revision as good";
            markRevisionAsGoodToolStripMenuItem.Click += MarkRevisionAsGoodToolStripMenuItemClick;
            // 
            // bisectSkipRevisionToolStripMenuItem
            // 
            bisectSkipRevisionToolStripMenuItem.Image = GitUI.Properties.Images.BisectSkip;
            bisectSkipRevisionToolStripMenuItem.Name = "bisectSkipRevisionToolStripMenuItem";
            bisectSkipRevisionToolStripMenuItem.Size = new Size(264, 24);
            bisectSkipRevisionToolStripMenuItem.Text = "Skip revision";
            bisectSkipRevisionToolStripMenuItem.Click += BisectSkipRevisionToolStripMenuItemClick;
            // 
            // stopBisectToolStripMenuItem
            // 
            stopBisectToolStripMenuItem.Image = GitUI.Properties.Images.BisectStop;
            stopBisectToolStripMenuItem.Name = "stopBisectToolStripMenuItem";
            stopBisectToolStripMenuItem.Size = new Size(264, 24);
            stopBisectToolStripMenuItem.Text = "Stop bisect";
            stopBisectToolStripMenuItem.Click += StopBisectToolStripMenuItemClick;
            // 
            // bisectSeparator
            // 
            bisectSeparator.Name = "bisectSeparator";
            bisectSeparator.Size = new Size(261, 6);
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(261, 6);
            // 
            // applyStashToolStripMenuItem
            // 
            applyStashToolStripMenuItem.Image = Properties.Images.Stash;
            applyStashToolStripMenuItem.Name = "applyStashToolStripMenuItem";
            applyStashToolStripMenuItem.Size = new Size(264, 24);
            applyStashToolStripMenuItem.Text = "Appl&y stash";
            applyStashToolStripMenuItem.Click += ApplyStashToolStripMenuItemClick;
            // 
            // popStashToolStripMenuItem
            // 
            popStashToolStripMenuItem.Image = Properties.Images.Stash;
            popStashToolStripMenuItem.Name = "popStashToolStripMenuItem";
            popStashToolStripMenuItem.Size = new Size(264, 24);
            popStashToolStripMenuItem.Text = "Pop &stash";
            popStashToolStripMenuItem.Click += PopStashToolStripMenuItemClick;
            // 
            // dropStashToolStripMenuItem
            // 
            dropStashToolStripMenuItem.Image = Properties.Images.Stash;
            dropStashToolStripMenuItem.Name = "dropStashToolStripMenuItem";
            dropStashToolStripMenuItem.Size = new Size(264, 24);
            dropStashToolStripMenuItem.Text = "&Drop stash...";
            dropStashToolStripMenuItem.Click += DropStashToolStripMenuItemClick;
            // 
            // stashStripSeparator
            // 
            stashStripSeparator.Name = "stashStripSeparator";
            stashStripSeparator.Size = new Size(220, 6);
            // 
            // checkoutBranchToolStripMenuItem
            // 
            checkoutBranchToolStripMenuItem.Image = Properties.Images.BranchCheckout;
            checkoutBranchToolStripMenuItem.Name = "checkoutBranchToolStripMenuItem";
            checkoutBranchToolStripMenuItem.Size = new Size(264, 24);
            checkoutBranchToolStripMenuItem.Text = "Chec&kout branch...";
            checkoutBranchToolStripMenuItem.Click += deleteBranchTagToolStripMenuItem_Click;
            // 
            // mergeBranchToolStripMenuItem
            // 
            mergeBranchToolStripMenuItem.Image = Properties.Images.Merge;
            mergeBranchToolStripMenuItem.Name = "mergeBranchToolStripMenuItem";
            mergeBranchToolStripMenuItem.Size = new Size(264, 24);
            mergeBranchToolStripMenuItem.Text = "&Merge into current branch...";
            mergeBranchToolStripMenuItem.Click += deleteBranchTagToolStripMenuItem_Click;
            // 
            // rebaseOnToolStripMenuItem
            // 
            rebaseOnToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            rebaseToolStripMenuItem,
            rebaseInteractivelyToolStripMenuItem,
            toolStripSeparator10,
            rebaseWithAdvOptionsToolStripMenuItem});
            rebaseOnToolStripMenuItem.Image = Properties.Images.Rebase;
            rebaseOnToolStripMenuItem.Name = "rebaseOnToolStripMenuItem";
            rebaseOnToolStripMenuItem.Size = new Size(270, 22);
            rebaseOnToolStripMenuItem.Text = "&Rebase current branch on";
            rebaseOnToolStripMenuItem.DropDownOpening += RebaseOnToolStripMenuItem_DropDownOpening;
            // 
            // resetCurrentBranchToHereToolStripMenuItem
            // 
            resetCurrentBranchToHereToolStripMenuItem.Image = Properties.Images.ResetCurrentBranchToHere;
            resetCurrentBranchToHereToolStripMenuItem.Name = "resetCurrentBranchToHereToolStripMenuItem";
            resetCurrentBranchToHereToolStripMenuItem.Size = new Size(223, 22);
            resetCurrentBranchToHereToolStripMenuItem.Text = "Reset c&urrent branch to here...";
            resetCurrentBranchToHereToolStripMenuItem.Click += ResetCurrentBranchToHereToolStripMenuItemClick;
            // 
            // resetAnotherBranchToHereToolStripMenuItem
            // 
            resetAnotherBranchToHereToolStripMenuItem.Image = Properties.Images.ResetAnotherBranchToHere;
            resetAnotherBranchToHereToolStripMenuItem.Name = "resetAnotherBranchToHereToolStripMenuItem";
            resetAnotherBranchToHereToolStripMenuItem.Size = new Size(223, 22);
            resetAnotherBranchToHereToolStripMenuItem.Text = "Reset an&other branch to here...";
            resetAnotherBranchToHereToolStripMenuItem.Click += ResetAnotherBranchToHereToolStripMenuItemClick;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(261, 6);
            // 
            // createNewBranchToolStripMenuItem
            // 
            createNewBranchToolStripMenuItem.Image = Properties.Images.BranchCreate;
            createNewBranchToolStripMenuItem.Name = "createNewBranchToolStripMenuItem";
            createNewBranchToolStripMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.B)));
            createNewBranchToolStripMenuItem.Size = new Size(264, 24);
            createNewBranchToolStripMenuItem.Text = "Create new branc&h here...";
            createNewBranchToolStripMenuItem.Click += CreateNewBranchToolStripMenuItemClick;
            // 
            // renameBranchToolStripMenuItem
            // 
            renameBranchToolStripMenuItem.Image = Properties.Images.Renamed;
            renameBranchToolStripMenuItem.Name = "renameBranchToolStripMenuItem";
            renameBranchToolStripMenuItem.Size = new Size(223, 22);
            renameBranchToolStripMenuItem.Text = "R&ename branch...";
            renameBranchToolStripMenuItem.Click += renameBranchToolStripMenuItem_Click;
            // 
            // deleteBranchToolStripMenuItem
            // 
            deleteBranchToolStripMenuItem.Image = Properties.Images.BranchDelete;
            deleteBranchToolStripMenuItem.Name = "deleteBranchToolStripMenuItem";
            deleteBranchToolStripMenuItem.Size = new Size(264, 24);
            deleteBranchToolStripMenuItem.Text = "&Delete branch...";
            deleteBranchToolStripMenuItem.Click += deleteBranchTagToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(220, 6);
            // 
            // openCommitsWithDiffToolMenuItem
            // 
            openCommitsWithDiffToolMenuItem.Name = "openCommitsWithDiffToolMenuItem";
            openCommitsWithDiffToolMenuItem.Size = new Size(230, 22);
            openCommitsWithDiffToolMenuItem.Text = "Open selected commits with &difftool";
            openCommitsWithDiffToolMenuItem.Click += diffSelectedCommitsMenuItem_Click;
            // 
            // compareStripSeparator
            // 
            compareStripSeparator.Name = "compareStripSeparator";
            compareStripSeparator.Size = new Size(220, 6);
            // 
            // compareToBranchToolStripMenuItem
            // 
            compareToBranchToolStripMenuItem.Name = "compareToBranchToolStripMenuItem";
            compareToBranchToolStripMenuItem.Size = new Size(230, 22);
            compareToBranchToolStripMenuItem.Text = "Compare &to branch...";
            compareToBranchToolStripMenuItem.Click += CompareToBranchToolStripMenuItem_Click;
            // 
            // compareWithCurrentBranchToolStripMenuItem
            // 
            compareWithCurrentBranchToolStripMenuItem.Name = "compareWithCurrentBranchToolStripMenuItem";
            compareWithCurrentBranchToolStripMenuItem.Size = new Size(230, 22);
            compareWithCurrentBranchToolStripMenuItem.Text = "Compare with &current branch";
            compareWithCurrentBranchToolStripMenuItem.Click += CompareWithCurrentBranchToolStripMenuItem_Click;
            // 
            // selectAsBaseToolStripMenuItem
            // 
            selectAsBaseToolStripMenuItem.Name = "selectAsBaseToolStripMenuItem";
            selectAsBaseToolStripMenuItem.Size = new Size(230, 22);
            selectAsBaseToolStripMenuItem.Text = "Select &as BASE to compare";
            selectAsBaseToolStripMenuItem.Click += selectAsBaseToolStripMenuItem_Click;
            // 
            // compareToBaseToolStripMenuItem
            // 
            compareToBaseToolStripMenuItem.Name = "compareToBaseToolStripMenuItem";
            compareToBaseToolStripMenuItem.Size = new Size(230, 22);
            compareToBaseToolStripMenuItem.Text = "Compare to &BASE";
            compareToBaseToolStripMenuItem.Enabled = false;
            compareToBaseToolStripMenuItem.Click += compareToBaseToolStripMenuItem_Click;
            // 
            // compareToWorkingDirectoryMenuItem
            // 
            compareToWorkingDirectoryMenuItem.Name = "compareToWorkingDirectoryMenuItem";
            compareToWorkingDirectoryMenuItem.Size = new Size(230, 22);
            compareToWorkingDirectoryMenuItem.Text = "Compare to &working directory";
            compareToWorkingDirectoryMenuItem.Click += compareToWorkingDirectoryMenuItem_Click;
            // 
            // compareSelectedCommitsMenuItem
            // 
            compareSelectedCommitsMenuItem.Name = "compareSelectedCommitsMenuItem";
            compareSelectedCommitsMenuItem.Size = new Size(230, 22);
            compareSelectedCommitsMenuItem.Text = "Compare &selected commits";
            compareSelectedCommitsMenuItem.Click += compareSelectedCommitsMenuItem_Click;
            // 
            // compareToolStripMenuItem
            // 
            compareToolStripMenuItem.Image = Properties.Images.Diff;
            compareToolStripMenuItem.Name = "compareToolStripMenuItem";
            compareToolStripMenuItem.Size = new Size(230, 22);
            compareToolStripMenuItem.Text = "Com&pare";
            compareToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                openCommitsWithDiffToolMenuItem,
                compareStripSeparator,
                compareToBranchToolStripMenuItem,
                compareWithCurrentBranchToolStripMenuItem,
                selectAsBaseToolStripMenuItem,
                compareToBaseToolStripMenuItem,
                compareToWorkingDirectoryMenuItem,
                compareSelectedCommitsMenuItem
            });
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(261, 6);
            // 
            // createTagToolStripMenuItem
            // 
            createTagToolStripMenuItem.Image = Properties.Images.TagCreate;
            createTagToolStripMenuItem.Name = "createTagToolStripMenuItem";
            createTagToolStripMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.T)));
            createTagToolStripMenuItem.Size = new Size(264, 24);
            createTagToolStripMenuItem.Text = "Create new ta&g here...";
            createTagToolStripMenuItem.Click += CreateTagToolStripMenuItemClick;
            // 
            // deleteTagToolStripMenuItem
            // 
            deleteTagToolStripMenuItem.Image = Properties.Images.TagDelete;
            deleteTagToolStripMenuItem.Name = "deleteTagToolStripMenuItem";
            deleteTagToolStripMenuItem.Size = new Size(264, 24);
            deleteTagToolStripMenuItem.Text = "De&lete tag...";
            deleteTagToolStripMenuItem.Click += deleteBranchTagToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(261, 6);
            // 
            // checkoutRevisionToolStripMenuItem
            // 
            checkoutRevisionToolStripMenuItem.Image = Properties.Images.Checkout;
            checkoutRevisionToolStripMenuItem.Name = "checkoutRevisionToolStripMenuItem";
            checkoutRevisionToolStripMenuItem.Size = new Size(264, 24);
            checkoutRevisionToolStripMenuItem.Text = "Checkout &this commit...";
            checkoutRevisionToolStripMenuItem.Click += CheckoutRevisionToolStripMenuItemClick;
            // 
            // revertCommitToolStripMenuItem
            // 
            revertCommitToolStripMenuItem.Image = Properties.Images.RevertCommit;
            revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            revertCommitToolStripMenuItem.Size = new Size(264, 24);
            revertCommitToolStripMenuItem.Text = "Re&vert this commit...";
            revertCommitToolStripMenuItem.Click += RevertCommitToolStripMenuItemClick;
            // 
            // cherryPickCommitToolStripMenuItem
            // 
            cherryPickCommitToolStripMenuItem.Image = Properties.Images.CherryPick;
            cherryPickCommitToolStripMenuItem.Name = "cherryPickCommitToolStripMenuItem";
            cherryPickCommitToolStripMenuItem.Size = new Size(264, 24);
            cherryPickCommitToolStripMenuItem.Text = "Cherr&y pick this commit...";
            cherryPickCommitToolStripMenuItem.Click += CherryPickCommitToolStripMenuItemClick;
            // 
            // archiveRevisionToolStripMenuItem
            // 
            archiveRevisionToolStripMenuItem.Image = Properties.Images.ArchiveRevision;
            archiveRevisionToolStripMenuItem.Name = "archiveRevisionToolStripMenuItem";
            archiveRevisionToolStripMenuItem.Size = new Size(264, 24);
            archiveRevisionToolStripMenuItem.Text = "Arch&ive this commit...";
            archiveRevisionToolStripMenuItem.Click += ArchiveRevisionToolStripMenuItemClick;
            // 
            // manipulateCommitToolStripMenuItem
            // 
            manipulateCommitToolStripMenuItem.Image = Properties.Images.Advanced;
            manipulateCommitToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            editCommitToolStripMenuItem,
            rewordCommitToolStripMenuItem,
            fixupCommitToolStripMenuItem,
            squashCommitToolStripMenuItem,
            amendCommitToolStripMenuItem,
            getHelpOnHowToUseTheseFeaturesToolStripMenuItem});
            manipulateCommitToolStripMenuItem.Name = "manipulateCommitToolStripMenuItem";
            manipulateCommitToolStripMenuItem.Size = new Size(264, 24);
            manipulateCommitToolStripMenuItem.Text = "&Advanced";
            //
            // fixupCommitToolStripMenuItem
            // 
            fixupCommitToolStripMenuItem.Name = "fixupCommitToolStripMenuItem";
            fixupCommitToolStripMenuItem.Size = new Size(180, 24);
            fixupCommitToolStripMenuItem.Text = "Create a &fixup commit...";
            fixupCommitToolStripMenuItem.Click += FixupCommitToolStripMenuItemClick;
            // 
            // squashCommitToolStripMenuItem
            // 
            squashCommitToolStripMenuItem.Name = "squashCommitToolStripMenuItem";
            squashCommitToolStripMenuItem.Size = new Size(180, 24);
            squashCommitToolStripMenuItem.Text = "Create a &squash commit...";
            squashCommitToolStripMenuItem.Click += SquashCommitToolStripMenuItemClick;
            // 
            // amendCommitToolStripMenuItem
            // 
            amendCommitToolStripMenuItem.Name = "amendCommitToolStripMenuItem";
            amendCommitToolStripMenuItem.Size = new Size(272, 22);
            amendCommitToolStripMenuItem.Text = "Create an &amend commit...";
            amendCommitToolStripMenuItem.Click += AmendCommitToolStripMenuItemClick;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(220, 6);
            // 
            // navigateToolStripMenuItem
            // 
            navigateToolStripMenuItem.Image = Properties.Images.GotoCommit;
            navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            navigateToolStripMenuItem.Size = new Size(223, 22);
            navigateToolStripMenuItem.Text = "&Navigate";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.Image = Properties.Images.AdvancedSettings;
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(223, 22);
            viewToolStripMenuItem.Text = "View";
            // 
            // runScriptToolStripMenuItem
            // 
            runScriptToolStripMenuItem.Image = Properties.Images.Console;
            runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            runScriptToolStripMenuItem.Size = new Size(223, 22);
            runScriptToolStripMenuItem.Text = "Run &script";
            // 
            // openBuildReportToolStripMenuItem
            // 
            openBuildReportToolStripMenuItem.Image = Properties.Images.Integration;
            openBuildReportToolStripMenuItem.Name = "openBuildReportToolStripMenuItem";
            openBuildReportToolStripMenuItem.Size = new Size(301, 26);
            openBuildReportToolStripMenuItem.Text = "View &build report in a browser";
            openBuildReportToolStripMenuItem.Click += openBuildReportToolStripMenuItem_Click;
            // 
            // openBuildReportToolStripMenuItem
            // 
            openPullRequestPageStripMenuItem.Image = Properties.Images.PullRequest;
            openPullRequestPageStripMenuItem.Name = "openPullRequestPageStripMenuItem";
            openPullRequestPageStripMenuItem.Size = new Size(301, 26);
            openPullRequestPageStripMenuItem.Text = "Vie&w pull request in a browser";
            openPullRequestPageStripMenuItem.Click += openPullRequestPageStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(149, 6);
            // 
            // getHelpOnHowToUseTheseFeaturesToolStripMenuItem
            // 
            getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Name = "getHelpOnHowToUseTheseFeaturesToolStripMenuItem";
            getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Size = new Size(333, 26);
            getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Text = "Get &help on how to use these features";
            getHelpOnHowToUseTheseFeaturesToolStripMenuItem.Click += getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click;
            // 
            // editCommitToolStripMenuItem
            // 
            editCommitToolStripMenuItem.Name = "editToolStripMenuItem";
            editCommitToolStripMenuItem.Size = new Size(272, 22);
            editCommitToolStripMenuItem.Text = "&Edit commit";
            editCommitToolStripMenuItem.Click += editCommitToolStripMenuItem_Click;
            // 
            // rewordCommitToolStripMenuItem
            //
            rewordCommitToolStripMenuItem.Name = "rewordCommitToolStripMenuItem";
            rewordCommitToolStripMenuItem.Size = new Size(272, 22);
            rewordCommitToolStripMenuItem.Text = "&Reword commit";
            rewordCommitToolStripMenuItem.Click += rewordCommitToolStripMenuItem_Click;
            // rebaseToolStripMenuItem
            // 
            rebaseToolStripMenuItem.Name = "rebaseToolStripMenuItem";
            rebaseToolStripMenuItem.Size = new Size(191, 22);
            rebaseToolStripMenuItem.Text = "&Selected commit";
            rebaseToolStripMenuItem.Click += ToolStripItemClickRebaseBranch;
            // 
            // rebaseInteractivelyToolStripMenuItem
            // 
            rebaseInteractivelyToolStripMenuItem.Name = "rebaseInteractivelyToolStripMenuItem";
            rebaseInteractivelyToolStripMenuItem.Size = new Size(191, 22);
            rebaseInteractivelyToolStripMenuItem.Text = "Selected commit &interactively...";
            rebaseInteractivelyToolStripMenuItem.Click += OnRebaseInteractivelyClicked;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(195, 6);
            // 
            // rebaseWithAdvOptionsToolStripMenuItem
            // 
            rebaseWithAdvOptionsToolStripMenuItem.Name = "rebaseWithAdvOptionsToolStripMenuItem";
            rebaseWithAdvOptionsToolStripMenuItem.Size = new Size(307, 22);
            rebaseWithAdvOptionsToolStripMenuItem.Text = "Selected commit with &advanced options...";
            rebaseWithAdvOptionsToolStripMenuItem.Click += OnRebaseWithAdvOptionsClicked;
            // 
            // RevisionGrid
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(_gridView);
            Name = "RevisionGridControl";
            Size = new Size(682, 235);
            ((System.ComponentModel.ISupportInitialize)(_gridView)).EndInit();
            mainContextMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RevisionDataGridView _gridView;

        private ContextMenuStrip mainContextMenu;
        private ToolStripMenuItem navigateToolStripMenuItem;
        private ToolStripMenuItem revertCommitToolStripMenuItem;
        private ToolStripMenuItem deleteTagToolStripMenuItem;
        private ToolStripMenuItem deleteBranchToolStripMenuItem;
        private ToolStripMenuItem checkoutRevisionToolStripMenuItem;
        private ToolStripMenuItem archiveRevisionToolStripMenuItem;
        private ToolStripMenuItem applyStashToolStripMenuItem;
        private ToolStripMenuItem popStashToolStripMenuItem;
        private ToolStripMenuItem dropStashToolStripMenuItem;
        private ToolStripMenuItem checkoutBranchToolStripMenuItem;
        private ToolStripMenuItem mergeBranchToolStripMenuItem;
        private ToolStripMenuItem cherryPickCommitToolStripMenuItem;
        private ToolStripMenuItem rebaseOnToolStripMenuItem;
        private ToolStripMenuItem rebaseToolStripMenuItem;
        private ToolStripMenuItem rebaseInteractivelyToolStripMenuItem;
        private ToolStripMenuItem markRevisionAsBadToolStripMenuItem;
        private ToolStripMenuItem markRevisionAsGoodToolStripMenuItem;
        private ToolStripMenuItem stopBisectToolStripMenuItem;
        private ToolStripMenuItem runScriptToolStripMenuItem;
        private ToolStripMenuItem manipulateCommitToolStripMenuItem;
        private ToolStripMenuItem rewordCommitToolStripMenuItem;
        private ToolStripMenuItem fixupCommitToolStripMenuItem;
        private ToolStripMenuItem squashCommitToolStripMenuItem;
        private ToolStripMenuItem renameBranchToolStripMenuItem;
        private ToolStripMenuItem bisectSkipRevisionToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem openCommitsWithDiffToolMenuItem;
        private ToolStripMenuItem compareToBranchToolStripMenuItem;
        private ToolStripMenuItem compareToolStripMenuItem;
        private ToolStripMenuItem compareWithCurrentBranchToolStripMenuItem;
        private ToolStripMenuItem compareToBaseToolStripMenuItem;
        private ToolStripMenuItem compareToWorkingDirectoryMenuItem;
        private ToolStripMenuItem compareSelectedCommitsMenuItem;
        private ToolStripMenuItem selectAsBaseToolStripMenuItem;
        private ToolStripMenuItem getHelpOnHowToUseTheseFeaturesToolStripMenuItem;
        private ToolStripMenuItem openBuildReportToolStripMenuItem;
        private ToolStripMenuItem openPullRequestPageStripMenuItem;
        private ToolStripMenuItem editCommitToolStripMenuItem;
        private ToolStripMenuItem rebaseWithAdvOptionsToolStripMenuItem;
        private ToolStripMenuItem createTagToolStripMenuItem;
        private ToolStripMenuItem createNewBranchToolStripMenuItem;
        private ToolStripMenuItem resetCurrentBranchToHereToolStripMenuItem;
        private ToolStripMenuItem resetAnotherBranchToHereToolStripMenuItem;
        private GitUI.UserControls.RevisionGrid.CopyContextMenuItem copyToClipboardToolStripMenuItem;

        private ToolStripSeparator bisectSeparator;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator compareStripSeparator;
        private ToolStripSeparator stashStripSeparator;
        private ToolStripMenuItem amendCommitToolStripMenuItem;
    }
}
