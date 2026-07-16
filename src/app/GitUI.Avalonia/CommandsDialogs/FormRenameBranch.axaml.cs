using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormRenameBranch.cs.
public sealed partial class FormRenameBranch : GitModuleForm
{
    private readonly IGitBranchNameNormaliser _branchNameNormaliser = null!;
    private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);
    private readonly string _oldName = string.Empty;

    public FormRenameBranch()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormRenameBranch(IGitUICommands commands, string defaultBranch)
        : base(commands, enablePositionRestore: false)
    {
        _branchNameNormaliser = commands.GetRequiredService<IGitBranchNameNormaliser>();

        InitializeComponent();
        WireControls();
        AcceptButton = Ok;
        InitializeComplete();
        BranchNameTextBox.Text = defaultBranch;
        _oldName = defaultBranch;
    }

    private void WireControls()
    {
        Ok.Click += OkClick;
        BranchNameTextBox.LostFocus += BranchNameTextBox_Leave;
    }

    private void BranchNameTextBox_Leave(object? sender, EventArgs e)
    {
        string branchName = BranchNameTextBox.Text ?? string.Empty;
        if (!AppSettings.AutoNormaliseBranchName || !branchName.Any(PathUtil.IsValidPathChar))
        {
            return;
        }

        int caretPosition = BranchNameTextBox.CaretIndex;
        BranchNameTextBox.Text = _branchNameNormaliser.Normalise(branchName, _gitBranchNameOptions);
        BranchNameTextBox.CaretIndex = Math.Min(caretPosition, BranchNameTextBox.Text.Length);
    }

    private void OkClick(object? sender, EventArgs e)
    {
        // Ok button set as the "AcceptButton" for the form
        // if the user hits [Enter] at any point, we need to trigger BranchNameTextBox Leave event
        Ok.Focus();

        string newName = BranchNameTextBox.Text ?? string.Empty;

        if (newName == _oldName)
        {
            DialogResult = WinFormsShims.DialogResult.Cancel;
            return;
        }

        ArgumentString command = Commands.RenameBranch(_oldName, newName);
        bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

        DialogResult = success ? WinFormsShims.DialogResult.OK : WinFormsShims.DialogResult.None;
    }
}
