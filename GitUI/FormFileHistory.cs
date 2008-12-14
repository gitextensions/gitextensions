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
        }

        private void FileChanges_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FileChanges_SelectionChanged(object sender, EventArgs e)
        {
            if (FileChanges.SelectedRows.Count == 0) return;

            if (FileChanges.SelectedRows[0].DataBoundItem is IGitItem)
            {
                IGitItem revision = (IGitItem)FileChanges.SelectedRows[0].DataBoundItem;

                View.Text = GitCommands.GitCommands.GetFileText(revision.Guid);
                View.Refresh();
            }

            if (FileChanges.SelectedRows.Count == 2)
            {
                if (FileChanges.SelectedRows[0].DataBoundItem is GitItem)
                    if (FileChanges.SelectedRows[1].DataBoundItem is GitItem)
                    {
                        GitItem revision1 = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;
                        GitItem revision2 = (GitItem)FileChanges.SelectedRows[1].DataBoundItem;

                        Diff diff = new Diff(new DiffDto(revision1.CommitGuid, revision2.CommitGuid, revision1.FileName));
                        diff.Execute();
                        EditorOptions.SetSyntax(Diff, FileName);
                        Diff.Text = diff.Dto.Result;
                        Diff.Refresh();
                    }
            }
            else
            if (FileChanges.SelectedRows.Count == 1)
            {
                if (FileChanges.SelectedRows[0].DataBoundItem is GitItem)
                {
                    GitItem revision1 = (GitItem)FileChanges.SelectedRows[0].DataBoundItem;

                    Diff diff = new Diff(new DiffDto(revision1.CommitGuid, revision1.CommitGuid + "^", FileName));
                    diff.Execute();
                    EditorOptions.SetSyntax(Diff, FileName);
                    Diff.Text = diff.Dto.Result;
                    Diff.Refresh();
                }
            }
            else
            {
                Diff.Text = "You need to select 2 files to view diff.";
                Diff.Refresh();
            }
        }
    }
}
