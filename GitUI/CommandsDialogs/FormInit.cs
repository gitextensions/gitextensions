using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public partial class FormInit : GitExtensionsForm
    {
        private readonly TranslationString _chooseDirectory =
            new TranslationString("Please choose a directory.");

        private readonly TranslationString _chooseDirectoryCaption =
            new TranslationString("Choose directory");

        private readonly TranslationString _chooseDirectoryNotFile =
            new TranslationString("Cannot initialize a new repository on a file.\nPlease choose a directory.");

        private readonly TranslationString _chooseDirectoryNotFileCaption =
            new TranslationString("Error");

        private readonly TranslationString _initMsgBoxCaption =
            new TranslationString("Initialize new repository");

        private readonly GitModuleChangedEventHandler GitModuleChanged;

        public FormInit(string dir, GitModuleChangedEventHandler GitModuleChanged)
        {
            this.GitModuleChanged = GitModuleChanged;
            InitializeComponent();
            Translate();

            if (string.IsNullOrEmpty(dir))
            {
                Directory.Text = AppSettings.DefaultCloneDestinationPath;
            }
            else
            {
                Directory.Text = dir;
            }
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
                MessageBox.Show(this, _chooseDirectory.Text,_chooseDirectoryCaption.Text);
                return;
            }

            if (File.Exists(Directory.Text))
            {
                MessageBox.Show(this, _chooseDirectoryNotFile.Text,_chooseDirectoryNotFileCaption.Text);
                return;
            }

            GitModule module = new GitModule(Directory.Text);

            if (!System.IO.Directory.Exists(module.WorkingDir))
                System.IO.Directory.CreateDirectory(module.WorkingDir);

            MessageBox.Show(this, module.Init(Central.Checked, Central.Checked), _initMsgBoxCaption.Text);

            if (GitModuleChanged != null)
                GitModuleChanged(module);

            Repositories.AddMostRecentRepository(Directory.Text);

            Close();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            using (var browseDialog = new FolderBrowserDialog())
            {

                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                    Directory.Text = browseDialog.SelectedPath;
            }
        }
    }
}