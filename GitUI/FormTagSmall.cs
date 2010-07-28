using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormTagSmall : GitExtensionsForm
    {
        TranslationString noTagMassage = new TranslationString("Please enter a tag message");
        TranslationString noRevisionSelected = new TranslationString("Select 1 revision to create the tag on.");
        TranslationString messageCaption = new TranslationString("Tag");

        public FormTagSmall()
        {
            InitializeComponent(); Translate();
        
            tagMessage.MistakeFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Underline);
        }

        public GitRevision Revision { get; set; }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {

                if (Revision == null)
                {
                    MessageBox.Show(noRevisionSelected.Text, messageCaption.Text);
                    return;
                }
                else
                {
                    if (annotate.Checked)
                    {
                        if (string.IsNullOrEmpty(tagMessage.Text))
                        {
                            MessageBox.Show(noTagMassage.Text, messageCaption.Text);
                            return;
                        }

                        File.WriteAllText(Settings.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
                    }

                    
                    string s = GitCommands.GitCommands.Tag(TName.Text, Revision.Guid, annotate.Checked);

                    if (!string.IsNullOrEmpty(s))
                        MessageBox.Show(s, messageCaption.Text);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void TName_TextChanged(object sender, EventArgs e)
        {

        }

        private void TName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Ok_Click(null, null);
        }

        private void annotate_CheckedChanged(object sender, EventArgs e)
        {
            if (annotate.Checked)
            {
                tagMessage.Enabled = true;
            }
            else
            {
                tagMessage.Enabled = false;
            }
        }
    }
}
