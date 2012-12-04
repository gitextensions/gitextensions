using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using ResourceManager.Translation;

namespace GitUI.Tag
{
    public partial class FormDeleteTag : GitModuleForm
    {
        private readonly TranslationString _deleteFromCaption = new TranslationString("Delete from '{0}'");
        
        public FormDeleteTag(GitUICommands aCommands, string tag)
            : base(aCommands)
        {
            InitializeComponent(); Translate();
            Tag = tag;
        }

        private string currentRemote = "";

        private void FormDeleteTagLoad(object sender, EventArgs e)
        {
            Tags.DisplayMember = "Name";
            Tags.DataSource = Module.GetHeads(true, false);
            Tags.Text = Tag as string;
            currentRemote = Module.GetCurrentRemote();
            deleteTag.Text = string.Format(_deleteFromCaption.Text, currentRemote);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                Module.DeleteTag(Tags.Text);

                if (deleteTag.Checked && !string.IsNullOrEmpty(Tags.Text))
                    RemoveRemoteTag(Tags.Text);

                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void RemoveRemoteTag(string tagName)
        {
            var pushCmd = string.Format("push \"{0}\" :refs/tags/{1}", currentRemote, tagName);

            ScriptManager.RunEventScripts(Module, ScriptEvent.BeforePush);

            using (var form = new FormRemoteProcess(Module, pushCmd)
            {
                Remote = currentRemote,
                Text = string.Format(_deleteFromCaption.Text, currentRemote),
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
    }
}
