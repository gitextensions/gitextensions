using System.Diagnostics;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCheckoutRevision : GitExtensionsDialog
    {
        private readonly TranslationString _noRevisionSelectedMsgBox = new("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption = new("Checkout");

        public FormCheckoutRevision(GitUICommands commands)
            : base(commands, enablePositionRestore: false)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public void SetRevision(string? commitHash)
        {
            commitPickerSmallControl1.SetSelectedCommitHash(commitHash);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                GitUIPluginInterfaces.ObjectId selectedObjectId = commitPickerSmallControl1.SelectedObjectId;

                if (selectedObjectId is null)
                {
                    MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                GitUIPluginInterfaces.ObjectId checkedOutObjectId = Module.GetCurrentCheckout();

                DebugHelpers.Assert(checkedOutObjectId is not null, "checkedOutObjectId is not null");

                bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeCheckout, new DefaultScriptHostControl(this, UICommands));
                if (!success)
                {
                    return;
                }

                string command = Commands.Checkout(selectedObjectId.ToString(), Force.Checked ? LocalChangesAction.Reset : 0);
                success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
                if (success)
                {
                    if (selectedObjectId != checkedOutObjectId)
                    {
                        UICommands.UpdateSubmodules(this);
                    }
                }

                ScriptsRunner.RunEventScripts(ScriptEvent.AfterCheckout, new DefaultScriptHostControl(this, UICommands));

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
