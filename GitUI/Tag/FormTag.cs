﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.Tag
{
    public partial class FormTag : GitExtensionsForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _noTagMessage = new TranslationString("Please enter a tag message");

        public FormTag()
        {
            InitializeComponent();
            Translate();

            tagMessage.MistakeFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Underline);
        }

        private void FormTagFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("tag");
        }

        private void FormTagLoad(object sender, EventArgs e)
        {
            GitRevisions.Load();

            RestorePosition("tag");
        }


        private void CreateTagClick(object sender, EventArgs e)
        {
            try
            {
                if (GitRevisions.GetRevisions().Count != 1)
                {
                    MessageBox.Show(this, _noRevisionSelected.Text, _messageCaption.Text);
                    return;
                }
                if (annotate.Checked)
                {
                    if (string.IsNullOrEmpty(tagMessage.Text))
                    {
                        MessageBox.Show(this, _noTagMessage.Text, _messageCaption.Text);
                        return;
                    }

                    File.WriteAllText(Settings.Module.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
                }


                var s = Settings.Module.Tag(Tagname.Text, GitRevisions.GetRevisions()[0].Guid,
                                                    annotate.Checked);

                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(this, s, _messageCaption.Text);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void AnnotateCheckedChanged(object sender, EventArgs e)
        {
            tagMessage.Enabled = annotate.Checked;
        }
    }
}