using System;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI
{
    public partial class FormAddSubmodule : GitExtensionsForm
    {
        public FormAddSubmodule()
        {
            InitializeComponent();
            Translate();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var browseDialog = new FolderBrowserDialog {SelectedPath = Directory.Text};

            if (browseDialog.ShowDialog() == DialogResult.OK)
                Directory.Text = browseDialog.SelectedPath;
        }

        private void AddClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text) || string.IsNullOrEmpty(LocalPath.Text))
            {
                MessageBox.Show("A remote path and local path are required");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            var addSubmoduleCmd = GitCommands.GitCommands.AddSubmoduleCmd(Directory.Text, LocalPath.Text, Branch.Text);
            new FormProcess(addSubmoduleCmd).ShowDialog();

            Close();
            Cursor.Current = Cursors.Default;
        }

        private void DirectorySelectedIndexChanged(object sender, EventArgs e)
        {
            DirectoryTextUpdate(null, null);
        }

        private void FormAddSubmoduleShown(object sender, EventArgs e)
        {
            Directory.DataSource = Repositories.RepositoryHistory.Repositories;
            Directory.DisplayMember = "Path";
            Directory.Text = "";
            LocalPath.Text = "";
        }

        private void BranchDropDown(object sender, EventArgs e)
        {
            var realWorkingDir = Settings.WorkingDir;
            Settings.WorkingDir = Directory.Text;

            var heads = GitCommands.GitCommands.GetHeads(false);

            heads.Insert(0, GitHead.NoHead);

            Branch.DisplayMember = "Name";
            Branch.DataSource = heads;

            Settings.WorkingDir = realWorkingDir;
        }

        private void DirectoryTextUpdate(object sender, EventArgs e)
        {
            var path = Directory.Text;
            path = path.TrimEnd(new[] { Settings.PathSeperator, Settings.PathSeperatorWrong });

            if (path.EndsWith(".git"))
                path = path.Replace(".git", "");

            if (path.Contains(Settings.PathSeperator.ToString()) || path.Contains(Settings.PathSeperatorWrong.ToString()))
                LocalPath.Text = path.Substring(path.LastIndexOfAny(new[] { Settings.PathSeperator, Settings.PathSeperatorWrong }) + 1);
        }
    }
}