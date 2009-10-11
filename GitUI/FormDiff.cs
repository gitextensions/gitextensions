using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Threading;
using PatchApply;

namespace GitUI
{
    public partial class FormDiff : GitExtensionsForm
    {
        public FormDiff()
        {
            InitializeComponent();
            //EditorOptions.SetSyntax(DiffText, "output.cs");
            //DiffText.SetHighlighting("Patch");
        }

        public FormDiff(GitRevision revision)
        {
            InitializeComponent();
            //EditorOptions.SetSyntax(DiffText, "output.cs");

            RevisionGrid.SetSelectedRevision( revision);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }




        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DiffFiles.DataSource = null;
                if (RevisionGrid.GetRevisions().Count == 0) return;

                DiffFiles.DisplayMember = "FileNameB";

                {
                    IGitItem revision = RevisionGrid.GetRevisions()[0];


                    if (RevisionGrid.GetRevisions().Count == 1)
                        DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(((GitRevision)RevisionGrid.GetRevisions()[0]).Guid, ((GitRevision)RevisionGrid.GetRevisions()[0]).ParentGuids[0]);
                }

                if (RevisionGrid.GetRevisions().Count == 2)
                {
                    {

                        DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(((GitRevision)RevisionGrid.GetRevisions()[0]).Guid, ((GitRevision)RevisionGrid.GetRevisions()[1]).Guid);

                    }
                }

            /*    ShortDiffDto shortdto = new ShortDiffDto(From.Text, To.Text);
                ShortDiff shortdiff = new ShortDiff(shortdto);
                shortdiff.Execute();

                string message = "There are " + shortdto.Result + ".\nWhen the difference report is big, showing the diff can take a while.";

                if (MessageBox.Show(message, "Diff", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    DiffDto dto = new DiffDto(From.Text, To.Text);
                    Diff diff = new Diff(dto);
                    diff.Execute();

                    DiffText.Text = dto.Result;
                    DiffText.Refresh();
                }*/
            }
            catch
            {
            }
        }

        private void From_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void FormDiff_Load(object sender, EventArgs e)
        {
            //From.DisplayMember = "Name";
            //From.DataSource = GitCommands.GitCommands.GetHeads();
            
            //To.DisplayMember = "Name";
            //To.DataSource = GitCommands.GitCommands.GetHeads();
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (DiffFiles.SelectedItem is Patch)
            {
                {
                    Patch patch = (Patch)DiffFiles.SelectedItem;
                    //DiffText.Text = patch.Text;
                    //DiffText.Refresh();
                    DiffText.ViewPatch(patch.Text);
                    //EditorOptions.SetSyntax(DiffText, patch.FileNameB);
                }

                //string changedFile = (string)DiffFiles.SelectedItem;


                //DiffText.Text = changedFile.PatchText;
            }
            else
                if (DiffFiles.SelectedItem is string)
                {
                    Patch selectedPatch = null;

                    if (RevisionGrid.GetRevisions().Count == 2)
                    {
                        selectedPatch = GitCommands.GitCommands.GetSingleDiff(((GitRevision)RevisionGrid.GetRevisions()[0]).Guid, ((GitRevision)RevisionGrid.GetRevisions()[1]).Guid, (string)DiffFiles.SelectedItem);
                    }
                    else
                    {
                        GitRevision revision = RevisionGrid.GetRevisions()[0];
                        selectedPatch = GitCommands.GitCommands.GetSingleDiff(revision.Guid, revision.ParentGuids[0], (string)DiffFiles.SelectedItem);
                    }

                    if (selectedPatch != null)
                    {
                        DiffText.ViewPatch(selectedPatch.Text);
                    }
                    else
                    {
                        DiffText.ViewPatch("");
                    }
                }
        }

        private void RevisionGrid_SelectionChanged(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is string)
            {
                {
                    GitUICommands.Instance.StartFileHistoryDialog((string)DiffFiles.SelectedItem);
                }
            }
        }
    }
}
