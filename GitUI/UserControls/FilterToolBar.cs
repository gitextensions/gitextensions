using System;
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
        private bool _isUnitTests;

        public FilterToolBar()
        {
            InitializeComponent();

            tsmiShowFirstParent.Checked = AppSettings.ShowFirstParent;

            tscboBranchFilter.Leave += (s, e) => ApplyBranchFilter(refresh: true);

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

        private IRevisionGridFilter RevisionGridFilter
        {
            get => _revisionGridFilter ?? throw new InvalidOperationException($"{nameof(Bind)} is not called.");
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

            RevisionGridFilter.SetAndApplyBranchFilter(filter, refresh);

            _isApplyingFilter = false;
        }

        private void ApplyRevisionFilter()
        {
            if (_isApplyingFilter)
            {
                return;
            }

            try
            {
                _isApplyingFilter = true;
                RevisionGridFilter.SetAndApplyRevisionFilter(new RevisionFilter(tstxtRevisionFilter.Text,
                                                                                tsmiCommitFilter.Checked,
                                                                                tsmiCommitterFilter.Checked,
                                                                                tsmiAuthorFilter.Checked,
                                                                                tsmiDiffContainsFilter.Checked));
            }
            catch (InvalidOperationException ex)
            {
                if (!_isUnitTests)
                {
                    MessageBox.Show(this, ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                SetRevisionFilter(string.Empty);
            }
            finally
            {
                _isApplyingFilter = false;
            }
        }

        public void Bind(Func<IGitModule> getModule, IRevisionGridFilter revisionGridFilter)
        {
            _getModule = getModule ?? throw new ArgumentNullException(nameof(getModule));
            _revisionGridFilter = revisionGridFilter ?? throw new ArgumentNullException(nameof(revisionGridFilter));
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

        /// <summary>
        ///  Clears the filter textbox without raising any events.
        /// </summary>
        public void ResetBranchesFilter() => tscboBranchFilter.Text = string.Empty;

        /// <summary>
        ///  Sets the branches filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="refresh"><see langword="true"/> to request the revision grid to refresh; otherwise <see langword="false"/>.</param>
        public void SetBranchFilter(string? filter, bool refresh)
        {
            tscboBranchFilter.Text = filter;
            ApplyBranchFilter(refresh);
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
            tstxtRevisionFilter.Text = filter;
            ApplyRevisionFilter();
        }

        private void UpdateBranchFilterItems()
        {
            tscboBranchFilter.Items.Clear();

            ThreadHelper.ThrowIfNotOnUIThread();

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
            RevisionGridFilter.ToggleShowFirstParent();
        }

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
            public ToolStripTextBox tstxtRevisionFilter => _control.tstxtRevisionFilter;
            public ToolStripLabel tslblRevisionFilter => _control.tslblRevisionFilter;
            public ToolStripButton tsbtnAdvancedFilter => _control.tsbtnAdvancedFilter;
            public ToolStripComboBox tscboBranchFilter => _control.tscboBranchFilter;
            public ToolStripDropDownButton tsddbtnBranchFilter => _control.tsddbtnBranchFilter;
            public ToolStripDropDownButton tsddbtnRevisionFilter => _control.tsddbtnRevisionFilter;
            public bool _isApplyingFilter => _control._isApplyingFilter;
            public bool _filterBeingChanged => _control._filterBeingChanged;

            public IRevisionGridFilter RevisionGridFilter => _control.RevisionGridFilter;

            public void ApplyBranchFilter(bool refresh) => _control.ApplyBranchFilter(refresh);

            public void ApplyRevisionFilter() => _control.ApplyRevisionFilter();

            public IGitModule GetModule() => _control.GetModule();

            public bool SetUnitTestsMode() => _control._isUnitTests = true;
        }
    }
}
