using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormMailMap : GitExtensionsForm
    {
        private readonly TranslationString _mailmapOnlyInWorkingDirSupported =
            new TranslationString(".mailmap is only supported when there is a working dir.");
        private readonly TranslationString _mailmapOnlyInWorkingDirSupportedCaption =
            new TranslationString("No working dir");

        private readonly TranslationString _cannotAccessMailmap =
            new TranslationString("Failed to save .mailmap." + Environment.NewLine + "Check if file is accessible.");
        private readonly TranslationString _cannotAccessMailmapCaption =
            new TranslationString("Failed to save .mailmap");
        
        
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
            try
            {
            FileInfoExtensions
                .MakeFileTemporaryWritable(
                    Settings.WorkingDir + ".mailmap",
                    x =>
                    {
                        this.MailMapFile = _NO_TRANSLATE_MailMapText.GetText();
                        if (!this.MailMapFile.EndsWith(Environment.NewLine))
                            this.MailMapFile += Environment.NewLine;

                        File.WriteAllBytes(x, Settings.AppEncoding.GetBytes(this.MailMapFile));
                        });
            }
            catch (Exception ex)
                        {
                MessageBox.Show(this, _cannotAccessMailmap.Text + Environment.NewLine + ex.Message, 
                    _cannotAccessMailmapCaption.Text);
                        }
                        Close();
        }

        private void FormMailMapFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit-mail-map");
        }

        private void FormMailMapLoad(object sender, EventArgs e)
        {
            RestorePosition("edit-mail-map");
            if (!Settings.Module.IsBareRepository()) return;
            MessageBox.Show(this, _mailmapOnlyInWorkingDirSupported.Text,_mailmapOnlyInWorkingDirSupportedCaption.Text);
            Close();
        }
    }
}