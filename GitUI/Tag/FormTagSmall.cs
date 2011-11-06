﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.Tag
{
    public partial class FormTagSmall : GitExtensionsForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _noTagMassage = new TranslationString("Please enter a tag message");

        public FormTagSmall()
        {
            InitializeComponent();
            Translate();

            tagMessage.MistakeFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Underline);
        }

        public GitRevision Revision { get; set; }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (Revision == null)
                {
                    MessageBox.Show(this, _noRevisionSelected.Text, _messageCaption.Text);
                    return;
                }
                if (annotate.Checked)
                {
                    if (string.IsNullOrEmpty(tagMessage.Text))
                    {
                        MessageBox.Show(this, _noTagMassage.Text, _messageCaption.Text);
                        return;
                    }

                    File.WriteAllText(Settings.Module.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
                }


                var s = Settings.Module.Tag(TName.Text, Revision.Guid, annotate.Checked);

                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(this, s, _messageCaption.Text);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }


        private void NameKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                OkClick(null, null);
        }

        private void AnnotateCheckedChanged(object sender, EventArgs e)
        {
            tagMessage.Enabled = annotate.Checked;
        }
    }
}