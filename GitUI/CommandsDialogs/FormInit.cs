using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager;

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
            new TranslationString("Create new repository");

        private readonly EventHandler<GitModuleEventArgs> _gitModuleChanged;

        public FormInit(string dir, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            _gitModuleChanged = gitModuleChanged;
            InitializeComponent();
            Translate();

            Directory.Text = string.IsNullOrEmpty(dir)
                ? AppSettings.DefaultCloneDestinationPath
                : dir;
        }

        private void DirectoryDropDown(object sender, EventArgs e)
        {
            Directory.DataSource = RepositoryManager.RepositoryHistory.Repositories;
            Directory.DisplayMember = nameof(Repository.Path);
        }

        private void InitClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text))
            {
                MessageBox.Show(this, _chooseDirectory.Text, _chooseDirectoryCaption.Text);
                return;
            }

            if (File.Exists(Directory.Text))
            {
                MessageBox.Show(this, _chooseDirectoryNotFile.Text, _chooseDirectoryNotFileCaption.Text);
                return;
            }

            GitModule module = new GitModule(Directory.Text);

            if (!System.IO.Directory.Exists(module.WorkingDir))
            {
                System.IO.Directory.CreateDirectory(module.WorkingDir);
            }

            MessageBox.Show(this, module.Init(Central.Checked, Central.Checked), _initMsgBoxCaption.Text);

            _gitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));

            RepositoryManager.AddMostRecentRepository(Directory.Text);

            Close();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this);

            if (userSelectedPath != null)
            {
                Directory.Text = userSelectedPath;
            }
        }
    }
}