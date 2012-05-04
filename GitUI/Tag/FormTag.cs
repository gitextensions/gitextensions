using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using GitUI.Script;

namespace GitUI.Tag
{
    public partial class FormTag : GitExtensionsForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _noTagMessage = new TranslationString("Please enter a tag message");

        private readonly TranslationString _pushToCaption = new TranslationString("Push tag to {0}");

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
                var tagName = CreateTag(sender, e);
                if (pushTag.Checked && !string.IsNullOrEmpty(tagName))
                    PushTag(tagName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
        
        private string CreateTag(object sender, EventArgs e)
        {
            if (GitRevisions.GetSelectedRevisions().Count != 1)
            {
                MessageBox.Show(this, _noRevisionSelected.Text, _messageCaption.Text);
                return string.Empty;
            }
            if (annotate.Checked)
            {
                if (string.IsNullOrEmpty(tagMessage.Text))
                {
                    MessageBox.Show(this, _noTagMessage.Text, _messageCaption.Text);
                    return string.Empty;
                }

                File.WriteAllText(Settings.Module.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
            }
            
            var s = Settings.Module.Tag(Tagname.Text, GitRevisions.GetSelectedRevisions()[0].Guid,
                                                annotate.Checked);

            if (!string.IsNullOrEmpty(s))
                MessageBox.Show(this, s, _messageCaption.Text);
            Close();

            return Tagname.Text;
        }

        private void PushTag(string tagName)
        {
            var currentBranchRemote = Settings.Module.GetSetting(string.Format("branch.{0}.remote", Settings.Module.GetSelectedBranch()));
            var pushCmd = GitCommandHelpers.PushTagCmd(currentBranchRemote, tagName, false);

            ScriptManager.RunEventScripts(ScriptEvent.BeforePush);

            var form = new FormRemoteProcess(pushCmd)
            {
                Remote = currentBranchRemote,
                Text = string.Format(_pushToCaption.Text, currentBranchRemote),
            };

            form.ShowDialog();

            if (!Settings.Module.InTheMiddleOfConflictedMerge() &&
                !Settings.Module.InTheMiddleOfRebase() && !form.ErrorOccurred())
            {
                ScriptManager.RunEventScripts(ScriptEvent.AfterPush);
            }
        }
        
        private void AnnotateCheckedChanged(object sender, EventArgs e)
        {
            tagMessage.Enabled = annotate.Checked;
        }
    }
}