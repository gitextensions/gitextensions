using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.GitIgnoreDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormGitIgnore : GitModuleForm
    {
        private readonly TranslationString _gitignoreOnlyInWorkingDirSupportedCaption =
            new TranslationString("No working directory");

        private readonly TranslationString _saveFileQuestionCaption =
            new TranslationString("Save changes?");

        private readonly bool _localExclude;
        private string _originalGitIgnoreFileContent = string.Empty;

        #region default patterns

        private static readonly string DefaultIgnorePatternsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitExtensions/DefaultIgnorePatterns.txt");

        private static readonly string[] DefaultIgnorePatterns =
        {
            "#Ignore thumbnails created by Windows",
            "Thumbs.db",
            "#Ignore files built by Visual Studio",
            "*.obj",
            "*.exe",
            "*.pdb",
            "*.user",
            "*.aps",
            "*.pch",
            "*.vspscc",
            "*_i.c",
            "*_p.c",
            "*.ncb",
            "*.suo",
            "*.tlb",
            "*.tlh",
            "*.bak",
            "*.cache",
            "*.ilk",
            "*.log",
            "[Bb]in",
            "[Dd]ebug*/",
            "*.lib",
            "*.sbr",
            "obj/",
            "[Rr]elease*/",
            "_ReSharper*/",
            "[Tt]est[Rr]esult*",
            ".vs/",
            "#Nuget packages folder",
            "packages/"
        };

        #endregion

        private readonly IGitIgnoreDialogModel _dialogModel;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormGitIgnore()
        {
            InitializeComponent();
        }

        public FormGitIgnore(GitUICommands commands, bool localExclude)
            : base(commands)
        {
            _localExclude = localExclude;
            InitializeComponent();
            InitializeComplete();

            _dialogModel = CreateGitIgnoreDialogModel(localExclude);

            Text = _dialogModel.FormCaption;
        }

        private IGitIgnoreDialogModel CreateGitIgnoreDialogModel(bool localExclude)
        {
            if (localExclude)
            {
                return new GitLocalExcludeModel(Module);
            }

            return new GitIgnoreModel(Module);
        }

        private string ExcludeFile => _dialogModel.ExcludeFile;

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);
            LoadGitIgnore();
            _NO_TRANSLATE_GitIgnoreEdit.TextLoaded += GitIgnoreFileLoaded;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void LoadGitIgnore()
        {
            try
            {
                if (File.Exists(ExcludeFile))
                {
                    _NO_TRANSLATE_GitIgnoreEdit.ViewFileAsync(ExcludeFile);
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
            if (!HasUnsavedChanges())
            {
                return false;
            }

            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(
                        ExcludeFile,
                        x =>
                        {
                            var fileContent = _NO_TRANSLATE_GitIgnoreEdit.GetText();
                            if (!fileContent.EndsWith(Environment.NewLine))
                            {
                                fileContent += Environment.NewLine;
                            }

                            File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(fileContent));
                            _originalGitIgnoreFileContent = fileContent;
                        });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _dialogModel.CannotAccessFile + Environment.NewLine + ex.Message,
                    _dialogModel.CannotAccessFileCaption);
                return false;
            }
        }

        private void FormGitIgnoreFormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                switch (MessageBox.Show(this, _dialogModel.SaveFileQuestion, _saveFileQuestionCaption.Text,
                                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        if (!SaveGitIgnore())
                        {
                            e.Cancel = true;
                        }

                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void FormGitIgnoreLoad(object sender, EventArgs e)
        {
            if (!Module.IsBareRepository())
            {
                return;
            }

            MessageBox.Show(this, _dialogModel.FileOnlyInWorkingDirSupported, _gitignoreOnlyInWorkingDirSupportedCaption.Text);
            Close();
        }

        private void AddDefaultClick(object sender, EventArgs e)
        {
            var defaultIgnorePatterns = File.Exists(DefaultIgnorePatternsFile) ? File.ReadAllLines(DefaultIgnorePatternsFile) : DefaultIgnorePatterns;

            var currentFileContent = _NO_TRANSLATE_GitIgnoreEdit.GetText();
            var patternsToAdd = defaultIgnorePatterns
                .Except(currentFileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                .ToArray();
            if (patternsToAdd.Length == 0)
            {
                return;
            }

            // workaround to prevent GitIgnoreFileLoaded event handling (it causes wrong _originalGitIgnoreFileContent update)
            // TODO: implement in FileViewer separate events for loading text from file and for setting text directly via ViewText
            _NO_TRANSLATE_GitIgnoreEdit.TextLoaded -= GitIgnoreFileLoaded;
            ThreadHelper.JoinableTaskFactory.RunAsync(
                () => _NO_TRANSLATE_GitIgnoreEdit.ViewTextAsync(
                    ExcludeFile,
                    currentFileContent + Environment.NewLine +
                    string.Join(Environment.NewLine, patternsToAdd) + Environment.NewLine + string.Empty));
            _NO_TRANSLATE_GitIgnoreEdit.TextLoaded += GitIgnoreFileLoaded;
        }

        private void AddPattern_Click(object sender, EventArgs e)
        {
            SaveGitIgnore();
            UICommands.StartAddToGitIgnoreDialog(this, _localExclude, "*.dll");
            LoadGitIgnore();
        }

        private bool HasUnsavedChanges()
        {
            return _originalGitIgnoreFileContent != _NO_TRANSLATE_GitIgnoreEdit.GetText();
        }

        private void GitIgnoreFileLoaded(object sender, EventArgs e)
        {
            _originalGitIgnoreFileContent = _NO_TRANSLATE_GitIgnoreEdit.GetText();
        }

        private void lnkGitIgnorePatterns_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/github/gitignore");
        }

        private void lnkGitIgnoreGenerate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://www.gitignore.io/");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}