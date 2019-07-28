using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
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
            InitializeComplete();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                Directory.DataSource = repositoryHistory;
                Directory.DisplayMember = nameof(Repository.Path);
            });

            Directory.SelectedIndex = -1;
            Directory.Text = string.IsNullOrEmpty(dir) ? AppSettings.DefaultCloneDestinationPath : dir;
        }

        private void InitClick(object sender, EventArgs e)
        {
            var directoryPath = Directory.Text;

            if (!IsRootedDirectoryPath(directoryPath))
            {
                MessageBox.Show(this, _chooseDirectory.Text, _chooseDirectoryCaption.Text);
                return;
            }

            if (File.Exists(directoryPath))
            {
                MessageBox.Show(this, _chooseDirectoryNotFile.Text, _chooseDirectoryNotFileCaption.Text);
                return;
            }

            var module = new GitModule(directoryPath);

            if (!System.IO.Directory.Exists(module.WorkingDir))
            {
                System.IO.Directory.CreateDirectory(module.WorkingDir);
            }

            MessageBox.Show(this, module.Init(Central.Checked, Central.Checked), _initMsgBoxCaption.Text);

            _gitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));

            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(directoryPath));
            Close();
        }

        private static bool IsRootedDirectoryPath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return false;
                }

                // this is going to throw if it's an invalid path (e.g. contains special chars)
                var info = new DirectoryInfo(path);

                return Path.IsPathRooted(path.Trim());
            }
            catch (Exception)
            {
                // The code in the try block is expected to throw when the input is not a valid directory path
                // OR when the user does not have the required permission.
                // In both cases we return "false" since the path is not representing a valid "usable" directory.
                // This is also the reason why we are catching all kind of exception here and not IO-related ones.
                return false;
            }
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this);

            if (userSelectedPath != null)
            {
                Directory.Text = userSelectedPath;
            }
        }

        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly FormInit _form;

            public TestAccessor(FormInit form)
            {
                _form = form;
            }

            public ComboBox DirectoryCombo => _form.Directory;

            public bool IsRootedDirectoryPath(string path)
            {
                return FormInit.IsRootedDirectoryPath(path);
            }
        }
    }
}