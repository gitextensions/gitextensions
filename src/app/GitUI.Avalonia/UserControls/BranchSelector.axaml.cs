using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitUI.UserControls;

public partial class BranchSelector : GitModuleControl
{
    public event EventHandler? SelectedIndexChanged;

    private readonly bool _isLoading;
    private IReadOnlyList<ObjectId>? _containObjectIds;
    private string[]? _localBranches;
    private string[]? _remoteBranches;
    public ObjectId CommitToCompare;

    public BranchSelector()
    {
        _isLoading = true;
        try
        {
            InitializeComponent();
            Branches.SelectionChanged += Branches_SelectedIndexChanged;
            LocalBranch.IsCheckedChanged += LocalBranch_CheckedChanged;
            Remotebranch.IsCheckedChanged += Remotebranch_CheckedChanged;
            InitializeComplete();
        }
        finally
        {
            _isLoading = false;
        }
    }

    public bool IsRemoteBranchChecked => Remotebranch.IsChecked == true;
    public string SelectedBranchName => Branches.Text ?? Branches.SelectedItem?.ToString() ?? string.Empty;

    // Avalonia UserControl has no Text property; retain the original read-only consumer boundary.
    public string Text => SelectedBranchName;

    public void Initialize(bool remote, IReadOnlyList<ObjectId>? containObjectIds)
    {
        lbChanges.Text = string.Empty;
        LocalBranch.IsChecked = !remote;
        Remotebranch.IsChecked = remote;

        _containObjectIds = containObjectIds;

        string[] branches = _containObjectIds is not null
            ? GetContainsRevisionBranches()
            : LocalBranch.IsChecked == true
                ? GetLocalBranches()
                : GetRemoteBranches();
        Branches.ItemsSource = branches;

        if (_containObjectIds is not null && branches.Length == 1)
        {
            Branches.SelectedIndex = 0;
        }
        else
        {
            Branches.SelectedItem = null;
            Branches.Text = string.Empty;
        }

        string[] GetLocalBranches()
        {
            return _localBranches ??= [.. Module.GetRefs(RefsFilter.Heads).Select(b => b.Name)];
        }

        string[] GetRemoteBranches()
        {
            return _remoteBranches ??= [.. Module.GetRefs(RefsFilter.Remotes).Select(b => b.Name)];
        }

        string[] GetContainsRevisionBranches()
        {
            HashSet<string> result = [];

            if (_containObjectIds.Count > 0)
            {
                IEnumerable<string> branches =
                    Module.GetAllBranchesWhichContainGivenCommit(_containObjectIds[0],
                                                                 getLocal: LocalBranch.IsChecked == true,
                                                                 getRemote: LocalBranch.IsChecked != true,
                                                                 cancellationToken: default)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.UnionWith(branches);
            }

            for (int index = 1; index < _containObjectIds.Count; index++)
            {
                ObjectId containObjectId = _containObjectIds[index];
                IEnumerable<string> branches =
                    Module.GetAllBranchesWhichContainGivenCommit(containObjectId,
                                                                 getLocal: LocalBranch.IsChecked == true,
                                                                 getRemote: LocalBranch.IsChecked != true,
                                                                 cancellationToken: default)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.IntersectWith(branches);
            }

            return [.. result];
        }
    }

    private void Branches_SelectedIndexChanged(object? sender, EventArgs e)
    {
        lbChanges.Text = string.Empty;
        FireSelectionChangedEvent(sender, e);

        if (string.IsNullOrWhiteSpace(SelectedBranchName))
        {
            lbChanges.Text = string.Empty;
        }
        else
        {
            string branchName = SelectedBranchName;
            ObjectId currentCheckout = CommitToCompare.IsZero ? Module.GetCurrentCheckout() : CommitToCompare;

            if (currentCheckout.IsZero)
            {
                lbChanges.Text = string.Empty;
                return;
            }

            ThreadHelper.FileAndForget(async () =>
            {
                string text = Module.GetCommitCountString(currentCheckout, branchName);
                await this.SwitchToMainThreadAsync();
                lbChanges.Text = text;
            });
        }
    }

    private void LocalBranch_CheckedChanged(object? sender, EventArgs e)
    {
        Branches.Focus();

        // We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
        ////BranchTypeChanged();
    }

    private void Remotebranch_CheckedChanged(object? sender, EventArgs e)
    {
        Branches.Focus();
        if (!_isLoading && Remotebranch.IsChecked == true)
        {
            Initialize(IsRemoteBranchChecked, null);
        }

        FireSelectionChangedEvent(sender, e);
    }

    private void FireSelectionChangedEvent(object? sender, EventArgs e)
    {
        SelectedIndexChanged?.Invoke(sender, e);
    }

    public void Focus()
    {
        Branches.Focus();
    }

    // parity-scaffolding: Exposes the original named fields to focused tests and paired capture seeding.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(BranchSelector control)
    {
        internal ComboBox Branches => control.Branches;
        internal RadioButton LocalBranch => control.LocalBranch;
        internal RadioButton Remotebranch => control.Remotebranch;
        internal TextBlock Changes => control.lbChanges;
    }
}
