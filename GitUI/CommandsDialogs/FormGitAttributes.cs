﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormGitAttributes : GitModuleForm
    {
        private readonly TranslationString noWorkingDir = 
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

        public FormGitAttributes(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
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
                if (File.Exists(Module.WorkingDir + ".gitattributes"))
                {
                    _NO_TRANSLATE_GitAttributesText.ViewFile(Module.WorkingDir + ".gitattributes");
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
                        Module.WorkingDir + ".gitattributes",
                        x =>
                        {
                            this.GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();
                            if (!this.GitAttributesFile.EndsWith(Environment.NewLine))
                                this.GitAttributesFile += Environment.NewLine;
                            File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(this.GitAttributesFile));
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

        private void FormGitAttributesLoad(object sender, EventArgs e)
        {
            if (!Module.IsBareRepository()) return;
            MessageBox.Show(this, noWorkingDir.Text, _noWorkingDirCaption.Text);
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

