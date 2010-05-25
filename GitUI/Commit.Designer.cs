namespace GitUI
{
    partial class FormCommit
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCommit));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.workingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIgnoredFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetAlltrackedChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.eToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllUntrackedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rescanChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.Unstaged = new GitUI.FileStatusList();
            this.UnstagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ResetChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPartOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileTogitignoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.filenameToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.AddFiles = new System.Windows.Forms.Button();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.filesListedToCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stageAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unstageAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.stageChunkOfFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UnstageFiles = new System.Windows.Forms.Button();
            this.Staged = new GitUI.FileStatusList();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.SelectedDiff = new GitUI.FileViewer();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Scan = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.Amend = new System.Windows.Forms.Button();
            this.Commit = new System.Windows.Forms.Button();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.Message = new GitUI.EditNetSpell();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip3 = new System.Windows.Forms.MenuStrip();
            this.commitMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CloseDialogAfterCommit = new System.Windows.Forms.CheckBox();
            this.CloseCommitDialogTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.fileTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.gitItemStatusBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.UnstagedFileContext.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.menuStrip3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.AccessibleDescription = null;
            this.splitContainer1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.BackgroundImage = null;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Font = null;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AccessibleDescription = null;
            this.splitContainer1.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.BackgroundImage = null;
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.Controls.Add(this.Ok);
            this.splitContainer1.Panel1.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer1.Panel1, resources.GetString("splitContainer1.Panel1.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer1.Panel1, resources.GetString("splitContainer1.Panel1.ToolTip1"));
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AccessibleDescription = null;
            this.splitContainer1.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.BackgroundImage = null;
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel2.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer1.Panel2, resources.GetString("splitContainer1.Panel2.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer1.Panel2, resources.GetString("splitContainer1.Panel2.ToolTip1"));
            this.fileTooltip.SetToolTip(this.splitContainer1, resources.GetString("splitContainer1.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer1, resources.GetString("splitContainer1.ToolTip1"));
            // 
            // splitContainer2
            // 
            this.splitContainer2.AccessibleDescription = null;
            this.splitContainer2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.BackgroundImage = null;
            this.splitContainer2.Font = null;
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.AccessibleDescription = null;
            this.splitContainer2.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
            this.splitContainer2.Panel1.BackgroundImage = null;
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
            this.splitContainer2.Panel1.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer2.Panel1, resources.GetString("splitContainer2.Panel1.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer2.Panel1, resources.GetString("splitContainer2.Panel1.ToolTip1"));
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AccessibleDescription = null;
            this.splitContainer2.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
            this.splitContainer2.Panel2.BackgroundImage = null;
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer2.Panel2.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer2.Panel2, resources.GetString("splitContainer2.Panel2.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer2.Panel2, resources.GetString("splitContainer2.Panel2.ToolTip1"));
            this.fileTooltip.SetToolTip(this.splitContainer2, resources.GetString("splitContainer2.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer2, resources.GetString("splitContainer2.ToolTip1"));
            // 
            // splitContainer4
            // 
            this.splitContainer4.AccessibleDescription = null;
            this.splitContainer4.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4, "splitContainer4");
            this.splitContainer4.BackgroundImage = null;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Font = null;
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.AccessibleDescription = null;
            this.splitContainer4.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4.Panel1, "splitContainer4.Panel1");
            this.splitContainer4.Panel1.BackgroundImage = null;
            this.splitContainer4.Panel1.Controls.Add(this.tableLayoutPanel5);
            this.splitContainer4.Panel1.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer4.Panel1, resources.GetString("splitContainer4.Panel1.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer4.Panel1, resources.GetString("splitContainer4.Panel1.ToolTip1"));
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.AccessibleDescription = null;
            this.splitContainer4.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer4.Panel2, "splitContainer4.Panel2");
            this.splitContainer4.Panel2.BackgroundImage = null;
            this.splitContainer4.Panel2.Controls.Add(this.Loading);
            this.splitContainer4.Panel2.Controls.Add(this.Unstaged);
            this.splitContainer4.Panel2.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer4.Panel2, resources.GetString("splitContainer4.Panel2.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer4.Panel2, resources.GetString("splitContainer4.Panel2.ToolTip1"));
            this.fileTooltip.SetToolTip(this.splitContainer4, resources.GetString("splitContainer4.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer4, resources.GetString("splitContainer4.ToolTip1"));
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.AccessibleDescription = null;
            this.tableLayoutPanel5.AccessibleName = null;
            resources.ApplyResources(this.tableLayoutPanel5, "tableLayoutPanel5");
            this.tableLayoutPanel5.BackgroundImage = null;
            this.tableLayoutPanel5.Controls.Add(this.progressBar, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.menuStrip1, 0, 0);
            this.tableLayoutPanel5.Font = null;
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.fileTooltip.SetToolTip(this.tableLayoutPanel5, resources.GetString("tableLayoutPanel5.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.tableLayoutPanel5, resources.GetString("tableLayoutPanel5.ToolTip1"));
            // 
            // progressBar
            // 
            this.progressBar.AccessibleDescription = null;
            this.progressBar.AccessibleName = null;
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.BackgroundImage = null;
            this.progressBar.Font = null;
            this.progressBar.Name = "progressBar";
            this.CloseCommitDialogTooltip.SetToolTip(this.progressBar, resources.GetString("progressBar.ToolTip"));
            this.fileTooltip.SetToolTip(this.progressBar, resources.GetString("progressBar.ToolTip1"));
            // 
            // menuStrip1
            // 
            this.menuStrip1.AccessibleDescription = null;
            this.menuStrip1.AccessibleName = null;
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.BackgroundImage = null;
            this.menuStrip1.Font = null;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.workingToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            this.CloseCommitDialogTooltip.SetToolTip(this.menuStrip1, resources.GetString("menuStrip1.ToolTip"));
            this.fileTooltip.SetToolTip(this.menuStrip1, resources.GetString("menuStrip1.ToolTip1"));
            // 
            // workingToolStripMenuItem
            // 
            this.workingToolStripMenuItem.AccessibleDescription = null;
            this.workingToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.workingToolStripMenuItem, "workingToolStripMenuItem");
            this.workingToolStripMenuItem.BackgroundImage = null;
            this.workingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showIgnoredFilesToolStripMenuItem,
            this.showUntrackedFilesToolStripMenuItem,
            this.toolStripSeparator3,
            this.deleteSelectedFilesToolStripMenuItem,
            this.resetSelectedFilesToolStripMenuItem,
            this.resetAlltrackedChangesToolStripMenuItem,
            this.toolStripSeparator1,
            this.eToolStripMenuItem,
            this.deleteAllUntrackedFilesToolStripMenuItem,
            this.rescanChangesToolStripMenuItem});
            this.workingToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.workingToolStripMenuItem.Name = "workingToolStripMenuItem";
            this.workingToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // showIgnoredFilesToolStripMenuItem
            // 
            this.showIgnoredFilesToolStripMenuItem.AccessibleDescription = null;
            this.showIgnoredFilesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.showIgnoredFilesToolStripMenuItem, "showIgnoredFilesToolStripMenuItem");
            this.showIgnoredFilesToolStripMenuItem.BackgroundImage = null;
            this.showIgnoredFilesToolStripMenuItem.Name = "showIgnoredFilesToolStripMenuItem";
            this.showIgnoredFilesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.showIgnoredFilesToolStripMenuItem.Click += new System.EventHandler(this.showIgnoredFilesToolStripMenuItem_Click);
            // 
            // showUntrackedFilesToolStripMenuItem
            // 
            this.showUntrackedFilesToolStripMenuItem.AccessibleDescription = null;
            this.showUntrackedFilesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.showUntrackedFilesToolStripMenuItem, "showUntrackedFilesToolStripMenuItem");
            this.showUntrackedFilesToolStripMenuItem.BackgroundImage = null;
            this.showUntrackedFilesToolStripMenuItem.Checked = true;
            this.showUntrackedFilesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showUntrackedFilesToolStripMenuItem.Name = "showUntrackedFilesToolStripMenuItem";
            this.showUntrackedFilesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.showUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.showUntrackedFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.AccessibleDescription = null;
            this.toolStripSeparator3.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // deleteSelectedFilesToolStripMenuItem
            // 
            this.deleteSelectedFilesToolStripMenuItem.AccessibleDescription = null;
            this.deleteSelectedFilesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.deleteSelectedFilesToolStripMenuItem, "deleteSelectedFilesToolStripMenuItem");
            this.deleteSelectedFilesToolStripMenuItem.BackgroundImage = null;
            this.deleteSelectedFilesToolStripMenuItem.Name = "deleteSelectedFilesToolStripMenuItem";
            this.deleteSelectedFilesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.deleteSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedFilesToolStripMenuItem_Click);
            // 
            // resetSelectedFilesToolStripMenuItem
            // 
            this.resetSelectedFilesToolStripMenuItem.AccessibleDescription = null;
            this.resetSelectedFilesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.resetSelectedFilesToolStripMenuItem, "resetSelectedFilesToolStripMenuItem");
            this.resetSelectedFilesToolStripMenuItem.BackgroundImage = null;
            this.resetSelectedFilesToolStripMenuItem.Name = "resetSelectedFilesToolStripMenuItem";
            this.resetSelectedFilesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.resetSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.resetSelectedFilesToolStripMenuItem_Click);
            // 
            // resetAlltrackedChangesToolStripMenuItem
            // 
            this.resetAlltrackedChangesToolStripMenuItem.AccessibleDescription = null;
            this.resetAlltrackedChangesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.resetAlltrackedChangesToolStripMenuItem, "resetAlltrackedChangesToolStripMenuItem");
            this.resetAlltrackedChangesToolStripMenuItem.BackgroundImage = null;
            this.resetAlltrackedChangesToolStripMenuItem.Name = "resetAlltrackedChangesToolStripMenuItem";
            this.resetAlltrackedChangesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.resetAlltrackedChangesToolStripMenuItem.Click += new System.EventHandler(this.resetAlltrackedChangesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.AccessibleDescription = null;
            this.toolStripSeparator1.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // eToolStripMenuItem
            // 
            this.eToolStripMenuItem.AccessibleDescription = null;
            this.eToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.eToolStripMenuItem, "eToolStripMenuItem");
            this.eToolStripMenuItem.BackgroundImage = null;
            this.eToolStripMenuItem.Name = "eToolStripMenuItem";
            this.eToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.eToolStripMenuItem.Click += new System.EventHandler(this.eToolStripMenuItem_Click);
            // 
            // deleteAllUntrackedFilesToolStripMenuItem
            // 
            this.deleteAllUntrackedFilesToolStripMenuItem.AccessibleDescription = null;
            this.deleteAllUntrackedFilesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.deleteAllUntrackedFilesToolStripMenuItem, "deleteAllUntrackedFilesToolStripMenuItem");
            this.deleteAllUntrackedFilesToolStripMenuItem.BackgroundImage = null;
            this.deleteAllUntrackedFilesToolStripMenuItem.Name = "deleteAllUntrackedFilesToolStripMenuItem";
            this.deleteAllUntrackedFilesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.deleteAllUntrackedFilesToolStripMenuItem.Click += new System.EventHandler(this.deleteAllUntrackedFilesToolStripMenuItem_Click);
            // 
            // rescanChangesToolStripMenuItem
            // 
            this.rescanChangesToolStripMenuItem.AccessibleDescription = null;
            this.rescanChangesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.rescanChangesToolStripMenuItem, "rescanChangesToolStripMenuItem");
            this.rescanChangesToolStripMenuItem.BackgroundImage = null;
            this.rescanChangesToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.rescanChangesToolStripMenuItem.Name = "rescanChangesToolStripMenuItem";
            this.rescanChangesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.rescanChangesToolStripMenuItem.Click += new System.EventHandler(this.rescanChangesToolStripMenuItem_Click);
            // 
            // Loading
            // 
            this.Loading.AccessibleDescription = null;
            this.Loading.AccessibleName = null;
            resources.ApplyResources(this.Loading, "Loading");
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.BackgroundImage = null;
            this.Loading.Font = null;
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
            this.Loading.ImageLocation = null;
            this.Loading.Name = "Loading";
            this.Loading.TabStop = false;
            this.CloseCommitDialogTooltip.SetToolTip(this.Loading, resources.GetString("Loading.ToolTip"));
            this.fileTooltip.SetToolTip(this.Loading, resources.GetString("Loading.ToolTip1"));
            // 
            // Unstaged
            // 
            this.Unstaged.AccessibleDescription = null;
            this.Unstaged.AccessibleName = null;
            resources.ApplyResources(this.Unstaged, "Unstaged");
            this.Unstaged.BackgroundImage = null;
            this.Unstaged.ContextMenuStrip = this.UnstagedFileContext;
            this.Unstaged.Font = null;
            this.Unstaged.GitItemStatusses = null;
            this.Unstaged.Name = "Unstaged";
            this.Unstaged.SelectedItem = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.Unstaged, resources.GetString("Unstaged.ToolTip"));
            this.fileTooltip.SetToolTip(this.Unstaged, resources.GetString("Unstaged.ToolTip1"));
            // 
            // UnstagedFileContext
            // 
            this.UnstagedFileContext.AccessibleDescription = null;
            this.UnstagedFileContext.AccessibleName = null;
            resources.ApplyResources(this.UnstagedFileContext, "UnstagedFileContext");
            this.UnstagedFileContext.BackgroundImage = null;
            this.UnstagedFileContext.Font = null;
            this.UnstagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ResetChanges,
            this.resetPartOfFileToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.addFileTogitignoreToolStripMenuItem,
            this.toolStripSeparator4,
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.openWithDifftoolToolStripMenuItem,
            this.toolStripSeparator5,
            this.filenameToClipboardToolStripMenuItem});
            this.UnstagedFileContext.Name = "UnstagedFileContext";
            this.CloseCommitDialogTooltip.SetToolTip(this.UnstagedFileContext, resources.GetString("UnstagedFileContext.ToolTip"));
            this.fileTooltip.SetToolTip(this.UnstagedFileContext, resources.GetString("UnstagedFileContext.ToolTip1"));
            // 
            // ResetChanges
            // 
            this.ResetChanges.AccessibleDescription = null;
            this.ResetChanges.AccessibleName = null;
            resources.ApplyResources(this.ResetChanges, "ResetChanges");
            this.ResetChanges.BackgroundImage = null;
            this.ResetChanges.Name = "ResetChanges";
            this.ResetChanges.ShortcutKeyDisplayString = null;
            this.ResetChanges.Click += new System.EventHandler(this.ResetSoft_Click);
            // 
            // resetPartOfFileToolStripMenuItem
            // 
            this.resetPartOfFileToolStripMenuItem.AccessibleDescription = null;
            this.resetPartOfFileToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.resetPartOfFileToolStripMenuItem, "resetPartOfFileToolStripMenuItem");
            this.resetPartOfFileToolStripMenuItem.BackgroundImage = null;
            this.resetPartOfFileToolStripMenuItem.Name = "resetPartOfFileToolStripMenuItem";
            this.resetPartOfFileToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.resetPartOfFileToolStripMenuItem.Click += new System.EventHandler(this.resetPartOfFileToolStripMenuItem_Click);
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.AccessibleDescription = null;
            this.deleteFileToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.deleteFileToolStripMenuItem, "deleteFileToolStripMenuItem");
            this.deleteFileToolStripMenuItem.BackgroundImage = null;
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.deleteFileToolStripMenuItem_Click);
            // 
            // addFileTogitignoreToolStripMenuItem
            // 
            this.addFileTogitignoreToolStripMenuItem.AccessibleDescription = null;
            this.addFileTogitignoreToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.addFileTogitignoreToolStripMenuItem, "addFileTogitignoreToolStripMenuItem");
            this.addFileTogitignoreToolStripMenuItem.BackgroundImage = null;
            this.addFileTogitignoreToolStripMenuItem.Name = "addFileTogitignoreToolStripMenuItem";
            this.addFileTogitignoreToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.addFileTogitignoreToolStripMenuItem.Click += new System.EventHandler(this.addFileTogitignoreToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.AccessibleDescription = null;
            this.toolStripSeparator4.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.AccessibleDescription = null;
            this.openToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.BackgroundImage = null;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.AccessibleDescription = null;
            this.openWithToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openWithToolStripMenuItem, "openWithToolStripMenuItem");
            this.openWithToolStripMenuItem.BackgroundImage = null;
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.AccessibleDescription = null;
            this.openWithDifftoolToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openWithDifftoolToolStripMenuItem, "openWithDifftoolToolStripMenuItem");
            this.openWithDifftoolToolStripMenuItem.BackgroundImage = null;
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.openWithDifftoolToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.AccessibleDescription = null;
            this.toolStripSeparator5.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // filenameToClipboardToolStripMenuItem
            // 
            this.filenameToClipboardToolStripMenuItem.AccessibleDescription = null;
            this.filenameToClipboardToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.filenameToClipboardToolStripMenuItem, "filenameToClipboardToolStripMenuItem");
            this.filenameToClipboardToolStripMenuItem.BackgroundImage = null;
            this.filenameToClipboardToolStripMenuItem.Name = "filenameToClipboardToolStripMenuItem";
            this.filenameToClipboardToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.filenameToClipboardToolStripMenuItem.Click += new System.EventHandler(this.filenameToClipboardToolStripMenuItem_Click);
            // 
            // splitContainer5
            // 
            this.splitContainer5.AccessibleDescription = null;
            this.splitContainer5.AccessibleName = null;
            resources.ApplyResources(this.splitContainer5, "splitContainer5");
            this.splitContainer5.BackgroundImage = null;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Font = null;
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.AccessibleDescription = null;
            this.splitContainer5.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer5.Panel1, "splitContainer5.Panel1");
            this.splitContainer5.Panel1.BackgroundImage = null;
            this.splitContainer5.Panel1.Controls.Add(this.tableLayoutPanel4);
            this.splitContainer5.Panel1.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer5.Panel1, resources.GetString("splitContainer5.Panel1.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer5.Panel1, resources.GetString("splitContainer5.Panel1.ToolTip1"));
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.AccessibleDescription = null;
            this.splitContainer5.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer5.Panel2, "splitContainer5.Panel2");
            this.splitContainer5.Panel2.BackgroundImage = null;
            this.splitContainer5.Panel2.Controls.Add(this.Staged);
            this.splitContainer5.Panel2.Controls.Add(this.Cancel);
            this.splitContainer5.Panel2.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer5.Panel2, resources.GetString("splitContainer5.Panel2.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer5.Panel2, resources.GetString("splitContainer5.Panel2.ToolTip1"));
            this.fileTooltip.SetToolTip(this.splitContainer5, resources.GetString("splitContainer5.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer5, resources.GetString("splitContainer5.ToolTip1"));
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AccessibleDescription = null;
            this.tableLayoutPanel4.AccessibleName = null;
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.BackgroundImage = null;
            this.tableLayoutPanel4.Controls.Add(this.AddFiles, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.menuStrip2, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.UnstageFiles, 1, 0);
            this.tableLayoutPanel4.Font = null;
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.fileTooltip.SetToolTip(this.tableLayoutPanel4, resources.GetString("tableLayoutPanel4.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.tableLayoutPanel4, resources.GetString("tableLayoutPanel4.ToolTip1"));
            // 
            // AddFiles
            // 
            this.AddFiles.AccessibleDescription = null;
            this.AddFiles.AccessibleName = null;
            resources.ApplyResources(this.AddFiles, "AddFiles");
            this.AddFiles.BackgroundImage = null;
            this.AddFiles.Font = null;
            this.AddFiles.Image = global::GitUI.Properties.Resources._4;
            this.AddFiles.Name = "AddFiles";
            this.fileTooltip.SetToolTip(this.AddFiles, resources.GetString("AddFiles.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.AddFiles, resources.GetString("AddFiles.ToolTip1"));
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.Stage_Click);
            // 
            // menuStrip2
            // 
            this.menuStrip2.AccessibleDescription = null;
            this.menuStrip2.AccessibleName = null;
            resources.ApplyResources(this.menuStrip2, "menuStrip2");
            this.menuStrip2.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip2.BackgroundImage = null;
            this.menuStrip2.Font = null;
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filesListedToCommitToolStripMenuItem});
            this.menuStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.menuStrip2.Name = "menuStrip2";
            this.CloseCommitDialogTooltip.SetToolTip(this.menuStrip2, resources.GetString("menuStrip2.ToolTip"));
            this.fileTooltip.SetToolTip(this.menuStrip2, resources.GetString("menuStrip2.ToolTip1"));
            // 
            // filesListedToCommitToolStripMenuItem
            // 
            this.filesListedToCommitToolStripMenuItem.AccessibleDescription = null;
            this.filesListedToCommitToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.filesListedToCommitToolStripMenuItem, "filesListedToCommitToolStripMenuItem");
            this.filesListedToCommitToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.filesListedToCommitToolStripMenuItem.BackgroundImage = null;
            this.filesListedToCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stageAllToolStripMenuItem,
            this.unstageAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.stageChunkOfFileToolStripMenuItem});
            this.filesListedToCommitToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.filesListedToCommitToolStripMenuItem.Name = "filesListedToCommitToolStripMenuItem";
            this.filesListedToCommitToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // stageAllToolStripMenuItem
            // 
            this.stageAllToolStripMenuItem.AccessibleDescription = null;
            this.stageAllToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.stageAllToolStripMenuItem, "stageAllToolStripMenuItem");
            this.stageAllToolStripMenuItem.BackgroundImage = null;
            this.stageAllToolStripMenuItem.Name = "stageAllToolStripMenuItem";
            this.stageAllToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.stageAllToolStripMenuItem.Click += new System.EventHandler(this.stageAllToolStripMenuItem_Click);
            // 
            // unstageAllToolStripMenuItem
            // 
            this.unstageAllToolStripMenuItem.AccessibleDescription = null;
            this.unstageAllToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.unstageAllToolStripMenuItem, "unstageAllToolStripMenuItem");
            this.unstageAllToolStripMenuItem.BackgroundImage = null;
            this.unstageAllToolStripMenuItem.Name = "unstageAllToolStripMenuItem";
            this.unstageAllToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.unstageAllToolStripMenuItem.Click += new System.EventHandler(this.unstageAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.AccessibleDescription = null;
            this.toolStripSeparator2.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // stageChunkOfFileToolStripMenuItem
            // 
            this.stageChunkOfFileToolStripMenuItem.AccessibleDescription = null;
            this.stageChunkOfFileToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.stageChunkOfFileToolStripMenuItem, "stageChunkOfFileToolStripMenuItem");
            this.stageChunkOfFileToolStripMenuItem.BackgroundImage = null;
            this.stageChunkOfFileToolStripMenuItem.Name = "stageChunkOfFileToolStripMenuItem";
            this.stageChunkOfFileToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.stageChunkOfFileToolStripMenuItem.Click += new System.EventHandler(this.stageChunkOfFileToolStripMenuItem_Click);
            // 
            // UnstageFiles
            // 
            this.UnstageFiles.AccessibleDescription = null;
            this.UnstageFiles.AccessibleName = null;
            resources.ApplyResources(this.UnstageFiles, "UnstageFiles");
            this.UnstageFiles.BackgroundImage = null;
            this.UnstageFiles.Font = null;
            this.UnstageFiles.Image = global::GitUI.Properties.Resources._3;
            this.UnstageFiles.Name = "UnstageFiles";
            this.fileTooltip.SetToolTip(this.UnstageFiles, resources.GetString("UnstageFiles.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.UnstageFiles, resources.GetString("UnstageFiles.ToolTip1"));
            this.UnstageFiles.UseVisualStyleBackColor = true;
            this.UnstageFiles.Click += new System.EventHandler(this.UnstageFiles_Click);
            // 
            // Staged
            // 
            this.Staged.AccessibleDescription = null;
            this.Staged.AccessibleName = null;
            resources.ApplyResources(this.Staged, "Staged");
            this.Staged.BackgroundImage = null;
            this.Staged.Font = null;
            this.Staged.GitItemStatusses = null;
            this.Staged.Name = "Staged";
            this.Staged.SelectedItem = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.Staged, resources.GetString("Staged.ToolTip"));
            this.fileTooltip.SetToolTip(this.Staged, resources.GetString("Staged.ToolTip1"));
            // 
            // Cancel
            // 
            this.Cancel.AccessibleDescription = null;
            this.Cancel.AccessibleName = null;
            resources.ApplyResources(this.Cancel, "Cancel");
            this.Cancel.BackgroundImage = null;
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Font = null;
            this.Cancel.Name = "Cancel";
            this.CloseCommitDialogTooltip.SetToolTip(this.Cancel, resources.GetString("Cancel.ToolTip"));
            this.fileTooltip.SetToolTip(this.Cancel, resources.GetString("Cancel.ToolTip1"));
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // Ok
            // 
            this.Ok.AccessibleDescription = null;
            this.Ok.AccessibleName = null;
            resources.ApplyResources(this.Ok, "Ok");
            this.Ok.BackgroundImage = null;
            this.Ok.Font = null;
            this.Ok.Name = "Ok";
            this.CloseCommitDialogTooltip.SetToolTip(this.Ok, resources.GetString("Ok.ToolTip"));
            this.fileTooltip.SetToolTip(this.Ok, resources.GetString("Ok.ToolTip1"));
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.AccessibleDescription = null;
            this.splitContainer3.AccessibleName = null;
            resources.ApplyResources(this.splitContainer3, "splitContainer3");
            this.splitContainer3.BackgroundImage = null;
            this.splitContainer3.Font = null;
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.AccessibleDescription = null;
            this.splitContainer3.Panel1.AccessibleName = null;
            resources.ApplyResources(this.splitContainer3.Panel1, "splitContainer3.Panel1");
            this.splitContainer3.Panel1.BackgroundImage = null;
            this.splitContainer3.Panel1.Controls.Add(this.SolveMergeconflicts);
            this.splitContainer3.Panel1.Controls.Add(this.SelectedDiff);
            this.splitContainer3.Panel1.Font = null;
            this.fileTooltip.SetToolTip(this.splitContainer3.Panel1, resources.GetString("splitContainer3.Panel1.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer3.Panel1, resources.GetString("splitContainer3.Panel1.ToolTip1"));
            this.splitContainer3.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer3_Panel1_Paint);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.AccessibleDescription = null;
            this.splitContainer3.Panel2.AccessibleName = null;
            resources.ApplyResources(this.splitContainer3.Panel2, "splitContainer3.Panel2");
            this.splitContainer3.Panel2.BackgroundImage = null;
            this.splitContainer3.Panel2.Controls.Add(this.tableLayoutPanel6);
            this.splitContainer3.Panel2.Font = null;
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer3.Panel2, resources.GetString("splitContainer3.Panel2.ToolTip"));
            this.fileTooltip.SetToolTip(this.splitContainer3.Panel2, resources.GetString("splitContainer3.Panel2.ToolTip1"));
            this.fileTooltip.SetToolTip(this.splitContainer3, resources.GetString("splitContainer3.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.splitContainer3, resources.GetString("splitContainer3.ToolTip1"));
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.AccessibleDescription = null;
            this.SolveMergeconflicts.AccessibleName = null;
            resources.ApplyResources(this.SolveMergeconflicts, "SolveMergeconflicts");
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.SeaShell;
            this.SolveMergeconflicts.BackgroundImage = null;
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.CloseCommitDialogTooltip.SetToolTip(this.SolveMergeconflicts, resources.GetString("SolveMergeconflicts.ToolTip"));
            this.fileTooltip.SetToolTip(this.SolveMergeconflicts, resources.GetString("SolveMergeconflicts.ToolTip1"));
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeconflicts_Click);
            // 
            // SelectedDiff
            // 
            this.SelectedDiff.AccessibleDescription = null;
            this.SelectedDiff.AccessibleName = null;
            resources.ApplyResources(this.SelectedDiff, "SelectedDiff");
            this.SelectedDiff.BackgroundImage = null;
            this.SelectedDiff.Font = null;
            this.SelectedDiff.IgnoreWhitespaceChanges = false;
            this.SelectedDiff.Name = "SelectedDiff";
            this.SelectedDiff.NumberOfVisibleLines = 3;
            this.SelectedDiff.ScrollPos = 0;
            this.SelectedDiff.ShowEntireFile = false;
            this.fileTooltip.SetToolTip(this.SelectedDiff, resources.GetString("SelectedDiff.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.SelectedDiff, resources.GetString("SelectedDiff.ToolTip1"));
            this.SelectedDiff.TreatAllFilesAsText = false;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.AccessibleDescription = null;
            this.tableLayoutPanel6.AccessibleName = null;
            resources.ApplyResources(this.tableLayoutPanel6, "tableLayoutPanel6");
            this.tableLayoutPanel6.BackgroundImage = null;
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 1, 0);
            this.tableLayoutPanel6.Font = null;
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.fileTooltip.SetToolTip(this.tableLayoutPanel6, resources.GetString("tableLayoutPanel6.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.tableLayoutPanel6, resources.GetString("tableLayoutPanel6.ToolTip1"));
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AccessibleDescription = null;
            this.tableLayoutPanel2.AccessibleName = null;
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.BackgroundImage = null;
            this.tableLayoutPanel2.Controls.Add(this.Scan, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.Reset, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.Amend, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.Commit, 0, 0);
            this.tableLayoutPanel2.Font = null;
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.CloseCommitDialogTooltip.SetToolTip(this.tableLayoutPanel2, resources.GetString("tableLayoutPanel2.ToolTip"));
            this.fileTooltip.SetToolTip(this.tableLayoutPanel2, resources.GetString("tableLayoutPanel2.ToolTip1"));
            // 
            // Scan
            // 
            this.Scan.AccessibleDescription = null;
            this.Scan.AccessibleName = null;
            resources.ApplyResources(this.Scan, "Scan");
            this.Scan.BackgroundImage = null;
            this.Scan.Font = null;
            this.Scan.Name = "Scan";
            this.fileTooltip.SetToolTip(this.Scan, resources.GetString("Scan.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.Scan, resources.GetString("Scan.ToolTip1"));
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
            // 
            // Reset
            // 
            this.Reset.AccessibleDescription = null;
            this.Reset.AccessibleName = null;
            resources.ApplyResources(this.Reset, "Reset");
            this.Reset.BackgroundImage = null;
            this.Reset.Font = null;
            this.Reset.Name = "Reset";
            this.fileTooltip.SetToolTip(this.Reset, resources.GetString("Reset.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.Reset, resources.GetString("Reset.ToolTip1"));
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // Amend
            // 
            this.Amend.AccessibleDescription = null;
            this.Amend.AccessibleName = null;
            resources.ApplyResources(this.Amend, "Amend");
            this.Amend.BackgroundImage = null;
            this.Amend.Font = null;
            this.Amend.Name = "Amend";
            this.fileTooltip.SetToolTip(this.Amend, resources.GetString("Amend.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.Amend, resources.GetString("Amend.ToolTip1"));
            this.Amend.UseVisualStyleBackColor = true;
            this.Amend.Click += new System.EventHandler(this.Amend_Click);
            // 
            // Commit
            // 
            this.Commit.AccessibleDescription = null;
            this.Commit.AccessibleName = null;
            resources.ApplyResources(this.Commit, "Commit");
            this.Commit.BackgroundImage = null;
            this.Commit.Font = null;
            this.Commit.Image = global::GitUI.Properties.Resources._10;
            this.Commit.Name = "Commit";
            this.fileTooltip.SetToolTip(this.Commit, resources.GetString("Commit.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.Commit, resources.GetString("Commit.ToolTip1"));
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.AccessibleDescription = null;
            this.tableLayoutPanel7.AccessibleName = null;
            resources.ApplyResources(this.tableLayoutPanel7, "tableLayoutPanel7");
            this.tableLayoutPanel7.BackgroundImage = null;
            this.tableLayoutPanel7.Controls.Add(this.Message, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel7.Font = null;
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.CloseCommitDialogTooltip.SetToolTip(this.tableLayoutPanel7, resources.GetString("tableLayoutPanel7.ToolTip"));
            this.fileTooltip.SetToolTip(this.tableLayoutPanel7, resources.GetString("tableLayoutPanel7.ToolTip1"));
            // 
            // Message
            // 
            this.Message.AccessibleDescription = null;
            this.Message.AccessibleName = null;
            resources.ApplyResources(this.Message, "Message");
            this.Message.BackgroundImage = null;
            this.Message.Font = null;
            this.Message.MistakeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.Message.Name = "Message";
            this.fileTooltip.SetToolTip(this.Message, resources.GetString("Message.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.Message, resources.GetString("Message.ToolTip1"));
            this.Message.Load += new System.EventHandler(this.Message_Load);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AccessibleDescription = null;
            this.tableLayoutPanel3.AccessibleName = null;
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.BackgroundImage = null;
            this.tableLayoutPanel3.Controls.Add(this.menuStrip3, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.CloseDialogAfterCommit, 1, 0);
            this.tableLayoutPanel3.Font = null;
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.CloseCommitDialogTooltip.SetToolTip(this.tableLayoutPanel3, resources.GetString("tableLayoutPanel3.ToolTip"));
            this.fileTooltip.SetToolTip(this.tableLayoutPanel3, resources.GetString("tableLayoutPanel3.ToolTip1"));
            // 
            // menuStrip3
            // 
            this.menuStrip3.AccessibleDescription = null;
            this.menuStrip3.AccessibleName = null;
            resources.ApplyResources(this.menuStrip3, "menuStrip3");
            this.menuStrip3.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip3.BackgroundImage = null;
            this.menuStrip3.Font = null;
            this.menuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commitMessageToolStripMenuItem});
            this.menuStrip3.Name = "menuStrip3";
            this.CloseCommitDialogTooltip.SetToolTip(this.menuStrip3, resources.GetString("menuStrip3.ToolTip"));
            this.fileTooltip.SetToolTip(this.menuStrip3, resources.GetString("menuStrip3.ToolTip1"));
            // 
            // commitMessageToolStripMenuItem
            // 
            this.commitMessageToolStripMenuItem.AccessibleDescription = null;
            this.commitMessageToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.commitMessageToolStripMenuItem, "commitMessageToolStripMenuItem");
            this.commitMessageToolStripMenuItem.BackgroundImage = null;
            this.commitMessageToolStripMenuItem.Image = global::GitUI.Properties.Resources._89;
            this.commitMessageToolStripMenuItem.Name = "commitMessageToolStripMenuItem";
            this.commitMessageToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.commitMessageToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.commitMessageToolStripMenuItem_DropDownItemClicked);
            this.commitMessageToolStripMenuItem.DropDownOpening += new System.EventHandler(this.commitMessageToolStripMenuItem_DropDownOpening);
            this.commitMessageToolStripMenuItem.Click += new System.EventHandler(this.commitMessageToolStripMenuItem_Click);
            // 
            // CloseDialogAfterCommit
            // 
            this.CloseDialogAfterCommit.AccessibleDescription = null;
            this.CloseDialogAfterCommit.AccessibleName = null;
            resources.ApplyResources(this.CloseDialogAfterCommit, "CloseDialogAfterCommit");
            this.CloseDialogAfterCommit.BackgroundImage = null;
            this.CloseDialogAfterCommit.Font = null;
            this.CloseDialogAfterCommit.Name = "CloseDialogAfterCommit";
            this.fileTooltip.SetToolTip(this.CloseDialogAfterCommit, resources.GetString("CloseDialogAfterCommit.ToolTip"));
            this.CloseCommitDialogTooltip.SetToolTip(this.CloseDialogAfterCommit, resources.GetString("CloseDialogAfterCommit.ToolTip1"));
            this.CloseDialogAfterCommit.UseVisualStyleBackColor = true;
            this.CloseDialogAfterCommit.CheckedChanged += new System.EventHandler(this.CloseDialogAfterCommit_CheckedChanged);
            // 
            // CloseCommitDialogTooltip
            // 
            this.CloseCommitDialogTooltip.AutomaticDelay = 1;
            this.CloseCommitDialogTooltip.AutoPopDelay = 5000;
            this.CloseCommitDialogTooltip.InitialDelay = 1;
            this.CloseCommitDialogTooltip.ReshowDelay = 1;
            this.CloseCommitDialogTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // fileTooltip
            // 
            this.fileTooltip.AutomaticDelay = 0;
            this.fileTooltip.AutoPopDelay = 500;
            this.fileTooltip.InitialDelay = 0;
            this.fileTooltip.ReshowDelay = 0;
            // 
            // gitItemStatusBindingSource
            // 
            this.gitItemStatusBindingSource.DataSource = typeof(GitCommands.GitItemStatus);
            // 
            // FormCommit
            // 
            this.AcceptButton = this.Commit;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.CancelButton = this.Cancel;
            this.Controls.Add(this.splitContainer1);
            this.Font = null;
            this.Icon = null;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormCommit";
            this.CloseCommitDialogTooltip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.fileTooltip.SetToolTip(this, resources.GetString("$this.ToolTip1"));
            this.Load += new System.EventHandler(this.FormCommit_Load);
            this.Shown += new System.EventHandler(this.FormCommit_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCommit_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.UnstagedFileContext.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.menuStrip3.ResumeLayout(false);
            this.menuStrip3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemStatusBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private FileStatusList Unstaged;
        private FileStatusList Staged;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.BindingSource gitItemStatusBindingSource;
        private System.Windows.Forms.Button UnstageFiles;
        private System.Windows.Forms.PictureBox Loading;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ContextMenuStrip UnstagedFileContext;
        private System.Windows.Forms.ToolStripMenuItem ResetChanges;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem workingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetAlltrackedChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem filesListedToCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stageAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unstageAllToolStripMenuItem;
        private FileViewer SelectedDiff;
        private EditNetSpell Message;
        private System.Windows.Forms.ToolStripMenuItem deleteAllUntrackedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem stageChunkOfFileToolStripMenuItem;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.CheckBox CloseDialogAfterCommit;
        private System.Windows.Forms.ToolStripMenuItem showIgnoredFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.MenuStrip menuStrip3;
        private System.Windows.Forms.ToolStripMenuItem commitMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFileTogitignoreToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ToolStripMenuItem showUntrackedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rescanChangesToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button Scan;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.Button Amend;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolTip CloseCommitDialogTooltip;
        private System.Windows.Forms.ToolStripMenuItem filenameToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private System.Windows.Forms.ToolTip fileTooltip;
        private System.Windows.Forms.ToolStripMenuItem resetPartOfFileToolStripMenuItem;
    }
}