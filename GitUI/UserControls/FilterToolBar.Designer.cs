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
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            this.tsmiBranchLocal = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiBranchRemote = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiBranchTag = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCommitFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCommitterFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAuthorFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDiffContainsFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tsbtnAdvancedFilter = new System.Windows.Forms.ToolStripSplitButton();
            this.tsmiResetPathFilters = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiResetAllFilters = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorAdvancedFilter = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiAdvancedFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.tssbtnShowBranches = new System.Windows.Forms.ToolStripSplitButton();
            this.tsmiShowBranchesAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowBranchesCurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowBranchesFiltered = new System.Windows.Forms.ToolStripMenuItem();
            this.tscboBranchFilter = new System.Windows.Forms.ToolStripComboBox();
            this.tsddbtnBranchFilter = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.tslblRevisionFilter = new System.Windows.Forms.ToolStripLabel();
            this.tstxtRevisionFilter = new System.Windows.Forms.ToolStripComboBox();
            this.tsddbtnRevisionFilter = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsbShowReflog = new System.Windows.Forms.ToolStripButton();
            this.tsmiShowOnlyFirstParent = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SuspendLayout();
            // 
            // tsmiCommitFilter
            // 
            this.tsmiCommitFilter.Checked = true;
            this.tsmiCommitFilter.CheckOnClick = true;
            this.tsmiCommitFilter.Name = "tsmiCommitFilter";
            this.tsmiCommitFilter.Text = "Commit &message";
            this.tsmiCommitFilter.CheckedChanged += new System.EventHandler(this.revisionFilterBox_CheckedChanged); 
            // 
            // tsmiCommitter
            // 
            this.tsmiCommitterFilter.CheckOnClick = true;
            this.tsmiCommitterFilter.Name = "tsmiCommitter";
            this.tsmiCommitterFilter.Text = "&Committer";
            this.tsmiCommitterFilter.CheckedChanged += new System.EventHandler(this.revisionFilterBox_CheckedChanged);
            // 
            // tsmiAuthor
            // 
            this.tsmiAuthorFilter.CheckOnClick = true;
            this.tsmiAuthorFilter.Name = "tsmiAuthor";
            this.tsmiAuthorFilter.Text = "&Author";
            this.tsmiAuthorFilter.CheckedChanged += new System.EventHandler(this.revisionFilterBox_CheckedChanged);
            // 
            // tsmiDiffContains
            // 
            this.tsmiDiffContainsFilter.CheckOnClick = true;
            this.tsmiDiffContainsFilter.Name = "tsmiDiffContains";
            this.tsmiDiffContainsFilter.Text = "&Diff contains (SLOW)";
            this.tsmiDiffContainsFilter.CheckedChanged += new System.EventHandler(this.revisionFilterBox_CheckedChanged);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 22);
            this.toolStripLabel1.Tag = "ToolBar_group:Branch filter";
            this.toolStripLabel1.Text = "&Branches:";
            this.toolStripLabel1.ToolTipText = "Branch filter";
            // 
            // tsbtnAdvancedFilter
            // 
            this.tsbtnAdvancedFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnAdvancedFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiResetPathFilters,
            this.tsmiResetAllFilters,
            this.toolStripSeparatorAdvancedFilter,
            this.tsmiAdvancedFilter});
            this.tsbtnAdvancedFilter.Image = global::GitUI.Properties.Images.FunnelPencil;
            this.tsbtnAdvancedFilter.Name = "tsbtnAdvancedFilter";
            this.tsbtnAdvancedFilter.Size = new System.Drawing.Size(32, 22);
            this.tsbtnAdvancedFilter.ToolTipText = "Advanced filter";
            this.tsbtnAdvancedFilter.ButtonClick += new System.EventHandler(this.tsbtnAdvancedFilter_ButtonClick);
            // 
            // tsmiResetPathFilters
            // 
            this.tsmiResetPathFilters.Name = "tsmiResetPathFilters";
            this.tsmiResetPathFilters.Size = new System.Drawing.Size(259, 22);
            this.tsmiResetPathFilters.Text = "Reset &path filter";
            this.tsmiResetPathFilters.Click += new System.EventHandler(this.tsmiDisablePathFilters_Click);
            // 
            // tsmiResetAllFilters
            // 
            this.tsmiResetAllFilters.Name = "tsmiResetAllFilters";
            this.tsmiResetAllFilters.Size = new System.Drawing.Size(259, 22);
            this.tsmiResetAllFilters.Text = "&Reset revision filters";
            this.tsmiResetAllFilters.Click += new System.EventHandler(this.tsmiDisableAllFilters_Click);
            // 
            // toolStripSeparatorAdvancedFilter
            // 
            this.toolStripSeparatorAdvancedFilter.Name = "toolStripSeparatorAdvancedFilter";
            this.toolStripSeparatorAdvancedFilter.Size = new System.Drawing.Size(6, 25);
            // 
            // tsmiAdvancedFilter
            // 
            this.tsmiAdvancedFilter.Name = "tsmiAdvancedFilter";
            this.tsmiAdvancedFilter.Size = new System.Drawing.Size(259, 22);
            this.tsmiAdvancedFilter.Text = "&Advanced filter";
            this.tsmiAdvancedFilter.Click += new System.EventHandler(this.tsmiAdvancedFilter_Click);
            // 
            // menuCommitInfoPosition
            // 
            this.tssbtnShowBranches.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.tssbtnShowBranches.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiShowBranchesAll,
            this.tsmiShowBranchesCurrent,
            this.tsmiShowBranchesFiltered});
            this.tssbtnShowBranches.Image = global::GitUI.Properties.Images.BranchLocal;
            this.tssbtnShowBranches.Name = "tssbtnShowBranches";
            this.tssbtnShowBranches.Size = new System.Drawing.Size(32, 22);
            this.tssbtnShowBranches.Click += new System.EventHandler(this.tssbtnShowBranches_Click);
            // 
            // tsmiShowBranchesAll
            // 
            this.tsmiShowBranchesAll.Image = global::GitUI.Properties.Images.BranchLocal;
            this.tsmiShowBranchesAll.Name = "tsmiShowBranchesAll";
            this.tsmiShowBranchesAll.Size = new System.Drawing.Size(259, 22);
            this.tsmiShowBranchesAll.Text = "&All branches";
            this.tsmiShowBranchesAll.ToolTipText = "Show all branches";
            this.tsmiShowBranchesAll.Click += new System.EventHandler(this.tsmiShowBranchesAll_Click);
            // 
            // tsmiShowBranchesCurrent
            // 
            this.tsmiShowBranchesCurrent.Image = global::GitUI.Properties.Images.BranchFilter;
            this.tsmiShowBranchesCurrent.Name = "tsmiShowBranchesCurrent";
            this.tsmiShowBranchesCurrent.Size = new System.Drawing.Size(259, 22);
            this.tsmiShowBranchesCurrent.Text = "&Current branch only";
            this.tsmiShowBranchesCurrent.ToolTipText = "Show current branch only";
            this.tsmiShowBranchesCurrent.Click += new System.EventHandler(this.tsmiShowBranchesCurrent_Click);
            // 
            // tsmiShowBranchesFiltered
            // 
            this.tsmiShowBranchesFiltered.Image = global::GitUI.Properties.Images.BranchFilter;
            this.tsmiShowBranchesFiltered.Name = "tsmiShowBranchesFiltered";
            this.tsmiShowBranchesFiltered.Size = new System.Drawing.Size(259, 22);
            this.tsmiShowBranchesFiltered.Text = "&Filtered branches";
            this.tsmiShowBranchesFiltered.ToolTipText = "Show filtered branches";
            this.tsmiShowBranchesFiltered.Click += new System.EventHandler(this.tsmiShowBranchesFiltered_Click);
            // 
            // tsbShowReflog
            // 
            this.tsbShowReflog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.tsbShowReflog.Image = global::GitUI.Properties.Images.Book;
            this.tsbShowReflog.Name = "tsbShowReflog";
            this.tsbShowReflog.Size = new System.Drawing.Size(23, 22);
            this.tsbShowReflog.Click += new System.EventHandler(this.tsmiShowReflog_Click);
            // 
            // tscboBranchFilter
            // 
            this.tscboBranchFilter.AutoSize = false;
            this.tscboBranchFilter.BackColor = System.Drawing.SystemColors.Control;
            this.tscboBranchFilter.DropDownWidth = 300;
            this.tscboBranchFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.tscboBranchFilter.Name = "tscboBranchFilter";
            this.tscboBranchFilter.Size = new System.Drawing.Size(100, 23);
            this.tscboBranchFilter.Tag = "ToolBar_group:Branch filter";
            this.tscboBranchFilter.Click += new System.EventHandler(this.tscboBranchFilter_Click);
            this.tscboBranchFilter.DropDown += new System.EventHandler(this.tscboBranchFilter_DropDown);
            this.tscboBranchFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tscboBranchFilter_KeyUp);
            this.tscboBranchFilter.TextChanged += new System.EventHandler(this.tscboBranchFilter_TextChanged);
            this.tscboBranchFilter.TextUpdate += new System.EventHandler(this.tscboBranchFilter_TextUpdate);
            // 
            // tsmiBranchLocal
            // 
            this.tsmiBranchLocal.Checked = true;
            this.tsmiBranchLocal.CheckOnClick = true;
            this.tsmiBranchLocal.Name = "tsmiBranchLocal";
            this.tsmiBranchLocal.Text = "&Local";
            // 
            // tsmiBranchRemote
            // 
            this.tsmiBranchRemote.CheckOnClick = true;
            this.tsmiBranchRemote.Name = "tsmiBranchRemote";
            this.tsmiBranchRemote.Text = "&Remote";
            // 
            // tsmiBranchTag
            // 
            this.tsmiBranchTag.CheckOnClick = true;
            this.tsmiBranchTag.Name = "tsmiBranchTag";
            this.tsmiBranchTag.Text = "&Tag";
            // 
            // tsddbtnBranchFilter
            // 
            this.tsddbtnBranchFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsddbtnBranchFilter.DropDownItems.AddRange(new ToolStripItem[] {
            this.tsmiBranchLocal,
            this.tsmiBranchRemote,
            this.tsmiBranchTag});
            this.tsddbtnBranchFilter.Image = global::GitUI.Properties.Images.EditFilter;
            this.tsddbtnBranchFilter.Name = "tsddbtnBranchFilter";
            this.tsddbtnBranchFilter.Size = new System.Drawing.Size(29, 22);
            this.tsddbtnBranchFilter.Tag = "ToolBar_group:Branch filter";
            this.tsddbtnBranchFilter.Text = "Branch type";
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // tslblRevisionFilter
            // 
            this.tslblRevisionFilter.Name = "tslblRevisionFilter";
            this.tslblRevisionFilter.Size = new System.Drawing.Size(36, 22);
            this.tslblRevisionFilter.Tag = "ToolBar_group:Text filter";
            this.tslblRevisionFilter.Text = "&Filter:";
            this.tslblRevisionFilter.ToolTipText = "Text filter";
            // 
            // tstxtRevisionFilter
            // 
            this.tstxtRevisionFilter.AutoSize = false;
            this.tstxtRevisionFilter.BackColor = System.Drawing.SystemColors.Control;
            this.tstxtRevisionFilter.DropDownWidth = 300;
            this.tstxtRevisionFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.tstxtRevisionFilter.Name = "tstxtRevisionFilter";
            this.tstxtRevisionFilter.Size = new System.Drawing.Size(100, 25);
            this.tstxtRevisionFilter.Tag = "ToolBar_group:Text filter";
            this.tstxtRevisionFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tstxtRevisionFilter_KeyUp);
            // 
            // tsddbtnRevisionFilter
            // 
            this.tsddbtnRevisionFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsddbtnRevisionFilter.Image = global::GitUI.Properties.Images.EditFilter;
            this.tsddbtnRevisionFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCommitFilter,
            this.tsmiCommitterFilter,
            this.tsmiAuthorFilter,
            this.tsmiDiffContainsFilter});
            this.tsddbtnRevisionFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbtnRevisionFilter.Name = "tsddbtnRevisionFilter";
            this.tsddbtnRevisionFilter.Size = new System.Drawing.Size(29, 22);
            this.tsddbtnRevisionFilter.Tag = "ToolBar_group:Text filter";
            this.tsddbtnRevisionFilter.Text = "Filter type";
            // 
            // tsmiShowOnlyFirstParent
            // 
            this.tsmiShowOnlyFirstParent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsmiShowOnlyFirstParent.Image = global::GitUI.Properties.Images.ShowOnlyFirstParent;
            this.tsmiShowOnlyFirstParent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmiShowOnlyFirstParent.Name = "tsmiShowOnlyFirstParent";
            this.tsmiShowOnlyFirstParent.Size = new System.Drawing.Size(23, 20);
            this.tsmiShowOnlyFirstParent.Click += new System.EventHandler(this.tsmiShowOnlyFirstParent_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 6);
            // 
            // FilterToolBar
            // 
            this.Dock = System.Windows.Forms.DockStyle.None;
            this.GripMargin = new System.Windows.Forms.Padding(0);
            this.GripEnabled = false;
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbtnAdvancedFilter,
            this.tsbShowReflog,
            this.tssbtnShowBranches,
            this.toolStripLabel1,
            this.tscboBranchFilter,
            this.tsddbtnBranchFilter,
            this.toolStripSeparator19,
            this.tslblRevisionFilter,
            this.tstxtRevisionFilter,
            this.tsddbtnRevisionFilter,
            this.tsmiShowOnlyFirstParent});
            this.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.Location = new System.Drawing.Point(584, 0);
            this.Name = "ToolStripFilters";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(339, 25);
            this.ResumeLayout(false);

        }

        #endregion

        private ToolStripMenuItem tsmiBranchLocal;
        private ToolStripMenuItem tsmiBranchRemote;
        private ToolStripMenuItem tsmiBranchTag;
        private ToolStripMenuItem tsmiCommitFilter;
        private ToolStripMenuItem tsmiCommitterFilter;
        private ToolStripMenuItem tsmiAuthorFilter;
        private ToolStripMenuItem tsmiDiffContainsFilter;
        private ToolStripButton tsbShowReflog;
        private ToolStripButton tsmiShowOnlyFirstParent;
        private ToolStripComboBox tstxtRevisionFilter;
        private ToolStripLabel tslblRevisionFilter;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripSplitButton tsbtnAdvancedFilter;
        private ToolStripMenuItem tsmiResetPathFilters;
        private ToolStripMenuItem tsmiResetAllFilters;
        private ToolStripSeparator toolStripSeparatorAdvancedFilter;
        private ToolStripMenuItem tsmiAdvancedFilter;
        private ToolStripSplitButton tssbtnShowBranches;
        private ToolStripMenuItem tsmiShowBranchesAll;
        private ToolStripMenuItem tsmiShowBranchesCurrent;
        private ToolStripMenuItem tsmiShowBranchesFiltered;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox tscboBranchFilter;
        private ToolStripDropDownButton tsddbtnBranchFilter;
        private ToolStripDropDownButton tsddbtnRevisionFilter;
    }
}
