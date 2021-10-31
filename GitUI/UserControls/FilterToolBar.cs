using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    internal partial class FilterToolBar : ToolStripEx
    {
        private static readonly string[] _noResultsFound = { TranslatedStrings.NoResultsFound };
        private Func<IGitModule>? _getModule;
        private IRevisionGridFilter? _revisionGridFilter;
        private bool _isApplyingFilter;
        private bool _filterBeingChanged;

        public FilterToolBar()
        {
            InitializeComponent();

            // Select an option until we get a filter bound.
            SelectShowBranchesFilterOption(selectedIndex: 0);

            tstxtRevisionFilter.KeyUp += (s, e) =>
            {
                if (e.KeyValue == (char)Keys.Enter)
                {
                    ApplyRevisionFilter();
                }
            };

            tscboBranchFilter.ComboBox.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        public RefsFilter BranchesFilter
        {
            get
            {
                // Options are interpreted as the refs the search should be limited too
                // If neither option is selected all refs will be queried also including stash and notes
                RefsFilter refs = (tsmiBranchLocal.Checked ? RefsFilter.Heads : RefsFilter.NoFilter)
                    | (tsmiBranchTag.Checked ? RefsFilter.Tags : RefsFilter.NoFilter)
                    | (tsmiBranchRemote.Checked ? RefsFilter.Remotes : RefsFilter.NoFilter);
                return refs;
            }
        }

        private IRevisionGridFilter RevisionGridFilter
        {
            get => _revisionGridFilter ?? throw new InvalidOperationException($"{nameof(Bind)} is not called.");
        }

        /// <summary>
        ///  Applies the preset branch filters, such as "show all", "show current", and "show filtered".
        /// </summary>
        private void ApplyPresetBranchesFilter(Action filterAction)
        {
            _filterBeingChanged = true;

            // Action the filter
            filterAction();

            _filterBeingChanged = false;
        }

        /// <summary>
        ///  Applies custom branch filters supplied via the filter textbox.
        /// </summary>
        private void ApplyCustomBranchFilter()
        {
            if (_isApplyingFilter)
            {
                return;
            }

            _isApplyingFilter = true;

            // The user has accepted the filter
            _filterBeingChanged = false;

            string filter = tscboBranchFilter.Items.Count > 0 ? tscboBranchFilter.Text : string.Empty;
            if (filter == TranslatedStrings.NoResultsFound)
            {
                filter = string.Empty;
            }

            RevisionGridFilter.SetAndApplyBranchFilter(filter);

            _isApplyingFilter = false;
        }

        private void ApplyRevisionFilter()
        {
            if (_isApplyingFilter)
            {
                return;
            }

            _isApplyingFilter = true;
            RevisionGridFilter.SetAndApplyRevisionFilter(new RevisionFilter(tstxtRevisionFilter.Text,
                                                                            tsmiCommitFilter.Checked,
                                                                            tsmiCommitterFilter.Checked,
                                                                            tsmiAuthorFilter.Checked,
                                                                            tsmiDiffContainsFilter.Checked));
            _isApplyingFilter = false;
        }

        public void Bind(Func<IGitModule> getModule, IRevisionGridFilter revisionGridFilter)
        {
            _getModule = getModule ?? throw new ArgumentNullException(nameof(getModule));

            Debug.Assert(_revisionGridFilter is null, $"{nameof(Bind)} must be invoked only once.");
            _revisionGridFilter = revisionGridFilter ?? throw new ArgumentNullException(nameof(revisionGridFilter));
            _revisionGridFilter.FilterChanged += revisionGridFilter_FilterChanged;
        }

        private void BindBranches(string[] branches)
        {
            var autoCompleteList = tscboBranchFilter.AutoCompleteCustomSource.Cast<string>();
            if (!autoCompleteList.SequenceEqual(branches))
            {
                tscboBranchFilter.AutoCompleteCustomSource.Clear();
                tscboBranchFilter.AutoCompleteCustomSource.AddRange(branches);
            }

            string filter = tscboBranchFilter.Items.Count > 0 ? tscboBranchFilter.Text : string.Empty;
            string[] matches = branches.Where(branch => branch.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();

            if (matches.Length == 0)
            {
                matches = _noResultsFound;
            }

            int index = tscboBranchFilter.SelectionStart;
            tscboBranchFilter.Items.Clear();
            tscboBranchFilter.Items.AddRange(matches);
            tscboBranchFilter.SelectionStart = index;
        }

        public void ClearFilters()
        {
            tscboBranchFilter.Text =
                tstxtRevisionFilter.Text = string.Empty;
        }

        private IGitModule GetModule()
        {
            if (_getModule is null)
            {
                throw new InvalidOperationException($"{nameof(Bind)} is not called.");
            }

            IGitModule module = _getModule();
            if (module is null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(GitModule)}");
            }

            return module;
        }

        private void InitBranchSelectionFilter(FilterChangedEventArgs e)
        {
            // Note: it is a weird combination, and it is mimicking the implementations in RevisionGridControl.
            // Refer to it for more details.

            byte selectedIndex = 0;

            if (e.ShowAllBranches)
            {
                // Show all branches
                selectedIndex = 0;
            }

            if (e.ShowCurrentBranchOnly)
            {
                // Show current branch only
                selectedIndex = 1;
            }

            if (e.ShowFilteredBranches)
            {
                // Show filtered branches
                selectedIndex = 2;
            }

            SelectShowBranchesFilterOption(selectedIndex);
        }

        public void InitToolStripStyles(Color toolForeColor, Color toolBackColor)
        {
            tsddbtnRevisionFilter.BackColor = toolBackColor;
            tsddbtnRevisionFilter.ForeColor = toolForeColor;

            var toolTextBoxBackColor = SystemColors.Window;
            tscboBranchFilter.BackColor = toolTextBoxBackColor;
            tscboBranchFilter.ForeColor = toolForeColor;
            tstxtRevisionFilter.BackColor = toolTextBoxBackColor;
            tstxtRevisionFilter.ForeColor = toolForeColor;
        }

        public void PreventToolStripSplitButtonClosing(ToolStripSplitButton? control)
        {
            if (control is null
                || Parent is not ContainerControl parentContainer
                || tscboBranchFilter.Focused
                || tstxtRevisionFilter.Focused)
            {
                return;
            }

            control.Tag = parentContainer.FindFocusedControl();
            control.DropDownClosed += ToolStripSplitButtonDropDownClosed;
            tscboBranchFilter.Focus();
        }

        private void SelectShowBranchesFilterOption(byte selectedIndex)
        {
            if (selectedIndex >= tssbtnShowBranches.DropDownItems.Count)
            {
                selectedIndex = 0;
            }

            var selectedMenuItem = tssbtnShowBranches.DropDownItems[selectedIndex];
            tssbtnShowBranches.Image = selectedMenuItem.Image;
            tssbtnShowBranches.Text = selectedMenuItem.Text;
            tssbtnShowBranches.ToolTipText = selectedMenuItem.ToolTipText;
        }

        /// <summary>
        ///  Sets the branches filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        public void SetBranchFilter(string? filter)
        {
            tscboBranchFilter.Text = filter;
            ApplyCustomBranchFilter();
        }

        public void SetFocus()
        {
            ToolStripControlHost filterToFocus = tstxtRevisionFilter.Focused
                ? tscboBranchFilter
                : tstxtRevisionFilter;
            filterToFocus.Focus();
        }

        /// <summary>
        ///  Sets the revision filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        public void SetRevisionFilter(string? filter)
        {
            if (string.IsNullOrEmpty(tstxtRevisionFilter.Text) && string.IsNullOrEmpty(filter))
            {
                // The current filter is empty and the new filter is empty. No-op
                return;
            }

            tstxtRevisionFilter.Text = filter;
            ApplyRevisionFilter();
        }

        public void UpdateBranchFilterItems()
        {
            tscboBranchFilter.Items.Clear();

            IGitModule module = GetModule();
            if (!module.IsValidGitWorkingDir())
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            RefsFilter branchesFilter = BranchesFilter;
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                string[] branches = module.GetRefs(branchesFilter).Select(branch => branch.Name).ToArray();

                await this.SwitchToMainThreadAsync();
                BindBranches(branches);
            }).FileAndForget();
        }

        private void revisionGridFilter_FilterChanged(object? sender, FilterChangedEventArgs e)
        {
            tsmiShowFirstParent.Checked = e.ShowFirstParent;
            tsmiShowReflogs.Checked = e.ShowReflogReferences;
            InitBranchSelectionFilter(e);
        }

        private static void ToolStripSplitButtonDropDownClosed(object sender, EventArgs e)
        {
            if (sender is ToolStripSplitButton control)
            {
                control.DropDownClosed -= ToolStripSplitButtonDropDownClosed;

                if (control.Tag is Control controlToFocus)
                {
                    controlToFocus.Focus();
                    control.Tag = null;
                }
            }
        }

        private void tsbtnAdvancedFilter_Click(object sender, EventArgs e)
        {
            RevisionGridFilter.ShowRevisionFilterDialog();
        }

        private void tscboBranchFilter_Click(object sender, EventArgs e)
        {
            if (tscboBranchFilter.Items.Count == 0)
            {
                return;
            }

            tscboBranchFilter.DroppedDown = true;
        }

        private void tscboBranchFilter_DropDown(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void tscboBranchFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyCustomBranchFilter();
            }
        }

        private void tscboBranchFilter_TextChanged(object sender, EventArgs e)
        {
            _filterBeingChanged = true;
        }

        private void tscboBranchFilter_TextUpdate(object sender, EventArgs e)
        {
            _filterBeingChanged = true;
            UpdateBranchFilterItems();
        }

        private void tsmiShowBranchesAll_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowAllBranches);

        private void tsmiShowBranchesCurrent_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowCurrentBranchOnly);

        private void tsmiShowBranchesFiltered_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowFilteredBranches);

        private void tsmiShowFirstParent_Click(object sender, EventArgs e) => RevisionGridFilter.ToggleShowFirstParent();

        private void tsmiShowReflogs_Click(object sender, EventArgs e) => RevisionGridFilter.ToggleShowReflogReferences();

        private void tssbtnShowBranches_Click(object sender, EventArgs e) => tssbtnShowBranches.ShowDropDown();

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FilterToolBar _control;

            public TestAccessor(FilterToolBar control)
            {
                _control = control;
            }

            public ToolStripMenuItem tsmiBranchLocal => _control.tsmiBranchLocal;
            public ToolStripMenuItem tsmiBranchRemote => _control.tsmiBranchRemote;
            public ToolStripMenuItem tsmiBranchTag => _control.tsmiBranchTag;
            public ToolStripMenuItem tsmiCommitFilter => _control.tsmiCommitFilter;
            public ToolStripMenuItem tsmiCommitterFilter => _control.tsmiCommitterFilter;
            public ToolStripMenuItem tsmiAuthorFilter => _control.tsmiAuthorFilter;
            public ToolStripMenuItem tsmiDiffContainsFilter => _control.tsmiDiffContainsFilter;
            public ToolStripMenuItem tsmiHash => _control.tsmiHash;
            public ToolStripButton tsmiShowFirstParent => _control.tsmiShowFirstParent;
            public ToolStripButton tsmiShowReflogs => _control.tsmiShowReflogs;
            public ToolStripTextBox tstxtRevisionFilter => _control.tstxtRevisionFilter;
            public ToolStripLabel tslblRevisionFilter => _control.tslblRevisionFilter;
            public ToolStripButton tsbtnAdvancedFilter => _control.tsbtnAdvancedFilter;
            public ToolStripSplitButton tssbtnShowBranches => _control.tssbtnShowBranches;
            public ToolStripMenuItem tsmiShowBranchesAll => _control.tsmiShowBranchesAll;
            public ToolStripMenuItem tsmiShowBranchesCurrent => _control.tsmiShowBranchesCurrent;
            public ToolStripMenuItem tsmiShowBranchesFiltered => _control.tsmiShowBranchesFiltered;
            public ToolStripComboBox tscboBranchFilter => _control.tscboBranchFilter;
            public ToolStripDropDownButton tsddbtnBranchFilter => _control.tsddbtnBranchFilter;
            public ToolStripDropDownButton tsddbtnRevisionFilter => _control.tsddbtnRevisionFilter;
            public bool _isApplyingFilter => _control._isApplyingFilter;
            public bool _filterBeingChanged => _control._filterBeingChanged;

            public IRevisionGridFilter RevisionGridFilter => _control.RevisionGridFilter;

            public void ApplyCustomBranchFilter() => _control.ApplyCustomBranchFilter();

            public void ApplyRevisionFilter() => _control.ApplyRevisionFilter();

            public IGitModule GetModule() => _control.GetModule();
        }
    }
}
