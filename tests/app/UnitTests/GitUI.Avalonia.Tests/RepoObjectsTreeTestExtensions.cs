using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.LeftPanel;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
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

    /// <summary>Creates a tree whose trees were built with the given revision-grid services, like FormBrowse's Initialize.</summary>
    internal static RepoObjectsTree Create(
        IGitUICommandsSource source,
        IAheadBehindDataProvider? aheadBehindDataProvider = null,
        ICheckRefs? refsSource = null,
        IRevisionGridInfo? revisionGridInfo = null)
    {
        RepoObjectsTree control = new();
        Attach(control, source, aheadBehindDataProvider, refsSource, revisionGridInfo);
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
        WaitForLoading();
        control.RefreshRevisionsLoaded();
        WaitForLoading();
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
        if (moduleChanged && control.UICommandsSource.GetType().Namespace == "Castle.Proxies")
        {
            // FormBrowse raises the module change before it refreshes the trees of another repository.
            control.UICommandsSource.UICommandsChanged += Raise.EventWith(control.UICommandsSource, new GitUICommandsChangedEventArgs(commands));
        }

        control.RefreshRevisionsLoading(
            filter => Filter(snapshot.Refs, filter),
            new Lazy<IReadOnlyCollection<GitRevision>>(() => snapshot.Stashes),
            forceRefresh: true);
        WaitForLoading();
        control.RefreshRevisionsLoaded();
        WaitForLoading();
    }

    private static Snapshot GetSnapshot(this RepoObjectsTree control)
        => Snapshots.GetValue(control, _ => new Snapshot());

    private static void Attach(
        RepoObjectsTree control,
        IGitUICommandsSource source,
        IAheadBehindDataProvider? aheadBehindDataProvider,
        ICheckRefs? refsSource,
        IRevisionGridInfo? revisionGridInfo)
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

        // FormBrowse initializes the revision-grid services before the command source creates the trees.
        SetField(control, "_aheadBehindDataProvider", aheadBehindDataProvider);
        SetField(control, "_refsSource", refsSource);
        SetField(control, "_revisionGridInfo", revisionGridInfo);
        control.UICommandsSource = source;
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

    /// <summary>Runs the UI thread until every tree has applied its background load.</summary>
    internal static void WaitForLoading()
    {
        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(30));
        Task pending = ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
        while (!pending.IsCompleted)
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

    private static void SetField(RepoObjectsTree control, string name, object? value)
        => (typeof(RepoObjectsTree).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(nameof(RepoObjectsTree), name)).SetValue(control, value);

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
