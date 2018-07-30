﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormReflog : GitModuleForm
    {
        private readonly TranslationString _continueResetCurrentBranchEvenWithChangesText = new TranslationString("You've got changes in your working directory that could be lost.\n\nDo you want to continue?");
        private readonly TranslationString _continueResetCurrentBranchCaptionText = new TranslationString("Changes not committed...");

        private readonly Regex _regexReflog = new Regex("^([^ ]+) ([^:]+): (.+)$", RegexOptions.Compiled);

        private string _currentBranch;
        private bool _isBranchCheckedOut;
        private bool _isDirtyDir;
        private int _lastHitRowIndex;

        public FormReflog(GitUICommands uiCommands)
            : base(uiCommands)
        {
            InitializeComponent();
            InitializeComplete();

            Sha.DataPropertyName = nameof(RefLine.Sha);
            Ref.DataPropertyName = nameof(RefLine.Ref);
            Action.DataPropertyName = nameof(RefLine.Action);
        }

        private void FormReflog_Load(object sender, EventArgs e)
        {
            _isDirtyDir = UICommands.Module.IsDirtyDir();
            _currentBranch = UICommands.Module.GetSelectedBranch();
            _isBranchCheckedOut = _currentBranch != "(no branch)";
            linkCurrentBranch.Text = "current branch (" + _currentBranch + ")";
            linkCurrentBranch.Visible = _isBranchCheckedOut;
            _lastHitRowIndex = 0;
            lblDirtyWorkingDirectory.Visible = _isDirtyDir;
            resetCurrentBranchOnThisCommitToolStripMenuItem.Enabled = _isBranchCheckedOut;

            var branches = new List<string> { "HEAD" };
            branches.AddRange(UICommands.Module.GetRefs(false, true).Select(r => r.Name).OrderBy(n => n));
            branches.AddRange(UICommands.Module.GetRemoteBranches().Select(r => r.Name).OrderBy(n => n));
            Branches.DataSource = branches;
        }

        public bool ShouldRefresh { get; set; }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(DisplayRefLog);

            async Task DisplayRefLog()
            {
                var item = (string)Branches.SelectedItem;
                await TaskScheduler.Default;
                var arguments = $"reflog --no-abbrev {item}";
                var output = UICommands.GitModule.RunGitCmd(arguments);
                var refLines = ConvertReflogOutput().ToList();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                gridReflog.DataSource = refLines;

                IEnumerable<RefLine> ConvertReflogOutput()
                    => from line in output.Split('\n')
                        where line.Length != 0
                        select _regexReflog.Match(line)
                        into match
                        where match.Success
                        select new RefLine
                        {
                            Sha = ObjectId.Parse(match.Groups[1].Value),
                            Ref = match.Groups[2].Value,
                            Action = match.Groups[3].Value,
                        };
            }
        }

        private void createABranchOnThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridReflog.SelectedCells.Count == 0 && gridReflog.SelectedRows.Count == 0)
            {
                return;
            }

            using (var form = new FormCreateBranch(UICommands, GetShaOfRefLine()))
            {
                form.CheckoutAfterCreation = false;
                form.UserAbleToChangeRevision = false;
                form.CouldBeOrphan = false;
                ShouldRefresh = form.ShowDialog(this) == DialogResult.OK;
            }
        }

        private ObjectId GetShaOfRefLine()
        {
            var row = GetSelectedRow();
            var refLine = (RefLine)row.DataBoundItem;
            return refLine.Sha;
        }

        private DataGridViewRow GetSelectedRow()
        {
            if (gridReflog.SelectedRows.Count > 0)
            {
                return gridReflog.SelectedRows[0];
            }

            return gridReflog.Rows[gridReflog.SelectedCells[0].RowIndex];
        }

        private void resetCurrentBranchOnThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isDirtyDir)
            {
                if (MessageBox.Show(this, _continueResetCurrentBranchEvenWithChangesText.Text,
                        _continueResetCurrentBranchCaptionText.Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }

            var gitRevision = UICommands.Module.GetRevision(GetShaOfRefLine());
            var resetType = _isDirtyDir ? FormResetCurrentBranch.ResetType.Soft : FormResetCurrentBranch.ResetType.Hard;
            var formResetCurrentBranch = new FormResetCurrentBranch(UICommands, gitRevision, resetType);
            var result = formResetCurrentBranch.ShowDialog(this);
            ShouldRefresh = result == DialogResult.OK;
        }

        private void copySha1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GetShaOfRefLine().ToString());
        }

        private void linkCurrentBranch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Branches.SelectedItem = _currentBranch;
        }

        private void linkHead_Click(object sender, EventArgs e)
        {
            Branches.SelectedIndex = 0;
        }

        private void gridReflog_MouseMove(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = gridReflog.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.Cell)
            {
                gridReflog.Rows[_lastHitRowIndex].Selected = false;
                _lastHitRowIndex = hit.RowIndex;
                gridReflog.Rows[_lastHitRowIndex].Selected = true;
            }
        }

        private void gridReflog_MouseClick(object sender, MouseEventArgs e)
        {
            contextMenuStripReflog.Show((Control)sender, e.Location);
        }
    }

    internal class RefLine
    {
        public ObjectId Sha { get; set; }
        public string Ref { get; set; }
        public string Action { get; set; }
    }
}
