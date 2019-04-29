using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI.Browsing.Dialogs;
using GitUI.Script;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public partial class FormDeleteTag : GitModuleForm
    {
        private readonly IScriptManager _scriptManager;
        private readonly IScriptRunner _scriptRunner;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormDeleteTag()
        {
            InitializeComponent();
        }

        public FormDeleteTag(GitUICommands commands, string tag)
            : base(commands)
        {
            InitializeComponent();

            // scale up for hi DPI
            MaximumSize = DpiUtil.Scale(new Size(1000, 210));
            MinimumSize = DpiUtil.Scale(new Size(470, 210));

            InitializeComplete();
            Tag = tag;

            _scriptManager = new ScriptManager();

            var gitUIEventArgs = new GitUIEventArgs(this, UICommands);
            var simpleDialog = new SimpleDialog(this);
            var scriptOptionsParser = new ScriptOptionsParser(simpleDialog);

            _scriptRunner = new ScriptRunner(Module, gitUIEventArgs, scriptOptionsParser, simpleDialog, _scriptManager);
        }

        private void FormDeleteTagLoad(object sender, EventArgs e)
        {
            Tags.DisplayMember = nameof(IGitRef.Name);
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
            var pushCmd = string.Format("push \"{0}\" :refs/tags/{1}", remotesComboboxControl1.SelectedRemote, tagName);

            var scripts = _scriptManager.GetScripts()
                .Where(x => x.Enabled && x.OnEvent == ScriptEvent.BeforePush)
                .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

            foreach (var script in scripts)
            {
                _scriptRunner.RunScript(script);
            }

            using (var form = new FormRemoteProcess(Module, pushCmd)
            {
                ////Remote = currentRemote,
                ////Text = string.Format(_deleteFromCaption.Text, currentRemote),
            })
            {
                form.ShowDialog();

                if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                {
                    scripts = _scriptManager.GetScripts()
                        .Where(x => x.Enabled && x.OnEvent == ScriptEvent.AfterPush)
                        .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                    foreach (var script in scripts)
                    {
                        _scriptRunner.RunScript(script);
                    }
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
