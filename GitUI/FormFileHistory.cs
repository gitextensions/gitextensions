using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormFileHistory : Form
    {
        public FormFileHistory(string fileName)
        {
            this.FileName = fileName;

            InitializeComponent();
        }

        public string FileName { get; set; }

        private void FormFileHistory_Load(object sender, EventArgs e)
        {
            EditorOptions.SetSyntax(View, FileName);
            
            FileChanges.DataSource = GitCommands.GitCommands.GetFileChanges(FileName);

            BlameGrid.RowTemplate.Height = 15;

            this.Text = "File History (" + FileName + ")";
        }

        private void FileChanges_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FileChanges_SelectionChanged(object sender, EventArgs e)
        {
            if (FileChanges.SelectedRows.Count == 0) return;

            if (FileChanges.SelectedRows[0].DataBoundItem is IGitItem)
            {
                GitItem revision = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;

                if (tabControl1.SelectedTab == Blame)
                {
                    BlameGrid.DataSource = GitCommands.GitCommands.Blame(FileName, revision.CommitGuid);
                }
                if (tabControl1.SelectedTab == ViewTab)
                {
                    View.Text = GitCommands.GitCommands.GetFileText(revision.Guid);
                    View.Refresh();
                }
            }

            if (FileChanges.SelectedRows.Count == 2)
            {
                if (FileChanges.SelectedRows[0].DataBoundItem is GitItem)
                    if (FileChanges.SelectedRows[1].DataBoundItem is GitItem)
                    {
                        GitItem revision1 = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;
                        GitItem revision2 = (GitItem)FileChanges.SelectedRows[1].DataBoundItem;

                        if (tabControl1.SelectedTab == DiffTab)
                        {
                            Diff diff = new Diff(new DiffDto(revision1.CommitGuid, revision2.CommitGuid, revision1.FileName));
                            diff.Execute();
                            ///EditorOptions.SetSyntax(Diff, FileName);
                            Diff.SetHighlighting("Patch");
                            Diff.Text = diff.Dto.Result;
                            Diff.Refresh();
                        }
                    }
            }
            else
            if (FileChanges.SelectedRows.Count == 1)
            {
                if (FileChanges.SelectedRows[0].DataBoundItem is GitItem)
                {
                    GitItem revision1 = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;

                    if (tabControl1.SelectedTab == DiffTab)
                    {
                        Diff diff = new Diff(new DiffDto(revision1.CommitGuid + "^", revision1.CommitGuid, FileName));
                        diff.Execute();
                        //EditorOptions.SetSyntax(Diff, FileName);
                        Diff.SetHighlighting("Patch");
                        Diff.Text = diff.Dto.Result;
                        Diff.Refresh();
                    }
                }
            }
            else
            {
                Diff.Text = "You need to select 2 files to view diff.";
                Diff.Refresh();
            }
        }

        private void Blame_Click(object sender, EventArgs e)
        {
            //if (FileChanges.SelectedRows.Count == 0) return;

            //if (FileChanges.SelectedRows[0].DataBoundItem is IGitItem)
            //{
            //    GitItem revision = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;
            //    BlameText.Text = GitCommands.GitCommands.Blame(FileName, revision.CommitGuid);
            //    BlameText.Refresh();
            //}          
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileChanges_SelectionChanged(sender, e);
        }

        private void FileChanges_DoubleClick(object sender, EventArgs e)
        {
            if (FileChanges.SelectedRows.Count > 0)
            {
                GitItem revision = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;

                FormDiffSmall form = new FormDiffSmall();
                form.SetRevision(revision.CommitGuid);
                form.ShowDialog();
            }
            else
                new FormDiff().ShowDialog();

        }

        private void textEditorControl1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void BlameGrid_DoubleClick(object sender, EventArgs e)
        {
            if (BlameGrid.SelectedRows.Count == 0)
                return;

            FormDiffSmall frm = new FormDiffSmall();
            frm.SetRevision(((GitBlame)BlameGrid.SelectedRows[0].DataBoundItem).CommitGuid);
            frm.ShowDialog();
        }

        private void BlameGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            
        }

        private void BlameGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                if (e.ColumnIndex == 0)
                {
                    e.Handled = true;
                    GitBlame blame = ((GitBlame)BlameGrid.Rows[e.RowIndex].DataBoundItem);
                    e.Graphics.FillRectangle(new SolidBrush(blame.color), e.CellBounds);
                    e.Graphics.DrawString(blame.Author, BlameGrid.Font, new SolidBrush(Color.Black), new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                }
            }
        }
    }
}
