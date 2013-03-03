using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using ResourceManager.Translation;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCreateTagAtRevision : GitModuleForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _noTagMassage = new TranslationString("Please enter a tag message");

        private readonly TranslationString _pushToCaption = new TranslationString("Push tag to '{0}'");

        private string currentRemote = "";
        
        public FormCreateTagAtRevision(GitUICommands aCommands, GitRevision revision)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            commitPickerSmallControl1.UICommandsSource = this;
            tagMessage.MistakeFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Underline);
            commitPickerSmallControl1.SelectedCommitHash = revision.Guid;
        }

        private void FormTagSmall_Load(object sender, EventArgs e)
        {
            textBoxTagName.Focus();
            currentRemote = Module.GetCurrentRemote();
            pushTag.Text = string.Format(_pushToCaption.Text, currentRemote);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var tagName = CreateTag();

                if (pushTag.Checked && !string.IsNullOrEmpty(tagName))
                    PushTag(tagName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private string CreateTag()
        {
            string revision = commitPickerSmallControl1.SelectedCommitHash;

            if (revision.IsNullOrEmpty())
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

                File.WriteAllText(Module.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
            }

            var s = Module.Tag(textBoxTagName.Text, revision, annotate.Checked, ForceTag.Checked);

            if (!string.IsNullOrEmpty(s))
                MessageBox.Show(this, s, _messageCaption.Text);

            if (s.Contains("fatal:"))
                return string.Empty;

            DialogResult = DialogResult.OK;
            return textBoxTagName.Text;
        }

        private void PushTag(string tagName)
        {
            var pushCmd = GitCommandHelpers.PushTagCmd(currentRemote, tagName, false);

            ScriptManager.RunEventScripts(Module, ScriptEvent.BeforePush);

            using (var form = new FormRemoteProcess(Module, pushCmd)
            {
                Remote = currentRemote,
                Text = string.Format(_pushToCaption.Text, currentRemote),
            })
            {

                form.ShowDialog();

                if (!Module.InTheMiddleOfConflictedMerge() &&
                    !Module.InTheMiddleOfRebase() && !form.ErrorOccurred())
                {
                    ScriptManager.RunEventScripts(Module, ScriptEvent.AfterPush);
                }
            }
        }

        private void AnnotateCheckedChanged(object sender, EventArgs e)
        {
            tagMessage.Enabled = annotate.Checked;
        }
    }
}