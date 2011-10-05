using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormMailMap : GitExtensionsForm
    {
        public string MailMapFile;

        public FormMailMap()
        {
            InitializeComponent();
            Translate();
            MailMapFile = "";

            try
            {
                if (File.Exists(Settings.WorkingDir + ".mailmap"))
                {
                    _NO_TRANSLATE_MailMapText.ViewFile(Settings.WorkingDir + ".mailmap");
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
                    Settings.WorkingDir + ".mailmap",
                    x =>
                    {
                        this.MailMapFile = _NO_TRANSLATE_MailMapText.GetText();

                        using (var file = File.OpenWrite(x))
                        {
                            var contents = Settings.Encoding.GetBytes(this.MailMapFile);
                            file.Write(contents, 0, contents.Length);
                        }

                        Close();
                    });
        }

        private void FormMailMapFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit-mail-map");
        }

        private void FormMailMapLoad(object sender, EventArgs e)
        {
            RestorePosition("edit-mail-map");
            if (!Settings.IsBareRepository()) return;
            MessageBox.Show(".mailmap is only supported when there is a working dir.");
            Close();
        }
    }
}