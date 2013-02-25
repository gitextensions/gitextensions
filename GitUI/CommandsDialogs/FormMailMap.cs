using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public partial class FormMailMap : GitModuleForm
    {
        private readonly TranslationString _mailmapOnlyInWorkingDirSupported =
            new TranslationString(".mailmap is only supported when there is a working dir.");
        private readonly TranslationString _mailmapOnlyInWorkingDirSupportedCaption =
            new TranslationString("No working dir");

        private readonly TranslationString _cannotAccessMailmap =
            new TranslationString("Failed to save .mailmap." + Environment.NewLine + "Check if file is accessible.");
        private readonly TranslationString _cannotAccessMailmapCaption =
            new TranslationString("Failed to save .mailmap");

        private readonly TranslationString _saveFileQuestion =
            new TranslationString("Save changes to .mailmap?");
        private readonly TranslationString _saveFileQuestionCaption =
            new TranslationString("Save changes?");
        

        public string MailMapFile = string.Empty;

        public FormMailMap(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);
            LoadFile();
            _NO_TRANSLATE_MailMapText.TextLoaded += MailMapFileLoaded;
        }

        private void LoadFile()
        {
            try
            {
                if (File.Exists(Module.WorkingDir + ".mailmap"))
                {
                    _NO_TRANSLATE_MailMapText.ViewFile(Module.WorkingDir + ".mailmap");
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
                        Module.WorkingDir + ".mailmap",
                        x =>
                        {
                            this.MailMapFile = _NO_TRANSLATE_MailMapText.GetText();
                            if (!this.MailMapFile.EndsWith(Environment.NewLine))
                                this.MailMapFile += Environment.NewLine;

                            File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(this.MailMapFile));
                        });

                UICommands.RepoChangedNotifier.Notify();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotAccessMailmap.Text + Environment.NewLine + ex.Message,
                    _cannotAccessMailmapCaption.Text);
                return false;
            }
        }

        private void FormMailMapFormClosing(object sender, FormClosingEventArgs e)
        {
            var needToClose = false;

            if (!IsFileUpToDate())
            {
                switch (MessageBox.Show(this, _saveFileQuestion.Text, _saveFileQuestionCaption.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        if (SaveFile())
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
        }

        private void FormMailMapLoad(object sender, EventArgs e)
        {
            if (!Module.IsBareRepository()) return;
            MessageBox.Show(this, _mailmapOnlyInWorkingDirSupported.Text,_mailmapOnlyInWorkingDirSupportedCaption.Text);
            Close();
        }

        private bool IsFileUpToDate()
        {
            return MailMapFile == _NO_TRANSLATE_MailMapText.GetText();
        }

        private void MailMapFileLoaded(object sender, EventArgs e)
        {
            MailMapFile = _NO_TRANSLATE_MailMapText.GetText();
        }
    }
}
