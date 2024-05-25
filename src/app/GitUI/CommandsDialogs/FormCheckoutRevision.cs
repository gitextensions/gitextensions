using System.Diagnostics;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCheckoutRevision : GitExtensionsDialog
    {
        private readonly TranslationString _noRevisionSelectedMsgBox = new("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption = new("Checkout");

        public FormCheckoutRevision(IGitUICommands commands)
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
                ObjectId selectedObjectId = commitPickerSmallControl1.SelectedObjectId;

                if (selectedObjectId is null)
                {
                    MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ObjectId checkedOutObjectId = Module.GetCurrentCheckout();

                DebugHelpers.Assert(checkedOutObjectId is not null, "checkedOutObjectId is not null");

                bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeCheckout, this);
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

                ScriptsRunner.RunEventScripts(ScriptEvent.AfterCheckout, this);

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
