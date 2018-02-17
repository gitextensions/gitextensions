using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class FilterBranchHelper : IDisposable
    {
        private bool _applyingFilter;
        private readonly ToolStripComboBox _NO_TRANSLATE_toolStripBranches;
        private readonly ToolStripDropDownButton _NO_TRANSLATE_toolStripDropDownButton2;
        private readonly RevisionGrid _NO_TRANSLATE_RevisionGrid;
        private readonly ToolStripMenuItem localToolStripMenuItem;
        private readonly ToolStripMenuItem tagsToolStripMenuItem;
        private readonly ToolStripMenuItem remoteToolStripMenuItem;
        private GitModule Module => _NO_TRANSLATE_RevisionGrid.Module;

        public FilterBranchHelper()
        {
            localToolStripMenuItem = new ToolStripMenuItem();
            tagsToolStripMenuItem = new ToolStripMenuItem();
            remoteToolStripMenuItem = new ToolStripMenuItem();
            // 
            // localToolStripMenuItem
            // 
            localToolStripMenuItem.Checked = true;
            localToolStripMenuItem.CheckOnClick = true;
            localToolStripMenuItem.Name = "localToolStripMenuItem";
            localToolStripMenuItem.Text = "Local";
            //
            // tagsToolStripMenuItem
            //
            tagsToolStripMenuItem.CheckOnClick = true;
            tagsToolStripMenuItem.Name = "tagToolStripMenuItem";
            tagsToolStripMenuItem.Text = "Tag";
            //
            // remoteToolStripMenuItem
            // 
            remoteToolStripMenuItem.CheckOnClick = true;
            remoteToolStripMenuItem.Name = "remoteToolStripMenuItem";
            remoteToolStripMenuItem.Size = new Size(115, 22);
            remoteToolStripMenuItem.Text = "Remote";        
        }

        public FilterBranchHelper(ToolStripComboBox toolStripBranches, ToolStripDropDownButton toolStripDropDownButton2, RevisionGrid revisionGrid)
            : this()
        {
            _NO_TRANSLATE_toolStripBranches = toolStripBranches;
            _NO_TRANSLATE_toolStripDropDownButton2 = toolStripDropDownButton2;
            _NO_TRANSLATE_RevisionGrid = revisionGrid;

            _NO_TRANSLATE_toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] {
                localToolStripMenuItem,
                tagsToolStripMenuItem,
                remoteToolStripMenuItem});

            _NO_TRANSLATE_toolStripBranches.DropDown += toolStripBranches_DropDown;
            _NO_TRANSLATE_toolStripBranches.TextUpdate += toolStripBranches_TextUpdate;
            _NO_TRANSLATE_toolStripBranches.Leave += toolStripBranches_Leave;
            _NO_TRANSLATE_toolStripBranches.KeyUp += toolStripBranches_KeyUp;           
        }

        public void InitToolStripBranchFilter()
        {
            bool local = localToolStripMenuItem.Checked;
            bool tag = tagsToolStripMenuItem.Checked;
            bool remote = remoteToolStripMenuItem.Checked;

            _NO_TRANSLATE_toolStripBranches.Items.Clear();

            if (Module.IsValidGitWorkingDir())
            {
                AsyncLoader.DoAsync(() => GetBranchAndTagRefs(local, tag, remote),
                    branches =>
                    {
                        foreach (var branch in branches)
                            _NO_TRANSLATE_toolStripBranches.Items.Add(branch);

                        var autoCompleteList = _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.Cast<string>();
                        if (!autoCompleteList.SequenceEqual(branches))
                        {
                            _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.Clear();
                            _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.AddRange(branches.ToArray());
                        }
                    });
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
                list.AddRange(GetTagsRefs());
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
            var branches = GetBranchAndTagRefs(localToolStripMenuItem.Checked, tagsToolStripMenuItem.Checked, remoteToolStripMenuItem.Checked);
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
                localToolStripMenuItem.Dispose();
                remoteToolStripMenuItem.Dispose();
                tagsToolStripMenuItem.Dispose();
            }
        }
    }
}
