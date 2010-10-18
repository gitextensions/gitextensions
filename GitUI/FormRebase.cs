using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormRebase : GitExtensionsForm
    {
        private readonly string _defaultBranch;

        public FormRebase(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormRebaseFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("rebase");
        }

        private void FormRebaseLoad(object sender, EventArgs e)
        {
            RestorePosition("rebase");

            var selectedHead = GitCommandHelpers.GetSelectedBranch();
            Currentbranch.Text = "Current branch: " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommandHelpers.GetHeads(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();

            splitContainer2.SplitterDistance = GitCommandHelpers.InTheMiddleOfRebase() ? 0 : 74;
            EnableButtons();
        }

        private void EnableButtons()
        {
            if (GitCommandHelpers.InTheMiddleOfRebase())
            {
                if (Height < 200)
                    Height = 500;

                Branches.Enabled = false;
                Ok.Enabled = false;

                AddFiles.Enabled = true;
                Resolved.Enabled = !GitCommandHelpers.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = GitCommandHelpers.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                Branches.Enabled = true;
                Ok.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            SolveMergeconflicts.Visible = GitCommandHelpers.InTheMiddleOfConflictedMerge();

            Resolved.Text = "Continue rebase";
            Mergetool.Text = "Solve conflicts";
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (GitCommandHelpers.InTheMiddleOfConflictedMerge())
            {
                AcceptButton = Mergetool;
                Mergetool.Focus();
                Mergetool.Text = ">Solve conflicts<";
                MergeToolPanel.BackColor = Color.Black;
            }
            else if (GitCommandHelpers.InTheMiddleOfRebase())
            {
                AcceptButton = Resolved;
                Resolved.Focus();
                Resolved.Text = ">Continue rebase<";
                ContinuePanel.BackColor = Color.Black;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartResolveConflictsDialog();
            EnableButtons();
        }

        private static void AddFilesClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog();
        }

        private void ResolvedClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommandHelpers.ContinueRebaseCmd()).ShowDialog();

            if (!GitCommandHelpers.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SkipClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommandHelpers.SkipRebaseCmd()).ShowDialog();

            if (!GitCommandHelpers.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void AbortClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommandHelpers.AbortRebaseCmd()).ShowDialog();

            if (!GitCommandHelpers.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void OkClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(Branches.Text))
            {
                MessageBox.Show("Please select a branch");
                return;
            }

            var form = new FormProcess(GitCommandHelpers.RebaseCmd(Branches.Text));
            form.ShowDialog();
            if (form.OutputString.ToString().Trim() == "Current branch a is up to date.")
                MessageBox.Show("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.", "Rebase");

            if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                !GitCommandHelpers.InTheMiddleOfRebase() &&
                !GitCommandHelpers.InTheMiddleOfPatch())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SolveMergeconflictsClick(object sender, EventArgs e)
        {
            MergetoolClick(sender, e);
        }
    }
}