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
        TranslationString noWorkingDir = new TranslationString(".gitattributes is only supported when there is a working dir.");

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
                            // Enter a newline to work around a wierd bug 
                            // that causes the first line to include 3 extra bytes. (encoding marker??)
                            GitAttributesFile = Environment.NewLine + _NO_TRANSLATE_GitAttributesText.GetText().Trim();
                            using (TextWriter tw = new StreamWriter(x, false, Settings.Encoding))
                                tw.Write(GitAttributesFile);
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
            if (!Settings.IsBareRepository()) return;
            MessageBox.Show(noWorkingDir.Text);
            Close();
        }
    }
}