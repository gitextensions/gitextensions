using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormGitAttributes : GitModuleForm
    {
        private readonly TranslationString _noWorkingDir =
            new TranslationString(".gitattributes is only supported when there is a working directory.");
        private readonly TranslationString _noWorkingDirCaption =
            new TranslationString("No working directory");

        private readonly TranslationString _cannotAccessGitattributes =
            new TranslationString("Failed to save .gitattributes." + Environment.NewLine + "Check if file is accessible.");
        private readonly TranslationString _cannotAccessGitattributesCaption =
            new TranslationString("Failed to save .gitattributes");

        private readonly TranslationString _saveFileQuestion =
            new TranslationString("Save changes to .gitattributes?");
        private readonly TranslationString _saveFileQuestionCaption =
            new TranslationString("Save changes?");

        public string GitAttributesFile = string.Empty;
        private readonly IFullPathResolver _fullPathResolver;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormGitAttributes()
        {
            InitializeComponent();
        }

        public FormGitAttributes(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);
            LoadFile();
            _NO_TRANSLATE_GitAttributesText.TextLoaded += GitAttributesFileLoaded;
        }

        private void LoadFile()
        {
            try
            {
                var path = _fullPathResolver.Resolve(".gitattributes");
                if (File.Exists(path))
                {
                    _NO_TRANSLATE_GitAttributesText.ViewFileAsync(path);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void SaveClick(object sender, EventArgs e)
        {
            SaveFile();
            Close();
        }

        private bool SaveFile()
        {
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(
                        _fullPathResolver.Resolve(".gitattributes"),
                        x =>
                        {
                            GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();
                            if (!GitAttributesFile.EndsWith(Environment.NewLine))
                            {
                                GitAttributesFile += Environment.NewLine;
                            }

                            File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(GitAttributesFile));
                        });

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotAccessGitattributes.Text + Environment.NewLine + ex.Message,
                    _cannotAccessGitattributesCaption.Text);
                return false;
            }
        }

        private void FormGitAttributesClosing(object sender, FormClosingEventArgs e)
        {
            var needToClose = false;

            if (!IsFileUpToDate())
            {
                switch (MessageBox.Show(this, _saveFileQuestion.Text, _saveFileQuestionCaption.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        if (SaveFile())
                        {
                            needToClose = true;
                        }

                        break;
                    case DialogResult.No:
                        needToClose = true;
                        break;
                }
            }
            else
            {
                needToClose = true;
            }

            if (!needToClose)
            {
                e.Cancel = true;
            }
        }

        private void FormGitAttributesLoad(object sender, EventArgs e)
        {
            if (!Module.IsBareRepository())
            {
                return;
            }

            MessageBox.Show(this, _noWorkingDir.Text, _noWorkingDirCaption.Text);
            Close();
        }

        private bool IsFileUpToDate()
        {
            return GitAttributesFile == _NO_TRANSLATE_GitAttributesText.GetText();
        }

        private void GitAttributesFileLoaded(object sender, EventArgs e)
        {
            GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();
        }
    }
}
