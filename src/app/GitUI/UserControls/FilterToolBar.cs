using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.UserControls
{
    internal partial class FilterToolBar : ToolStripEx
    {
        internal const string ReflogButtonName = nameof(tsbShowReflog);
        private static readonly string[] _noResultsFound = [TranslatedStrings.NoResultsFound];
        private Func<IGitModule>? _getModule;
        private IRevisionGridFilter? _revisionGridFilter;
        private bool _isApplyingFilter;
        private bool _filterBeingChanged;
        private Func<RefsFilter, IReadOnlyList<IGitRef>> _getRefs;
        private string _tslblRevisionFilterToolTip;

        public FilterToolBar()
        {
            InitializeComponent();

            // Select an option until we get a filter bound.
            SelectShowBranchesFilterOption(selectedIndex: 0);

            tstxtRevisionFilter.Items.AddRange(AppSettings.RevisionFilterDropdowns
                .Union([
                    @"--invert-grep --grep=""EXCLUDE_COMMIT_MESSAGE_REGEX_PATTERN""",
                    @"--perl-regexp --author=""^(?!.*EXCLUDE_AUTHOR_REGEX_PATTERN)""",
                    @"--exclude=refs/remotes/EXCLUDE_REMOTE_REGEX_PATTERN"
                    ])
                .ToArray());
            tstxtRevisionFilter.ComboBox.ResizeDropDownWidth();
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
        private void ApplyCustomBranchFilter(bool checkBranch = true)
        {
            if (_isApplyingFilter)
            {
                return;
            }

            _isApplyingFilter = true;

            // The user has accepted the filter
            _filterBeingChanged = false;

            // Apply the textbox contents, no check if the (multiple) options is in tscboBranchFilter.Items (or that the list is generated)
            string filter = tscboBranchFilter.Text == TranslatedStrings.NoResultsFound ? string.Empty : tscboBranchFilter.Text;
            if (checkBranch && !string.IsNullOrWhiteSpace(filter))
            {
                List<string> newFilter = [];
                IReadOnlyList<IGitRef> refs = _getRefs(RefsFilter.NoFilter);

                // Split at whitespace (char[])null is default) but with split options.
                // Ignore quoting, Git revisions do not allow spaces.
                foreach (string branch in filter.Split((char[])null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    bool wildcardBranchFilter = branch.IndexOfAny(['?', '*', '[']) >= 0;
                    if (branch.StartsWith("--") || refs.Any(r => r.LocalName == branch) || branch.Contains(".."))
                    {
                        // Added as git-log option or revision filter
                    }
                    else if (wildcardBranchFilter)
                    {
                        // Added as --branches= option
                    }
                    else
                    {
                        string gitref = branch.StartsWith('^') ? branch[1..] : branch;
                        ObjectId oid = GetModule().RevParse(gitref);
                        if (oid is null)
                        {
                            TaskDialogPage page = new()
                            {
                                Heading = string.Format(TranslatedStrings.IgnoringReference, branch),
                                Caption = TranslatedStrings.NonexistingGitRevision,
                                Buttons = { TaskDialogButton.OK },
                                Icon = TaskDialogIcon.Warning,
                                SizeToContent = true
                            };

                            TaskDialog.ShowDialog(this, page);
                            continue;
                        }
                    }

                    newFilter.Add(branch);
                }

                filter = string.Join(" ", newFilter);
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
            RevisionGridFilter.SetAndApplyRevisionFilter(new RevisionFilter(tstxtRevisionFilter.Text.Trim(),
                                                                            tsmiCommitFilter.Checked,
                                                                            tsmiCommitterFilter.Checked,
                                                                            tsmiAuthorFilter.Checked,
                                                                            tsmiDiffContainsFilter.Checked));
            _isApplyingFilter = false;
        }

        public void Bind(Func<IGitModule> getModule, IRevisionGridFilter revisionGridFilter)
        {
            _getModule = getModule ?? throw new ArgumentNullException(nameof(getModule));

            DebugHelpers.Assert(_revisionGridFilter is null, $"{nameof(Bind)} must be invoked only once.");
            _revisionGridFilter = revisionGridFilter ?? throw new ArgumentNullException(nameof(revisionGridFilter));
            _revisionGridFilter.FilterChanged += revisionGridFilter_FilterChanged;
        }

        public void ClearQuickFilters()
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
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        private void InitBranchSelectionFilter(FilterChangedEventArgs e)
        {
            // Note: it is a weird combination, and it is mimicking the implementations in RevisionGridControl.
            // Refer to it for more details.

            ToolStripItem selectedItem = tsmiShowBranchesAll;

            if (e.ShowAllBranches)
            {
                // Show all branches
                selectedItem = tsmiShowBranchesAll;
            }

            if (e.ShowFilteredBranches)
            {
                // Show filtered branches
                selectedItem = tsmiShowBranchesFiltered;

                // Keep value if other filter
                tscboBranchFilter.Text = e.BranchFilter;
            }

            if (e.ShowCurrentBranchOnly)
            {
                // Show current branch only
                selectedItem = tsmiShowBranchesCurrent;
            }

            int selectedIndex = tssbtnShowBranches.DropDownItems.IndexOf(selectedItem);
            SelectShowBranchesFilterOption(selectedIndex);
        }

        public void InitToolStripStyles(Color toolForeColor, Color toolBackColor)
        {
            tsddbtnRevisionFilter.BackColor = toolBackColor;
            tsddbtnRevisionFilter.ForeColor = toolForeColor;

            Color toolTextBoxBackColor = SystemColors.Window;
            tscboBranchFilter.BackColor = toolTextBoxBackColor;
            tscboBranchFilter.ForeColor = toolForeColor;
            tstxtRevisionFilter.BackColor = toolTextBoxBackColor;
            tstxtRevisionFilter.ForeColor = toolForeColor;
        }

        private void SelectShowBranchesFilterOption(int selectedIndex)
        {
            if (selectedIndex >= tssbtnShowBranches.DropDownItems.Count)
            {
                selectedIndex = 0;
            }

            ToolStripItem selectedMenuItem = tssbtnShowBranches.DropDownItems[selectedIndex];
            tssbtnShowBranches.Image = selectedMenuItem.Image;
            tssbtnShowBranches.Text = selectedMenuItem.Text;
            tssbtnShowBranches.ToolTipText = selectedMenuItem.ToolTipText;
        }

        /// <summary>
        ///  Sets the branches filter.
        ///  No check that the branches exist (must be checked already, expected to be called from left panel).
        /// </summary>
        /// <param name="filter">The branches to filter separated by whitespace.</param>
        public void SetBranchFilter(string? filter)
        {
            tscboBranchFilter.Text = filter;
            ApplyCustomBranchFilter(checkBranch: false);
        }

        /// <summary>
        /// If focus on branch filter, focus revision filter otherwise branch filter.
        /// </summary>
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

        /// <summary>
        /// Update the function to get refs for branch dropdown filter
        /// </summary>
        /// <param name="getRefs">Function to get refs, expected to be cached</param>
        public void RefreshRevisionFunction(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            _getRefs = getRefs;
            tscboBranchFilter.Items.Clear();
        }

        /// <summary>
        /// Update the tscboBranchFilter dropdown items matching the current filter.
        /// This is called when dropdown clicked or text is manually changed
        /// (so tscboBranchFilter.Items is not necessarily available when set externally
        /// from the left panel or FormBrowse).
        /// </summary>
        private void UpdateBranchFilterItems()
        {
            IGitModule module = GetModule();
            if (!module.IsValidGitWorkingDir())
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            ThreadHelper.FileAndForget(async () =>
            {
                if (_getRefs is null)
                {
                    DebugHelpers.Fail("getRefs is unexpectedly null");
                    return;
                }

                RefsFilter branchesFilter = BranchesFilter();
                IReadOnlyList<IGitRef> refs = _getRefs(branchesFilter);
                string[] branches = refs.Select(branch => branch.Name).ToArray();

                await this.SwitchToMainThreadAsync();
                BindBranches(branches);
            });

            return;

            RefsFilter BranchesFilter()
            {
                // Options are interpreted as the refs the search should be limited too
                // If neither option is selected all refs will be queried also including stash and notes
                RefsFilter refs = (tsmiBranchLocal.Checked ? RefsFilter.Heads : RefsFilter.NoFilter)
                    | (tsmiBranchTag.Checked ? RefsFilter.Tags : RefsFilter.NoFilter)
                    | (tsmiBranchRemote.Checked ? RefsFilter.Remotes : RefsFilter.NoFilter);
                return refs;
            }

            void BindBranches(string[] branches)
            {
                IEnumerable<string> autoCompleteList = tscboBranchFilter.AutoCompleteCustomSource.Cast<string>();
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
                tscboBranchFilter.ComboBox.ResizeDropDownWidth();
            }
        }

        public void SetShortcutKeys(Action<ToolStripMenuItem, RevisionGridControl.Command> setShortcutString)
        {
            setShortcutString(tsmiResetPathFilters, RevisionGridControl.Command.ResetRevisionPathFilter);
            setShortcutString(tsmiResetAllFilters, RevisionGridControl.Command.ResetRevisionFilter);
            setShortcutString(tsmiAdvancedFilter, RevisionGridControl.Command.RevisionFilter);
        }

        private void revisionGridFilter_FilterChanged(object? sender, FilterChangedEventArgs e)
        {
            tsmiShowOnlyFirstParent.Checked = e.ShowOnlyFirstParent;
            tsbShowReflog.Checked = e.ShowReflogReferences;
            InitBranchSelectionFilter(e);

            List<(string filter, ToolStripMenuItem menuItem)> revFilters =
            [
                (e.MessageFilter, tsmiCommitFilter),
                (e.CommitterFilter, tsmiCommitterFilter),
                (e.AuthorFilter, tsmiAuthorFilter),
                (e.DiffContentFilter, tsmiDiffContainsFilter),
            ];

            // If there is no filter in filterInfo, clear text but retain checks
            tstxtRevisionFilter.Text = "";
            if (revFilters.Any(item => !string.IsNullOrWhiteSpace(item.filter)))
            {
                foreach ((string filter, ToolStripMenuItem menuItem) item in revFilters)
                {
                    // Check the first menuitem that matches and following identical filters
                    if (!string.IsNullOrWhiteSpace(item.filter)
                        && (string.IsNullOrWhiteSpace(tstxtRevisionFilter.Text)
                            || item.filter == tstxtRevisionFilter.Text))
                    {
                        tstxtRevisionFilter.Text = item.filter;
                        item.menuItem.Checked = true;
                    }
                    else
                    {
                        item.menuItem.Checked = false;
                    }
                }
            }

            // Add to dropdown and settings, unless already included
            string filter = tstxtRevisionFilter.Text.Trim();
            if (!string.IsNullOrWhiteSpace(filter) && (tstxtRevisionFilter.Items.Count == 0 || filter != (string)tstxtRevisionFilter.Items[0]))
            {
                if (tstxtRevisionFilter.Items.Contains(filter))
                {
                    tstxtRevisionFilter.Items.Remove(filter);
                }

                tstxtRevisionFilter.Items.Insert(0, filter);
                tstxtRevisionFilter.Text = filter;
                const int maxFilterItems = 30;
                AppSettings.RevisionFilterDropdowns = tstxtRevisionFilter.Items.Cast<object>()
                    .Select(item => item.ToString()).Take(maxFilterItems).ToArray();
            }

            tsbtnAdvancedFilter.ToolTipText = e.FilterSummary;
            tsbtnAdvancedFilter.AutoToolTip = !string.IsNullOrEmpty(tsbtnAdvancedFilter.ToolTipText);
            tsbtnAdvancedFilter.Image = e.HasFilter ? Properties.Images.FunnelExclamation : Properties.Images.FunnelPencil;
            tsmiResetPathFilters.Enabled = !string.IsNullOrEmpty(e.PathFilter);
            tsmiResetAllFilters.Enabled = e.HasFilter;
        }

        private void revisionFilterBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tstxtRevisionFilter.Text))
            {
                ApplyRevisionFilter();
            }
        }

        private void tsbtnAdvancedFilter_ButtonClick(object sender, EventArgs e)
        {
            if (!tsmiResetAllFilters.Enabled)
            {
                RevisionGridFilter.ShowRevisionFilterDialog();
            }
            else
            {
                tsbtnAdvancedFilter.ShowDropDown();
            }
        }

        private void tstxtRevisionFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Enter)
            {
                ApplyRevisionFilter();
            }
        }

        private void tscboBranchFilter_Click(object sender, EventArgs e)
        {
            if (!tscboBranchFilter.DroppedDown)
            {
                tscboBranchFilter.DroppedDown = true;
            }
        }

        private void tscboBranchFilter_DropDown(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void tscboBranchFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyCustomBranchFilter(checkBranch: true);
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

        private void tsmiDisablePathFilters_Click(object sender, EventArgs e) => RevisionGridFilter.SetAndApplyPathFilter("");

        private void tsmiDisableAllFilters_Click(object sender, EventArgs e) => RevisionGridFilter.ResetAllFiltersAndRefresh();

        private void tsmiAdvancedFilter_Click(object sender, EventArgs e) => RevisionGridFilter.ShowRevisionFilterDialog();

        private void tsmiShowReflogBranches_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowReflog);

        private void tsmiShowBranchesAll_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowAllBranches);

        private void tsmiShowBranchesCurrent_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowCurrentBranchOnly);

        private void tsmiShowBranchesFiltered_Click(object sender, EventArgs e) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowFilteredBranches);

        private void tsmiShowOnlyFirstParent_Click(object sender, EventArgs e) => RevisionGridFilter.ToggleShowOnlyFirstParent();

        private void tsmiShowReflog_Click(object sender, EventArgs e) => RevisionGridFilter.ToggleShowReflogReferences();

        private void tssbtnShowBranches_Click(object sender, EventArgs e) => tssbtnShowBranches.ShowDropDown();

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal void RefreshBrowseDialogShortcutKeys(IReadOnlyList<HotkeyCommand> hotkeys)
        {
            _tslblRevisionFilterToolTip ??= tslblRevisionFilter.ToolTipText;

            tslblRevisionFilter.ToolTipText = _tslblRevisionFilterToolTip.UpdateSuffix(hotkeys.GetShortcutToolTip(FormBrowse.Command.FocusFilter));
        }

        internal void RefreshRevisionGridShortcutKeys(IReadOnlyList<HotkeyCommand> hotkeys)
        {
            tsbShowReflog.ToolTipText = TranslatedStrings.ShowReflogTooltip.UpdateSuffix(hotkeys.GetShortcutToolTip(RevisionGridControl.Command.ShowReflogReferences));
            tsmiShowOnlyFirstParent.ToolTipText = TranslatedStrings.ShowOnlyFirstParent.UpdateSuffix(hotkeys.GetShortcutToolTip(RevisionGridControl.Command.ShowCurrentBranchOnly));

            tsmiShowBranchesAll.ShortcutKeyDisplayString = hotkeys.GetShortcutDisplay(RevisionGridControl.Command.ShowAllBranches);
            tsmiShowBranchesFiltered.ShortcutKeyDisplayString = hotkeys.GetShortcutDisplay(RevisionGridControl.Command.ShowFilteredBranches);
            tsmiShowBranchesCurrent.ShortcutKeyDisplayString = hotkeys.GetShortcutDisplay(RevisionGridControl.Command.ShowCurrentBranchOnly);
        }

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
            public ToolStripButton tsmiShowOnlyFirstParent => _control.tsmiShowOnlyFirstParent;
            public ToolStripButton tsbShowReflog => _control.tsbShowReflog;
            public ToolStripComboBox tstxtRevisionFilter => _control.tstxtRevisionFilter;
            public ToolStripLabel tslblRevisionFilter => _control.tslblRevisionFilter;
            public ToolStripSplitButton tsbtnAdvancedFilter => _control.tsbtnAdvancedFilter;
            public ToolStripSplitButton tssbtnShowBranches => _control.tssbtnShowBranches;
            public ToolStripMenuItem tsmiShowBranchesAll => _control.tsmiShowBranchesAll;
            public ToolStripMenuItem tsmiShowBranchesCurrent => _control.tsmiShowBranchesCurrent;
            public ToolStripMenuItem tsmiShowBranchesFiltered => _control.tsmiShowBranchesFiltered;
            public ToolStripComboBox tscboBranchFilter => _control.tscboBranchFilter;
            public void RefreshRevisionFunction(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs) => _control.RefreshRevisionFunction(getRefs);
            public ToolStripDropDownButton tsddbtnBranchFilter => _control.tsddbtnBranchFilter;
            public ToolStripDropDownButton tsddbtnRevisionFilter => _control.tsddbtnRevisionFilter;
            public bool _isApplyingFilter => _control._isApplyingFilter;
            public bool _filterBeingChanged => _control._filterBeingChanged;

            public IRevisionGridFilter RevisionGridFilter => _control.RevisionGridFilter;

            public void ApplyCustomBranchFilter(bool checkBranch) => _control.ApplyCustomBranchFilter(checkBranch);

            public void ApplyRevisionFilter() => _control.ApplyRevisionFilter();

            public IGitModule GetModule() => _control.GetModule();
        }
    }
}
