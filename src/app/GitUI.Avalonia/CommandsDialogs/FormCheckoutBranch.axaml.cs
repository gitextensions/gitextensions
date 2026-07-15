using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
using ResourceManager;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormCheckoutBranch.cs, initially limited to local branches.
// The remote-branch controls remain named in AXAML but hidden until their create/reset flow is ported.
public partial class FormCheckoutBranch : GitExtensionsDialog
{
    private TranslationString _invalidBranchName = new("An existing branch must be selected.");

    private readonly IReadOnlyList<ObjectId>? _containObjectIds;
    private bool? _isDirtyDir;
    private IReadOnlyList<IGitRef>? _localBranches;

    public FormCheckoutBranch()
    {
        InitializeComponent();
    }

    public FormCheckoutBranch(IGitUICommands commands, string branch, bool remote, IReadOnlyList<ObjectId>? containObjectIds = null)
        : base(commands, true)
    {
        ArgumentNullException.ThrowIfNull(commands);

        _containObjectIds = containObjectIds;

        InitializeComponent();

        Ok.Click += OkClick;
        Branches.SelectionChanged += Branches_SelectedIndexChanged;
        rbReset.IsCheckedChanged += rbReset_CheckedChanged;
        Activated += FormCheckoutBranch_Activated;

        AcceptButton = Ok;
        ManualSectionAnchorName = "checkout-branch";
        ManualSectionSubfolder = "branches";

        PopulateBranches();

        if (!remote && !string.IsNullOrEmpty(branch))
        {
            Branches.SelectedItem = GetBranchNames()
                .FirstOrDefault(name => name.Equals(branch, StringComparison.Ordinal));
        }

        if (_containObjectIds is not null && GetBranchNames().Count == 1)
        {
            Branches.SelectedIndex = 0;
        }

        _isDirtyDir = AppSettings.CheckForUncommittedChangesInCheckoutBranch
            ? Module.IsDirtyDir()
            : null;
        localChangesGB.IsVisible = HasUncommittedChanges;

        LocalChangesAction configuredAction = AppSettings.CheckoutBranchAction;
        ChangesMode = configuredAction == LocalChangesAction.Stash
            ? LocalChangesAction.DontChange
            : configuredAction;

        InitializeComplete();
    }

    private LocalChangesAction ChangesMode
    {
        get
        {
            if (rbReset.IsChecked == true)
            {
                return LocalChangesAction.Reset;
            }

            if (rbMerge.IsChecked == true)
            {
                return LocalChangesAction.Merge;
            }

            return LocalChangesAction.DontChange;
        }
        set
        {
            rbReset.IsChecked = value == LocalChangesAction.Reset;
            rbMerge.IsChecked = value == LocalChangesAction.Merge;
            rbDontChange.IsChecked = value == LocalChangesAction.DontChange;
        }
    }

    private bool HasUncommittedChanges => _isDirtyDir ?? true;

    public DialogResult DoDefaultActionOrShow(IWin32Window? owner)
    {
        bool localBranchSelected = GetSelectedBranch() is not null;
        if (!AppSettings.AlwaysShowCheckoutBranchDlg && localBranchSelected &&
            (!HasUncommittedChanges || AppSettings.UseDefaultCheckoutBranchAction))
        {
            return PerformCheckout(owner);
        }

        return ShowDialog(owner);
    }

    private void PopulateBranches()
    {
        IEnumerable<string> branchNames = _containObjectIds is null
            ? GetLocalBranches().Select(branch => branch.Name)
            : GetContainsObjectIdBranches();

        Branches.ItemsSource = branchNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private IReadOnlyList<string> GetContainsObjectIdBranches()
    {
        HashSet<string> result = [];
        if (_containObjectIds is null || _containObjectIds.Count == 0)
        {
            return result.ToList();
        }

        result.UnionWith(GetBranchesContaining(_containObjectIds[0]));
        for (int index = 1; index < _containObjectIds.Count; index++)
        {
            result.IntersectWith(GetBranchesContaining(_containObjectIds[index]));
        }

        return result.ToList();

        IEnumerable<string> GetBranchesContaining(ObjectId objectId)
            => Module.GetAllBranchesWhichContainGivenCommit(
                    objectId,
                    getLocal: true,
                    getRemote: false,
                    cancellationToken: default)
                .Where(name => !DetachedHeadParser.IsDetachedHead(name) && !name.EndsWith("/HEAD"));
    }

    private void OkClick(object? sender, EventArgs e)
    {
        DialogResult result = PerformCheckout(this);
        if (result == DialogResult.OK)
        {
            DialogResult = result;
        }
    }

    private DialogResult PerformCheckout(IWin32Window? owner)
    {
        Ok.Focus();

        string? branchName = GetSelectedBranch();
        if (branchName is null)
        {
            MessageBoxes.Show(this, _invalidBranchName.Text, Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return DialogResult.None;
        }

        LocalChangesAction localChanges = localChangesGB.IsVisible
            ? ChangesMode
            : LocalChangesAction.DontChange;

        if (localChanges != LocalChangesAction.Reset && chkSetLocalChangesActionAsDefault.IsChecked == true)
        {
            AppSettings.CheckoutBranchAction = localChanges;
        }

        return UICommands.StartCommandLineProcessDialog(
            owner,
            Commands.CheckoutBranch(branchName, remote: false, localChanges))
                ? DialogResult.OK
                : DialogResult.None;
    }

    private void Branches_SelectedIndexChanged(object? sender, EventArgs e)
    {
        string? branch = GetSelectedBranch();
        lbChanges.Text = string.Empty;
        if (branch is null)
        {
            return;
        }

        ThreadHelper.FileAndForget(async () =>
        {
            ObjectId currentCheckout = Module.GetCurrentCheckout();
            string aheadBehindInfo = currentCheckout.IsZero
                ? string.Empty
                : Module.GetCommitCountString(currentCheckout, branch);

            await this.SwitchToMainThreadAsync();

            if (GetSelectedBranch() == branch)
            {
                lbChanges.Text = aheadBehindInfo;
            }
        });
    }

    private IEnumerable<IGitRef> GetLocalBranches()
        => _localBranches ??= Module.GetRefs(RefsFilter.Heads);

    private IReadOnlyList<string> GetBranchNames()
        => Branches.ItemsSource?.OfType<string>().ToList() ?? [];

    private string? GetSelectedBranch()
    {
        string? branch = Branches.SelectedItem as string;
        return branch is not null && GetBranchNames().Contains(branch, StringComparer.Ordinal)
            ? branch
            : null;
    }

    private void FormCheckoutBranch_Activated(object? sender, EventArgs e)
    {
        Branches.Focus();
    }

    private void rbReset_CheckedChanged(object? sender, EventArgs e)
    {
        chkSetLocalChangesActionAsDefault.IsEnabled = rbReset.IsChecked != true;
        if (rbReset.IsChecked == true)
        {
            chkSetLocalChangesActionAsDefault.IsChecked = false;
        }
    }
}
