using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormClone : GitExtensionsForm
    {
        private readonly TranslationString _infoNewRepositoryLocation = 
            new TranslationString("The repository will be cloned to a new directory located here:"  + Environment.NewLine +
                                  "{0}");

        private readonly TranslationString _infoDirectoryExists =
            new TranslationString("(Directory already exists)");

        private readonly TranslationString _infoDirectoryNew =
            new TranslationString("(New directory)");


        private readonly TranslationString _questionOpenRepo =
            new TranslationString("The repository has been cloned successfully." + Environment.NewLine +
                                  "Do you want to open the new repository \"{0}\" now?");
        
        private readonly TranslationString _questionOpenRepoCaption = 
            new TranslationString("Open");

        private readonly TranslationString _questionInitSubmodules =
            new TranslationString("The cloned has submodules configured." + Environment.NewLine +
                                  "Do you want to initialize the submodules?" + Environment.NewLine +
                                  "This will initialize and update all submodules recursive.");
        
        private readonly TranslationString _questionInitSubmodulesCaption =
            new TranslationString("Submodules");


        public FormClone(string url = null)
        {
            InitializeComponent();
            Translate();

            if (url != null)
            {
                _NO_TRANSLATE_From.Text = url;
                if (!Settings.Module.ValidWorkingDir())
                    _NO_TRANSLATE_To.Text = Settings.WorkingDir;
            }
            else
            {
                if (Settings.Module.ValidWorkingDir())
                    _NO_TRANSLATE_From.Text = Settings.WorkingDir;
                else
                    _NO_TRANSLATE_To.Text = Settings.WorkingDir;
            }

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
                    new FormRemoteProcess(Settings.GitCommand,
                                    GitModule.CloneCmd(_NO_TRANSLATE_From.Text, dirTo,
                                                                     CentralRepository.Checked, Branches.Text, null));
                fromProcess.SetUrlTryingToConnect(_NO_TRANSLATE_From.Text);
                fromProcess.ShowDialog();

                if (fromProcess.ErrorOccurred() || Settings.Module.InTheMiddleOfPatch())
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
                MessageBox.Show(this, "Exception: " + ex.Message, "Clone failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AskIfNewRepositoryShouldBeOpened(string dirTo)
        {
            return MessageBox.Show(string.Format(_questionOpenRepo.Text, dirTo), _questionOpenRepoCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private bool AskIfSubmodulesShouldBeInitialized()
        {
            return MessageBox.Show(_questionInitSubmodules.Text, _questionInitSubmodulesCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
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

            const string standardRepositorySuffix = ".git";

            if (path.EndsWith(standardRepositorySuffix))
                path = path.Substring(0, path.Length - standardRepositorySuffix.Length);

            if (path.Contains("\\") || path.Contains("/"))
                _NO_TRANSLATE_NewDirectory.Text = path.Substring(path.LastIndexOfAny(new[] { '\\', '/' }) + 1);

            ToTextUpdate(sender, e);
        }

        private void ToTextUpdate(object sender, EventArgs e)
        {
            string destinationPath = string.Empty;                

            if (string.IsNullOrEmpty(_NO_TRANSLATE_To.Text))
                destinationPath += "[" + label2.Text + "]";
            else
                destinationPath += _NO_TRANSLATE_To.Text.TrimEnd(new[] { '\\', '/' });

            destinationPath += "\\";
            
            if (string.IsNullOrEmpty(_NO_TRANSLATE_NewDirectory.Text))
                destinationPath += "[" + label3.Text + "]";
            else
                destinationPath += _NO_TRANSLATE_NewDirectory.Text;

            Info.Text = string.Format(_infoNewRepositoryLocation.Text, destinationPath);

            if (destinationPath.Contains("[") || destinationPath.Contains("]"))
            {
                Info.ForeColor = Color.Red;
                return;
            }

            if (Directory.Exists(destinationPath))
            {
                if (Directory.GetDirectories(destinationPath).Length > 0 || Directory.GetFiles(destinationPath).Length > 0)
                {
                    Info.Text += " " + _infoDirectoryExists.Text;
                    Info.ForeColor = Color.Red;
                }
                else
                {
                    Info.ForeColor = Color.Black;
                }
            }
            else
            {
                Info.Text += " " + _infoDirectoryNew.Text;
                Info.ForeColor = Color.Black;
            }
        }

        private void NewDirectoryTextChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private void ToSelectedIndexChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private void Branches_DropDown(object sender, EventArgs e)
        {
            Branches.DisplayMember = "LocalName";
            Branches.DataSource = Settings.Module.GetRemoteHeads(_NO_TRANSLATE_From.Text, false, true);
        }
    }
}
