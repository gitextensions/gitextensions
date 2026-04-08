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
            Branches.SelectedIndexChanged += Branches_SelectedIndexChanged;
            InitializeComplete();
        }
        finally
        {
            _isLoading = false;
        }
    }

    public bool IsRemoteBranchChecked => Remotebranch.Checked;
    public string SelectedBranchName => Branches.Text;
    public override string Text => Branches.Text;

    public void Initialize(bool remote, IReadOnlyList<ObjectId>? containObjectIds)
    {
        lbChanges.Text = "";
        LocalBranch.Checked = !remote;
        Remotebranch.Checked = remote;

        _containObjectIds = containObjectIds;

        Branches.Items.Clear();
        Branches.Items.AddRange(_containObjectIds is not null
            ? GetContainsRevisionBranches()
            : LocalBranch.Checked
                ? GetLocalBranches()
                : GetRemoteBranches());

        Branches.ResizeDropDownWidth();

        if (_containObjectIds is not null && Branches.Items.Count == 1)
        {
            Branches.SelectedIndex = 0;
        }
        else
        {
            Branches.Text = null;
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
                                                                 getLocal: LocalBranch.Checked,
                                                                 getRemote: !LocalBranch.Checked,
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
                                                                 getLocal: LocalBranch.Checked,
                                                                 getRemote: !LocalBranch.Checked,
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
        lbChanges.Text = "";
        FireSelectionChangedEvent(sender, e);

        if (string.IsNullOrWhiteSpace(SelectedBranchName))
        {
            lbChanges.Text = "";
        }
        else
        {
            string branchName = SelectedBranchName;
            ObjectId currentCheckout = CommitToCompare.IsZero ? Module.GetCurrentCheckout() : CommitToCompare;

            if (currentCheckout.IsZero)
            {
                lbChanges.Text = "";
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

    private void LocalBranch_CheckedChanged(object sender, EventArgs e)
    {
        Branches.Focus();

        // We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
        ////BranchTypeChanged();
    }

    private void Remotebranch_CheckedChanged(object? sender, EventArgs e)
    {
        Branches.Focus();
        if (!_isLoading)
        {
            Initialize(IsRemoteBranchChecked, null);
        }

        FireSelectionChangedEvent(sender, e);
    }

    private void FireSelectionChangedEvent(object? sender, EventArgs e)
    {
        SelectedIndexChanged?.Invoke(sender, e);
    }

    public new void Focus()
    {
        Branches.Focus();
    }
}
