using System;
using System.Drawing;
using System.Windows.Forms;

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

            var selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            Currentbranch.Text = "Current branch: " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();

            splitContainer2.SplitterDistance = GitCommands.GitCommands.InTheMiddleOfRebase() ? 0 : 74;
            EnableButtons();
        }

        private void EnableButtons()
        {
            if (GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                if (Height < 200)
                    Height = 500;

                Branches.Enabled = false;
                Ok.Enabled = false;

                AddFiles.Enabled = true;
                Resolved.Enabled = !GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
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

            SolveMergeconflicts.Visible = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();

            Resolved.Text = "Continue rebase";
            Mergetool.Text = "Solve conflicts";
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                AcceptButton = Mergetool;
                Mergetool.Focus();
                Mergetool.Text = ">Solve conflicts<";
                MergeToolPanel.BackColor = Color.Black;
            }
            else if (GitCommands.GitCommands.InTheMiddleOfRebase())
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
            new FormProcess(GitCommands.GitCommands.ContinueRebaseCmd()).ShowDialog();

            if (!GitCommands.GitCommands.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SkipClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.SkipRebaseCmd()).ShowDialog();

            if (!GitCommands.GitCommands.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void AbortClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.AbortRebaseCmd()).ShowDialog();

            if (!GitCommands.GitCommands.InTheMiddleOfRebase())
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

            var form = new FormProcess(GitCommands.GitCommands.RebaseCmd(Branches.Text));
            form.ShowDialog();
            if (form.OutputString.ToString().Trim() == "Current branch a is up to date.")
                MessageBox.Show("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.", "Rebase");

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() &&
                !GitCommands.GitCommands.InTheMiddleOfRebase() && 
                !GitCommands.GitCommands.InTheMiddleOfPatch())
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