using System.Windows.Forms;

namespace GitUI.UserControls
{
    internal partial class FilterToolBar
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tsmiCommitFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCommitter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAuthor = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiHash = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDiffContains = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tsbtnAdvancedFilter = new System.Windows.Forms.ToolStripButton();
            this.tscboBranchFilter = new System.Windows.Forms.ToolStripComboBox();
            this.tsddbtnBranchFilter = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.tslblRevisionFilter = new System.Windows.Forms.ToolStripLabel();
            this.tstxtRevisionFilter = new System.Windows.Forms.ToolStripTextBox();
            this.tsddbtnRevisionFilter = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiShowFirstParent = new System.Windows.Forms.ToolStripButton();
            this.SuspendLayout();
            // 
            // commitFilterToolStripMenuItem
            // 
            this.tsmiCommitFilter.Checked = true;
            this.tsmiCommitFilter.CheckOnClick = true;
            this.tsmiCommitFilter.Name = "commitFilterToolStripMenuItem";
            this.tsmiCommitFilter.Text = "Commit message and hash";
            // 
            // committerToolStripMenuItem
            // 
            this.tsmiCommitter.CheckOnClick = true;
            this.tsmiCommitter.Name = "committerToolStripMenuItem";
            this.tsmiCommitter.Text = "Committer";
            // 
            // authorToolStripMenuItem
            // 
            this.tsmiAuthor.CheckOnClick = true;
            this.tsmiAuthor.Name = "authorToolStripMenuItem";
            this.tsmiAuthor.Text = "Author";
            // 
            // hashToolStripMenuItem
            // 
            this.tsmiHash.CheckOnClick = true;
            this.tsmiHash.Name = "hashToolStripMenuItem";
            this.tsmiHash.Text = "Hash";
            // 
            // diffContainsToolStripMenuItem
            // 
            this.tsmiDiffContains.CheckOnClick = true;
            this.tsmiDiffContains.Name = "diffContainsToolStripMenuItem";
            this.tsmiDiffContains.Text = "Diff contains (SLOW)";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel1.Tag = "ToolBar_group:Branch filter";
            this.toolStripLabel1.Text = "Branches:";
            this.toolStripLabel1.ToolTipText = "Branch filter";
            // 
            // toolStripAdvancedFilterButton
            // 
            this.tsbtnAdvancedFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnAdvancedFilter.Image = global::GitUI.Properties.Images.FunnelPencil;
            this.tsbtnAdvancedFilter.Name = "toolStripAdvancedFilterButton";
            this.tsbtnAdvancedFilter.Size = new System.Drawing.Size(23, 22);
            this.tsbtnAdvancedFilter.ToolTipText = "Advanced filter";
            this.tsbtnAdvancedFilter.Click += new System.EventHandler(this.tsbtnAdvancedFilter_Click);
            // 
            // toolStripBranchFilterComboBox
            // 
            this.tscboBranchFilter.AutoSize = false;
            this.tscboBranchFilter.BackColor = System.Drawing.SystemColors.Control;
            this.tscboBranchFilter.DropDownWidth = 300;
            this.tscboBranchFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.tscboBranchFilter.Name = "toolStripBranchFilterComboBox";
            this.tscboBranchFilter.Size = new System.Drawing.Size(100, 23);
            this.tscboBranchFilter.Tag = "ToolBar_group:Branch filter";
            this.tscboBranchFilter.Click += new System.EventHandler(this.tscboBranchFilter_Click);
            this.tscboBranchFilter.DropDown += new System.EventHandler(this.tscboBranchFilter_ResizeDropDownWidth);
            // 
            // toolStripBranchFilterDropDownButton
            // 
            this.tsddbtnBranchFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsddbtnBranchFilter.Image = global::GitUI.Properties.Images.EditFilter;
            this.tsddbtnBranchFilter.Name = "toolStripBranchFilterDropDownButton";
            this.tsddbtnBranchFilter.Size = new System.Drawing.Size(29, 22);
            this.tsddbtnBranchFilter.Tag = "ToolBar_group:Branch filter";
            this.tsddbtnBranchFilter.Text = "Branch type";
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripRevisionFilterLabel
            // 
            this.tslblRevisionFilter.Name = "toolStripRevisionFilterLabel";
            this.tslblRevisionFilter.Size = new System.Drawing.Size(36, 22);
            this.tslblRevisionFilter.Tag = "ToolBar_group:Text filter";
            this.tslblRevisionFilter.Text = "Filter:";
            this.tslblRevisionFilter.ToolTipText = "Text filter";
            // 
            // toolStripRevisionFilterTextBox
            // 
            this.tstxtRevisionFilter.BackColor = System.Drawing.SystemColors.Control;
            this.tstxtRevisionFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tstxtRevisionFilter.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tstxtRevisionFilter.Name = "toolStripRevisionFilterTextBox";
            this.tstxtRevisionFilter.Size = new System.Drawing.Size(100, 25);
            this.tstxtRevisionFilter.Tag = "ToolBar_group:Text filter";
            // 
            // toolStripRevisionFilterDropDownButton
            // 
            this.tsddbtnRevisionFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsddbtnRevisionFilter.Image = global::GitUI.Properties.Images.EditFilter;
            this.tsddbtnRevisionFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCommitFilter,
            this.tsmiCommitter,
            this.tsmiAuthor,
            this.tsmiDiffContains});
            this.tsddbtnRevisionFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbtnRevisionFilter.Name = "toolStripRevisionFilterDropDownButton";
            this.tsddbtnRevisionFilter.Size = new System.Drawing.Size(29, 22);
            this.tsddbtnRevisionFilter.Tag = "ToolBar_group:Text filter";
            this.tsddbtnRevisionFilter.Text = "Filter type";
            // 
            // ShowFirstParent
            // 
            this.tsmiShowFirstParent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsmiShowFirstParent.Image = global::GitUI.Properties.Images.ShowFirstParent;
            this.tsmiShowFirstParent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmiShowFirstParent.Name = "ShowFirstParent";
            this.tsmiShowFirstParent.Size = new System.Drawing.Size(23, 20);
            this.tsmiShowFirstParent.ToolTipText = "Show first parents";
            this.tsmiShowFirstParent.Click += new System.EventHandler(this.tsmiShowFirstParentt_Click);
            // 
            // FilterToolBar
            // 
            this.Dock = System.Windows.Forms.DockStyle.None;
            this.GripMargin = new System.Windows.Forms.Padding(0);
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbtnAdvancedFilter,
            this.toolStripLabel1,
            this.tscboBranchFilter,
            this.tsddbtnBranchFilter,
            this.toolStripSeparator19,
            this.tslblRevisionFilter,
            this.tstxtRevisionFilter,
            this.tsddbtnRevisionFilter,
            this.tsmiShowFirstParent});
            this.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.Location = new System.Drawing.Point(584, 0);
            this.Name = "ToolStripFilters";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(339, 25);
            this.TabIndex = 1;
            this.Text = "Filters";
            this.ResumeLayout(false);

        }

        #endregion

        private ToolStripMenuItem tsmiCommitFilter;
        private ToolStripMenuItem tsmiCommitter;
        private ToolStripMenuItem tsmiAuthor;
        private ToolStripMenuItem tsmiDiffContains;
        private ToolStripMenuItem tsmiHash;
        private ToolStripButton tsmiShowFirstParent;
        private ToolStripTextBox tstxtRevisionFilter;
        private ToolStripLabel tslblRevisionFilter;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripButton tsbtnAdvancedFilter;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox tscboBranchFilter;
        private ToolStripDropDownButton tsddbtnBranchFilter;
        private ToolStripDropDownButton tsddbtnRevisionFilter;
    }
}
