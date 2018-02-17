﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitUI.Script;

namespace GitUI.CommandsDialogs
{
    public partial class FormDeleteTag : GitModuleForm
    { 
        public FormDeleteTag(GitUICommands aCommands, string tag)
            : base(aCommands)
        {
            InitializeComponent(); Translate();
            Tag = tag;
        }

        private void FormDeleteTagLoad(object sender, EventArgs e)
        {
            Tags.DisplayMember = "Name";
            Tags.DataSource = Module.GetRefs(true, false);
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
                    RemoveRemoteTag(Tags.Text);

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
            var pushCmd = string.Format("push \"{0}\" :refs/tags/{1}", remotesComboboxControl1.SelectedRemote, tagName);

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);

            using (var form = new FormRemoteProcess(Module, pushCmd))
            {
                form.ShowDialog();

                if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                {
                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
                }
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
