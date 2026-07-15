using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormCreateBranch : GitExtensionsDialog
{
    private readonly TranslationString _noRevisionSelected = new("Select 1 revision to create the branch on.");
    private readonly TranslationString _branchNameIsEmpty = new("Enter branch name.");
    private readonly TranslationString _branchNameIsNotValid = new("“{0}” is not valid branch name.");
    private readonly TranslationString _creatingOrphanBranch = new("Creating orphan branch (repository has no commits)");
    private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);
    private readonly IGitBranchNameNormaliser _branchNameNormaliser = null!;

    public bool CheckoutAfterCreation { get; set; } = true;
    public bool UserAbleToChangeRevision { get; set; } = true;
    public bool CouldBeOrphan { get; set; } = true;

    public FormCreateBranch()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormCreateBranch(IGitUICommands commands, ObjectId objectId, string? newBranchNamePrefix = null)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        BranchNameTextBox.LostFocus += BranchNameTextBox_Leave;
        cmdOk.Click += cmdOk_Click;
        chkCreateOrphan.IsCheckedChanged += chkCreateOrphan_CheckedChanged;
        commitPicker.SelectedObjectIdChanged += commitPicker_SelectedObjectIdChanged;
        commitPicker.UICommandsSource = this;

        AcceptButton = cmdOk;
        ManualSectionAnchorName = "create-branch";
        ManualSectionSubfolder = "branches";

        InitializeComplete();

        _branchNameNormaliser = commands.GetRequiredService<IGitBranchNameNormaliser>();
        commitSummaryUserControl1.Revision = null;

        if (objectId.IsArtificial)
        {
            objectId = default;
        }

        if (objectId.IsZero)
        {
            objectId = Module.GetCurrentCheckout();
        }

        if (!objectId.IsZero)
        {
            commitPicker.SetSelectedCommitHash(objectId.ToString());

            if (string.IsNullOrWhiteSpace(newBranchNamePrefix))
            {
                GitRevision revision = Module.GetRevision(objectId, shortFormat: true, loadRefs: true);
                IGitRef? firstRef = revision.Refs.FirstOrDefault(gitRef => !gitRef.IsTag)
                    ?? revision.Refs.FirstOrDefault(gitRef => gitRef.IsTag);
                newBranchNamePrefix = firstRef?.LocalName;
                commitSummaryUserControl1.Revision = revision;
            }
        }
        else if (!Module.IsBareRepository())
        {
            ConfigureForOrphanBranch();
        }

        if (!string.IsNullOrWhiteSpace(newBranchNamePrefix))
        {
            BranchNameTextBox.Text = newBranchNamePrefix;
        }
    }

    private void ConfigureForOrphanBranch()
    {
        Grid.SetColumnSpan(lblCreateBranch, 2);
        lblCreateBranch.Content = _creatingOrphanBranch.Text;
        commitPicker.IsVisible = false;
        chkCheckoutAfterCreate.IsChecked = true;
        chkCheckoutAfterCreate.IsEnabled = false;
        grpOrphan.IsEnabled = true;
        chkCreateOrphan.IsChecked = true;
        chkCreateOrphan.IsEnabled = false;
        chkClearOrphan.IsChecked = false;
        CouldBeOrphan = true;
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        chkCheckoutAfterCreate.IsChecked = CheckoutAfterCreation;
        commitPicker.IsEnabled = UserAbleToChangeRevision;
        grpOrphan.IsEnabled = CouldBeOrphan;
        BranchNameTextBox.Focus();
    }

    private void cmdOk_Click(object? sender, EventArgs e)
    {
        cmdOk.Focus();

        ObjectId objectId = default;
        if (chkCreateOrphan.IsChecked != true)
        {
            objectId = commitPicker.SelectedObjectId;
            if (objectId.IsZeroOrArtificial)
            {
                MessageBoxes.Show(this, _noRevisionSelected.Text, Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
        }

        string branchName = BranchNameTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(branchName))
        {
            MessageBoxes.Show(_branchNameIsEmpty.Text, Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
            return;
        }

        if (!Module.CheckBranchFormat(branchName))
        {
            MessageBoxes.Show(string.Format(_branchNameIsNotValid.Text, branchName), Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
            return;
        }

        try
        {
            ArgumentString command = chkCreateOrphan.IsChecked == true
                ? Commands.CreateOrphan(branchName, objectId)
                : Commands.Branch(branchName, objectId, chkCheckoutAfterCreate.IsChecked == true);

            bool success = UICommands.StartGitCommandProcessDialog(this, command);
            if (chkCreateOrphan.IsChecked == true && success && chkClearOrphan.IsChecked == true)
            {
                UICommands.StartGitCommandProcessDialog(this, Commands.Remove());
            }

            DialogResult = success ? DialogResult.OK : DialogResult.None;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void chkCreateOrphan_CheckedChanged(object? sender, EventArgs e)
    {
        bool isOrphan = chkCreateOrphan.IsChecked == true;
        chkClearOrphan.IsEnabled = isOrphan;
        chkCheckoutAfterCreate.IsEnabled = !isOrphan;
        if (isOrphan)
        {
            chkCheckoutAfterCreate.IsChecked = true;
        }

        commitPicker.IsEnabled = !isOrphan;
    }

    private void commitPicker_SelectedObjectIdChanged(object? sender, EventArgs e)
    {
        if (commitPicker.SelectedObjectId.IsZeroOrArtificial)
        {
            commitSummaryUserControl1.Revision = null;
            return;
        }

        commitSummaryUserControl1.Revision = Module.GetRevision(commitPicker.SelectedObjectId, shortFormat: true, loadRefs: true);
    }
}
