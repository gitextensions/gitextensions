using System.Diagnostics;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;

namespace GitUI.CommandsDialogs
{
    public partial class FormDeleteTag : GitModuleForm
    {
        public FormDeleteTag(IGitUICommands commands, string? tag)
            : base(commands)
        {
            InitializeComponent();

            // scale up for hi DPI
            MaximumSize = DpiUtil.Scale(new Size(1000, 210));
            MinimumSize = DpiUtil.Scale(new Size(470, 210));

            InitializeComplete();
            Tag = tag;
        }

        private void FormDeleteTagLoad(object sender, EventArgs e)
        {
            Tags.DisplayMember = nameof(IGitRef.LocalName);
            Tags.DataSource = Module.GetRefs(RefsFilter.Tags);
            Tags.Text = Tag as string;
            remotesComboboxControl1.SelectedRemote = Module.GetCurrentRemote();
            EnableOrDisableRemotesCombobox();
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                Module.DeleteTag(Tags.Text);

                if (deleteTag.Checked && !string.IsNullOrEmpty(Tags.Text))
                {
                    RemoveRemoteTag(Tags.Text);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void RemoveRemoteTag(string tagName)
        {
            string pushCmd = string.Format("push \"{0}\" :refs/tags/{1}", remotesComboboxControl1.SelectedRemote, tagName);

            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforePush, this);
            if (!success)
            {
                return;
            }

            using FormRemoteProcess form = new(UICommands, pushCmd);
            ////Remote = currentRemote,
            ////Text = string.Format(_deleteFromCaption.Text, currentRemote),
            form.ShowDialog();

            if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
            {
                ScriptsRunner.RunEventScripts(ScriptEvent.AfterPush, this);
            }
        }

        private void deleteTag_CheckedChanged(object sender, EventArgs e)
        {
            EnableOrDisableRemotesCombobox();
        }

        private void EnableOrDisableRemotesCombobox()
        {
            remotesComboboxControl1.Enabled = deleteTag.Checked;
        }
    }
}
