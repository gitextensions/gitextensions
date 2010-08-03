using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI
{
    public partial class FormClone : GitExtensionsForm
    {
        public FormClone()
        {
            InitializeComponent();
            Translate();

            if (Settings.ValidWorkingDir())
                _From.Text = Settings.WorkingDir;
            else
                _To.Text = Settings.WorkingDir;

            FromTextUpdate(null, null);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var dirTo = _To.Text;
                if (!dirTo.EndsWith("\\") && !dirTo.EndsWith("/"))
                    dirTo += "\\";

                dirTo += _NewDirectory.Text;

                Repositories.RepositoryHistory.AddMostRecentRepository(_From.Text);
                Repositories.RepositoryHistory.AddMostRecentRepository(dirTo);


                var fromProcess =
                    new FormProcess(Settings.GitCommand,
                                    GitCommands.GitCommands.CloneCmd(_From.Text, dirTo,
                                                                     CentralRepository.Checked, null));
                fromProcess.ShowDialog();

                if (fromProcess.ErrorOccured() || GitCommands.GitCommands.InTheMiddleOfPatch())
                    return;

                if (ShowInTaskbar == false && AskIfNewRepositoryShouldBeOpened(dirTo))
                {
                    Settings.WorkingDir = dirTo;

                    if (File.Exists(Settings.WorkingDir + ".gitmodules") && 
                        AskIfSubmodulesShouldBeInitialized())
                        InitSubmodules();
                }
                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static bool AskIfNewRepositoryShouldBeOpened(string dirTo)
        {
            return MessageBox.Show(
                "The repository has been cloned successfully." + Environment.NewLine +
                "Do you want to open the new repository \"" + dirTo + "\" now?", "Open",
                MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private static bool AskIfSubmodulesShouldBeInitialized()
        {
            return MessageBox.Show(
                "The cloned has submodules configured." + Environment.NewLine +
                "Do you want to initialize the submodules?" + Environment.NewLine +
                Environment.NewLine +
                "This will initialize and update all submodules recursive.", "Submodules",
                MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private static void InitSubmodules()
        {
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
            process.ShowDialog();
            InitializeSubmodulesRecursive();
        }

        private static void InitializeSubmodulesRecursive()
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (string.IsNullOrEmpty(submodule.LocalPath))
                    continue;

                Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                if (Settings.WorkingDir != oldworkingdir &&
                    File.Exists(Settings.WorkingDir + ".gitmodules"))
                    InitSubmodules();

                Settings.WorkingDir = oldworkingdir;
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private void FromBrowseClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog {SelectedPath = _From.Text};
            if (dialog.ShowDialog() == DialogResult.OK)
                _From.Text = dialog.SelectedPath;

            ToTextUpdate(sender, e);
        }

        private void ToBrowseClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog {SelectedPath = _To.Text};
            if (dialog.ShowDialog() == DialogResult.OK)
                _To.Text = dialog.SelectedPath;

            ToTextUpdate(sender, e);
        }

        private void FromDropDown(object sender, EventArgs e)
        {
            _From.DataSource = Repositories.RepositoryHistory.Repositories;
            _From.DisplayMember = "Path";
        }

        private void ToDropDown(object sender, EventArgs e)
        {
            _To.DataSource = Repositories.RepositoryHistory.Repositories;
            _To.DisplayMember = "Path";
        }


        private static void LoadSshKeyClick(object sender, EventArgs e)
        {
            new FormLoadPuttySSHKey().ShowDialog();
        }

        private void FormCloneLoad(object sender, EventArgs e)
        {
            if (!GitCommands.GitCommands.Plink())
                LoadSSHKey.Visible = false;
        }


        private void FromSelectedIndexChanged(object sender, EventArgs e)
        {
            FromTextUpdate(sender, e);
        }

        private void FromTextUpdate(object sender, EventArgs e)
        {
            var path = _From.Text;
            path = path.TrimEnd(new[] {'\\', '/'});

            if (path.EndsWith(".git"))
                path = path.Replace(".git", "");

            if (path.Contains("\\") || path.Contains("/"))
                _NewDirectory.Text = path.Substring(path.LastIndexOfAny(new[] {'\\', '/'}) + 1);

            ToTextUpdate(sender, e);
        }

        private void ToTextUpdate(object sender, EventArgs e)
        {
            var destinationPath = "";

            Info.Text = "The repository will be cloned to a new directory located here:" + Environment.NewLine;
            if (string.IsNullOrEmpty(_To.Text))
                destinationPath += "[destination]";
            else
                destinationPath += _To.Text.TrimEnd(new[] {'\\', '/'});
            ;
            destinationPath += "\\";

            if (string.IsNullOrEmpty(_NewDirectory.Text))
                destinationPath += "[directory]";
            else
                destinationPath += _NewDirectory.Text;

            Info.Text += "     " + destinationPath;

            if (destinationPath.Contains("[") || destinationPath.Contains("]"))
            {
                Info.ForeColor = Color.Red;
                return;
            }

            if (Directory.Exists(destinationPath))
            {
                Info.Text += " (directory already exists)";
                Info.ForeColor = Color.Red;
            }
            else
                Info.ForeColor = Color.Black;
        }

        private void NewDirectoryTextChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private void ToSelectedIndexChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }
    }
}