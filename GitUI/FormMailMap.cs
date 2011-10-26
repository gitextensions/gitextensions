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
                            File.WriteAllBytes(x, Settings.Encoding.GetBytes(this.MailMapFile));
                        });
            }
            catch (Exception ex)
            {
                MessageBox.Show(_cannotAccessMailmap.Text + Environment.NewLine + ex.Message, 
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
            if (!Settings.IsBareRepository()) return;
            MessageBox.Show(_mailmapOnlyInWorkingDirSupported.Text,_mailmapOnlyInWorkingDirSupportedCaption.Text);
            Close();
        }
    }
}