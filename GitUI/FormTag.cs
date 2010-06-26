using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ResourceManager.Translation;
using System.IO;
using GitCommands;

namespace GitUI
{
    public partial class FormTag : GitExtensionsForm
    {
        TranslationString noTagMassage = new TranslationString("Please enter a tag message");
        TranslationString noRevisionSelected = new TranslationString("Select 1 revision to create the tag on.");
        TranslationString messageCaption = new TranslationString("Tag");

        public FormTag()
        {
            InitializeComponent(); Translate();
        }

        private void FormTag_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("tag");
        }

        private void FormTag_Load(object sender, EventArgs e)
        {
            RestorePosition("tag");
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CreateTag_Click(object sender, EventArgs e)
        {
            try
            {

                if (GitRevisions.GetRevisions().Count != 1)
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


                    string s = GitCommands.GitCommands.Tag(Tagname.Text, GitRevisions.GetRevisions()[0].Guid, annotate.Checked);

                    if (!string.IsNullOrEmpty(s))
                        MessageBox.Show(s, "Tag");
                    Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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
