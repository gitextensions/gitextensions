using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormAddSubmodule : GitExtensionsForm
    {
        public FormAddSubmodule()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.SelectedPath = Directory.Text;

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                Directory.Text = browseDialog.SelectedPath;
            }

        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text) || string.IsNullOrEmpty(LocalPath.Text))
            {
                MessageBox.Show("A remote path and local path are required");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            FormProcess formProcess = new FormProcess(GitCommands.GitCommands.AddSubmoduleCmd(Directory.Text, LocalPath.Text, Branch.Text));

            Close();
        }

        private void Directory_SelectedIndexChanged(object sender, EventArgs e)
        {
            Directory_TextUpdate(null, null);
        }

        private void FormAddSubmodule_Shown(object sender, EventArgs e)
        {
            Directory.DataSource = GitCommands.RepositoryHistory.MostRecentRepositories;
            Directory.Text = "";
            LocalPath.Text = "";
        }

        private void Branch_DropDown(object sender, EventArgs e)
        {
            string realWorkingDir = GitCommands.Settings.WorkingDir;
            GitCommands.Settings.WorkingDir = Directory.Text;

            List<GitCommands.GitHead> heads = GitCommands.GitCommands.GetHeads(false);

            GitCommands.GitHead noHead = new GitCommands.GitHead();
            noHead.Name = "";
            heads.Insert(0, noHead);

            Branch.DisplayMember = "Name";
            Branch.DataSource = heads;

            GitCommands.Settings.WorkingDir = realWorkingDir;

        }

        private void Directory_TextUpdate(object sender, EventArgs e)
        {
            string path = Directory.Text;
            path = path.TrimEnd(new char[] { '\\', '/' });

            if (path.EndsWith(".git"))
                path = path.Replace(".git", "");

            if (path.Contains("\\") || path.Contains("/"))
                LocalPath.Text = path.Substring(path.LastIndexOfAny(new char[] { '\\', '/' }) + 1);

        }
    }
}
