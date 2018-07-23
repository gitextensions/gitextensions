using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public class FilterBranchHelper : IDisposable
    {
        private bool _applyingFilter;
        private readonly ToolStripComboBox _NO_TRANSLATE_toolStripBranches;
        private readonly RevisionGridControl _NO_TRANSLATE_RevisionGrid;
        private readonly ToolStripMenuItem _localToolStripMenuItem;
        private readonly ToolStripMenuItem _tagsToolStripMenuItem;
        private readonly ToolStripMenuItem _remoteToolStripMenuItem;
        private GitModule Module => _NO_TRANSLATE_RevisionGrid.Module;

        public FilterBranchHelper()
        {
            _localToolStripMenuItem = new ToolStripMenuItem();
            _tagsToolStripMenuItem = new ToolStripMenuItem();
            _remoteToolStripMenuItem = new ToolStripMenuItem();

            //
            // localToolStripMenuItem
            //
            _localToolStripMenuItem.Checked = true;
            _localToolStripMenuItem.CheckOnClick = true;
            _localToolStripMenuItem.Name = "localToolStripMenuItem";
            _localToolStripMenuItem.Text = "Local";

            //
            // tagsToolStripMenuItem
            //
            _tagsToolStripMenuItem.CheckOnClick = true;
            _tagsToolStripMenuItem.Name = "tagToolStripMenuItem";
            _tagsToolStripMenuItem.Text = "Tag";

            //
            // remoteToolStripMenuItem
            //
            _remoteToolStripMenuItem.CheckOnClick = true;
            _remoteToolStripMenuItem.Name = "remoteToolStripMenuItem";
            _remoteToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            _remoteToolStripMenuItem.Text = "Remote";
        }

        public FilterBranchHelper(ToolStripComboBox toolStripBranches, ToolStripDropDownButton toolStripDropDownButton2, RevisionGridControl revisionGrid)
            : this()
        {
            _NO_TRANSLATE_toolStripBranches = toolStripBranches;
            _NO_TRANSLATE_RevisionGrid = revisionGrid;

            toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[]
            {
                _localToolStripMenuItem,
                _tagsToolStripMenuItem,
                _remoteToolStripMenuItem
            });

            _NO_TRANSLATE_toolStripBranches.DropDown += toolStripBranches_DropDown;
            _NO_TRANSLATE_toolStripBranches.TextUpdate += toolStripBranches_TextUpdate;
            _NO_TRANSLATE_toolStripBranches.Leave += toolStripBranches_Leave;
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

        private List<string> GetBranchHeads(bool local, bool remote)
        {
            var list = new List<string>();
            if (local && remote)
            {
                var branches = Module.GetRefs(true, true);
                list.AddRange(branches.Where(branch => !branch.IsTag).Select(branch => branch.Name));
            }
            else if (local)
            {
                var branches = Module.GetRefs(false);
                list.AddRange(branches.Select(branch => branch.Name));
            }
            else if (remote)
            {
                var branches = Module.GetRefs(true, true);
                list.AddRange(branches.Where(branch => branch.IsRemote && !branch.IsTag).Select(branch => branch.Name));
            }

            return list;
        }

        private IEnumerable<string> GetTagsRefs()
        {
            return Module.GetRefs(true, false).Select(tag => tag.Name);
        }

        private List<string> GetBranchAndTagRefs(bool local, bool tag, bool remote)
        {
            var list = GetBranchHeads(local, remote);
            if (tag)
            {
                list.AddRange(GetTagsRefs());
            }

            return list;
        }

        private void toolStripBranches_TextUpdate(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void toolStripBranches_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Enter)
            {
                ApplyBranchFilter(true);
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
            try
            {
                string filter = _NO_TRANSLATE_toolStripBranches.Items.Count > 0 ? _NO_TRANSLATE_toolStripBranches.Text : string.Empty;
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

            var index = _NO_TRANSLATE_toolStripBranches.SelectionStart;
            _NO_TRANSLATE_toolStripBranches.Items.Clear();
            _NO_TRANSLATE_toolStripBranches.Items.AddRange(matches);
            _NO_TRANSLATE_toolStripBranches.SelectionStart = index;
        }

        public void SetBranchFilter(string filter, bool refresh)
        {
            _NO_TRANSLATE_toolStripBranches.Text = filter;
            ApplyBranchFilter(refresh);
        }

        private void toolStripBranches_Leave(object sender, EventArgs e)
        {
            ApplyBranchFilter(true);
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
