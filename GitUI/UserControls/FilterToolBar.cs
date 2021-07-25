using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI.UserControls
{
    internal partial class FilterToolBar : ToolStripEx
    {
        private static readonly string[] _noResultsFound = { TranslatedStrings.NoResultsFound };
        private bool _isApplyingFilter;
        private bool _filterBeingChanged;

        /// <summary>
        /// Occurs whenever the advanced filter button is clicked.
        /// </summary>
        public event EventHandler AdvancedFilterRequested;

        /// <summary>
        /// Occurs whenever the branch filter is applied.
        /// </summary>
        public event EventHandler<BranchFilterEventArgs> BranchFilterApplied;

        /// <summary>
        /// Occurs whenever the branches filter dropdown needs to be udpated.
        /// </summary>
        public event EventHandler BranchesUpdateRequested;

        /// <summary>
        /// Occurs whenever the revision filter is applied.
        /// </summary>
        public event EventHandler<RevisionFilterEventArgs> RevisionFilterApplied;

        /// <summary>
        /// Occurs whenever the 'ShowFirstParents' checked state changed.
        /// </summary>
        public event EventHandler ShowFirstParentsCheckedChanged;

        public FilterToolBar()
        {
            InitializeComponent();

            tsmiShowFirstParent.Checked = AppSettings.ShowFirstParent;

            tstxtRevisionFilter.Leave += (s, e) => ApplyRevisionFilter();
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

        public bool ShowFirstParentChecked
        {
            get => tsmiShowFirstParent.Checked;
            set => tsmiShowFirstParent.Checked = value;
        }

        private void ApplyBranchFilter(bool refresh)
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

            BranchFilterApplied?.Invoke(this, new BranchFilterEventArgs(filter, refresh));

            _isApplyingFilter = false;
        }

        private void ApplyRevisionFilter()
        {
            if (_isApplyingFilter)
            {
                return;
            }

            _isApplyingFilter = true;
            RevisionFilterApplied?.Invoke(this,
                                          new RevisionFilterEventArgs(tstxtRevisionFilter.Text,
                                                                      tsmiCommitFilter.Checked,
                                                                      tsmiCommitter.Checked,
                                                                      tsmiAuthor.Checked,
                                                                      tsmiDiffContains.Checked));
            _isApplyingFilter = false;
        }

        public void BindBranches(string[] branches)
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

        /// <summary>
        ///  Clears the filter textbox without raising any events.
        /// </summary>
        public void ResetBranchesFilter() => tscboBranchFilter.Text = string.Empty;

        /// <summary>
        ///  Sets the branches filter and raises <see cref="BranchFilterApplied"/> event.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="refresh"><see langword="true"/> to request the revision grid to refresh; otherwise <see langword="false"/>.</param>
        public void SetBranchFilter(string? filter, bool refresh)
        {
            tscboBranchFilter.Text = filter;
            ApplyBranchFilter(refresh);
        }

        /// <summary>
        ///  Sets the revision filter and raises <see cref="RevisionFilterApplied"/> event.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        public void SetRevisionFilter(string filter)
        {
            tstxtRevisionFilter.Text = filter;
            ApplyRevisionFilter();
        }

        public void SetFocus()
        {
            ToolStripControlHost filterToFocus = tstxtRevisionFilter.Focused
                ? tscboBranchFilter
                : tstxtRevisionFilter;
            filterToFocus.Focus();
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

            // Scale tool strip items according to DPI
            tscboBranchFilter.Size = DpiUtil.Scale(tscboBranchFilter.Size);
            tstxtRevisionFilter.Size = DpiUtil.Scale(tstxtRevisionFilter.Size);
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

        private void UpdateBranchFilterItems()
        {
            tscboBranchFilter.Items.Clear();
            BranchesUpdateRequested?.Invoke(this, EventArgs.Empty);
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
            AdvancedFilterRequested?.Invoke(tsbtnAdvancedFilter, EventArgs.Empty);
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
                ApplyBranchFilter(refresh: _filterBeingChanged);
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

        private void tsmiShowFirstParentt_Click(object sender, EventArgs e)
        {
            ShowFirstParentsCheckedChanged?.Invoke(tsmiShowFirstParent, EventArgs.Empty);
        }
    }
}
