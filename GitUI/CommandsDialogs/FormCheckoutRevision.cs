using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Browsing.Dialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCheckoutRevision : GitModuleForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox = new TranslationString("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption = new TranslationString("Checkout");

        private readonly IScriptManager _scriptManager;
        private readonly IScriptRunner _scriptRunner;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCheckoutRevision()
        {
            InitializeComponent();
        }

        public FormCheckoutRevision(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            _scriptManager = new ScriptManager();

            var gitUIEventArgs = new GitUIEventArgs(this, UICommands);
            var simpleDialog = new SimpleDialog(this);
            var scriptOptionsParser = new ScriptOptionsParser(simpleDialog, () => Module);

            _scriptRunner = new ScriptRunner(() => Module, gitUIEventArgs, scriptOptionsParser, simpleDialog, _scriptManager);
        }

        public void SetRevision(string commitHash)
        {
            commitPickerSmallControl1.SetSelectedCommitHash(commitHash);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var selectedObjectId = commitPickerSmallControl1.SelectedObjectId;

                if (selectedObjectId == null)
                {
                    MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text);
                    return;
                }

                var checkedOutObjectId = Module.GetCurrentCheckout();

                Debug.Assert(checkedOutObjectId != null, "checkedOutObjectId != null");

                var scripts = _scriptManager.GetScripts()
                    .Where(x => x.Enabled && x.OnEvent == ScriptEvent.BeforeCheckout)
                    .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                foreach (var script in scripts)
                {
                    _scriptRunner.RunScript(script);
                }

                string command = GitCommandHelpers.CheckoutCmd(selectedObjectId.ToString(), Force.Checked ? LocalChangesAction.Reset : 0);
                if (FormProcess.ShowDialog(this, command))
                {
                    if (selectedObjectId != checkedOutObjectId)
                    {
                        UICommands.UpdateSubmodules(this);
                    }
                }

                scripts = _scriptManager.GetScripts()
                    .Where(x => x.Enabled && x.OnEvent == ScriptEvent.AfterCheckout)
                    .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                foreach (var script in scripts)
                {
                    _scriptRunner.RunScript(script);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
