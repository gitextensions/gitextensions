using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.CommandsDialogs;

// Reduced shell: the read-only browse panels and first repository commands are functional;
// the remaining upstream menus and toolbars arrive with their corresponding dialogs.
public sealed partial class FormBrowse : GitModuleForm
{
    public static readonly string HotkeySettingsName = "Browse";

    internal enum Command
    {
        GitBash = 0,
        Commit = 7,
        CheckoutBranch = 10,
        PullOrFetch = 39,
        Push = 40,
        CreateBranch = 41,
        MergeBranches = 42,
        CreateTag = 43,
        Rebase = 44,

        // WinForms routes F5 through ToolStripItem.ShortcutKeys. Avalonia has no ToolStrip,
        // so refresh joins the same command dispatcher without changing persisted upstream IDs.
        Refresh = 50,
    }

    public FormBrowse()
    {
        InitializeComponent();
        InitializeComplete();
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
        commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
        checkoutBranchToolStripMenuItem.Click += CheckoutBranchToolStripMenuItemClick;
        branchToolStripMenuItem.Click += CreateBranchToolStripMenuItemClick;
        deleteBranchToolStripMenuItem.Click += DeleteBranchToolStripMenuItemClick;
        pullToolStripMenuItem.Click += PullToolStripMenuItemClick;
        fetchAllToolStripMenuItem.Click += fetchAllToolStripMenuItem_Click;
        mergeBranchToolStripMenuItem.Click += MergeBranchToolStripMenuItemClick;
        rebaseToolStripMenuItem.Click += RebaseToolStripMenuItemClick;
        tagToolStripMenuItem.Click += TagToolStripMenuItemClick;
        deleteTagToolStripMenuItem.Click += DeleteTagToolStripMenuItemClick;
        stashToolStripMenuItem.Click += StashToolStripMenuItemClick;
        userShell.Click += userShell_Click;
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

        ReloadRepository();

        InitializeComplete();
        HotkeysEnabled = true;
        LoadHotkeys(HotkeySettingsName);
    }

    private void ReloadRepository()
    {
        IGitModule module = Module;

        bool isValidWorkingDir = module.IsValidGitWorkingDir();
        string branchName = isValidWorkingDir ? module.GetSelectedBranch() : string.Empty;

        IAppTitleGenerator appTitleGenerator = UICommands.GetRequiredService<IAppTitleGenerator>();
        Title = appTitleGenerator.Generate(module.WorkingDir, isValidWorkingDir, branchName);

        refreshToolStripMenuItem.IsEnabled = isValidWorkingDir;
        commitToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        checkoutBranchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        branchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        deleteBranchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        pullToolStripMenuItem.IsEnabled = isValidWorkingDir;
        fetchAllToolStripMenuItem.IsEnabled = isValidWorkingDir;
        mergeBranchToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        rebaseToolStripMenuItem.IsEnabled = false;
        tagToolStripMenuItem.IsEnabled = isValidWorkingDir;
        deleteTagToolStripMenuItem.IsEnabled = isValidWorkingDir;
        stashToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();

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
        rebaseToolStripMenuItem.IsEnabled =
            RevisionGrid.SelectedRevision is { IsArtificial: false }
            && !Module.IsBareRepository();

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

    private void CommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCommitDialog(this);
    }

    private void CreateBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCreateBranchDialog(this, RevisionGrid.SelectedRevision?.ObjectId ?? default);
    }

    private void DeleteBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartDeleteBranchDialog(this, string.Empty);
    }

    private void PullToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartPullDialog(this);
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

    private void StashToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartStashDialog(this);
    }

    private void MergeBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartMergeBranchDialog(this, branch: null);
    }

    private void RebaseToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (RevisionGrid.SelectedRevision is not { IsArtificial: false } revision)
        {
            return;
        }

        UICommands.StartRebaseDialog(this, revision.ObjectId.ToString());
    }

    private void TagToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCreateTagDialog(this, RevisionGrid.SelectedRevision);
    }

    private void DeleteTagToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartDeleteTagDialog(this, null);
    }

    private void userShell_Click(object? sender, EventArgs e)
    {
        try
        {
            UICommands.GetRequiredService<ITerminalLauncher>().Launch(Module.WorkingDir);
        }
        catch (Exception exception)
        {
            MessageBoxes.FailedToRunShell(this, "Git bash", exception);
        }
    }

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.GitBash: userShell_Click(this, EventArgs.Empty); break;
            case Command.Refresh: RefreshToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Commit: CommitToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.PullOrFetch: PullToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Push: UICommands.StartPushDialog(this, pushOnShow: false); break;
            case Command.CreateBranch: CreateBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.MergeBranches: MergeBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CreateTag: TagToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Rebase: RebaseToolStripMenuItemClick(this, EventArgs.Empty); break;
            default: return base.ExecuteCommand(command);
        }

        return true;
    }

    protected override bool CloseOnEscape => false;

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormBrowse), nameof(userShell), "ToolTipText", "Git bash");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string? translated = translation.TranslateItem(
            nameof(FormBrowse),
            nameof(userShell),
            "ToolTipText",
            () => "Git bash");
        if (!string.IsNullOrEmpty(translated) && userShell.Header is TextBlock header)
        {
            header.Text = translated;
        }
    }
}
