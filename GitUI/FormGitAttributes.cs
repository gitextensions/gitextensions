using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormGitAttributes : GitExtensionsForm
    {
        private readonly TranslationString noWorkingDir = 
            new TranslationString(".gitattributes is only supported when there is a working dir.");
        private readonly TranslationString _noWorkingDirCaption =
            new TranslationString("No working dir");

        private readonly TranslationString _cannotAccessGitattributes =
            new TranslationString("Failed to save .gitattributes." + Environment.NewLine + "Check if file is accessible.");
        private readonly TranslationString _cannotAccessGitattributesCaption =
            new TranslationString("Failed to save .gitattributes");

        public string GitAttributesFile;

        public FormGitAttributes()
        {
            InitializeComponent();
            Translate();
            GitAttributesFile = "";

            try
            {
                if (File.Exists(Settings.WorkingDir + ".gitattributes"))
                {
                    _NO_TRANSLATE_GitAttributesText.ViewFile(Settings.WorkingDir + ".gitattributes");
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void SaveClick(object sender, EventArgs e)
        {
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(
                        Settings.WorkingDir + ".gitattributes",
                        x =>
                        {
                            this.GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();
                            if (!this.GitAttributesFile.EndsWith(Environment.NewLine))
                                this.GitAttributesFile += Environment.NewLine;
                            File.WriteAllBytes(x,Settings.Encoding.GetBytes(this.GitAttributesFile));
                        });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotAccessGitattributes.Text + Environment.NewLine + ex.Message,
                    _cannotAccessGitattributesCaption.Text);
            }
            Close();
        }

        private void FormMailMapFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit-gitattributes");
        }

        private void FormMailMapLoad(object sender, EventArgs e)
        {
            RestorePosition("edit-gitattributes");
            if (!Settings.Module.IsBareRepository()) return;
            MessageBox.Show(this, noWorkingDir.Text, _noWorkingDirCaption.Text);
            Close();
        }
    }
}