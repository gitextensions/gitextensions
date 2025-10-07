using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormRenameBranch : GitModuleForm
{
    private readonly IGitBranchNameNormaliser _branchNameNormaliser;
    private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);
    private readonly string _oldName;

    public FormRenameBranch(IGitUICommands commands, string defaultBranch)
        : base(commands)
    {
        _branchNameNormaliser = new GitBranchNameNormaliser();

        InitializeComponent();
        InitializeComplete();
        BranchNameTextBox.Text = defaultBranch;
        _oldName = defaultBranch;
    }

    private void BranchNameTextBox_Leave(object sender, EventArgs e)
    {
        if (!AppSettings.AutoNormaliseBranchName || !BranchNameTextBox.Text.Any(GitBranchNameNormaliser.IsValidChar))
        {
            return;
        }

        int caretPosition = BranchNameTextBox.SelectionStart;
        string branchName = _branchNameNormaliser.Normalise(BranchNameTextBox.Text, _gitBranchNameOptions);
        BranchNameTextBox.Text = branchName;
        BranchNameTextBox.SelectionStart = caretPosition;
    }

    private void OkClick(object sender, EventArgs e)
    {
        // Ok button set as the "AcceptButton" for the form
        // if the user hits [Enter] at any point, we need to trigger BranchNameTextBox Leave event
        Ok.Focus();

        string newName = BranchNameTextBox.Text;

        if (newName == _oldName)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        ArgumentString command = Commands.RenameBranch(_oldName, newName);
        bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

        DialogResult = success ? DialogResult.OK : DialogResult.None;
    }
}
