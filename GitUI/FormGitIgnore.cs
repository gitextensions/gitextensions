using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor.Util;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormGitIgnore : GitExtensionsForm
    {
        private readonly TranslationString _gitignoreOnlyInWorkingDirSupported =
            new TranslationString(".gitignore is only supported when there is a working dir.");
        private readonly TranslationString _gitignoreOnlyInWorkingDirSupportedCaption =
            new TranslationString("No working dir");

        private readonly TranslationString _cannotAccessGitignore =
            new TranslationString("Failed to save .gitignore." + Environment.NewLine + "Check if file is accessible.");
        private readonly TranslationString _cannotAccessGitignoreCaption =
            new TranslationString("Failed to save .gitignore");

        private readonly TranslationString _saveFileQuestion =
            new TranslationString("Save changes to .gitignore?");
        private readonly TranslationString _saveFileQuestionCaption =
            new TranslationString("Save changes?");

        public string GitIgnoreFile = string.Empty;

        public FormGitIgnore()
        {
            InitializeComponent();
            Translate();

            LoadGitIgnore();
            _NO_TRANSLATE_GitIgnoreEdit.TextLoaded += GitIgnoreFileLoaded;
        }

        private void LoadGitIgnore()
        {
            try
            {
                if (File.Exists(Settings.WorkingDir + ".gitignore"))
                {
                    _NO_TRANSLATE_GitIgnoreEdit.ViewFile(Settings.WorkingDir + ".gitignore");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void SaveClick(object sender, EventArgs e)
        {
            SaveGitIgnore();
            Close();
        }

        private bool SaveGitIgnore()
        {
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(
                        Settings.WorkingDir + ".gitignore",
                        x =>
                        {
                            this.GitIgnoreFile = _NO_TRANSLATE_GitIgnoreEdit.GetText();
                            if (!this.GitIgnoreFile.EndsWith(Environment.NewLine))
                                this.GitIgnoreFile += Environment.NewLine;
                            File.WriteAllBytes(x, Settings.SystemEncoding.GetBytes(this.GitIgnoreFile));    
                        });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotAccessGitignore.Text + Environment.NewLine + ex.Message, 
                    _cannotAccessGitignoreCaption.Text);
                return false;
            }
        }

        private void FormGitIgnoreFormClosing(object sender, FormClosingEventArgs e)
        {
            var needToClose = false;

            if (!IsFileUpToDate())
            {
                switch (MessageBox.Show(this, _saveFileQuestion.Text, _saveFileQuestionCaption.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        if (SaveGitIgnore())
                            needToClose = true;
                        break;
                    case DialogResult.No:
                        needToClose = true;
                        break;
                    default:
                        break;
                }
            }
            else
                needToClose = true;

            if (!needToClose)
                e.Cancel = true;
            else
                SavePosition("edit-git-ignore");
        }

        private void FormGitIgnoreLoad(object sender, EventArgs e)
        {
            RestorePosition("edit-git-ignore");
            if (!Settings.Module.IsBareRepository()) return;
            MessageBox.Show(this, _gitignoreOnlyInWorkingDirSupported.Text, _gitignoreOnlyInWorkingDirSupportedCaption.Text);
            Close();
        }

        private void AddDefaultClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_GitIgnoreEdit.ViewText(".gitignore",
                _NO_TRANSLATE_GitIgnoreEdit.GetText() +
                Environment.NewLine + "#ignore thumbnails created by windows" +
                Environment.NewLine + "Thumbs.db" +
                Environment.NewLine + "#Ignore files build by Visual Studio" +
                Environment.NewLine + "*.obj" +
                Environment.NewLine + "*.exe" +
                Environment.NewLine + "*.pdb" +
                Environment.NewLine + "*.user" +
                Environment.NewLine + "*.aps" +
                Environment.NewLine + "*.pch" +
                Environment.NewLine + "*.vspscc" +
                Environment.NewLine + "*_i.c" +
                Environment.NewLine + "*_p.c" +
                Environment.NewLine + "*.ncb" +
                Environment.NewLine + "*.suo" +
                Environment.NewLine + "*.tlb" +
                Environment.NewLine + "*.tlh" +
                Environment.NewLine + "*.bak" +
                Environment.NewLine + "*.cache" +
                Environment.NewLine + "*.ilk" +
                Environment.NewLine + "*.log" +
                Environment.NewLine + "[Bb]in" +
                Environment.NewLine + "[Dd]ebug*/" +
                Environment.NewLine + "*.lib" +
                Environment.NewLine + "*.sbr" +
                Environment.NewLine + "obj/" +
                Environment.NewLine + "[Rr]elease*/" +
                Environment.NewLine + "_ReSharper*/" +
                Environment.NewLine + "[Tt]est[Rr]esult*" +
                Environment.NewLine + "");
        }

        private void AddPattern_Click(object sender, EventArgs e)
        {
            SaveGitIgnore();
            new FormAddToGitIgnore("*.dll").ShowDialog(this);
            LoadGitIgnore();
        }

        private bool IsFileUpToDate()
        {
            return GitIgnoreFile == _NO_TRANSLATE_GitIgnoreEdit.GetText();
        }

        private void GitIgnoreFileLoaded(object sender, EventArgs e)
        {
            GitIgnoreFile = _NO_TRANSLATE_GitIgnoreEdit.GetText();
        }



    }
}