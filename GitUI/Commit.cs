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
    public partial class FormCommit : Form
    {
        public FormCommit()
        {
            InitializeComponent();
        }

        private void FormCommit_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            List<GitItemStatus> itemStatusList = GitCommands.GitCommands.GitStatus();

            List<GitItemStatus> untrackedItemStatus = new List<GitItemStatus>();
            List<GitItemStatus> trackedItemStatus = new List<GitItemStatus>();
            foreach (GitItemStatus itemStatus in itemStatusList)
            {
                if (itemStatus.IsTracked == false)
                    untrackedItemStatus.Add(itemStatus);
                else
                    trackedItemStatus.Add(itemStatus);
            }

            Tracked.DataSource = trackedItemStatus;
            Untracked.DataSource = untrackedItemStatus;
        }

        protected void ShowChanges(GitItemStatus item)
        {
            SelectedDiff.Text = GitCommands.GitCommands.GetCurrentChanges(item.Name);
        }

        private void Tracked_SelectionChanged(object sender, EventArgs e)
        {
            if (Tracked.SelectedRows.Count == 0) return;

            if (Tracked.SelectedRows[0].DataBoundItem is GitItemStatus)
            {
                ShowChanges((GitItemStatus)Tracked.SelectedRows[0].DataBoundItem);
            }
        }

        private void Untracked_SelectionChanged(object sender, EventArgs e)
        {
            if (Untracked.SelectedRows.Count == 0) return;

            if (Untracked.SelectedRows[0].DataBoundItem is GitItemStatus)
            {
                ShowChanges((GitItemStatus)Untracked.SelectedRows[0].DataBoundItem);
            }
        }

        private void Commit_Click(object sender, EventArgs e)
        {
            try
            {
                OutPut.Text = "";

                CommitDto dto = new CommitDto(Message.Text);
                GitCommands.Commit commit = new GitCommands.Commit(dto);
                commit.Execute();

                if (OutPut.Text.Length == 0)
                    OutPut.Text = "Command executed \n";

                OutPut.Text += dto.Result;

                Initialize();
            }
            catch
            {
            }
        }

        private void Scan_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Stage_Click(object sender, EventArgs e)
        {

            string result = "";
            foreach (DataGridViewRow row in Untracked.SelectedRows)
            {
                if (row.DataBoundItem is GitItemStatus)
                {
                    GitItemStatus item = (GitItemStatus)row.DataBoundItem;

                    AddFilesDto dto = new AddFilesDto(item.Name);
                    AddFiles addFiles = new AddFiles(dto);
                    addFiles.Execute();

                    result += dto.Result + "\n";

                }
            }

            if (result.Length > 0)
                OutPut.Text = result;

            Initialize();
        }
    }
}
