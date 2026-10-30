using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUI.LeftPanel;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

/// <summary>
///  Preserves repository-tree identities and hidden-root update boundaries through actual submodule refreshes.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class RepoObjectsTreeRefreshStateTests
{
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    /// <summary>
    ///  Restores the same submodule's caret and expanded state when only its displayed branch changes.
    /// </summary>
    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Structure_refresh_should_keep_stable_submodule_state_when_branch_text_changes(bool providerRefresh)
    {
        WithIsolatedSettings(() =>
        {
            AppSettings.RepoObjectsTreeShowSubmodules = true;
            string topPath = Path.Combine(Path.GetTempPath(), "GitExtensions-submodule-state-model");
            string submodulePath = Path.Combine(topPath, "externals", "foo");
            ISubmoduleStatusProvider provider = Substitute.For<ISubmoduleStatusProvider>();
            IGitModule module = CreateModule(topPath);
            IBrowseRepo browse = Substitute.For<IBrowseRepo>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.BrowseRepo.Returns(browse);
            commands.GetService(typeof(ISubmoduleStatusProvider)).Returns(provider);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RepoObjectsTree control = new() { UICommandsSource = source };
            try
            {
                control.SetRefs([], []);
                RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
                accessor.SetSubmodules(CreateResult(module, topPath, "old"));
                TreeViewItem before = FindSubmodule(SubmoduleRoot(control), submodulePath);
                TreeViewItem nested = FindSubmodule(before, Path.Combine(submodulePath, "nested", "child"));
                before.IsExpanded = true;
                nested.IsExpanded = false;
                accessor.Tree.SelectedItem = before;
                string identity = RepoObjectsTree.GetNodeIdentity((NodeBase)before.Tag!);
                int changes = 0;
                control.NodeSelectionChanged += (_, _) => changes++;
                browse.ClearReceivedCalls();

                SubmoduleInfoResult replacement = CreateResult(module, topPath, "new");
                if (providerRefresh)
                {
                    Notify(provider, replacement);
                }
                else
                {
                    accessor.SetSubmodules(replacement);
                }

                Dispatcher.UIThread.RunJobs();
                TreeViewItem after = FindSubmodule(SubmoduleRoot(control), submodulePath);
                after.Should().NotBeSameAs(before, "a structure refresh must exercise newly built node owners");
                RepoObjectsTree.GetNodeIdentity((NodeBase)after.Tag!).Should().Be(identity);
                ((SubmoduleNode)after.Tag!).Info.Text.Should().Contain("(new)");
                ((TextBlock)((StackPanel)after.Header!).Children[1]).Text.Should().Be("foo (new)");
                accessor.Tree.SelectedItem.Should().BeSameAs(after);
                after.IsExpanded.Should().BeTrue();
                FindSubmodule(after, Path.Combine(submodulePath, "nested", "child")).IsExpanded.Should().BeFalse();
                changes.Should().Be(0, "restoring source state is not a new user selection or activation");
                browse.DidNotReceiveWithAnyArgs().GoToRef(default!, default, default);
            }
            finally
            {
                DisposeRoots(control);
            }
        });
    }

    /// <summary>
    ///  Keeps hidden roots unchanged, including notifications queued before hiding, without inventing reattach replay.
    /// </summary>
    [AvaloniaTest]
    [TestCase(true, false)]
    [TestCase(false, false)]
    [TestCase(false, true)]
    public void Hidden_submodule_root_should_wait_for_a_new_visible_notification(bool initiallyHidden, bool queuedBeforeHide)
    {
        WithIsolatedSettings(() =>
        {
            AppSettings.RepoObjectsTreeShowSubmodules = !initiallyHidden;
            string topPath = Path.Combine(Path.GetTempPath(), "GitExtensions-submodule-hidden-model");
            string submodulePath = Path.Combine(topPath, "externals", "foo");
            ISubmoduleStatusProvider provider = Substitute.For<ISubmoduleStatusProvider>();
            IGitModule module = CreateModule(topPath);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.GetService(typeof(ISubmoduleStatusProvider)).Returns(provider);
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RepoObjectsTree control = new() { UICommandsSource = source };
            try
            {
                control.SetRefs([], []);
                RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
                TreeViewItem root = SubmoduleRoot(control);
                if (!initiallyHidden)
                {
                    Notify(provider, CreateResult(module, topPath, "old"));
                }

                TreeViewItem[] before = [.. root.Items.Cast<TreeViewItem>()];
                SubmoduleInfoResult hiddenUpdate = CreateResult(module, topPath, "hidden");
                if (queuedBeforeHide)
                {
                    Exception? workerFailure = null;
                    Thread worker = new(() =>
                    {
                        try
                        {
                            Notify(provider, hiddenUpdate);
                        }
                        catch (Exception exception)
                        {
                            workerFailure = exception;
                        }
                    });
                    worker.Start();
                    worker.Join(TimeSpan.FromSeconds(10)).Should().BeTrue();
                    workerFailure.Should().BeNull();
                }

                SetVisible(accessor, visible: false);
                if (!queuedBeforeHide)
                {
                    Notify(provider, hiddenUpdate);
                }

                Dispatcher.UIThread.RunJobs();
                accessor.Tree.Items.Cast<TreeViewItem>().Should().NotContain(root);
                root.Items.Cast<TreeViewItem>().Should().Equal(before);

                SetVisible(accessor, visible: true);
                Dispatcher.UIThread.RunJobs();
                accessor.Tree.Items.Cast<TreeViewItem>().Should().Contain(root);
                root.Items.Cast<TreeViewItem>().Should().Equal(before, "the original has no OnAttached replay of cached status");

                Notify(provider, CreateResult(module, topPath, "visible"));
                Dispatcher.UIThread.RunJobs();
                ((SubmoduleNode)FindSubmodule(root, submodulePath).Tag!).Info.Text.Should().Contain("(visible)");
                root.Items.Cast<TreeViewItem>().Should().NotEqual(before);
            }
            finally
            {
                DisposeRoots(control);
            }
        });
    }

    private static void Notify(ISubmoduleStatusProvider provider, SubmoduleInfoResult result)
        => provider.StatusUpdated += Raise.Event<EventHandler<SubmoduleStatusEventArgs>>(
            provider, new SubmoduleStatusEventArgs(result, structureUpdated: true, CancellationToken.None));

    private static void SetVisible(RepoObjectsTree.TestAccessor accessor, bool visible)
    {
        accessor.ShowSubmodulesButton.IsChecked = visible;
        accessor.ShowSubmodulesButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static IGitModule CreateModule(string topPath)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(topPath);
        module.GetTopModule().Returns(module);
        module.SuperprojectModule.Returns((IGitModule?)null);
        return module;
    }

    private static SubmoduleInfoResult CreateResult(IGitModule module, string topPath, string branch)
    {
        SubmoduleInfoResult result = new()
        {
            Module = module,
            TopProject = new SubmoduleInfo("repository (main)", topPath, bold: true),
        };
        string submodulePath = Path.Combine(topPath, "externals", "foo");
        result.AllSubmodules.Add(new SubmoduleInfo($"externals/foo ({branch})", submodulePath, bold: false));
        result.AllSubmodules.Add(new SubmoduleInfo("nested/child (main)", Path.Combine(submodulePath, "nested", "child"), bold: false));
        return result;
    }

    private static TreeViewItem SubmoduleRoot(RepoObjectsTree control)
    {
        FieldInfo field = typeof(RepoObjectsTree).GetField("_submoduleTree", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The persistent submodule root must exist.");
        return (field.GetValue(control) as SubmoduleTree)?.TreeViewNode
            ?? throw new AssertionException("The persistent submodule root must retain its actual tree item.");
    }

    private static TreeViewItem FindSubmodule(TreeViewItem root, string path)
        => Descendants(root).Single(item => item.Tag is SubmoduleNode node && node.Info.Path == path);

    private static IEnumerable<TreeViewItem> Descendants(TreeViewItem root)
    {
        yield return root;
        foreach (TreeViewItem child in root.Items.Cast<TreeViewItem>())
        {
            foreach (TreeViewItem descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static void DisposeRoots(RepoObjectsTree control)
    {
        FieldInfo field = typeof(RepoObjectsTree).GetField("_rootNodes", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The actual tree owners must exist.");
        foreach (Tree root in field.GetValue(control) as IEnumerable<Tree> ?? [])
        {
            root.Dispose();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.SubmoduleRefreshState", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using GitExtSettingsCache cache = new(Path.Combine(directory, "capture.settings"), autoSave: false);
            DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
            AppSettings.UsingContainer(settings, action);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
