using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.CommandsDialogs;

// Reduced shell: the read-only browse panels and first repository commands are functional;
// the remaining upstream menus and toolbars arrive with their corresponding dialogs.
public sealed partial class FormBrowse : GitModuleForm
{
    public FormBrowse()
    {
        InitializeComponent();
    }

    public FormBrowse(IServiceProvider serviceProvider, GitModule module)
        : this(new GitUICommands(serviceProvider, module))
    {
    }

    public FormBrowse(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();

        RevisionGrid.UICommandsSource = this;
        RevisionGrid.SelectionChanged += RevisionGrid_SelectionChanged;
        fileStatusList.SelectedIndexChanged += FileStatusList_SelectedIndexChanged;
        repoObjectsTree.SelectionChanged += RepoObjectsTree_SelectionChanged;
        refreshToolStripMenuItem.Click += RefreshToolStripMenuItemClick;
        checkoutBranchToolStripMenuItem.Click += CheckoutBranchToolStripMenuItemClick;
        branchToolStripMenuItem.Click += CreateBranchToolStripMenuItemClick;
        fetchAllToolStripMenuItem.Click += fetchAllToolStripMenuItem_Click;
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

        ReloadRepository();

        InitializeComplete();
    }

    private void ReloadRepository()
    {
        IGitModule module = Module;

        bool isValidWorkingDir = module.IsValidGitWorkingDir();
        string branchName = isValidWorkingDir ? module.GetSelectedBranch() : string.Empty;

        IAppTitleGenerator appTitleGenerator = UICommands.GetRequiredService<IAppTitleGenerator>();
        Title = appTitleGenerator.Generate(module.WorkingDir, isValidWorkingDir, branchName);

        refreshToolStripMenuItem.IsEnabled = isValidWorkingDir;
        checkoutBranchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        branchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        fetchAllToolStripMenuItem.IsEnabled = isValidWorkingDir;

        if (isValidWorkingDir)
        {
            lblRepoPath.Text = $"{module.WorkingDir}  —  {branchName}";
            lblStatus.Text = $"git: {GitVersion.Current}";
            RevisionGrid.ReloadRevisions(module);

            ThreadHelper.FileAndForget(async () =>
            {
                IReadOnlyList<IGitRef> refs = module.GetRefs(RefsFilter.NoFilter);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                repoObjectsTree.SetRefs(refs);
            });
        }
        else
        {
            lblRepoPath.Text = "No git repository";
            lblStatus.Text = "Start the app inside a repository or pass one on the command line: GitExtensions.Avalonia browse <path>";
        }
    }

    private void RepoObjectsTree_SelectionChanged(object? sender, EventArgs e)
    {
        if (repoObjectsTree.SelectedRef is IGitRef gitRef)
        {
            RevisionGrid.SelectRevision(gitRef.ObjectId);
        }
    }

    private void RevisionGrid_SelectionChanged(object? sender, EventArgs e)
    {
        fileStatusList.Clear();
        fileViewer.ViewPatch(string.Empty);
        CommitInfo.Revision = RevisionGrid.SelectedRevision;

        if (RevisionGrid.SelectedRevision is not GitRevision revision
            || !revision.HasParent)
        {
            return;
        }

        IGitModule module = Module;
        ObjectId parentId = revision.FirstParentId;
        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<GitItemStatus> diffs = module.GetDiffFilesWithSubmodulesStatus(
                parentId,
                revision.ObjectId,
                parentToSecond: parentId,
                excludeSkipWorktreeFiles: false,
                UntrackedFilesMode.Default,
                CancellationToken.None);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (ReferenceEquals(RevisionGrid.SelectedRevision, revision))
            {
                fileStatusList.SetDiffs(diffs);
            }
        });
    }

    private void FileStatusList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (RevisionGrid.SelectedRevision is not GitRevision revision
            || !revision.HasParent
            || fileStatusList.SelectedItem is not GitItemStatus item)
        {
            return;
        }

        IGitModule module = Module;
        ObjectId parentId = revision.FirstParentId;
        ThreadHelper.FileAndForget(async () =>
        {
            (Patch? patch, string? errorMessage) = await module.GetSingleDiffAsync(
                parentId,
                revision.ObjectId,
                item.Name,
                item.OldName,
                extraDiffArguments: "",
                module.FilesEncoding,
                cacheResult: true,
                isTracked: item.IsTracked,
                useGitColoring: false,
                GitCommandConfiguration.Default,
                CancellationToken.None);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (ReferenceEquals(fileStatusList.SelectedItem, item))
            {
                fileViewer.ViewPatch(patch?.Text ?? errorMessage);
            }
        });
    }

    private void UICommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        this.InvokeAndForget(ReloadRepository);
    }

    private void RefreshToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.RepoChangedNotifier.Notify();
    }

    private void CheckoutBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCheckoutBranch(this);
    }

    private void CreateBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCreateBranchDialog(this, RevisionGrid.SelectedRevision?.ObjectId ?? default);
    }

    private void fetchAllToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ArgumentString arguments = new GitArgumentBuilder("fetch")
        {
            "--all",
            "--progress",
        };

        if (UICommands.StartGitCommandProcessDialog(this, arguments))
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }
}
