using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public class FilterBranchHelper : IDisposable
    {
        private bool _applyingFilter;
        private bool _filterBeingChanged;

        private readonly ToolStripComboBox _NO_TRANSLATE_toolStripBranches;
        private readonly RevisionGridControl _NO_TRANSLATE_RevisionGrid;
        private readonly ToolStripMenuItem _localToolStripMenuItem;
        private readonly ToolStripMenuItem _tagsToolStripMenuItem;
        private readonly ToolStripMenuItem _remoteToolStripMenuItem;
        private GitModule Module => _NO_TRANSLATE_RevisionGrid.Module;

        private static readonly string[] _noResultsFound = { TranslatedStrings.NoResultsFound };

        public FilterBranchHelper(ToolStripComboBox toolStripBranches, ToolStripDropDownButton toolStripDropDownButton2, RevisionGridControl revisionGrid)
        {
            //
            // localToolStripMenuItem
            //
            _localToolStripMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "localToolStripMenuItem",
                Text = TranslatedStrings.Local
            };

            //
            // tagsToolStripMenuItem
            //
            _tagsToolStripMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = "tagToolStripMenuItem",
                Text = TranslatedStrings.Tag
            };

            //
            // remoteToolStripMenuItem
            //
            _remoteToolStripMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = "remoteToolStripMenuItem",
                Size = new System.Drawing.Size(115, 22),
                Text = TranslatedStrings.Remote
            };

            _NO_TRANSLATE_toolStripBranches = toolStripBranches;
            _NO_TRANSLATE_RevisionGrid = revisionGrid;
            _NO_TRANSLATE_RevisionGrid.RefFilterOptionsChanged += (s, e) =>
            {
                if (e.RefFilterOptions.HasFlag(RefFilterOptions.All | RefFilterOptions.Boundary))
                {
                    // This means show all branches
                    _NO_TRANSLATE_toolStripBranches.Text = string.Empty;
                }
            };

            toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[]
            {
                _localToolStripMenuItem,
                _tagsToolStripMenuItem,
                _remoteToolStripMenuItem
            });

            _NO_TRANSLATE_toolStripBranches.DropDown += toolStripBranches_DropDown;
            _NO_TRANSLATE_toolStripBranches.TextUpdate += toolStripBranches_TextUpdate;
            _NO_TRANSLATE_toolStripBranches.TextChanged += (s, e) => _filterBeingChanged = true;
            _NO_TRANSLATE_toolStripBranches.KeyUp += toolStripBranches_KeyUp;
        }

        public void InitToolStripBranchFilter()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool local = _localToolStripMenuItem.Checked;
            bool tag = _tagsToolStripMenuItem.Checked;
            bool remote = _remoteToolStripMenuItem.Checked;

            _NO_TRANSLATE_toolStripBranches.Items.Clear();

            if (Module.IsValidGitWorkingDir())
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;

                    var branches = GetBranchAndTagRefs(local, tag, remote);

                    await _NO_TRANSLATE_toolStripBranches.SwitchToMainThreadAsync();

                    foreach (var branch in branches)
                    {
                        _NO_TRANSLATE_toolStripBranches.Items.Add(branch);
                    }

                    var autoCompleteList = _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.Cast<string>();
                    if (!autoCompleteList.SequenceEqual(branches))
                    {
                        _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.Clear();
                        _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.AddRange(branches.ToArray());
                    }
                }).FileAndForget();
            }

            _NO_TRANSLATE_toolStripBranches.Enabled = Module.IsValidGitWorkingDir();
        }

        private List<string> GetBranchAndTagRefs(bool local, bool tag, bool remote)
        {
            // Options are interpreted as the refs the search should be limited too
            // If neither option is selected all refs will be queried also including stash and notes
            RefsFilter refs = (local ? RefsFilter.Heads : RefsFilter.NoFilter)
                | (tag ? RefsFilter.Tags : RefsFilter.NoFilter)
                | (remote ? RefsFilter.Remotes : RefsFilter.NoFilter);

            return Module.GetRefs(refs).Select(branch => branch.Name).ToList();
        }

        private void toolStripBranches_TextUpdate(object sender, EventArgs e)
        {
            _filterBeingChanged = true;
            UpdateBranchFilterItems();
        }

        private void toolStripBranches_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyBranchFilter(refresh: _filterBeingChanged);
            }
        }

        private void toolStripBranches_DropDown(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void ApplyBranchFilter(bool refresh)
        {
            if (_applyingFilter)
            {
                return;
            }

            _applyingFilter = true;

            // The user has accepted the filter
            _filterBeingChanged = false;

            try
            {
                string filter = _NO_TRANSLATE_toolStripBranches.Items.Count > 0 ? _NO_TRANSLATE_toolStripBranches.Text : string.Empty;

                if (filter == TranslatedStrings.NoResultsFound)
                {
                    filter = string.Empty;
                }

                bool success = _NO_TRANSLATE_RevisionGrid.SetAndApplyBranchFilter(filter);
                if (success && refresh)
                {
                    _NO_TRANSLATE_RevisionGrid.ForceRefreshRevisions();
                }
            }
            finally
            {
                _applyingFilter = false;
            }
        }

        private void UpdateBranchFilterItems()
        {
            string filter = _NO_TRANSLATE_toolStripBranches.Items.Count > 0 ? _NO_TRANSLATE_toolStripBranches.Text : string.Empty;
            var branches = GetBranchAndTagRefs(_localToolStripMenuItem.Checked, _tagsToolStripMenuItem.Checked, _remoteToolStripMenuItem.Checked);
            var matches = branches.Where(branch => branch.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();

            if (matches.Length == 0)
            {
                matches = _noResultsFound;
            }

            var index = _NO_TRANSLATE_toolStripBranches.SelectionStart;
            _NO_TRANSLATE_toolStripBranches.Items.Clear();
            _NO_TRANSLATE_toolStripBranches.Items.AddRange(matches);
            _NO_TRANSLATE_toolStripBranches.SelectionStart = index;
        }

        public void SetBranchFilter(string? filter, bool refresh)
        {
            _NO_TRANSLATE_toolStripBranches.Text = filter;
            ApplyBranchFilter(refresh);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localToolStripMenuItem.Dispose();
                _remoteToolStripMenuItem.Dispose();
                _tagsToolStripMenuItem.Dispose();
            }
        }
    }
}
