using System.Diagnostics;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormCreateBranch : GitExtensionsDialog
{
    private readonly TranslationString _noRevisionSelected = new("Select 1 revision to create the branch on.");
    private readonly TranslationString _branchNameIsEmpty = new("Enter branch name.");
    private readonly TranslationString _branchNameIsNotValid = new("“{0}” is not valid branch name.");
    private readonly TranslationString _creatingOrphanBranch = new("Creating orphan branch (repository has no commits)");
    private readonly IGitBranchNameNormaliser _branchNameNormaliser = new GitBranchNameNormaliser();
    private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);

    public bool CheckoutAfterCreation { get; set; } = true;
    public bool UserAbleToChangeRevision { get; set; } = true;
    public bool CouldBeOrphan { get; set; } = true;

    public FormCreateBranch(IGitUICommands commands, ObjectId? objectId, string? newBranchNamePrefix = null)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        MinimumSize = new Size(Width, PreferredMinimumHeight);

        InitializeComplete();

        grpOrphan.AutoSize = true;

        if (objectId?.IsArtificial is true)
        {
            objectId = null;
        }

        commitSummaryUserControl1.Revision = null;

        objectId ??= Module.GetCurrentCheckout();
        if (objectId is not null)
        {
            commitPicker.SetSelectedCommitHash(objectId.ToString());

            if (string.IsNullOrWhiteSpace(newBranchNamePrefix))
            {
                GitRevision revision = Module.GetRevision(objectId, shortFormat: true, loadRefs: true);
                IGitRef firstRef = revision.Refs.FirstOrDefault(r => !r.IsTag) ?? revision.Refs.FirstOrDefault(r => r.IsTag);
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
        tableLayout.SetColumnSpan(lblCreateBranch, 2);
        lblCreateBranch.Text = _creatingOrphanBranch.Text;
        commitPicker.Visible = false;
        chkCheckoutAfterCreate.Checked = true;
        chkCheckoutAfterCreate.Enabled = false;
        grpOrphan.Enabled = true;
        chkCreateOrphan.Checked = true;
        chkCreateOrphan.Enabled = false;
        chkClearOrphan.Checked = false;

        CouldBeOrphan = true;
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

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // ensure all labels are wrapped if required
        // this must happen only after the label texts have been set
        foreach (Label label in this.FindDescendantsOfType<Label>())
        {
            label.AutoSize = true;
        }

        chkCheckoutAfterCreate.Checked = CheckoutAfterCreation;
        commitPicker.Enabled = UserAbleToChangeRevision;
        grpOrphan.Enabled = CouldBeOrphan;

        BranchNameTextBox.Focus();
    }

    private void cmdOk_Click(object sender, EventArgs e)
    {
        // Ok button set as the "AcceptButton" for the form
        // if the user hits [Enter] at any point, we need to trigger BranchNameTextBox Leave event
        cmdOk.Focus();

        ObjectId objectId = null;

        if (!chkCreateOrphan.Checked)
        {
            objectId = commitPicker.SelectedObjectId;
            if (objectId is null)
            {
                MessageBox.Show(this, _noRevisionSelected.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
        }

        string branchName = BranchNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(branchName))
        {
            MessageBox.Show(_branchNameIsEmpty.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
            return;
        }

        if (!Module.CheckBranchFormat(branchName))
        {
            MessageBox.Show(string.Format(_branchNameIsNotValid.Text, branchName), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
            return;
        }

        try
        {
            ObjectId originalHash = Module.GetCurrentCheckout();

            ArgumentString command = chkCreateOrphan.Checked
                ? Commands.CreateOrphan(branchName, objectId)
                : Commands.Branch(branchName, objectId.ToString(), chkCheckoutAfterCreate.Checked);

            bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
            if (chkCreateOrphan.Checked && success && chkClearOrphan.Checked)
            {
                // orphan AND orphan creation success AND clear
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.Remove(), Module.WorkingDir, input: null, useDialogSettings: true);
            }

            if (success && chkCheckoutAfterCreate.Checked && objectId != originalHash)
            {
                UICommands.UpdateSubmodules(this);
            }

            DialogResult = success ? DialogResult.OK : DialogResult.None;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void chkCreateOrphan_CheckedChanged(object sender, EventArgs e)
    {
        bool isOrphan = chkCreateOrphan.Checked;
        chkClearOrphan.Enabled = isOrphan;

        chkCheckoutAfterCreate.Enabled = isOrphan == false; // auto-checkout for orphan
        if (isOrphan)
        {
            chkCheckoutAfterCreate.Checked = true;
        }

        commitPicker.Enabled = !isOrphan;
    }

    private void commitPicker_SelectedObjectIdChanged(object sender, EventArgs e)
    {
        GitRevision revision = Module.GetRevision(commitPicker.SelectedObjectId, shortFormat: true, loadRefs: true);
        commitSummaryUserControl1.Revision = revision;
    }
}
