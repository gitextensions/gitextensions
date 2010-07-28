using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormInit : GitExtensionsForm
    {
        private readonly TranslationString _chooseDirectory =
            new TranslationString("Please choose a directory.");

        private readonly TranslationString _chooseDirectoryNotFile =
            new TranslationString("Cannot initialize a new repository on a file.\nPlease choose a directory.");

        public FormInit(string dir)
        {
            InitializeComponent();
            Translate();
            Directory.Text = dir;
        }

        public FormInit()
        {
            InitializeComponent();
            Translate();

            if (!Settings.ValidWorkingDir())
                Directory.Text = Settings.WorkingDir;
        }

        private void DirectoryDropDown(object sender, EventArgs e)
        {
            Directory.DataSource = Repositories.RepositoryHistory.Repositories;
            Directory.DisplayMember = "Path";
        }

        private void InitClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text))
            {
                MessageBox.Show(_chooseDirectory.Text);
                return;
            }

            if (File.Exists(Directory.Text))
            {
                MessageBox.Show(_chooseDirectoryNotFile.Text);
                return;
            }

            Settings.WorkingDir = Directory.Text;

            if (!System.IO.Directory.Exists(Settings.WorkingDir))
                System.IO.Directory.CreateDirectory(Settings.WorkingDir);

            MessageBox.Show(GitCommands.GitCommands.Init(Central.Checked, Central.Checked), "Initialize new repository");

            Repositories.RepositoryHistory.AddMostRecentRepository(Directory.Text);

            Close();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
                Directory.Text = browseDialog.SelectedPath;
        }
    }
}