using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class FormRebase : GitExtensionsForm
    {
        public FormRebase()
        {
            InitializeComponent();
        }

        private void FormRebase_Load(object sender, EventArgs e)
        {
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            Currentbranch.Text = "Current branch: " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(true, true);
            Branches.Select();

            if (GitCommands.GitCommands.InTheMiddleOfRebase())
                splitContainer2.SplitterDistance = 0;
            else
                splitContainer2.SplitterDistance = 74;
            

            EnableButtons();
        }

        private void EnableButtons()
        {


            if (GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                if (this.Height < 200)
                    this.Height = 500;

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
            else
                if (GitCommands.GitCommands.InTheMiddleOfRebase())
                {
                    AcceptButton = Resolved;
                    Resolved.Focus();
                    Resolved.Text = ">Continue rebase<";
                    ContinuePanel.BackColor = Color.Black;
                }

            
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartResolveConflictsDialog();
            EnableButtons();
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog();
        }

        private void Resolved_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.ContinueRebaseCmd());

            if (!GitCommands.GitCommands.InTheMiddleOfRebase())
                Close();
            
            EnableButtons();
            patchGrid1.Initialize();

        }

        private void Skip_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor; 
            new FormProcess(GitCommands.GitCommands.SkipRebaseCmd());

            if (!GitCommands.GitCommands.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.AbortRebaseCmd());

            if (!GitCommands.GitCommands.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(Branches.Text))
            {
                MessageBox.Show("Please select a branch");
                return;
            }

            FormProcess form = new FormProcess(GitCommands.GitCommands.RebaseCmd(Branches.Text));
            if (form.outputString.ToString().Trim() == "Current branch a is up to date.")
                MessageBox.Show("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.", "Rebase");

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !GitCommands.GitCommands.InTheMiddleOfPatch())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
        }

        private void SolveMergeconflicts_Click(object sender, EventArgs e)
        {
            Mergetool_Click(sender, e);
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
