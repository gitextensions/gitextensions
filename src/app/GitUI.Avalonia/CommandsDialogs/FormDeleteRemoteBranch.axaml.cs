using System.Text;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormDeleteRemoteBranch : GitExtensionsDialog
{
    private readonly TranslationString _deleteRemoteBranchesCaption = new("Delete remote branches");
    private readonly TranslationString _confirmDeleteUnmergedRemoteBranchMessage =
        new("At least one remote branch is unmerged. Are you sure you want to delete it?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");
    private readonly TranslationString _toDeleteCandidates = new("Local tracking branche(s) candidate to deletion:");
    private readonly TranslationString _andMore = new("and {0} more...");

    private readonly string _defaultRemoteBranch = null!;
    private readonly TaskManager _taskManager = GitUI.Compat.DesignTimeTaskManager.Create();
    private HashSet<string>? _mergedBranches;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormDeleteRemoteBranch()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormDeleteRemoteBranch(IGitUICommands commands, string defaultRemoteBranch)
        : base(commands, enablePositionRestore: false)
    {
        _taskManager.FileAndForget(() => _mergedBranches = [.. Module.GetMergedRemoteBranches()]);

        _defaultRemoteBranch = defaultRemoteBranch;

        InitializeComponent();
        Delete.Click += Delete_Click;
        Branches.SelectedValueChanged += Branches_SelectedValueChanged;
        DeleteRemote.IsCheckedChanged += DeleteRemote_CheckedChanged;
        DeleteLocalTrackingBranch.IsCheckedChanged += DeleteRemote_CheckedChanged;
        AcceptButton = Delete;
        ManualSectionAnchorName = "delete-branch";
        ManualSectionSubfolder = "branches";

        InitializeComplete();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Remotes).ToList();

        if (_defaultRemoteBranch is not null)
        {
            Branches.SetSelectedText(_defaultRemoteBranch);
        }

        // WinForms raised the tracking-candidate refresh and initial focus from OnShown; Avalonia
        // folds them into the load event.
        CheckDeleteTrackingAllowed();
        Branches.Focus();
    }

    private List<IGitRef> GetSelectedRemotRefs() => [.. Branches.GetSelectedBranches()];
    private void CheckDeleteTrackingAllowed()
    {
        string[] localTracking = GetTrackingReferenceOfRemoteRefs(GetSelectedRemotRefs());
        bool localTrackingBranchesExists = localTracking.Length != 0;
        const int maxDisplayed = 8;

        if (!localTrackingBranchesExists)
        {
            DeleteLocalTrackingBranch.IsChecked = false;
            DeleteLocalTrackingBranch.IsEnabled = false;
            _NO_TRANSLATE_labelLocalTrackingBranches.Text = string.Empty;
        }
        else
        {
            DeleteLocalTrackingBranch.IsEnabled = true;

            StringBuilder branchesToDelete = new();
            branchesToDelete.AppendLine(_toDeleteCandidates.Text);
            foreach (string branch in localTracking.Take(maxDisplayed))
            {
                branchesToDelete.Append(" - ").AppendLine(branch);
            }

            if (localTracking.Length > maxDisplayed)
            {
                branchesToDelete
                    .AppendLine()
                    .AppendFormat(_andMore.Text, localTracking.Length - maxDisplayed);
            }

            // Avalonia constraint: TextBlock renders a final line break as an extra blank line;
            // WinForms Label measures the same StringBuilder output without that blank row.
            _NO_TRANSLATE_labelLocalTrackingBranches.Text = branchesToDelete.ToString().TrimEnd();
        }
    }

    private string[] GetTrackingReferenceOfRemoteRefs(List<IGitRef> remoteRefs)
        => [.. Module.GetRefs(RefsFilter.Heads)
                 .Where(b => remoteRefs.Any(r => b.IsTrackingRemote(r)))
                 .Select(r => r.LocalName)];

    private void Branches_SelectedValueChanged(object? sender, EventArgs e)
        => CheckDeleteTrackingAllowed();

    private void Delete_Click(object? sender, EventArgs e)
    {
        if (!(DeleteRemote.IsChecked == true))
        {
            return;
        }

        List<IGitRef> selectedBranches = GetSelectedRemotRefs();

        // wait for _mergedBranches to be filled
        _taskManager.JoinPendingOperations();

        Validates.NotNull(_mergedBranches);

        bool hasUnmergedBranches = selectedBranches.Any(branch => !_mergedBranches.Contains(branch.CompleteName));
        if (hasUnmergedBranches)
        {
            if (MessageBoxes.Show(this,
                                _confirmDeleteUnmergedRemoteBranchMessage.Text,
                                _deleteRemoteBranchesCaption.Text,
                                WinFormsShims.MessageBoxButtons.YesNo,
                                WinFormsShims.MessageBoxIcon.Question,
                                WinFormsShims.MessageBoxDefaultButton.Button2) != WinFormsShims.DialogResult.Yes)
            {
                return;
            }
        }

        foreach ((string remote, IEnumerable<IGitRef> branches) in selectedBranches.GroupBy(b => b.Remote))
        {
            // Cross-platform constraint: PuTTY/Pageant key loading is Windows-only; OpenSSH owns
            // portable authentication through FormRemoteProcess (see the platform matrix).
            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforePush, this);
            if (!success)
            {
                return;
            }

            IGitCommand cmd = Commands.DeleteRemoteBranches(remote, branches.Select(x => x.LocalName));
            using FormRemoteProcess form = new(UICommands, cmd.Arguments)
            {
                Remote = remote
            };
            form.ShowDialog(this);

            if (!form.ErrorOccurred() && !Module.InTheMiddleOfAction())
            {
                ScriptsRunner.RunEventScripts(ScriptEvent.AfterPush, this);
                if (DeleteLocalTrackingBranch.IsChecked == true)
                {
                    UICommands.StartDeleteBranchDialog(this, GetTrackingReferenceOfRemoteRefs(selectedBranches));
                }
            }
        }

        UICommands.RepoChangedNotifier.Notify();
        Close();
    }

    private void DeleteRemote_CheckedChanged(object? sender, EventArgs e)
    {
        Delete.IsEnabled = DeleteRemote.IsChecked == true;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormDeleteRemoteBranch form)
    {
        public Avalonia.Controls.Button Delete => form.Delete;
        public Avalonia.Controls.CheckBox DeleteRemote => form.DeleteRemote;
        public Avalonia.Controls.CheckBox DeleteLocalTrackingBranch => form.DeleteLocalTrackingBranch;
        public BranchComboBox Branches => form.Branches;
        public string TrackingCandidateText => form._NO_TRANSLATE_labelLocalTrackingBranches.Text ?? string.Empty;

        public void Load() => form.OnRuntimeLoad(EventArgs.Empty);
    }
}
