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
                _NO_TRANSLATE_From.Text = Settings.WorkingDir;
            else
                _NO_TRANSLATE_To.Text = Settings.WorkingDir;

            FromTextUpdate(null, null);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var dirTo = _NO_TRANSLATE_To.Text;
                if (!dirTo.EndsWith(Settings.PathSeparator.ToString()) && !dirTo.EndsWith(Settings.PathSeparatorWrong.ToString()))
                    dirTo += Settings.PathSeparator.ToString();

                dirTo += _NO_TRANSLATE_NewDirectory.Text;

                Repositories.RepositoryHistory.AddMostRecentRepository(_NO_TRANSLATE_From.Text);
                Repositories.RepositoryHistory.AddMostRecentRepository(dirTo);


                var fromProcess =
                    new FormProcess(Settings.GitCommand,
                                    GitCommandHelpers.CloneCmd(_NO_TRANSLATE_From.Text, dirTo,
                                                                     CentralRepository.Checked, null));
                fromProcess.ShowDialog();

                if (fromProcess.ErrorOccurred() || GitCommandHelpers.InTheMiddleOfPatch())
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
                MessageBox.Show(this, "Exception: " + ex.Message, "Clone failed");
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
            var process = new FormProcess(GitCommandHelpers.SubmoduleInitCmd(""));
            process.ShowDialog();
            InitializeSubmodulesRecursive();
        }

        private static void InitializeSubmodulesRecursive()
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommandsInstance()).GetSubmodules())
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
            var dialog = new FolderBrowserDialog { SelectedPath = _NO_TRANSLATE_From.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                _NO_TRANSLATE_From.Text = dialog.SelectedPath;

            FromTextUpdate(sender, e);
        }

        private void ToBrowseClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = _NO_TRANSLATE_To.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                _NO_TRANSLATE_To.Text = dialog.SelectedPath;

            ToTextUpdate(sender, e);
        }

        private void FromDropDown(object sender, EventArgs e)
        {          
            System.ComponentModel.BindingList<Repository> repos = Repositories.RepositoryHistory.Repositories;
            if (_NO_TRANSLATE_From.Items.Count != repos.Count) 
            {
                _NO_TRANSLATE_To.Items.Clear();
                foreach (Repository repo in repos)
                    _NO_TRANSLATE_From.Items.Add(repo.Path);
            }
        }

        private void ToDropDown(object sender, EventArgs e)
        {
            System.ComponentModel.BindingList<Repository> repos = Repositories.RepositoryHistory.Repositories;
            if (_NO_TRANSLATE_To.Items.Count != repos.Count)
            {
                _NO_TRANSLATE_To.Items.Clear();
                foreach (Repository repo in repos)
                    _NO_TRANSLATE_To.Items.Add(repo.Path);
            }
        }


        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            new FormLoadPuttySshKey().ShowDialog();
        }

        private void FormCloneLoad(object sender, EventArgs e)
        {
            if (!GitCommandHelpers.Plink())
                LoadSSHKey.Visible = false;
        }


        private void FromSelectedIndexChanged(object sender, EventArgs e)
        {
            FromTextUpdate(sender, e);
        }

        private void FromTextUpdate(object sender, EventArgs e)
        {
            var path = _NO_TRANSLATE_From.Text;
            path = path.TrimEnd(new[] { '\\', '/' });

            if (path.EndsWith(".git"))
                path = path.Replace(".git", "");

            if (path.Contains("\\") || path.Contains("/"))
                _NO_TRANSLATE_NewDirectory.Text = path.Substring(path.LastIndexOfAny(new[] { '\\', '/' }) + 1);

            ToTextUpdate(sender, e);
        }

        private void ToTextUpdate(object sender, EventArgs e)
        {
            var destinationPath = "";

            Info.Text = "The repository will be cloned to a new directory located here:" + Environment.NewLine;
            if (string.IsNullOrEmpty(_NO_TRANSLATE_To.Text))
                destinationPath += "[destination]";
            else
                destinationPath += _NO_TRANSLATE_To.Text.TrimEnd(new[] { '\\', '/' });
            ;
            destinationPath += "\\";

            if (string.IsNullOrEmpty(_NO_TRANSLATE_NewDirectory.Text))
                destinationPath += "[directory]";
            else
                destinationPath += _NO_TRANSLATE_NewDirectory.Text;

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