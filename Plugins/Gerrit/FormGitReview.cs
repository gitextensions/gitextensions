using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace Gerrit
{
    public sealed partial class FormGitReview : GitExtensionsForm, IGitUICommandsSource
    {
        private readonly TranslationString _gitreviewOnlyInWorkingDirSupported =
            new TranslationString(".gitreview is only supported when there is a working dir.");
        private readonly TranslationString _gitreviewOnlyInWorkingDirSupportedCaption =
            new TranslationString("No working dir");

        private readonly TranslationString _cannotAccessGitreview =
            new TranslationString("Failed to save .gitreview." + Environment.NewLine + "Check if file is accessible.");
        private readonly TranslationString _cannotAccessGitreviewCaption =
            new TranslationString("Failed to save .gitreview");

        private readonly TranslationString _saveFileQuestion =
            new TranslationString("Save changes to .gitreview?");
        private readonly TranslationString _saveFileQuestionCaption =
            new TranslationString("Save changes?");

        private string _originalGitReviewFileContent = string.Empty;
        private IGitModule Module { get { return UICommands.GitModule; } }

        public event GitUICommandsChangedEventHandler GitUICommandsChanged;

        private void OnGitUICommandsChanged(GitUICommands oldcommands)
        {
            GitUICommandsChangedEventHandler handler = GitUICommandsChanged;
            if (handler != null)
                handler(this, oldcommands);
        }

        private GitUICommands _uiCommands;
        public GitUICommands UICommands
        {
            get { return _uiCommands; }
            set
            {
                var oldcommands = _uiCommands;
                _uiCommands = value;
                OnGitUICommandsChanged(oldcommands);
            }
        }

        public FormGitReview(IGitUICommands aUICommands)
            : base(true)
        {
            InitializeComponent();
            Translate();

            UICommands = (GitUICommands)aUICommands;
            if (UICommands != null)
            {
                LoadGitReview();
                _NO_TRANSLATE_GitReviewEdit.TextLoaded += GitReviewFileLoaded;
            }
        }

        private void LoadGitReview()
        {
            try
            {
                if (File.Exists(Module.GitWorkingDir + ".gitreview"))
                    _NO_TRANSLATE_GitReviewEdit.ViewFile(Module.GitWorkingDir + ".gitreview");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void SaveClick(object sender, EventArgs e)
        {
            SaveGitReview();
            Close();
        }

        private bool SaveGitReview()
        {
            if (!HasUnsavedChanges())
                return false;
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(
                        Module.GitWorkingDir + ".gitreview",
                        x =>
                        {
                            var fileContent = _NO_TRANSLATE_GitReviewEdit.GetText();
                            if (!fileContent.EndsWith(Environment.NewLine))
                                fileContent += Environment.NewLine;
                            File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(fileContent));
                            _originalGitReviewFileContent = fileContent;
                        });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotAccessGitreview.Text + Environment.NewLine + ex.Message,
                    _cannotAccessGitreviewCaption.Text);
                return false;
            }
        }

        private void FormGitReviewFormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                switch (MessageBox.Show(this, _saveFileQuestion.Text, _saveFileQuestionCaption.Text,
                                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        if (!SaveGitReview())
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                }
            }
        }

        private void FormGitIgnoreLoad(object sender, EventArgs e)
        {
            if (!Module.IsBareRepository())
                return;
            MessageBox.Show(this, _gitreviewOnlyInWorkingDirSupported.Text, _gitreviewOnlyInWorkingDirSupportedCaption.Text);
            Close();
        }

        private bool HasUnsavedChanges()
        {
            return _originalGitReviewFileContent != _NO_TRANSLATE_GitReviewEdit.GetText();
        }

        private void GitReviewFileLoaded(object sender, EventArgs e)
        {
            _originalGitReviewFileContent = _NO_TRANSLATE_GitReviewEdit.GetText();
        }

        private void lnkGitReviewPatterns_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://github.com/openstack-infra/git-review#git-review");
        }
    }
}