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
        readonly TranslationString noWorkingDir = new TranslationString(".gitattributes is only supported when there is a working dir.");

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
            FileInfoExtensions
                .MakeFileTemporaryWritable(
                    Settings.WorkingDir + ".gitattributes",
                    x =>
                    {
                        this.GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();

                        using (var file = File.OpenWrite(x))
                        {
                            var contents = Settings.Encoding.GetBytes(this.GitAttributesFile);
                            file.Write(contents, 0, contents.Length);
                        }

                        Close();
                    });
        }

        private void FormMailMapFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit-gitattributes");
        }

        private void FormMailMapLoad(object sender, EventArgs e)
        {
            RestorePosition("edit-gitattributes");
            if (!Settings.Module.IsBareRepository()) return;
            MessageBox.Show(noWorkingDir.Text);
            Close();
        }
    }
}