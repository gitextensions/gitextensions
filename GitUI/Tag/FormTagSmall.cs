using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using GitUI.Script;

namespace GitUI.Tag
{
    public partial class FormTagSmall : GitExtensionsForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _noTagMassage = new TranslationString("Please enter a tag message");

        private readonly TranslationString _pushToCaption = new TranslationString("Push tag to {0}");

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
            if (Revision == null)
            {
                MessageBox.Show(this, _noRevisionSelected.Text, _messageCaption.Text);
                return string.Empty;
            }
            if (annotate.Checked)
            {
                if (string.IsNullOrEmpty(tagMessage.Text))
                {
                    MessageBox.Show(this, _noTagMassage.Text, _messageCaption.Text);
                    return string.Empty;
                }

                File.WriteAllText(Settings.Module.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
            }


            var s = Settings.Module.Tag(TName.Text, Revision.Guid, annotate.Checked);

            if (!string.IsNullOrEmpty(s))
                MessageBox.Show(this, s, _messageCaption.Text);

            if (s.Contains("fatal:"))
                return string.Empty;

            DialogResult = DialogResult.OK;
            return TName.Text;
        }

        private void PushTag(string tagName)
        {
            var currentBranchRemote = Settings.Module.GetSetting(string.Format("branch.{0}.remote", Settings.Module.GetSelectedBranch()));
            var pushCmd = GitCommandHelpers.PushTagCmd(currentBranchRemote, tagName, false);

            ScriptManager.RunEventScripts(ScriptEvent.BeforePush);

            using (var form = new FormRemoteProcess(pushCmd)
            {
                Remote = currentBranchRemote,
                Text = string.Format(_pushToCaption.Text, currentBranchRemote),
            })
            {

                form.ShowDialog();

                if (!Settings.Module.InTheMiddleOfConflictedMerge() &&
                    !Settings.Module.InTheMiddleOfRebase() && !form.ErrorOccurred())
                {
                    ScriptManager.RunEventScripts(ScriptEvent.AfterPush);
                }
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