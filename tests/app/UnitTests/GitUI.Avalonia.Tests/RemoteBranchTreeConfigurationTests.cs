using System.ComponentModel.Design;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.LeftPanel;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RemoteBranchTreeConfigurationTests
{
    private ServiceContainer _services = null!;
    private string _directory = null!;
    private bool _showRemotes;
    private string _prioritizedBranches = null!;
    private string _prioritizedRemotes = null!;
    private Window? _window;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _services = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_services);
        _services.AddService<IGitDirectoryResolver>(new GitDirectoryResolver(new System.IO.Abstractions.FileSystem()));
        GitCommands.ServiceContainerRegistry.RegisterServices(_services);
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RemoteTree-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _showRemotes = AppSettings.RepoObjectsTreeShowRemotes;
        _prioritizedBranches = AppSettings.PrioritizedBranchNames;
        _prioritizedRemotes = AppSettings.PrioritizedRemoteNames;
        AppSettings.RepoObjectsTreeShowRemotes = true;
        AppSettings.PrioritizedBranchNames = string.Empty;
        AppSettings.PrioritizedRemoteNames = string.Empty;
    }

    [TearDown]
    public void TearDown()
    {
        _window?.Close();
        _window = null;
        _services.Dispose();
        TestDirectory.Delete(_directory);
        AppSettings.RepoObjectsTreeShowRemotes = _showRemotes;
        AppSettings.PrioritizedBranchNames = _prioritizedBranches;
        AppSettings.PrioritizedRemoteNames = _prioritizedRemotes;
    }

    [AvaloniaTest]
    [TestCase("upstream")]
    [TestCase("Origin")]
    [TestCase("origin-other")]
    public async Task Runtime_remote_tree_should_exclude_orphan_tracking_refs_but_include_configured_empty_remotes(string orphanName)
    {
        GitModule module = CreateRepository();
        AddRemote(module, "origin");
        AddRemote(module, "empty");
        AddTrackingRef(module, "origin/main");
        AddTrackingRef(module, "origin/feature/ui");
        // Packed configured refs preserve their exact names on case-insensitive hosts
        // before the distinct orphan namespace is created as a loose ref.
        module.GitExecutable.RunCommand(new GitArgumentBuilder("pack-refs") { "--all", "--prune" }).Should().BeTrue();
        AddTrackingRef(module, $"{orphanName}/orphan");
        RepoObjectsTree control = CreateTree(module);

        await PublishSnapshotAsync(control, module);

        RemoteBranchTree tree = GetRemotes(control);
        tree.Nodes.OfType<RemoteRepoNode>().Select(node => node.FullPath).Should().Equal("empty", "origin");
        tree.DescendantsAndSelf().OfType<RemoteBranchNode>().Select(node => node.FullPath)
            .Should().BeEquivalentTo("origin/main", "origin/feature/ui");
        tree.Nodes.OfType<RemoteRepoNode>().Single(node => node.FullPath == "empty").HasChildren.Should().BeFalse();
        module.GetRefs(RefsFilter.Remotes).Select(reference => reference.Name).Should().Contain($"{orphanName}/orphan",
            "tree membership must filter the original refs, never prune or hide them from the Git model");

        AppSettings.PrioritizedRemoteNames = $"{orphanName};origin;empty";
        await PublishSnapshotAsync(control, module);
        tree = GetRemotes(control);
        tree.Nodes.OfType<RemoteRepoNode>().Select(node => node.FullPath).Should().Equal("origin", "empty");
        tree.RefreshInternal(filter => module.GetRefs(filter));
        tree.Nodes.OfType<RemoteRepoNode>().Select(node => node.FullPath).Should().Equal(["origin", "empty"],
            because: "a source-style ref refresh must preserve the explicit configuration filter");
        tree.DescendantsAndSelf().OfType<RemoteBranchNode>().Should().HaveCount(2);
    }

    [AvaloniaTest]
    public async Task Runtime_remote_tree_should_remove_and_recreate_configured_nodes_without_deleting_the_tracking_refs()
    {
        GitModule module = CreateRepository();
        AddRemote(module, "origin");
        AddTrackingRef(module, "origin/main");
        AddTrackingRef(module, "origin/feature/ui");
        RepoObjectsTree control = CreateTree(module);
        await PublishSnapshotAsync(control, module);
        GetRemotes(control).Nodes.OfType<RemoteRepoNode>().Select(node => node.FullPath).Should().Equal("origin");

        module.RemoveConfigSection("remote", "origin");
        module.GetRefs(RefsFilter.Remotes).Should().HaveCount(2,
            "the native mismatch arises when refs survive their configuration section");
        await PublishSnapshotAsync(control, module);
        GetRemotes(control).Nodes.Should().BeEmpty(
            "an explicitly empty enabled-remote snapshot is not the standalone preview's omitted snapshot");

        AddRemote(module, "origin");
        await PublishSnapshotAsync(control, module);
        GetRemotes(control).Nodes.OfType<RemoteRepoNode>().Select(node => node.FullPath).Should().Equal("origin");
        GetRemotes(control).DescendantsAndSelf().OfType<RemoteBranchNode>().Select(node => node.FullPath)
            .Should().BeEquivalentTo("origin/main", "origin/feature/ui");
    }

    [AvaloniaTest]
    public async Task Runtime_remote_tree_should_keep_disabled_remotes_only_in_the_inactive_folder_until_reenabled()
    {
        GitModule module = CreateRepository();
        AddRemote(module, "origin");
        AddTrackingRef(module, "origin/main");
        RepoObjectsTree control = CreateTree(module);
        await PublishSnapshotAsync(control, module);
        ConfigFileRemoteSettingsManager manager = new(() => module);

        manager.ToggleRemoteState("origin", disabled: true);
        AddTrackingRef(module, "origin/main");
        await PublishSnapshotAsync(control, module);
        RemoteBranchTree tree = GetRemotes(control);
        tree.Nodes.Should().ContainSingle().Which.Should().BeOfType<RemoteRepoFolderNode>();
        RemoteRepoNode inactive = tree.DescendantsAndSelf().OfType<RemoteRepoNode>().Should().ContainSingle().Which;
        inactive.FullPath.Should().Be("origin");
        inactive.Enabled.Should().BeFalse();
        inactive.HasChildren.Should().BeFalse();
        tree.DescendantsAndSelf().OfType<RemoteBranchNode>().Should().BeEmpty(
            "a surviving ref cannot create a second enabled copy of a disabled configured remote");

        manager.ToggleRemoteState("origin", disabled: false);
        await PublishSnapshotAsync(control, module);
        tree = GetRemotes(control);
        tree.Nodes.Should().ContainSingle().Which.Should().BeOfType<RemoteRepoNode>().Which.Enabled.Should().BeTrue();
        tree.DescendantsAndSelf().OfType<RemoteBranchNode>().Select(node => node.FullPath).Should().Equal("origin/main");
        tree.DescendantsAndSelf().OfType<RemoteRepoFolderNode>().Should().BeEmpty();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Remote_tree_should_reject_a_zero_object_id_before_testing_configured_membership(bool configured)
    {
        GitModule module = CreateRepository();
        RepoObjectsTree control = CreateTree(module);
        GitRef branch = new(module, objectId: default, "refs/remotes/origin/main", remote: "origin");
        Remote[] remotes = configured ? [new("origin", "https://example.invalid/origin.git", "https://example.invalid/origin.git")] : [];

        Action construct = () => _ = new RemoteBranchTree(control, [branch], enabledRemotes: remotes);

        construct.Should().Throw<InvalidOperationException>().WithMessage("Branch 'origin/main' has no ObjectId.");
    }

    [AvaloniaTest]
    public async Task Runtime_remote_tree_should_observe_cancellation_before_testing_orphan_membership()
    {
        GitModule module = CreateRepository();
        AddTrackingRef(module, "orphan/main");
        RepoObjectsTree control = CreateTree(module);
        await PublishSnapshotAsync(control, module);
        RemoteBranchTree tree = GetRemotes(control);
        MethodInfo fill = typeof(RemoteBranchTree).GetMethod("FillTree", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The source tree-fill boundary must remain available.");
        Action refresh = () => fill.Invoke(tree, [module.GetRefs(RefsFilter.Remotes), new CancellationToken(canceled: true)]);

        refresh.Should().Throw<TargetInvocationException>().WithInnerException<OperationCanceledException>();
        tree.Nodes.Should().BeEmpty();
    }

    private static void AddRemote(GitModule module, string name)
        => module.AddRemote(name, $"https://example.invalid/{name}.git").Should().BeEmpty();

    private static void AddTrackingRef(GitModule module, string name)
        => module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref")
        {
            $"refs/remotes/{name}",
            module.GetCurrentCheckout().ToString(),
        }).Should().BeTrue();

    private GitModule CreateRepository()
    {
        GitModule module = new(_services.GetRequiredService<IGitExecutorProvider>(), _directory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Tree Test");
        module.SetSetting("user.email", "avalonia-tree@example.invalid");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "--allow-empty", "-m", "initial" }).Should().BeTrue();
        return module;
    }

    private RepoObjectsTree CreateTree(GitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        RepoObjectsTree control = new() { UICommandsSource = source };
        _window = new Window { Width = 360, Height = 560, Content = control };
        _window.Show();
        return control;
    }

    private static RemoteBranchTree GetRemotes(RepoObjectsTree control)
        => control.GetTestAccessor().Tree.Items.Cast<TreeViewItem>()
            .Select(item => item.Tag).OfType<RemoteBranchTree>().Single();

    private static async Task PublishSnapshotAsync(RepoObjectsTree control, GitModule module)
    {
        module.InvalidateGitSettings();
        ConfigFileRemoteSettingsManager manager = new(() => module);
        control.SetRefs(
            module.GetRefs(RefsFilter.NoFilter),
            [],
            "main",
            await module.GetRemotesAsync(),
            manager.GetDisabledRemotes(),
            manager,
            currentWorkingDirectory: module.WorkingDir);
        Dispatcher.UIThread.RunJobs();
    }
}
