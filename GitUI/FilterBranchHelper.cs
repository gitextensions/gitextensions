using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class FilterBranchHelper
    {
        private ToolStripComboBox _NO_TRANSLATE_toolStripBranches;
        private ToolStripDropDownButton _NO_TRANSLATE_toolStripDropDownButton2;
        private RevisionGrid _NO_TRANSLATE_RevisionGrid;
        private ToolStripMenuItem localToolStripMenuItem;
        private ToolStripMenuItem remoteToolStripMenuItem;
        private GitModule Module { get { return _NO_TRANSLATE_RevisionGrid.Module; } }

        public FilterBranchHelper()
        {
            this.localToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // localToolStripMenuItem
            // 
            this.localToolStripMenuItem.Checked = true;
            this.localToolStripMenuItem.CheckOnClick = true;
            this.localToolStripMenuItem.Name = "localToolStripMenuItem";
            this.localToolStripMenuItem.Text = "Local";
            // 
            // remoteToolStripMenuItem
            // 
            this.remoteToolStripMenuItem.CheckOnClick = true;
            this.remoteToolStripMenuItem.Name = "remoteToolStripMenuItem";
            this.remoteToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.remoteToolStripMenuItem.Text = "Remote";        
        }

        public FilterBranchHelper(ToolStripComboBox toolStripBranches, ToolStripDropDownButton toolStripDropDownButton2, RevisionGrid RevisionGrid)
            : this()
        {
            this._NO_TRANSLATE_toolStripBranches = toolStripBranches;
            this._NO_TRANSLATE_toolStripDropDownButton2 = toolStripDropDownButton2;
            this._NO_TRANSLATE_RevisionGrid = RevisionGrid;

            this._NO_TRANSLATE_toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] {
                this.localToolStripMenuItem,
                this.remoteToolStripMenuItem});

            this._NO_TRANSLATE_toolStripBranches.DropDown += this.toolStripBranches_DropDown;
            this._NO_TRANSLATE_toolStripBranches.TextUpdate += this.toolStripBranches_TextUpdate;
            this._NO_TRANSLATE_toolStripBranches.Leave += this.toolStripBranches_Leave;
            this._NO_TRANSLATE_toolStripBranches.KeyUp += this.toolStripBranches_KeyUp;           
        }

        public void InitToolStripBranchFilter()
        {
            bool local = localToolStripMenuItem.Checked;
            bool remote = remoteToolStripMenuItem.Checked;

            _NO_TRANSLATE_toolStripBranches.Items.Clear();

            AsyncLoader.DoAsync(() => GetBranchAndTagRefs(local, remote),
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

        private IEnumerable<string> GetTagsRefs(bool local, bool remote)
        {
            var list = new List<string>();
            if (!remote)
                return list;
            if (local)
            {
                var tags = Module.GetRefs(true, true);
                list.AddRange(tags.Where(tag => tag.IsTag).Select(tag => tag.Name));
            }
            else
            {
                var tags = Module.GetRefs(true, true);
                list.AddRange(tags.Where(tag => tag.IsRemote && tag.IsTag).Select(tag => tag.Name));
            }
            return list;
        }

        private List<string> GetBranchAndTagRefs(bool local, bool remote)
        {
            var list = GetBranchHeads(local, remote);
            list.AddRange(GetTagsRefs(local, remote));
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
            InitToolStripBranchFilter();
            UpdateBranchFilterItems();
        }

        private void ApplyBranchFilter(bool refresh)
        {
            bool success = _NO_TRANSLATE_RevisionGrid.SetAndApplyBranchFilter(_NO_TRANSLATE_toolStripBranches.Text);
            if (success && refresh)
                _NO_TRANSLATE_RevisionGrid.ForceRefreshRevisions();
        }

        private void UpdateBranchFilterItems()
        {
            string filter = _NO_TRANSLATE_toolStripBranches.Text;
            _NO_TRANSLATE_toolStripBranches.Items.Clear();
            var index = _NO_TRANSLATE_toolStripBranches.Text.Length;
            var branches = GetBranchAndTagRefs(localToolStripMenuItem.Checked, remoteToolStripMenuItem.Checked);
            _NO_TRANSLATE_toolStripBranches.Items.AddRange(branches.Where(branch => branch.Contains(filter)).ToArray());
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
    }
}