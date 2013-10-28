using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormFileHistory
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
            _asyncLoader.Cancel();
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.FileChanges = new GitUI.RevisionGrid();
            this.DiffContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWithDifftoolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffToolremotelocalStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.manipuleerCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherryPickThisCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.followFileHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ViewTab = new System.Windows.Forms.TabPage();
            this.View = new GitUI.Editor.FileViewer();
            this.DiffTab = new System.Windows.Forms.TabPage();
            this.Diff = new GitUI.Editor.FileViewer();
            this.BlameTab = new System.Windows.Forms.TabPage();
            this.Blame = new GitUI.Blame.BlameControl();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripBranches = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitLoad = new System.Windows.Forms.ToolStripSplitButton();
            this.loadHistoryOnShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadBlameOnShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
#endif
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.DiffTab.SuspendLayout();
            this.BlameTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.FileChanges);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(935, 527);
            this.splitContainer1.SplitterDistance = 128;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // FileChanges
            // 
            this.FileChanges.ContextMenuStrip = this.DiffContextMenu;
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Location = new System.Drawing.Point(0, 0);
            this.FileChanges.Margin = new System.Windows.Forms.Padding(4);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.RevisionGraphDrawStyle = GitUI.RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
            this.FileChanges.ShowUncommitedChangesIfPossible = true;
            this.FileChanges.Size = new System.Drawing.Size(935, 128);
            this.FileChanges.TabIndex = 2;
            this.FileChanges.DoubleClick += new System.EventHandler(this.FileChangesDoubleClick);
            // 
            // DiffContextMenu
            // 
            this.DiffContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWithDifftoolToolStripMenuItem,
            this.diffToolremotelocalStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.viewCommitToolStripMenuItem,
            this.toolStripSeparator2,
            this.manipuleerCommitToolStripMenuItem,
            this.toolStripSeparator1,
            this.followFileHistoryToolStripMenuItem,
            this.fullHistoryToolStripMenuItem});
            this.DiffContextMenu.Name = "DiffContextMenu";
            this.DiffContextMenu.Size = new System.Drawing.Size(264, 184);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // diffToolremotelocalStripMenuItem
            // 
            this.diffToolremotelocalStripMenuItem.Name = "diffToolremotelocalStripMenuItem";
            this.diffToolremotelocalStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.diffToolremotelocalStripMenuItem.Text = "Difftool selected < - > local";
            this.diffToolremotelocalStripMenuItem.Click += new System.EventHandler(this.diffToolremotelocalStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // viewCommitToolStripMenuItem
            // 
            this.viewCommitToolStripMenuItem.Name = "viewCommitToolStripMenuItem";
            this.viewCommitToolStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.viewCommitToolStripMenuItem.Text = "View commit";
            this.viewCommitToolStripMenuItem.Click += new System.EventHandler(this.viewCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(260, 6);
            // 
            // manipuleerCommitToolStripMenuItem
            // 
            this.manipuleerCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.revertCommitToolStripMenuItem,
            this.cherryPickThisCommitToolStripMenuItem});
            this.manipuleerCommitToolStripMenuItem.Name = "manipuleerCommitToolStripMenuItem";
            this.manipuleerCommitToolStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.manipuleerCommitToolStripMenuItem.Text = "Manipulate commit";
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.revertCommitToolStripMenuItem_Click);
            // 
            // cherryPickThisCommitToolStripMenuItem
            // 
            this.cherryPickThisCommitToolStripMenuItem.Name = "cherryPickThisCommitToolStripMenuItem";
            this.cherryPickThisCommitToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.cherryPickThisCommitToolStripMenuItem.Text = "Cherry pick commit";
            this.cherryPickThisCommitToolStripMenuItem.Click += new System.EventHandler(this.cherryPickThisCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(260, 6);
            // 
            // followFileHistoryToolStripMenuItem
            // 
            this.followFileHistoryToolStripMenuItem.Name = "followFileHistoryToolStripMenuItem";
            this.followFileHistoryToolStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.followFileHistoryToolStripMenuItem.Text = "Detect and follow renames";
            this.followFileHistoryToolStripMenuItem.Click += new System.EventHandler(this.followFileHistoryToolStripMenuItem_Click);
            // 
            // fullHistoryToolStripMenuItem
            // 
            this.fullHistoryToolStripMenuItem.Name = "fullHistoryToolStripMenuItem";
            this.fullHistoryToolStripMenuItem.Size = new System.Drawing.Size(263, 24);
            this.fullHistoryToolStripMenuItem.Text = "Full history";
            this.fullHistoryToolStripMenuItem.Click += new System.EventHandler(this.fullHistoryToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.ViewTab);
            this.tabControl1.Controls.Add(this.DiffTab);
            this.tabControl1.Controls.Add(this.BlameTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(935, 394);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // ViewTab
            // 
            this.ViewTab.Controls.Add(this.View);
            this.ViewTab.Location = new System.Drawing.Point(4, 32);
            this.ViewTab.Margin = new System.Windows.Forms.Padding(4);
            this.ViewTab.Name = "ViewTab";
            this.ViewTab.Padding = new System.Windows.Forms.Padding(4);
            this.ViewTab.Size = new System.Drawing.Size(927, 358);
            this.ViewTab.TabIndex = 0;
            this.ViewTab.Text = "View";
            this.ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Location = new System.Drawing.Point(4, 4);
            this.View.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.View.Name = "View";
            this.View.Size = new System.Drawing.Size(919, 350);
            this.View.TabIndex = 0;
            // 
            // DiffTab
            // 
            this.DiffTab.Controls.Add(this.Diff);
            this.DiffTab.Location = new System.Drawing.Point(4, 32);
            this.DiffTab.Margin = new System.Windows.Forms.Padding(4);
            this.DiffTab.Name = "DiffTab";
            this.DiffTab.Padding = new System.Windows.Forms.Padding(4);
            this.DiffTab.Size = new System.Drawing.Size(927, 358);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.Location = new System.Drawing.Point(4, 4);
            this.Diff.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Diff.Name = "Diff";
            this.Diff.Size = new System.Drawing.Size(919, 350);
            this.Diff.TabIndex = 0;
            // 
            // BlameTab
            // 
            this.BlameTab.Controls.Add(this.Blame);
            this.BlameTab.Location = new System.Drawing.Point(4, 32);
            this.BlameTab.Margin = new System.Windows.Forms.Padding(4);
            this.BlameTab.Name = "BlameTab";
            this.BlameTab.Size = new System.Drawing.Size(927, 358);
            this.BlameTab.TabIndex = 2;
            this.BlameTab.Text = "Blame";
            this.BlameTab.UseVisualStyleBackColor = true;
            // 
            // Blame
            // 
            this.Blame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Blame.Location = new System.Drawing.Point(0, 0);
            this.Blame.Margin = new System.Windows.Forms.Padding(5);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(927, 358);
            this.Blame.TabIndex = 0;
            this.Blame.CommandClick += new System.EventHandler<GitUI.CommitInfo.CommandEventArgs>(this.Blame_CommandClick);
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // ToolStrip
            // 
            this.ToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripBranches,
            this.toolStripDropDownButton2,
            this.toolStripSeparator19,
            this.toolStripLabel2,
            this.toolStripTextBoxFilter,
            this.toolStripDropDownButton1,
            this.toolStripSeparator3,
            this.toolStripSplitLoad});
            this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStrip.Size = new System.Drawing.Size(935, 28);
            this.ToolStrip.TabIndex = 5;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(71, 25);
            this.toolStripLabel1.Text = "Branches:";
            // 
            // toolStripBranches
            // 
            this.toolStripBranches.Name = "toolStripBranches";
            this.toolStripBranches.Size = new System.Drawing.Size(186, 28);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton2.Image = global::GitUI.Properties.Resources.Settings;
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(29, 25);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(45, 25);
            this.toolStripLabel2.Text = "Filter:";
            // 
            // toolStripTextBoxFilter
            // 
            this.toolStripTextBoxFilter.ForeColor = System.Drawing.Color.Black;
            this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            this.toolStripTextBoxFilter.Size = new System.Drawing.Size(149, 28);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.Image = global::GitUI.Properties.Resources.Settings;
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 25);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSplitLoad
            // 
            this.toolStripSplitLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitLoad.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadHistoryOnShowToolStripMenuItem,
            this.loadBlameOnShowToolStripMenuItem});
            this.toolStripSplitLoad.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.toolStripSplitLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitLoad.Name = "toolStripSplitLoad";
            this.toolStripSplitLoad.Size = new System.Drawing.Size(32, 25);
            this.toolStripSplitLoad.ToolTipText = "Load file history";
            this.toolStripSplitLoad.ButtonClick += new System.EventHandler(this.toolStripSplitLoad_ButtonClick);
            // 
            // loadHistoryOnShowToolStripMenuItem
            // 
            this.loadHistoryOnShowToolStripMenuItem.Checked = true;
            this.loadHistoryOnShowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loadHistoryOnShowToolStripMenuItem.Name = "loadHistoryOnShowToolStripMenuItem";
            this.loadHistoryOnShowToolStripMenuItem.Size = new System.Drawing.Size(218, 24);
            this.loadHistoryOnShowToolStripMenuItem.Text = "Load history on show";
            this.loadHistoryOnShowToolStripMenuItem.Click += new System.EventHandler(this.loadHistoryOnShowToolStripMenuItem_Click);
            // 
            // loadBlameOnShowToolStripMenuItem
            // 
            this.loadBlameOnShowToolStripMenuItem.Checked = true;
            this.loadBlameOnShowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loadBlameOnShowToolStripMenuItem.Name = "loadBlameOnShowToolStripMenuItem";
            this.loadBlameOnShowToolStripMenuItem.Size = new System.Drawing.Size(218, 24);
            this.loadBlameOnShowToolStripMenuItem.Text = "Load blame on show";
            this.loadBlameOnShowToolStripMenuItem.Click += new System.EventHandler(this.loadBlameOnShowToolStripMenuItem_Click);
            // 
            // FormFileHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(935, 555);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ToolStrip);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(370, 238);
            this.Name = "FormFileHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File History";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
#if !__MonoCS__ || Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
#endif
            this.splitContainer1.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.DiffTab.ResumeLayout(false);
            this.BlameTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ViewTab;
        private System.Windows.Forms.TabPage DiffTab;
        private System.Windows.Forms.TabPage BlameTab;
        private System.Diagnostics.EventLog eventLog1;
        private FileViewer View;
        private FileViewer Diff;
        private RevisionGrid FileChanges;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private Blame.BlameControl Blame;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem followFileHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manipuleerCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherryPickThisCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem viewCommitToolStripMenuItem;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripBranches;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem diffToolremotelocalStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitLoad;
        private System.Windows.Forms.ToolStripMenuItem loadHistoryOnShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem loadBlameOnShowToolStripMenuItem;
    }
}