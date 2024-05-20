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
            ToolStripSeparator toolStripSeparator1;
            tsmiBranchLocal = new ToolStripMenuItem();
            tsmiBranchRemote = new ToolStripMenuItem();
            tsmiBranchTag = new ToolStripMenuItem();
            tsmiCommitFilter = new ToolStripMenuItem();
            tsmiCommitterFilter = new ToolStripMenuItem();
            tsmiAuthorFilter = new ToolStripMenuItem();
            tsmiDiffContainsFilter = new ToolStripMenuItem();
            toolStripLabel1 = new ToolStripLabel();
            tsbtnAdvancedFilter = new ToolStripSplitButton();
            tsmiResetPathFilters = new ToolStripMenuItem();
            tsmiResetAllFilters = new ToolStripMenuItem();
            toolStripSeparatorAdvancedFilter = new ToolStripSeparator();
            tsmiAdvancedFilter = new ToolStripMenuItem();
            tssbtnShowBranches = new ToolStripSplitButton();
            tsmiShowBranchesAll = new ToolStripMenuItem();
            tsmiShowBranchesCurrent = new ToolStripMenuItem();
            tsmiShowBranchesFiltered = new ToolStripMenuItem();
            tscboBranchFilter = new ToolStripComboBox();
            tsddbtnBranchFilter = new ToolStripDropDownButton();
            toolStripSeparator19 = new ToolStripSeparator();
            tslblRevisionFilter = new ToolStripLabel();
            tstxtRevisionFilter = new ToolStripComboBox();
            tsddbtnRevisionFilter = new ToolStripDropDownButton();
            tsbShowReflog = new ToolStripButton();
            tsmiShowOnlyFirstParent = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            SuspendLayout();
            // 
            // tsmiCommitFilter
            // 
            tsmiCommitFilter.Checked = true;
            tsmiCommitFilter.CheckOnClick = true;
            tsmiCommitFilter.Name = "tsmiCommitFilter";
            tsmiCommitFilter.Text = "Commit &message";
            tsmiCommitFilter.CheckedChanged += new System.EventHandler(revisionFilterBox_CheckedChanged); 
            // 
            // tsmiCommitter
            // 
            tsmiCommitterFilter.CheckOnClick = true;
            tsmiCommitterFilter.Name = "tsmiCommitter";
            tsmiCommitterFilter.Text = "&Committer";
            tsmiCommitterFilter.CheckedChanged += revisionFilterBox_CheckedChanged;
            // 
            // tsmiAuthor
            // 
            tsmiAuthorFilter.CheckOnClick = true;
            tsmiAuthorFilter.Name = "tsmiAuthor";
            tsmiAuthorFilter.Text = "&Author";
            tsmiAuthorFilter.CheckedChanged += revisionFilterBox_CheckedChanged;
            // 
            // tsmiDiffContains
            // 
            tsmiDiffContainsFilter.CheckOnClick = true;
            tsmiDiffContainsFilter.Name = "tsmiDiffContains";
            tsmiDiffContainsFilter.Text = "&Diff contains (SLOW)";
            tsmiDiffContainsFilter.CheckedChanged += revisionFilterBox_CheckedChanged;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(58, 22);
            toolStripLabel1.Tag = "ToolBar_group:Branch filter";
            toolStripLabel1.Text = "&Branches:";
            toolStripLabel1.ToolTipText = "Branch filter";
            // 
            // tsbtnAdvancedFilter
            // 
            tsbtnAdvancedFilter.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbtnAdvancedFilter.DropDownItems.AddRange(new ToolStripItem[] {
            tsmiResetPathFilters,
            tsmiResetAllFilters,
            toolStripSeparatorAdvancedFilter,
            tsmiAdvancedFilter});
            tsbtnAdvancedFilter.Image = Properties.Images.FunnelPencil;
            tsbtnAdvancedFilter.Name = "tsbtnAdvancedFilter";
            tsbtnAdvancedFilter.Size = new Size(32, 22);
            tsbtnAdvancedFilter.ToolTipText = "Advanced filter";
            tsbtnAdvancedFilter.ButtonClick += tsbtnAdvancedFilter_ButtonClick;
            // 
            // tsmiResetPathFilters
            // 
            tsmiResetPathFilters.Name = "tsmiResetPathFilters";
            tsmiResetPathFilters.Size = new Size(259, 22);
            tsmiResetPathFilters.Text = "Reset &path filter";
            tsmiResetPathFilters.Click += tsmiDisablePathFilters_Click;
            // 
            // tsmiResetAllFilters
            // 
            tsmiResetAllFilters.Name = "tsmiResetAllFilters";
            tsmiResetAllFilters.Size = new Size(259, 22);
            tsmiResetAllFilters.Text = "&Reset revision filters";
            tsmiResetAllFilters.Click += tsmiDisableAllFilters_Click;
            // 
            // toolStripSeparatorAdvancedFilter
            // 
            toolStripSeparatorAdvancedFilter.Name = "toolStripSeparatorAdvancedFilter";
            toolStripSeparatorAdvancedFilter.Size = new Size(6, 25);
            // 
            // tsmiAdvancedFilter
            // 
            tsmiAdvancedFilter.Name = "tsmiAdvancedFilter";
            tsmiAdvancedFilter.Size = new Size(259, 22);
            tsmiAdvancedFilter.Text = "&Advanced filter";
            tsmiAdvancedFilter.Click += tsmiAdvancedFilter_Click;
            // 
            // menuCommitInfoPosition
            // 
            tssbtnShowBranches.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tssbtnShowBranches.DropDownItems.AddRange(new ToolStripItem[] {
            tsmiShowBranchesAll,
            tsmiShowBranchesCurrent,
            tsmiShowBranchesFiltered});
            tssbtnShowBranches.Image = Properties.Images.BranchLocal;
            tssbtnShowBranches.Name = "tssbtnShowBranches";
            tssbtnShowBranches.Size = new Size(32, 22);
            tssbtnShowBranches.Click += tssbtnShowBranches_Click;
            // 
            // tsmiShowBranchesAll
            // 
            tsmiShowBranchesAll.Image = Properties.Images.BranchLocal;
            tsmiShowBranchesAll.Name = "tsmiShowBranchesAll";
            tsmiShowBranchesAll.Size = new Size(259, 22);
            tsmiShowBranchesAll.Text = "&All branches";
            tsmiShowBranchesAll.ToolTipText = "Show all branches";
            tsmiShowBranchesAll.Click += tsmiShowBranchesAll_Click;
            // 
            // tsmiShowBranchesCurrent
            // 
            tsmiShowBranchesCurrent.Image = Properties.Images.BranchFilter;
            tsmiShowBranchesCurrent.Name = "tsmiShowBranchesCurrent";
            tsmiShowBranchesCurrent.Size = new Size(259, 22);
            tsmiShowBranchesCurrent.Text = "&Current branch only";
            tsmiShowBranchesCurrent.ToolTipText = "Show current branch only";
            tsmiShowBranchesCurrent.Click += tsmiShowBranchesCurrent_Click;
            // 
            // tsmiShowBranchesFiltered
            // 
            tsmiShowBranchesFiltered.Image = Properties.Images.BranchFilter;
            tsmiShowBranchesFiltered.Name = "tsmiShowBranchesFiltered";
            tsmiShowBranchesFiltered.Size = new Size(259, 22);
            tsmiShowBranchesFiltered.Text = "&Filtered branches";
            tsmiShowBranchesFiltered.ToolTipText = "Show filtered branches";
            tsmiShowBranchesFiltered.Click += tsmiShowBranchesFiltered_Click;
            // 
            // tsbShowReflog
            // 
            tsbShowReflog.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbShowReflog.Image = Properties.Images.Book;
            tsbShowReflog.Name = "tsbShowReflog";
            tsbShowReflog.Size = new Size(23, 22);
            tsbShowReflog.Click += tsmiShowReflog_Click;
            // 
            // tscboBranchFilter
            // 
            tscboBranchFilter.AutoSize = false;
            tscboBranchFilter.BackColor = SystemColors.Control;
            tscboBranchFilter.DropDownWidth = 300;
            tscboBranchFilter.FlatStyle = FlatStyle.System;
            tscboBranchFilter.Name = "tscboBranchFilter";
            tscboBranchFilter.Size = new Size(100, 23);
            tscboBranchFilter.Tag = "ToolBar_group:Branch filter";
            tscboBranchFilter.Click += tscboBranchFilter_Click;
            tscboBranchFilter.DropDown += tscboBranchFilter_DropDown;
            tscboBranchFilter.KeyUp += tscboBranchFilter_KeyUp;
            tscboBranchFilter.TextChanged += tscboBranchFilter_TextChanged;
            tscboBranchFilter.TextUpdate += tscboBranchFilter_TextUpdate;
            // 
            // tsmiBranchLocal
            // 
            tsmiBranchLocal.Checked = true;
            tsmiBranchLocal.CheckOnClick = true;
            tsmiBranchLocal.Name = "tsmiBranchLocal";
            tsmiBranchLocal.Text = "&Local";
            // 
            // tsmiBranchRemote
            // 
            tsmiBranchRemote.CheckOnClick = true;
            tsmiBranchRemote.Name = "tsmiBranchRemote";
            tsmiBranchRemote.Text = "&Remote";
            // 
            // tsmiBranchTag
            // 
            tsmiBranchTag.CheckOnClick = true;
            tsmiBranchTag.Name = "tsmiBranchTag";
            tsmiBranchTag.Text = "&Tag";
            // 
            // tsddbtnBranchFilter
            // 
            tsddbtnBranchFilter.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsddbtnBranchFilter.DropDownItems.AddRange(new ToolStripItem[] {
            tsmiBranchLocal,
            tsmiBranchRemote,
            tsmiBranchTag});
            tsddbtnBranchFilter.Image = Properties.Images.EditFilter;
            tsddbtnBranchFilter.Name = "tsddbtnBranchFilter";
            tsddbtnBranchFilter.Size = new Size(29, 22);
            tsddbtnBranchFilter.Tag = "ToolBar_group:Branch filter";
            tsddbtnBranchFilter.Text = "Branch type";
            // 
            // toolStripSeparator19
            // 
            toolStripSeparator19.Name = "toolStripSeparator19";
            toolStripSeparator19.Size = new Size(6, 25);
            // 
            // tslblRevisionFilter
            // 
            tslblRevisionFilter.Name = "tslblRevisionFilter";
            tslblRevisionFilter.Size = new Size(36, 22);
            tslblRevisionFilter.Tag = "ToolBar_group:Text filter";
            tslblRevisionFilter.Text = "&Filter:";
            tslblRevisionFilter.ToolTipText = "Text filter";
            // 
            // tstxtRevisionFilter
            // 
            tstxtRevisionFilter.AutoSize = false;
            tstxtRevisionFilter.BackColor = SystemColors.Control;
            tstxtRevisionFilter.DropDownWidth = 300;
            tstxtRevisionFilter.FlatStyle = FlatStyle.System;
            tstxtRevisionFilter.Name = "tstxtRevisionFilter";
            tstxtRevisionFilter.Size = new Size(100, 25);
            tstxtRevisionFilter.Tag = "ToolBar_group:Text filter";
            tstxtRevisionFilter.KeyUp += tstxtRevisionFilter_KeyUp;
            // 
            // tsddbtnRevisionFilter
            // 
            tsddbtnRevisionFilter.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsddbtnRevisionFilter.Image = Properties.Images.EditFilter;
            tsddbtnRevisionFilter.DropDownItems.AddRange(new ToolStripItem[] {
            tsmiCommitFilter,
            tsmiCommitterFilter,
            tsmiAuthorFilter,
            tsmiDiffContainsFilter});
            tsddbtnRevisionFilter.ImageTransparentColor = Color.Magenta;
            tsddbtnRevisionFilter.Name = "tsddbtnRevisionFilter";
            tsddbtnRevisionFilter.Size = new Size(29, 22);
            tsddbtnRevisionFilter.Tag = "ToolBar_group:Text filter";
            tsddbtnRevisionFilter.Text = "Filter type";
            // 
            // tsmiShowOnlyFirstParent
            // 
            tsmiShowOnlyFirstParent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsmiShowOnlyFirstParent.Image = Properties.Images.ShowOnlyFirstParent;
            tsmiShowOnlyFirstParent.ImageTransparentColor = Color.Magenta;
            tsmiShowOnlyFirstParent.Name = "tsmiShowOnlyFirstParent";
            tsmiShowOnlyFirstParent.Size = new Size(23, 20);
            tsmiShowOnlyFirstParent.Click += tsmiShowOnlyFirstParent_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 6);
            // 
            // FilterToolBar
            // 
            Dock = DockStyle.None;
            GripMargin = new Padding(0);
            GripEnabled = false;
            ImeMode = ImeMode.NoControl;
            Items.AddRange(new ToolStripItem[] {
            tsbtnAdvancedFilter,
            tsbShowReflog,
            tssbtnShowBranches,
            toolStripLabel1,
            tscboBranchFilter,
            tsddbtnBranchFilter,
            toolStripSeparator19,
            tslblRevisionFilter,
            tstxtRevisionFilter,
            tsddbtnRevisionFilter,
            tsmiShowOnlyFirstParent});
            LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            Location = new Point(584, 0);
            Name = "ToolStripFilters";
            Padding = new Padding(0);
            Size = new Size(339, 25);
            ResumeLayout(false);

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
