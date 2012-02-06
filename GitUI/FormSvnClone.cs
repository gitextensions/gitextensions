

namespace GitUI
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using GitCommands;
    using GitCommands.Repository;

    using ResourceManager.Translation;

    public partial class FormSvnClone : GitExtensionsForm
    {
        private readonly TranslationString _questionOpenRepo =
           new TranslationString("The repository has been cloned successfully." + Environment.NewLine +
                                 "Do you want to open the new repository \"{0}\" now?");
        
        private readonly TranslationString _questionOpenRepoCaption =
            new TranslationString("Open");

        private readonly TranslationString _questionContinueWithoutAuthors =
            new TranslationString("Authors file \"{0}\" does not exists. Continue without authors file?");

        private readonly TranslationString _questionContinueWithoutAuthorsCaption = new TranslationString("Authors file");

        public FormSvnClone()
        {
            InitializeComponent();
            this.Translate();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                var dirTo = this._NO_TRANSLATE_destinationComboBox.Text;
                if (!dirTo.EndsWith(Settings.PathSeparator.ToString()) && !dirTo.EndsWith(Settings.PathSeparatorWrong.ToString()))
                    dirTo += Settings.PathSeparator.ToString();

                dirTo += this._NO_TRANSLATE_subdirectoryTextBox.Text;

                //Repositories.RepositoryHistory.AddMostRecentRepository(_NO_TRANSLATE_From.Text);
                //Repositories.RepositoryHistory.AddMostRecentRepository(dirTo);

                if (!Directory.Exists(dirTo))
                    Directory.CreateDirectory(dirTo);

                var authorsfile = this._NO_TRANSLATE_authorsFileTextBox.Text;
                bool resetauthorsfile = false;
                if (authorsfile != null && authorsfile.Trim().Length != 0 && !File.Exists(authorsfile.Trim()) && !(resetauthorsfile = this.AskContinutWithoutAuthorsFile(authorsfile)))
                {
                    return;
                }
                if (resetauthorsfile)
                {
                    authorsfile = null;
                }
                var fromProcess = new FormProcess(
                    Settings.GitCommand, GitSvnCommandHelpers.CloneCmd(this._NO_TRANSLATE_svnRepositoryComboBox.Text, dirTo, authorsfile));
                
                fromProcess.ShowDialog(this);

                if (fromProcess.ErrorOccurred() || Settings.Module.InTheMiddleOfPatch())
                    return;
                if (ShowInTaskbar == false && AskIfNewRepositoryShouldBeOpened(dirTo))
                {
                    Settings.WorkingDir = dirTo;
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Exception: " + ex.Message, "Clone failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AskContinutWithoutAuthorsFile(string authorsfile)
        {
            return MessageBox.Show(
                this, string.Format(_questionContinueWithoutAuthors.Text, authorsfile), this._questionContinueWithoutAuthorsCaption.Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private bool AskIfNewRepositoryShouldBeOpened(string dirTo)
        {
            return MessageBox.Show(this, string.Format(_questionOpenRepo.Text, dirTo), _questionOpenRepoCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = this._NO_TRANSLATE_destinationComboBox.Text };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                this._NO_TRANSLATE_destinationComboBox.Text = dialog.SelectedPath;
        }

        private void authorsFileBrowseButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() { InitialDirectory = this._NO_TRANSLATE_destinationComboBox.Text };
            if (dialog.ShowDialog(this) == DialogResult.OK) 
                this._NO_TRANSLATE_authorsFileTextBox.Text = dialog.FileName;
        }

        private void destinationComboBox_DropDown(object sender, EventArgs e)
        {
            System.ComponentModel.BindingList<Repository> repos = Repositories.RepositoryHistory.Repositories;
            if (this._NO_TRANSLATE_destinationComboBox.Items.Count != repos.Count)
            {
                this._NO_TRANSLATE_destinationComboBox.Items.Clear();
                foreach (Repository repo in repos)
                    this._NO_TRANSLATE_destinationComboBox.Items.Add(repo.Path);
            }
        }
    }
}
