using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.LeftPanel;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

/// <summary>
///  Drives <see cref="RepoObjectsTree"/> the way FormBrowse does: the trees are created when the command
///  source is set and each one reloads itself from the module through <see cref="RepoObjectsTree.RefreshRevisionsLoading"/>.
///  A substitute module answers with the refs, remotes and worktrees of the last published snapshot.
/// </summary>
internal static class RepoObjectsTreeTestExtensions
{
    private static readonly ConditionalWeakTable<RepoObjectsTree, Snapshot> Snapshots = new();

    // parity-scaffolding: inspect the original per-tree reload ownership without joining unrelated application tasks.
    private static readonly FieldInfo RootNodes = typeof(RepoObjectsTree).GetField("_rootNodes", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo ReloadTaskRunner = typeof(GitUI.LeftPanel.Tree).GetField("_reloadTaskRunner", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo RunningTask = typeof(ExclusiveTaskRunner).GetField("_runningTask", BindingFlags.Instance | BindingFlags.NonPublic)!;

    // parity-scaffolding: inspect actual source-shaped control state without adding shipping accessor members.
    public static IEnumerable<TreeViewItem> GetLogicalSelection(this RepoObjectsTree.TestAccessor accessor)
        => ((IEnumerable<GitUI.LeftPanel.Tree>)RootNodes.GetValue(GetOwner(accessor))!)
            .Where(tree => tree.IsEnabled)
            .SelectMany(tree => tree.GetSelectedNodes())
            .Select(node => node.TreeViewNode);

    public static SearchControl<string> GetSearchBox(this RepoObjectsTree.TestAccessor accessor)
        => ReadField<SearchControl<string>>(GetOwner(accessor), "_txtBranchCriterion");

    public static Nodes GetNodes(this RepoObjectsTree.TestAccessor accessor, TreeViewItem item)
        => ((NodeBase)item.Tag!).Nodes;

    public static void Search(this RepoObjectsTree.TestAccessor accessor)
        => GetOwner(accessor).FindControl<Button>("btnSearch")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    public static void SetSubmodules(this RepoObjectsTree.TestAccessor accessor, SubmoduleInfoResult result)
        => ((IEnumerable<GitUI.LeftPanel.Tree>)RootNodes.GetValue(GetOwner(accessor))!).OfType<SubmoduleTree>().Single().Load(result);

    public static MenuItem GetActionMenuItem(this RepoObjectsTree.TestAccessor accessor, string action)
    {
        RepoObjectsTree control = GetOwner(accessor);
        IDictionary items = ReadField<IDictionary>(control, "_actionItems");
        Type keyType = items.GetType().GetGenericArguments()[0];
        if (Enum.TryParse(keyType, action, out object? key) && items.Contains(key))
        {
            return (MenuItem)items[key]!;
        }

        return action switch
        {
            "Copy" => ReadField<MenuItem>(control, "copyContextMenuItem"),
            "CheckoutLocal" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.GitRefCheckout),
            "CheckoutRemote" => GetMenuItem(ReadField<RemoteBranchMenuItems<RemoteBranchNode>>(control, "_remoteBranchMenuItems"), MenuItemKey.GitRefCheckout),
            "Merge" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.GitRefMerge),
            "RebaseLocal" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.GitRefRebase),
            "RebaseRemote" => GetMenuItem(ReadField<RemoteBranchMenuItems<RemoteBranchNode>>(control, "_remoteBranchMenuItems"), MenuItemKey.GitRefRebase),
            "RebaseTag" => GetMenuItem(ReadField<TagMenuItems<TagNode>>(control, "_tagNodeMenuItems"), MenuItemKey.GitRefRebase),
            "CreateBranch" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.GitRefCreateBranch),
            "Reset" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.GitRefReset),
            "RenameBranch" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.Rename),
            "DeleteBranch" => GetMenuItem(ReadField<LocalBranchMenuItems<LocalBranchNode>>(control, "_localBranchMenuItems"), MenuItemKey.Delete),
            "DeleteRemoteBranch" => GetMenuItem(ReadField<RemoteBranchMenuItems<RemoteBranchNode>>(control, "_remoteBranchMenuItems"), MenuItemKey.Delete),
            "CheckoutTag" => GetMenuItem(ReadField<TagMenuItems<TagNode>>(control, "_tagNodeMenuItems"), MenuItemKey.GitRefCheckout),
            "DeleteTag" => GetMenuItem(ReadField<TagMenuItems<TagNode>>(control, "_tagNodeMenuItems"), MenuItemKey.Delete),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }

    private static MenuItem GetMenuItem<TNode>(MenuItemsGenerator<TNode> generator, MenuItemKey key) where TNode : class, INode
    {
        return generator.TryGetMenuItem(key, out Control? item) && item is MenuItem menuItem
            ? menuItem
            : throw new ArgumentOutOfRangeException(nameof(key), key, null);
    }

    private static RepoObjectsTree GetOwner(RepoObjectsTree.TestAccessor accessor)
        => accessor.TreeView.FindLogicalAncestorOfType<RepoObjectsTree>()
            ?? throw new InvalidOperationException("The tree view is not hosted by a RepoObjectsTree.");

    private static T ReadField<T>(RepoObjectsTree control, string name)
        => (T)(typeof(RepoObjectsTree).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(control)
            ?? throw new MissingFieldException(typeof(RepoObjectsTree).FullName, name));

    /// <summary>Creates a tree whose trees were built with the given revision-grid services, like FormBrowse's Initialize.</summary>
    internal static RepoObjectsTree Create(
        IGitUICommandsSource? source = null,
        IAheadBehindDataProvider? aheadBehindDataProvider = null,
        ICheckRefs? refsSource = null,
        IRevisionGridInfo? revisionGridInfo = null,
        Action<string?>? filterRevisionGridBySpaceSeparatedRefs = null)
    {
        if (source is null)
        {
            source = Substitute.For<IGitUICommandsSource>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(Substitute.For<IGitModule>());
            source.UICommands.Returns(commands);
        }

        RepoObjectsTree control = new();
        Attach(control, source, aheadBehindDataProvider, refsSource, revisionGridInfo, filterRevisionGridBySpaceSeparatedRefs);
        return control;
    }

    public static void SetRefs(this RepoObjectsTree control, IReadOnlyList<IGitRef> refs)
        => control.Publish(refs, [], currentBranch: string.Empty, enabledRemotes: null, disabledRemotes: null, worktrees: null, currentWorkingDirectory: string.Empty);

    public static void SetRefs(this RepoObjectsTree control, IReadOnlyList<IGitRef> refs, IReadOnlyCollection<GitRevision> stashes)
        => control.Publish(refs, stashes, currentBranch: string.Empty, enabledRemotes: null, disabledRemotes: null, worktrees: null, currentWorkingDirectory: string.Empty);

    public static void SetRefs(this RepoObjectsTree control, IReadOnlyList<IGitRef> refs, IReadOnlyCollection<GitRevision> stashes, string currentBranch)
        => control.Publish(refs, stashes, currentBranch, enabledRemotes: null, disabledRemotes: null, worktrees: null, currentWorkingDirectory: string.Empty);

    public static void SetRefs(
        this RepoObjectsTree control,
        IReadOnlyList<IGitRef> refs,
        IReadOnlyCollection<GitRevision> stashes,
        string currentBranch,
        IReadOnlyList<Remote> enabledRemotes,
        IReadOnlyList<Remote> disabledRemotes,
        IConfigFileRemoteSettingsManager remotesManager,
        IReadOnlyList<GitWorktree>? worktrees = null,
        string currentWorkingDirectory = "")
    {
        _ = remotesManager;
        control.Publish(refs, stashes, currentBranch, enabledRemotes, disabledRemotes, worktrees, currentWorkingDirectory);
    }

    /// <summary>Reloads the worktree tree from the given worktrees, keeping the refs of the last snapshot.</summary>
    public static void SetWorktrees(this RepoObjectsTree.TestAccessor accessor, IReadOnlyList<GitWorktree> worktrees, string currentWorkingDirectory)
    {
        RepoObjectsTree control = accessor.TreeView.FindLogicalAncestorOfType<RepoObjectsTree>()
            ?? throw new InvalidOperationException("The accessor's tree view is not hosted by a RepoObjectsTree.");
        Snapshot snapshot = control.GetSnapshot();
        control.Publish(snapshot.Refs, snapshot.Stashes, snapshot.CurrentBranch, snapshot.EnabledRemotes, snapshot.DisabledRemotes, worktrees, currentWorkingDirectory);
    }

    /// <summary>Reloads every tree from the module of the control's command source, like FormBrowse's refresh.</summary>
    public static void RefreshFromModule(this RepoObjectsTree control, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        control.RefreshRevisionsLoading(getRefs, new Lazy<IReadOnlyCollection<GitRevision>>(() => []), forceRefresh: true);
        WaitForLoading(control);
        control.RefreshRevisionsLoaded();
        WaitForLoading(control);
    }

    private static void Publish(
        this RepoObjectsTree control,
        IReadOnlyList<IGitRef> refs,
        IReadOnlyCollection<GitRevision> stashes,
        string currentBranch,
        IReadOnlyList<Remote>? enabledRemotes,
        IReadOnlyList<Remote>? disabledRemotes,
        IReadOnlyList<GitWorktree>? worktrees,
        string currentWorkingDirectory)
    {
        Snapshot snapshot = control.GetSnapshot();
        bool moduleChanged = !string.IsNullOrEmpty(snapshot.WorkingDirectory)
            && !string.IsNullOrEmpty(currentWorkingDirectory)
            && snapshot.WorkingDirectory != currentWorkingDirectory;
        snapshot.Refs = refs;
        snapshot.Stashes = stashes;
        snapshot.CurrentBranch = currentBranch;
        snapshot.EnabledRemotes = enabledRemotes;
        snapshot.DisabledRemotes = disabledRemotes ?? [];
        snapshot.Worktrees = worktrees ?? [];
        snapshot.WorkingDirectory = currentWorkingDirectory;

        if (!control.TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(Substitute.For<IGitModule>());
            source.UICommands.Returns(commands);
            Attach(control, source, aheadBehindDataProvider: null, refsSource: null, revisionGridInfo: null);
        }

        Configure(commands.Module, snapshot);
        IGitUICommandsSource sourceToNotify = control.UICommandsSource is CommandsHost host ? host.Source : control.UICommandsSource;
        if (moduleChanged && sourceToNotify.GetType().Namespace == "Castle.Proxies")
        {
            // FormBrowse raises the module change before it refreshes the trees of another repository.
            sourceToNotify.UICommandsChanged += Raise.EventWith(sourceToNotify, new GitUICommandsChangedEventArgs(commands));
        }

        control.RefreshRevisionsLoading(
            filter => Filter(snapshot.Refs, filter),
            new Lazy<IReadOnlyCollection<GitRevision>>(() => snapshot.Stashes),
            forceRefresh: true);
        WaitForLoading(control);
        control.RefreshRevisionsLoaded();
        WaitForLoading(control);
    }

    private static Snapshot GetSnapshot(this RepoObjectsTree control)
        => Snapshots.GetValue(control, _ => new Snapshot());

    private static void Attach(
        RepoObjectsTree control,
        IGitUICommandsSource source,
        IAheadBehindDataProvider? aheadBehindDataProvider,
        ICheckRefs? refsSource,
        IRevisionGridInfo? revisionGridInfo,
        Action<string?>? filterRevisionGridBySpaceSeparatedRefs = null)
    {
        Snapshot snapshot = control.GetSnapshot();
        if (refsSource is null)
        {
            refsSource = Substitute.For<ICheckRefs>();
            refsSource.Contains(Arg.Any<ObjectId>()).Returns(true);
        }

        if (revisionGridInfo is null)
        {
            revisionGridInfo = Substitute.For<IRevisionGridInfo>();
            revisionGridInfo.GetCurrentBranch().Returns(_ => snapshot.CurrentBranch);
        }

        // Initialize supplies the services before lazy command-source discovery constructs the trees.
        CommandsHost host = new(source);
        Avalonia.StyledElement? parent = control.Parent;
        int index = parent is Panel panel ? panel.Children.IndexOf(control) : -1;

        void ReplaceChild(Control child)
        {
            switch (parent)
            {
                case null:
                    break;
                case Decorator decorator:
                    decorator.Child = child;
                    break;
                case ContentControl content:
                    content.Content = child;
                    break;
                case Panel container:
                    container.Children[index] = child;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported test host: {parent.GetType().Name}.");
            }
        }

        ReplaceChild(host);
        host.Child = control;
        try
        {
            control.Initialize(aheadBehindDataProvider!, filterRevisionGridBySpaceSeparatedRefs ?? (_ => { }), refsSource, revisionGridInfo);
        }
        finally
        {
            host.Child = null;
            ReplaceChild(control);
        }
    }

    private static void Configure(IGitModule module, Snapshot snapshot)
    {
        if (module.GetType().Namespace != "Castle.Proxies")
        {
            return;
        }

        module.IsValidGitWorkingDir().Returns(true);
        module.GetRemotesAsync().Returns(_ => Task.FromResult<IReadOnlyList<Remote>>(snapshot.EnabledRemotes ?? GetRemotesOfRefs(snapshot.Refs)));
        module.GetWorktrees().Returns(_ => snapshot.Worktrees);
        if (!string.IsNullOrEmpty(snapshot.WorkingDirectory))
        {
            module.WorkingDir.Returns(snapshot.WorkingDirectory);
        }

        module.GetAllLocalSettings().Returns(_ => snapshot.DisabledRemotes
            .Select(remote => ($"{ConfigFileRemoteSettingsManager.SectionRemoteDisabled}.{remote.Name}.url", remote.FetchUrl))
            .ToList());
        module.GetEffectiveSetting(Arg.Any<string>()).Returns(call => snapshot.DisabledRemotes
            .Where(remote => call.ArgAt<string>(0) == $"{ConfigFileRemoteSettingsManager.SectionRemoteDisabled}.{remote.Name}.url")
            .Select(remote => remote.FetchUrl)
            .FirstOrDefault(string.Empty));
        module.GetSettings(Arg.Any<string>()).Returns(call => snapshot.DisabledRemotes
            .Where(remote => call.ArgAt<string>(0) == $"{ConfigFileRemoteSettingsManager.SectionRemoteDisabled}.{remote.Name}.pushurl")
            .SelectMany(remote => remote.PushUrls)
            .ToList());
    }

    private static IReadOnlyList<Remote> GetRemotesOfRefs(IReadOnlyList<IGitRef> refs)
        => [.. refs
            .Where(gitRef => gitRef.IsRemote)
            .Select(gitRef => gitRef.Name.Split('/')[0])
            .Distinct(StringComparer.Ordinal)
            .Select(name => new Remote(name, $"https://example.invalid/{name}.git", $"https://example.invalid/{name}.git"))];

    private static IReadOnlyList<IGitRef> Filter(IReadOnlyList<IGitRef> refs, RefsFilter filter)
        => filter switch
        {
            RefsFilter.Heads => [.. refs.Where(gitRef => gitRef.IsHead && !gitRef.IsTag)],
            RefsFilter.Remotes => [.. refs.Where(gitRef => gitRef.IsRemote)],
            RefsFilter.Tags => [.. refs.Where(gitRef => gitRef.IsTag && !gitRef.IsDereference)],
            _ => refs,
        };

    /// <summary>Runs the UI thread until this control's trees have applied their background loads.</summary>
    internal static void WaitForLoading(RepoObjectsTree control)
    {
        List<Task> tasks = [];
        foreach (GitUI.LeftPanel.Tree tree in (IEnumerable<GitUI.LeftPanel.Tree>)RootNodes.GetValue(control)!)
        {
            ExclusiveTaskRunner runner = (ExclusiveTaskRunner)ReloadTaskRunner.GetValue(tree)!;
            if (RunningTask.GetValue(runner) is JoinableTask running)
            {
                tasks.Add(running.Task);
            }
        }

        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(30));
        Task pending = Task.WhenAll(tasks);
        while (!pending.IsCompleted && !deadline.IsCancellationRequested)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        if (!pending.IsCompletedSuccessfully)
        {
            throw new TimeoutException("The repository trees did not finish loading.", pending.Exception);
        }

        Dispatcher.UIThread.RunJobs();
    }

    // parity-scaffolding: provide the external command source through the production ancestor-discovery path.
    private sealed class CommandsHost(IGitUICommandsSource source) : Decorator, IGitUICommandsSource
    {
        public IGitUICommandsSource Source => source;

        public IGitUICommands UICommands => source.UICommands;

        public event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged
        {
            add => source.UICommandsChanged += value;
            remove => source.UICommandsChanged -= value;
        }
    }

    private sealed class Snapshot
    {
        public IReadOnlyList<IGitRef> Refs { get; set; } = [];

        public IReadOnlyCollection<GitRevision> Stashes { get; set; } = [];

        public string CurrentBranch { get; set; } = string.Empty;

        public IReadOnlyList<Remote>? EnabledRemotes { get; set; }

        public IReadOnlyList<Remote> DisabledRemotes { get; set; } = [];

        public IReadOnlyList<GitWorktree> Worktrees { get; set; } = [];

        public string WorkingDirectory { get; set; } = string.Empty;
    }
}
