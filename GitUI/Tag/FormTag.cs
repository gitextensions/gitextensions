using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using ResourceManager.Translation;

namespace GitUI.Tag
{
    public partial class FormTag : GitModuleForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _noTagMessage = new TranslationString("Please enter a tag message");

        private readonly TranslationString _pushToCaption = new TranslationString("Push tag to '{0}'");

        private string currentRemote = "";

        public FormTag(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            tagMessage.MistakeFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Underline);
        }

        private void FormTagLoad(object sender, EventArgs e)
        {
            currentRemote = Module.GetCurrentRemote();
            pushTag.Text = string.Format(_pushToCaption.Text, currentRemote);

            GitRevisions.Load();
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

                File.WriteAllText(Module.WorkingDirGitDir() + "\\TAGMESSAGE", tagMessage.Text);
            }
            
            var s = Module.Tag(Tagname.Text, GitRevisions.GetSelectedRevisions()[0].Guid,
                                                annotate.Checked, ForceTag.Checked);

            if (!string.IsNullOrEmpty(s))
                MessageBox.Show(this, s, _messageCaption.Text);
            Close();

            if (s.Contains("fatal:"))
                return string.Empty;
            else
                UICommands.RepoChangedNotifier.Notify();

            return Tagname.Text;
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

                if (!form.ErrorOccurred())
                    UICommands.RepoChangedNotifier.Notify();

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