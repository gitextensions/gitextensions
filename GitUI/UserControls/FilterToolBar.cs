using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;

namespace GitUI.UserControls
{
    internal partial class FilterToolBar : ToolStripEx
    {
        private bool _isApplyingFilter;

        /// <summary>
        /// Occurs whenever the advanced filter button is clicked.
        /// </summary>
        public event EventHandler AdvancedFilterRequested;

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

            tstxtRevisionFilter.Leave += (s, e) => ApplyFilter();
            tstxtRevisionFilter.KeyUp += (s, e) =>
            {
                if (e.KeyValue == (char)Keys.Enter)
                {
                    ApplyFilter();
                }
            };
        }

        public bool ShowFirstParentChecked
        {
            get => tsmiShowFirstParent.Checked;
            set => tsmiShowFirstParent.Checked = value;
        }

        private void ApplyFilter()
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

        public void SetFilter(string filter)
        {
            tstxtRevisionFilter.Text = filter;
            ApplyFilter();
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

        private void tscboBranchFilter_ResizeDropDownWidth(object sender, EventArgs e)
        {
            tscboBranchFilter.ComboBox.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        private void tsmiShowFirstParentt_Click(object sender, EventArgs e)
        {
            ShowFirstParentsCheckedChanged?.Invoke(tsmiShowFirstParent, EventArgs.Empty);
        }
    }
}
