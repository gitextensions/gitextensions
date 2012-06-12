using GitUI.Editor;

namespace GitUI
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
            this.Blame = new System.Windows.Forms.TabPage();
            this.blameControl1 = new GitUI.Blame.BlameControl();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripBranches = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.DiffContextMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.ViewTab.SuspendLayout();
            this.DiffTab.SuspendLayout();
            this.Blame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
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
            this.splitContainer1.Size = new System.Drawing.Size(748, 419);
            this.splitContainer1.SplitterDistance = 102;
            this.splitContainer1.TabIndex = 0;
            // 
            // FileChanges
            // 
            this.FileChanges.BranchFilter = "";
            this.FileChanges.ContextMenuStrip = this.DiffContextMenu;
            this.FileChanges.CurrentCheckout = "";
            this.FileChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileChanges.Filter = "";
            this.FileChanges.FixedFilter = "";
            this.FileChanges.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.FileChanges.InMemAuthorFilter = "";
            this.FileChanges.InMemCommitterFilter = "";
            this.FileChanges.InMemMessageFilter = "";
            this.FileChanges.LastRow = 0;
            this.FileChanges.Location = new System.Drawing.Point(0, 0);
            this.FileChanges.Name = "FileChanges";
            this.FileChanges.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FileChanges.Size = new System.Drawing.Size(748, 102);
            this.FileChanges.SuperprojectCurrentCheckout = null;
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
            this.DiffContextMenu.Size = new System.Drawing.Size(216, 192);
            // 
            // openWithDifftoolToolStripMenuItem
            // 
            this.openWithDifftoolToolStripMenuItem.Name = "openWithDifftoolToolStripMenuItem";
            this.openWithDifftoolToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.openWithDifftoolToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.openWithDifftoolToolStripMenuItem.Text = "Open with difftool";
            this.openWithDifftoolToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDifftoolToolStripMenuItemClick);
            // 
            // diffToolremotelocalStripMenuItem
            // 
            this.diffToolremotelocalStripMenuItem.Name = "diffToolremotelocalStripMenuItem";
            this.diffToolremotelocalStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.diffToolremotelocalStripMenuItem.Text = "Difftool remote < - > local";
            this.diffToolremotelocalStripMenuItem.Click += new System.EventHandler(this.diffToolremotelocalStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // viewCommitToolStripMenuItem
            // 
            this.viewCommitToolStripMenuItem.Name = "viewCommitToolStripMenuItem";
            this.viewCommitToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.viewCommitToolStripMenuItem.Text = "View commit";
            this.viewCommitToolStripMenuItem.Click += new System.EventHandler(this.viewCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(212, 6);
            // 
            // manipuleerCommitToolStripMenuItem
            // 
            this.manipuleerCommitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.revertCommitToolStripMenuItem,
            this.cherryPickThisCommitToolStripMenuItem});
            this.manipuleerCommitToolStripMenuItem.Name = "manipuleerCommitToolStripMenuItem";
            this.manipuleerCommitToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.manipuleerCommitToolStripMenuItem.Text = "Manipulate commit";
            // 
            // revertCommitToolStripMenuItem
            // 
            this.revertCommitToolStripMenuItem.Name = "revertCommitToolStripMenuItem";
            this.revertCommitToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.revertCommitToolStripMenuItem.Text = "Revert commit";
            this.revertCommitToolStripMenuItem.Click += new System.EventHandler(this.revertCommitToolStripMenuItem_Click);
            // 
            // cherryPickThisCommitToolStripMenuItem
            // 
            this.cherryPickThisCommitToolStripMenuItem.Name = "cherryPickThisCommitToolStripMenuItem";
            this.cherryPickThisCommitToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.cherryPickThisCommitToolStripMenuItem.Text = "Cherry pick commit";
            this.cherryPickThisCommitToolStripMenuItem.Click += new System.EventHandler(this.cherryPickThisCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(212, 6);
            // 
            // followFileHistoryToolStripMenuItem
            // 
            this.followFileHistoryToolStripMenuItem.Name = "followFileHistoryToolStripMenuItem";
            this.followFileHistoryToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.followFileHistoryToolStripMenuItem.Text = "Detect and follow renames";
            this.followFileHistoryToolStripMenuItem.Click += new System.EventHandler(this.followFileHistoryToolStripMenuItem_Click);
            // 
            // fullHistoryToolStripMenuItem
            // 
            this.fullHistoryToolStripMenuItem.Name = "fullHistoryToolStripMenuItem";
            this.fullHistoryToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.fullHistoryToolStripMenuItem.Text = "Full history";
            this.fullHistoryToolStripMenuItem.Click += new System.EventHandler(this.fullHistoryToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.ViewTab);
            this.tabControl1.Controls.Add(this.DiffTab);
            this.tabControl1.Controls.Add(this.Blame);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(748, 313);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // ViewTab
            // 
            this.ViewTab.Controls.Add(this.View);
            this.ViewTab.Location = new System.Drawing.Point(4, 24);
            this.ViewTab.Name = "ViewTab";
            this.ViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.ViewTab.Size = new System.Drawing.Size(740, 285);
            this.ViewTab.TabIndex = 0;
            this.ViewTab.Text = "View";
            this.ViewTab.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.View.Location = new System.Drawing.Point(3, 3);
            this.View.Name = "View";
            this.View.Size = new System.Drawing.Size(734, 279);
            this.View.TabIndex = 0;
            // 
            // DiffTab
            // 
            this.DiffTab.Controls.Add(this.Diff);
            this.DiffTab.Location = new System.Drawing.Point(4, 24);
            this.DiffTab.Name = "DiffTab";
            this.DiffTab.Padding = new System.Windows.Forms.Padding(3);
            this.DiffTab.Size = new System.Drawing.Size(740, 285);
            this.DiffTab.TabIndex = 1;
            this.DiffTab.Text = "Diff";
            this.DiffTab.UseVisualStyleBackColor = true;
            // 
            // Diff
            // 
            this.Diff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Diff.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.Diff.Location = new System.Drawing.Point(3, 3);
            this.Diff.Name = "Diff";
            this.Diff.Size = new System.Drawing.Size(734, 279);
            this.Diff.TabIndex = 0;
            // 
            // Blame
            // 
            this.Blame.Controls.Add(this.blameControl1);
            this.Blame.Location = new System.Drawing.Point(4, 24);
            this.Blame.Name = "Blame";
            this.Blame.Size = new System.Drawing.Size(740, 285);
            this.Blame.TabIndex = 2;
            this.Blame.Text = "Blame";
            this.Blame.UseVisualStyleBackColor = true;
            // 
            // blameControl1
            // 
            this.blameControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blameControl1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.blameControl1.Location = new System.Drawing.Point(0, 0);
            this.blameControl1.Name = "blameControl1";
            this.blameControl1.Size = new System.Drawing.Size(740, 285);
            this.blameControl1.TabIndex = 0;
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
            this.toolStripDropDownButton1});
            this.ToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStrip.Size = new System.Drawing.Size(748, 25);
            this.ToolStrip.TabIndex = 5;
            this.ToolStrip.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel1.Text = "Branches:";
            // 
            // toolStripBranches
            // 
            this.toolStripBranches.Name = "toolStripBranches";
            this.toolStripBranches.Size = new System.Drawing.Size(150, 25);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton2.Image = global::GitUI.Properties.Resources.Settings;
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(29, 22);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel2.Text = "Filter:";
            // 
            // toolStripTextBoxFilter
            // 
            this.toolStripTextBoxFilter.ForeColor = System.Drawing.Color.Black;
            this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            this.toolStripTextBoxFilter.Size = new System.Drawing.Size(120, 25);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.Image = global::GitUI.Properties.Resources.Settings;
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 22);
            // 
            // FormFileHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 444);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ToolStrip);
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "FormFileHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File History";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormFileHistoryFormClosing);
            this.Load += new System.EventHandler(this.FormFileHistoryLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.DiffContextMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ViewTab.ResumeLayout(false);
            this.DiffTab.ResumeLayout(false);
            this.Blame.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage Blame;
        private System.Diagnostics.EventLog eventLog1;
        private FileViewer View;
        private FileViewer Diff;
        private RevisionGrid FileChanges;
        private System.Windows.Forms.ContextMenuStrip DiffContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openWithDifftoolToolStripMenuItem;
        private Blame.BlameControl blameControl1;
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
    }
}